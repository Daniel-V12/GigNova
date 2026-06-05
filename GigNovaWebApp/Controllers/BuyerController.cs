using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Mvc;

namespace GigNovaWebApp.Controllers
{
    public class BuyerController : Controller
    {
        // ============================== Home & Profile ==============================

        // Buyer landing page. Tries to fetch the display name from the WS to show "Welcome, X".
        [HttpGet]
        public async Task<IActionResult> HomePage()
        {
            ViewData["HomeActor"] = "buyer";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterBuyerPage.cshtml";

            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId != null)
            {
                try
                {
                    ApiClient<BuyerProfileViewmodel> client = BuildClient<BuyerProfileViewmodel>("api/Buyer/GetBuyerProfileViewModel");
                    client.AddParameter("buyer_id", buyerId);

                    BuyerProfileViewmodel viewModel = await client.GetAsync();
                    if (viewModel != null && viewModel.buyer != null && viewModel.buyer.Buyer_display_name != null)
                    {
                        ViewData["BuyerDisplayName"] = viewModel.buyer.Buyer_display_name;
                    }
                }
                catch
                {
                }
            }

            return View("~/Views/Shared/HomePage.cshtml");
        }

        // Shows the buyer's profile page. Falls back to the session id if no id is in the URL.
        [HttpGet]
        public async Task<IActionResult> BuyerProfile(string buyer_id)
        {
            if (buyer_id == null)
            {
                buyer_id = HttpContext.Session.GetString("person_id");
            }
            if (buyer_id == null)
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<BuyerProfileViewmodel> client = BuildClient<BuyerProfileViewmodel>("api/Buyer/GetBuyerProfileViewModel");
            client.AddParameter("buyer_id", buyer_id);

            BuyerProfileViewmodel viewModel = await client.GetAsync();

            // Make sure none of the nested objects are null (the view assumes they exist).
            viewModel = viewModel ?? new BuyerProfileViewmodel();
            viewModel.buyer = viewModel.buyer ?? new Buyer();
            viewModel.buyer_person = viewModel.buyer_person ?? new Person();

            if (string.IsNullOrEmpty(viewModel.buyer.Person_join_date))
            {
                viewModel.buyer.Person_join_date = viewModel.buyer_person.Person_join_date;
            }

            return View(viewModel);
        }

        // Handles the profile update POST. Calls the WS, then redirects back with a success/failure message.
        [HttpPost]
        public async Task<IActionResult> BuyerProfile(BuyerProfileUpdateViewModel viewModel)
        {
            if (viewModel == null)
            {
                TempData["BuyerProfileMessage"] = "Failed to update profile.";
                return RedirectToAction("BuyerProfile");
            }

            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId == null)
            {
                return RedirectToAction("HomePage", "Guest");
            }

            viewModel.Person_id = buyerId;
            viewModel.Buyer_description = viewModel.Buyer_description ?? "";

            ApiClient<BuyerProfileUpdateViewModel> client = BuildClient<BuyerProfileUpdateViewModel>("api/Buyer/UpdateBuyerProfile");
            bool response = await client.PostAsyncReturn<BuyerProfileUpdateViewModel, bool>(viewModel);
            if (response)
            {
                TempData["BuyerProfileMessage"] = "Profile updated successfully.";
            }
            else
            {
                TempData["BuyerProfileMessage"] = "Failed to update profile.";
            }

            return RedirectToAction("BuyerProfile", new { buyer_id = buyerId });
        }

        // Sends current + new password to the WS. The WS verifies the current password before updating.
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId == null)
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                TempData["BuyerProfileMessage"] = "Please enter current and new password.";
                return RedirectToAction("BuyerProfile", new { buyer_id = buyerId });
            }

            ApiClient<string> client = BuildClient<string>("api/Buyer/ChangeBuyerPassword");
            client.AddParameter("buyer_id", buyerId);
            client.AddParameter("current_password", currentPassword);
            client.AddParameter("new_password", newPassword);

            bool response = await client.PostAsyncReturn<string, bool>("");
            if (response)
            {
                TempData["BuyerProfileMessage"] = "Password changed successfully.";
            }
            else
            {
                TempData["BuyerProfileMessage"] = "Failed to change password. Make sure current password is correct.";
            }

            return RedirectToAction("BuyerProfile", new { buyer_id = buyerId });
        }

        // Clears the session and sends the user back to the guest home page.
        [HttpPost]
        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("person_id");
            return RedirectToAction("HomePage", "Guest");
        }


        // ============================== Orders (View / Customize / Complete / Delivery) ==============================

        // Shows the buyer's paginated orders. Page size is 6; "has next page" is true when the page returned a full 6.
        [HttpGet]
        public async Task<IActionResult> ViewOrders(string buyerId, int page = 1)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                buyerId = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrEmpty(buyerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (page < 1)
            {
                page = 1;
            }

            int pageSize = 6;
            List<CustomizeOrderViewModel> pagedOrders = new List<CustomizeOrderViewModel>();

            ApiClient<List<CustomizeOrderViewModel>> client = BuildClient<List<CustomizeOrderViewModel>>("api/Buyer/GetOrderedGigsDetailsViewModel");
            client.AddParameter("buyerId", buyerId);
            client.AddParameter("page", page.ToString());
            client.AddParameter("pageSize", pageSize.ToString());

            List<CustomizeOrderViewModel> loaded = await client.GetAsync();
            if (loaded != null)
            {
                pagedOrders = loaded;
            }

            // If we got a full page back, assume there's at least one more page.
            bool hasNextPage = pagedOrders.Count == pageSize;
            ViewData["CurrentPage"] = page;
            if (hasNextPage)
            {
                ViewData["TotalPages"] = page + 1;
            }
            else
            {
                ViewData["TotalPages"] = page;
            }
            ViewData["BuyerIdForPaging"] = buyerId;

            return View("~/Views/Buyer/ViewOrders.cshtml", pagedOrders);
        }

        // Shows the detail page for one order. Bounces back to the orders list if the order can't be loaded.
        [HttpGet]
        public async Task<IActionResult> SelectedOrder(string order_id)
        {
            if (string.IsNullOrEmpty(order_id))
            {
                return RedirectToAction("ViewOrders");
            }

            CustomizeOrderViewModel selectedOrder;
            try
            {
                selectedOrder = await GetOrderDetails(order_id);
            }
            catch
            {
                selectedOrder = null;
            }

            if (selectedOrder == null || selectedOrder.order == null || string.IsNullOrEmpty(selectedOrder.order.Order_id))
            {
                return RedirectToAction("ViewOrders");
            }

            return View("~/Views/Buyer/SelectedOrder.cshtml", selectedOrder);
        }

        // Renders the "customize order" page either from an existing order id or from a gig id (for a brand new order).
        [HttpGet]
        public async Task<IActionResult> CustomizeOrder(string order_id = null, string gig_id = null)
        {
            ApiClient<CustomizeOrderViewModel> client = BuildClient<CustomizeOrderViewModel>("api/Buyer/GetCustomizeOrderViewModel");
            if (order_id != null)
            {
                client.AddParameter("order_id", order_id);
            }
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId != null)
            {
                client.AddParameter("buyer_id", buyerId);
            }
            CustomizeOrderViewModel customizeOrderViewModel = await client.GetAsync();
            return View(customizeOrderViewModel);
        }

        // Handles the "customize order" POST: validates the model + files, then sends them to the WS to create the order.
        [HttpPost]
        public async Task<IActionResult> CustomizeOrder([FromForm] CustomizeOrderModel model)
        {
            if (model == null)
            {
                TempData["CustomizeOrderMessage"] = "Something went wrong. Please try again.";
                return RedirectToAction("ViewCatalogPage", "Guest");
            }

            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId == null)
            {
                return RedirectToAction("HomePage", "Guest");
            }
            model.Buyer_id = buyerId;

            if (model.Gig_id <= 0)
            {
                TempData["CustomizeOrderMessage"] = "Please pick a gig first.";
                return RedirectToAction("ViewCatalogPage", "Guest");
            }

            model.requirements = model.requirements ?? "";

            if (ModelState.IsValid == false)
            {
                string firstError = "Please fill required fields.";
                foreach (var entry in ModelState.Values)
                {
                    if (entry.Errors.Count > 0)
                    {
                        firstError = entry.Errors[0].ErrorMessage;
                        break;
                    }
                }
                TempData["CustomizeOrderMessage"] = firstError;
                return RedirectToAction("CustomizeOrder", new { gig_id = model.Gig_id });
            }

            // Order file rules: max 5 files, max 50MB each, no blocked extensions.
            if (model.Files != null && model.Files.Count > 0)
            {
                if (model.Files.Count > 5)
                {
                    TempData["CustomizeOrderMessage"] = "You can upload up to 5 files only.";
                    return RedirectToAction("CustomizeOrder", new { gig_id = model.Gig_id });
                }

                string[] blockedExtensions = new string[] { ".exe", ".bat", ".cmd", ".sh", ".ps1", ".msi", ".dll", ".vbs" };
                long maxFileSize = 50 * 1024 * 1024;

                foreach (IFormFile file in model.Files)
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }
                    if (file.Length > maxFileSize)
                    {
                        TempData["CustomizeOrderMessage"] = "Each file must be 50MB or smaller.";
                        return RedirectToAction("CustomizeOrder", new { gig_id = model.Gig_id });
                    }
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    for (int i = 0; i < blockedExtensions.Length; i++)
                    {
                        if (blockedExtensions[i] == ext)
                        {
                            TempData["CustomizeOrderMessage"] = "File type not allowed. Blocked: exe, bat, cmd, sh, ps1, msi, dll, vbs.";
                            return RedirectToAction("CustomizeOrder", new { gig_id = model.Gig_id });
                        }
                    }
                }
            }

            // Collect non-empty file streams to forward to the WS.
            List<Stream> filesToSend = new List<Stream>();
            List<string> fileNames = new List<string>();
            if (model.Files != null)
            {
                foreach (IFormFile file in model.Files)
                {
                    if (file.Length > 0)
                    {
                        filesToSend.Add(file.OpenReadStream());
                        fileNames.Add(file.FileName);
                    }
                }
            }

            bool response = await PostCustomizeOrder(model, filesToSend, fileNames);

            foreach (Stream stream in filesToSend)
            {
                stream.Dispose();
            }

            if (response == false)
            {
                TempData["CustomizeOrderMessage"] = "Failed to create order. Please try again.";
                return RedirectToAction("CustomizeOrder", new { gig_id = model.Gig_id });
            }

            TempData["OrdersMessage"] = "Your order has been created successfully!";
            return RedirectToAction("ViewOrders", new { buyerId = buyerId });
        }

        // Marks an order as completed via the WS, then redirects back to the order detail page.
        [HttpPost]
        public async Task<IActionResult> CompleteOrder(string order_id)
        {
            string buyerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(buyerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }
            if (string.IsNullOrWhiteSpace(order_id))
            {
                return RedirectToAction("ViewOrders", new { buyerId = buyerId });
            }

            ApiClient<string> client = BuildClient<string>("api/Buyer/CompleteOrder");
            client.AddParameter("order_id", order_id);
            client.AddParameter("buyer_id", buyerId);
            await client.PostAsyncReturn<string, bool>("");

            return RedirectToAction("SelectedOrder", new { order_id = order_id });
        }

        // Shows the delivery files attached to an order; supports paging through multiple deliveries by index.
        [HttpGet]
        public async Task<IActionResult> DeliveryDetails(string order_id, int delivery_index = 0)
        {
            if (string.IsNullOrWhiteSpace(order_id))
            {
                return RedirectToAction("ViewOrders");
            }

            CustomizeOrderViewModel orderDetails = await GetOrderDetails(order_id);
            orderDetails = orderDetails ?? new CustomizeOrderViewModel();
            if (orderDetails.order == null)
            {
                orderDetails.order = new Order();
                orderDetails.order.Order_id = order_id;
            }

            ApiClient<List<Delivery>> client = BuildClient<List<Delivery>>("api/Buyer/GetDeliveriesByOrder");
            client.AddParameter("order_id", order_id);

            List<Delivery> deliveries = await client.GetAsync();
            deliveries = deliveries ?? new List<Delivery>();

            if (deliveries.Count == 0)
            {
                TempData["DeliveryDetailsMessage"] = "No deliveries found for this order yet.";
                return RedirectToAction("SelectedOrder", new { order_id = order_id });
            }

            // Clamp delivery_index to a valid range.
            if (delivery_index < 0)
            {
                delivery_index = 0;
            }
            if (delivery_index >= deliveries.Count)
            {
                delivery_index = deliveries.Count - 1;
            }

            // Split the pipe-separated file paths on the selected delivery into a list.
            Delivery selectedDelivery = deliveries[delivery_index];
            List<string> files = new List<string>();
            if (selectedDelivery != null && string.IsNullOrWhiteSpace(selectedDelivery.Delivery_file) == false)
            {
                string[] split = selectedDelivery.Delivery_file.Split('|');
                foreach (string part in split)
                {
                    if (string.IsNullOrWhiteSpace(part) == false)
                    {
                        files.Add(part.Trim());
                    }
                }
            }

            ViewData["Actor"] = HttpContext.Session.GetString("actor");
            ViewData["OrderId"] = order_id;
            ViewData["DeliveryCount"] = deliveries.Count;
            ViewData["DeliveryIndex"] = delivery_index;
            ViewData["SelectedDelivery"] = selectedDelivery;
            ViewData["DeliveryFiles"] = files;
            ViewData["Gig"] = orderDetails.gig;
            return View("~/Views/Buyer/DeliveryDetails.cshtml", orderDetails);
        }

        // Loads the full CustomizeOrderViewModel for one order from the WS. Used by SelectedOrder and DeliveryDetails.
        private async Task<CustomizeOrderViewModel> GetOrderDetails(string orderId)
        {
            ApiClient<CustomizeOrderViewModel> detailsClient = BuildClient<CustomizeOrderViewModel>("api/Buyer/GetCustomizeOrderViewModel");
            detailsClient.AddParameter("order_id", orderId);
            return await detailsClient.GetAsync();
        }

        // Posts a CustomizeOrderModel + its files to the WS as a multipart form.
        private async Task<bool> PostCustomizeOrder(CustomizeOrderModel model, List<Stream> filesToSend, List<string> fileNames)
        {
            ApiClient<CustomizeOrderModel> client = BuildClient<CustomizeOrderModel>("api/Buyer/CreateOrderAndPayWithFiles");
            return await client.PostAsync(model, filesToSend, fileNames);
        }


        // ============================== Messaging ==============================

        // Shows the messages page (optionally filtered by a specific order).
        [HttpGet]
        public async Task<IActionResult> MessagingBox(string buyer_id, string order_id = null, string from_role = null)
        {
            if (string.IsNullOrEmpty(buyer_id))
            {
                buyer_id = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrEmpty(buyer_id))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<MessagesBoxViewModel> client = BuildClient<MessagesBoxViewModel>("api/Buyer/MessagingBoxViewModel");
            client.AddParameter("person_id", buyer_id);
            if (string.IsNullOrEmpty(order_id) == false)
            {
                client.AddParameter("order_id", order_id);
            }
            MessagesBoxViewModel viewModel = await client.GetAsync();
            ViewBag.CurrentPersonId = buyer_id;
            ViewBag.OrderId = order_id;
            ViewBag.FromRole = from_role;
            return View(viewModel);
        }

        // Sends a new message via the WS; the WS auto-sets the receiver based on who the sender is on the order.
        [HttpPost]
        public async Task<IActionResult> SendMessage(Message message)
        {
            string senderId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(senderId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (message == null || message.Order_id <= 0 || string.IsNullOrWhiteSpace(message.Message_text))
            {
                TempData["MessagingBoxMessage"] = "Please write a message before sending.";
                return RedirectToAction("MessagingBox", new { order_id = message?.Order_id.ToString() });
            }

            message.Sender_id = Convert.ToInt32(senderId);

            ApiClient<Message> client = BuildClient<Message>("api/Buyer/SendMessage");
            bool response = false;
            try
            {
                response = await client.PostAsync(message);
            }
            catch
            {
                response = false;
            }

            if (response)
            {
                TempData["MessagingBoxMessage"] = "Message sent.";
            }
            else
            {
                TempData["MessagingBoxMessage"] = "Failed to send message. Please try again.";
            }
            return RedirectToAction("MessagingBox", new { order_id = message.Order_id.ToString() });
        }


        // ============================== Reviews ==============================

        // Uploads a new review for a gig. Validates locally before sending to the WS; returns JSON for the AJAX caller.
        [HttpPost]
        public async Task<IActionResult> UploadGigReview(Review review)
        {
            string buyerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrEmpty(buyerId))
            {
                return Json(new { success = false, message = "Please log in first." });
            }

            if (review == null)
            {
                return Json(new { success = false, message = "Invalid review data." });
            }

            review.Buyer_id = Convert.ToInt32(buyerId);
            review.Review_creation_date = DateTime.Now.ToShortDateString();

            review.Validate();
            if (review.HasErrors)
            {
                string firstError = "Invalid review.";
                foreach (KeyValuePair<string, List<string>> entry in review.AllErrors())
                {
                    if (entry.Value != null && entry.Value.Count > 0)
                    {
                        firstError = entry.Value[0];
                        break;
                    }
                }
                return Json(new { success = false, message = firstError });
            }

            ApiClient<Review> client = BuildClient<Review>("api/Buyer/UploadGigReview");
            bool response = await client.PostAsync(review);
            if (response == false)
            {
                return Json(new { success = false, message = "You can review only completed orders, and you cannot review your own gig." });
            }

            return Json(new { success = true, message = "Review uploaded successfully." });
        }


        // ============================== Become A Seller ==============================

        // Renders the "Become A Seller" form.
        [HttpGet]
        public IActionResult BecomeASellerPage()
        {
            return View("BecomeASellerPage", new Seller());
        }

        // Handles the "Become A Seller" POST. Sends the seller info (and optional avatar file) to the WS.
        [HttpPost]
        public async Task<IActionResult> BecomeASeller(Seller seller, IFormFile sellerAvatarFile)
        {
            string personId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrEmpty(personId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            seller = seller ?? new Seller();
            seller.Seller_id = personId;

            if (ModelState.IsValid == false)
            {
                string firstError = "Please fill required fields.";
                foreach (var entry in ModelState.Values)
                {
                    if (entry.Errors.Count > 0)
                    {
                        firstError = entry.Errors[0].ErrorMessage;
                        break;
                    }
                }
                ViewBag.ErrorMessage = firstError;
                return View("BecomeASellerPage", seller);
            }

            ApiClient<Seller> client = BuildClient<Seller>("api/Buyer/BecomeASeller");
            bool response = false;
            Stream avatarStream = null;
            try
            {
                if (sellerAvatarFile != null && sellerAvatarFile.Length > 0)
                {
                    avatarStream = sellerAvatarFile.OpenReadStream();
                    response = await client.PostAsync(seller, avatarStream, sellerAvatarFile.FileName);
                }
                else
                {
                    response = await client.PostAsync(seller);
                }
            }
            catch
            {
                response = false;
            }
            finally
            {
                if (avatarStream != null)
                {
                    avatarStream.Dispose();
                }
            }

            if (response)
            {
                TempData["BecomeSellerMessage"] = "You are now a seller.";
                return RedirectToAction("HomePage", "Seller");
            }

            ViewBag.ErrorMessage = "Could not become a seller. Please try again later.";
            return View("BecomeASellerPage", seller);
        }


        // ============================== Helpers ==============================

        // Builds an ApiClient<T> pointing at our WS (https://localhost:7059) at the given path. Saves repeating 4 lines per call.
        private ApiClient<T> BuildClient<T>(string path)
        {
            ApiClient<T> client = new ApiClient<T>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = path;
            return client;
        }
    }
}
