using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Mvc;

namespace GigNovaWebApp.Controllers
{
    public class SellerController : Controller
    {
        // ============================== Home & Profile ==============================

        // Seller landing page. Tries to fetch the display name from the WS to show "Welcome, X".
        [HttpGet]
        public async Task<IActionResult> HomePage()
        {
            ViewData["HomeActor"] = "seller";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterSellerPage.cshtml";

            string sellerId = HttpContext.Session.GetString("person_id");
            if (sellerId != null)
            {
                try
                {
                    ApiClient<SellerProfileViewModel> client = BuildClient<SellerProfileViewModel>("api/Seller/GetSellerProfileViewModel");
                    client.AddParameter("seller_id", sellerId);

                    SellerProfileViewModel viewModel = await client.GetAsync();
                    if (viewModel != null && viewModel.seller != null && viewModel.seller.Seller_display_name != null)
                    {
                        ViewData["SellerDisplayName"] = viewModel.seller.Seller_display_name;
                    }
                }
                catch
                {
                }
            }

            return View("~/Views/Shared/HomePage.cshtml");
        }

        // Shows the seller's profile page. Falls back to the session id if no id is in the URL.
        [HttpGet]
        public async Task<IActionResult> SellerProfile(string seller_id)
        {
            if (string.IsNullOrEmpty(seller_id))
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrEmpty(seller_id))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<SellerProfileViewModel> client = BuildClient<SellerProfileViewModel>("api/Seller/GetSellerProfileViewModel");
            client.AddParameter("seller_id", seller_id);

            SellerProfileViewModel viewModel = await client.GetAsync();

            // Make sure none of the nested objects are null (the view assumes they exist).
            viewModel = viewModel ?? new SellerProfileViewModel();
            viewModel.seller = viewModel.seller ?? new Seller();
            viewModel.seller_person = viewModel.seller_person ?? new Person();

            viewModel.seller.Seller_id = seller_id;
            if (string.IsNullOrEmpty(viewModel.seller_person.Person_id))
            {
                viewModel.seller_person.Person_id = seller_id;
            }

            return View(viewModel);
        }

        // Handles the seller profile update POST. Reuses the BecomeASeller WS endpoint (which handles create + update + avatar).
        [HttpPost]
        public async Task<IActionResult> SellerProfile(SellerProfileViewModel viewModel, IFormFile sellerAvatarFile)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrEmpty(sellerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            Seller sellerToUpdate = new Seller();
            if (viewModel != null && viewModel.seller != null)
            {
                sellerToUpdate = viewModel.seller;
            }
            sellerToUpdate.Seller_id = sellerId;

            sellerToUpdate.Validate();
            if (sellerToUpdate.HasErrors)
            {
                string firstError = "Please fix the highlighted fields.";
                foreach (KeyValuePair<string, List<string>> entry in sellerToUpdate.AllErrors())
                {
                    if (entry.Value != null && entry.Value.Count > 0)
                    {
                        firstError = entry.Value[0];
                        break;
                    }
                }
                TempData["SellerProfileMessage"] = firstError;
                return RedirectToAction("SellerProfile", new { seller_id = sellerId });
            }

            ApiClient<Seller> client = BuildClient<Seller>("api/Buyer/BecomeASeller");

            // Post the seller info + optional avatar file. Always dispose the stream in finally.
            bool response = false;
            Stream avatarStream = null;
            try
            {
                if (sellerAvatarFile != null && sellerAvatarFile.Length > 0)
                {
                    avatarStream = sellerAvatarFile.OpenReadStream();
                    response = await client.PostAsync(sellerToUpdate, avatarStream, sellerAvatarFile.FileName);
                }
                else
                {
                    response = await client.PostAsync(sellerToUpdate);
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
                TempData["SellerProfileMessage"] = "Profile updated successfully.";
            }
            else
            {
                TempData["SellerProfileMessage"] = "Failed to update profile.";
            }

            return RedirectToAction("SellerProfile", new { seller_id = sellerId });
        }

        // Sends current + new password to the WS. The WS verifies the current password before updating.
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrEmpty(sellerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                TempData["SellerProfileMessage"] = "Please enter current and new password.";
                return RedirectToAction("SellerProfile", new { seller_id = sellerId });
            }

            ApiClient<string> client = BuildClient<string>("api/Seller/ChangeSellerPassword");
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("current_password", currentPassword);
            client.AddParameter("new_password", newPassword);

            bool response = await client.PostAsyncReturn<string, bool>("");
            if (response)
            {
                TempData["SellerProfileMessage"] = "Password changed successfully.";
            }
            else
            {
                TempData["SellerProfileMessage"] = "Failed to change password. Make sure current password is correct.";
            }

            return RedirectToAction("SellerProfile", new { seller_id = sellerId });
        }


        // ============================== Gigs Management ==============================

        // Renders the manage-gigs page: list of seller's gigs + the currently selected gig (or a draft if creating new).
        [HttpGet]
        public async Task<IActionResult> ManageGigs(string seller_id, string gig_id = null, bool create_new = false, int page = 0)
        {
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<ManageGigsViewModel> client = BuildClient<ManageGigsViewModel>("api/Seller/GetManageGigsViewModel");
            client.AddParameter("seller_id", seller_id);
            if (page != 0)
            {
                client.AddParameter("page", page.ToString());
            }

            ManageGigsViewModel viewModel = await client.GetAsync();
            viewModel = viewModel ?? new ManageGigsViewModel();
            viewModel.Gigs = viewModel.Gigs ?? new List<Gig>();
            viewModel.DeliveryTimes = viewModel.DeliveryTimes ?? new List<Delivery_time>();

            int sellerIdValue;
            int.TryParse(seller_id, out sellerIdValue);

            // If "create new" was requested, insert a draft gig at the top of the list.
            if (create_new || gig_id == "__new")
            {
                Gig draftGig = new Gig();
                draftGig.Gig_id = "__new";
                draftGig.Seller_id = sellerIdValue;
                draftGig.Gig_name = "New Gig Draft";
                draftGig.Gig_description = "Example: I will create a clean, professional result based on your brief. Tell me your goals, style, and deadline.";
                draftGig.Gig_price = 50;
                draftGig.Delivery_time_id = 1;
                draftGig.Is_publish = false;
                viewModel.Gigs.Insert(0, draftGig);
                gig_id = "__new";
            }

            // Find the gig that matches gig_id, or the first gig in the list, or a fresh empty Gig.
            Gig selectedGig = null;
            if (string.IsNullOrWhiteSpace(gig_id) == false)
            {
                foreach (Gig g in viewModel.Gigs)
                {
                    if (g != null && g.Gig_id == gig_id)
                    {
                        selectedGig = g;
                        break;
                    }
                }
            }
            if (selectedGig == null)
            {
                foreach (Gig g in viewModel.Gigs)
                {
                    if (g != null)
                    {
                        selectedGig = g;
                        break;
                    }
                }
            }
            if (selectedGig == null)
            {
                selectedGig = new Gig();
                selectedGig.Seller_id = sellerIdValue;
            }

            viewModel.SelectedGig = selectedGig;
            ViewData["SellerId"] = seller_id;
            return View(viewModel);
        }

        // Handles the "add new gig" POST. Sends the gig + required photo file to the WS.
        [HttpPost]
        public async Task<IActionResult> AddGig(Gig gig, IFormFile gigPhotoFile, List<string> Category_ids)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            gig = gig ?? new Gig();

            int sellerIdValue;
            int.TryParse(sellerId, out sellerIdValue);
            gig.Seller_id = sellerIdValue;

            // Language selection isn't implemented in the form yet, so always default to 1.
            gig.Language_id = 1;

            // These fields are auto-set on the server; don't let an empty form invalidate the model.
            ModelState.Remove("Gig_id");
            ModelState.Remove("Gig_date");
            ModelState.Remove("Gig_photo");
            ModelState.Remove("Category_id");
            ModelState.Remove("gigPhotoFile");

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
                TempData["ManageGigMessage"] = firstError;
                return RedirectToAction("ManageGigs", new { seller_id = sellerId });
            }

            if (gigPhotoFile == null || gigPhotoFile.Length == 0)
            {
                TempData["ManageGigMessage"] = "Please upload a gig photo.";
                return RedirectToAction("ManageGigs", new { seller_id = sellerId });
            }

            // De-duplicate the posted category ids.
            gig.Category_ids = new List<string>();
            if (Category_ids != null)
            {
                foreach (string categoryId in Category_ids)
                {
                    if (string.IsNullOrWhiteSpace(categoryId) == false && gig.Category_ids.Contains(categoryId) == false)
                    {
                        gig.Category_ids.Add(categoryId);
                    }
                }
            }

            // A gig can have at most 4 categories.
            if (gig.Category_ids.Count > 4)
            {
                TempData["ManageGigMessage"] = "You can select up to 4 categories only.";
                return RedirectToAction("ManageGigs", new { seller_id = sellerId });
            }

            ApiClient<Gig> client = BuildClient<Gig>("api/Seller/AddGig");

            bool response = false;
            Stream photoStream = null;
            try
            {
                photoStream = gigPhotoFile.OpenReadStream();
                response = await client.PostAsync(gig, photoStream, gigPhotoFile.FileName);
            }
            catch
            {
                response = false;
            }
            finally
            {
                if (photoStream != null)
                {
                    photoStream.Dispose();
                }
            }

            if (response)
            {
                TempData["ManageGigMessage"] = "Gig created successfully.";
            }
            else
            {
                TempData["ManageGigMessage"] = "Failed to create gig.";
            }
            return RedirectToAction("ManageGigs", new { seller_id = sellerId });
        }

        // Handles the "edit existing gig" POST. Photo upload is optional - if no new photo is sent, the WS keeps the existing one.
        [HttpPost]
        public async Task<IActionResult> EditGig(Gig gig, IFormFile gigPhotoFile, List<string> Category_ids)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (gig == null || string.IsNullOrWhiteSpace(gig.Gig_id))
            {
                TempData["ManageGigMessage"] = "Please select a gig to edit.";
                return RedirectToAction("ManageGigs", new { seller_id = sellerId });
            }

            int sellerIdValue;
            int.TryParse(sellerId, out sellerIdValue);
            gig.Seller_id = sellerIdValue;

            if (gig.Language_id <= 0)
            {
                gig.Language_id = 1;
            }

            ModelState.Remove("Gig_date");
            ModelState.Remove("Category_id");
            ModelState.Remove("gigPhotoFile");

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
                TempData["ManageGigMessage"] = firstError;
                return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig.Gig_id });
            }

            // De-duplicate the posted category ids.
            gig.Category_ids = new List<string>();
            if (Category_ids != null)
            {
                foreach (string categoryId in Category_ids)
                {
                    if (string.IsNullOrWhiteSpace(categoryId) == false && gig.Category_ids.Contains(categoryId) == false)
                    {
                        gig.Category_ids.Add(categoryId);
                    }
                }
            }

            // A gig can have at most 4 categories.
            if (gig.Category_ids.Count > 4)
            {
                TempData["ManageGigMessage"] = "You can select up to 4 categories only.";
                return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig.Gig_id });
            }

            ApiClient<Gig> client = BuildClient<Gig>("api/Seller/EditGig");

            bool response = false;
            Stream photoStream = null;
            try
            {
                if (gigPhotoFile != null && gigPhotoFile.Length > 0)
                {
                    photoStream = gigPhotoFile.OpenReadStream();
                    response = await client.PostAsync(gig, photoStream, gigPhotoFile.FileName);
                }
                else
                {
                    response = await client.PostAsync(gig);
                }
            }
            catch
            {
                response = false;
            }
            finally
            {
                if (photoStream != null)
                {
                    photoStream.Dispose();
                }
            }

            if (response)
            {
                TempData["ManageGigMessage"] = "Gig updated successfully.";
            }
            else
            {
                TempData["ManageGigMessage"] = "Failed to update gig.";
            }
            return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig.Gig_id });
        }

        // Deletes a gig via the WS. The WS refuses if the gig already has orders.
        [HttpPost]
        public async Task<IActionResult> DeleteGig(string seller_id, string gig_id)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                sellerId = seller_id;
            }

            ApiClient<string> client = BuildClient<string>("api/Seller/DeleteGig");
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("gig_id", gig_id);

            string response = await client.PostAsyncReturn<string, string>("");
            if (string.IsNullOrWhiteSpace(response))
            {
                TempData["ManageGigMessage"] = "Gig deleted successfully.";
            }
            else
            {
                TempData["ManageGigMessage"] = response;
            }
            return RedirectToAction("ManageGigs", new { seller_id = sellerId });
        }

        // Publishes a gig (makes it visible in the buyer catalog). The WS sends back an error message if validation fails.
        [HttpPost]
        public async Task<IActionResult> PublishGig(string seller_id, string gig_id)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                sellerId = seller_id;
            }

            ApiClient<string> client = BuildClient<string>("api/Seller/PublishGig");
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("gig_id", gig_id);

            string response = await client.PostAsyncReturn<string, string>("");
            if (string.IsNullOrWhiteSpace(response))
            {
                TempData["ManageGigMessage"] = "Gig published to catalog.";
            }
            else
            {
                TempData["ManageGigMessage"] = "Cannot publish: " + response;
            }
            return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig_id });
        }

        // Unpublishes a gig (hides it from the buyer catalog).
        [HttpPost]
        public async Task<IActionResult> UnpublishGig(string seller_id, string gig_id)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                sellerId = seller_id;
            }

            ApiClient<string> client = BuildClient<string>("api/Seller/UnpublishGig");
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("gig_id", gig_id);

            bool response = await client.PostAsyncReturn<string, bool>("");
            if (response)
            {
                TempData["ManageGigMessage"] = "Gig unpublished from catalog.";
            }
            else
            {
                TempData["ManageGigMessage"] = "Failed to unpublish gig.";
            }
            return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig_id });
        }


        // ============================== Incoming Orders & Delivery ==============================

        // Shows the seller's incoming orders: pairs each order with its buyer, sorts non-completed first newest first, and fetches each gig's name.
        [HttpGet]
        public async Task<IActionResult> IncomingOrders(string seller_id)
        {
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<OrdersViewModel> client = BuildClient<OrdersViewModel>("api/Seller/GetOrdersViewModel");
            client.AddParameter("seller_id", seller_id);

            OrdersViewModel ordersViewModel = await client.GetAsync();
            ordersViewModel = ordersViewModel ?? new OrdersViewModel();
            ordersViewModel.Orders = ordersViewModel.Orders ?? new List<Order>();
            ordersViewModel.Buyers = ordersViewModel.Buyers ?? new List<Buyer>();

            // Pair each order with the buyer at the same index.
            List<SelectedOrderViewModel> notifications = new List<SelectedOrderViewModel>();
            for (int i = 0; i < ordersViewModel.Orders.Count; i++)
            {
                Order order = ordersViewModel.Orders[i];
                if (order == null)
                {
                    continue;
                }

                Buyer buyer = null;
                if (i < ordersViewModel.Buyers.Count)
                {
                    buyer = ordersViewModel.Buyers[i];
                }

                notifications.Add(new SelectedOrderViewModel
                {
                    Order = order,
                    Buyer = buyer
                });
            }

            // Step 1: split into "non-completed" and "completed" groups.
            List<SelectedOrderViewModel> nonCompleted = new List<SelectedOrderViewModel>();
            List<SelectedOrderViewModel> completed = new List<SelectedOrderViewModel>();
            foreach (SelectedOrderViewModel n in notifications)
            {
                if (n.Order.Order_status_id == 3)
                {
                    completed.Add(n);
                }
                else
                {
                    nonCompleted.Add(n);
                }
            }

            // Step 2: sort each group so the newest order id comes first.
            SortNotificationsByOrderIdDescending(nonCompleted);
            SortNotificationsByOrderIdDescending(completed);

            // Step 3: combine (non-completed first, completed last).
            notifications = new List<SelectedOrderViewModel>();
            foreach (SelectedOrderViewModel n in nonCompleted)
            {
                notifications.Add(n);
            }
            foreach (SelectedOrderViewModel n in completed)
            {
                notifications.Add(n);
            }

            // Look up each order's gig name so the view can show it.
            Dictionary<string, string> gigNamesByOrderId = new Dictionary<string, string>();
            foreach (SelectedOrderViewModel notification in notifications)
            {
                string orderId = notification.Order.Order_id;
                if (string.IsNullOrWhiteSpace(orderId))
                {
                    continue;
                }

                ApiClient<CustomizeOrderViewModel> orderClient = BuildClient<CustomizeOrderViewModel>("api/Buyer/GetCustomizeOrderViewModel");
                orderClient.AddParameter("order_id", orderId);

                CustomizeOrderViewModel details = await orderClient.GetAsync();
                string gigName = "Gig";
                if (details != null && details.gig != null && string.IsNullOrWhiteSpace(details.gig.Gig_name) == false)
                {
                    gigName = details.gig.Gig_name;
                }
                gigNamesByOrderId[orderId] = gigName;
            }

            ViewData["SellerId"] = seller_id;
            ViewData["GigNamesByOrderId"] = gigNamesByOrderId;
            return View("~/Views/Seller/IncomingOrders.cshtml", notifications);
        }

        // Bubble sort: arranges the list so the highest numeric Order_id comes first.
        private void SortNotificationsByOrderIdDescending(List<SelectedOrderViewModel> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = 0; j < list.Count - 1 - i; j++)
                {
                    int idA = Convert.ToInt32(list[j].Order.Order_id);
                    int idB = Convert.ToInt32(list[j + 1].Order.Order_id);
                    if (idA < idB)
                    {
                        SelectedOrderViewModel temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }
        }

        // Shows the detail page for one incoming order (with buyer, gig details, and any deliveries the seller already sent).
        [HttpGet]
        public async Task<IActionResult> SelectedIncomingOrder(string order_id, string seller_id)
        {
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrWhiteSpace(order_id) || string.IsNullOrWhiteSpace(seller_id))
            {
                return RedirectToAction("IncomingOrders", new { seller_id = seller_id });
            }

            ApiClient<SelectedOrderViewModel> client = BuildClient<SelectedOrderViewModel>("api/Seller/SelectOrder");
            client.AddParameter("order_id", order_id);
            client.AddParameter("seller_id", seller_id);
            SelectedOrderViewModel viewModel = await client.GetAsync();

            ApiClient<CustomizeOrderViewModel> orderClient = BuildClient<CustomizeOrderViewModel>("api/Buyer/GetCustomizeOrderViewModel");
            orderClient.AddParameter("order_id", order_id);
            CustomizeOrderViewModel orderDetails = await orderClient.GetAsync();

            ApiClient<List<Delivery>> deliveriesClient = BuildClient<List<Delivery>>("api/Seller/GetDeliveriesByOrder");
            deliveriesClient.AddParameter("order_id", order_id);
            List<Delivery> deliveries = await deliveriesClient.GetAsync();
            deliveries = deliveries ?? new List<Delivery>();

            ViewData["OrderDetails"] = orderDetails;
            ViewData["SellerId"] = seller_id;
            ViewData["Deliveries"] = deliveries;
            return View("~/Views/Seller/SelectedIncomingOrder.cshtml", viewModel);
        }

        // Handles the "deliver gig" POST. Validates the delivery file then sends it to the WS.
        [HttpPost]
        public async Task<IActionResult> DeliverGig(string order_id, string seller_id, string delivery_text, List<IFormFile> deliveryFiles)
        {
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }
            if (string.IsNullOrWhiteSpace(order_id) || string.IsNullOrWhiteSpace(seller_id))
            {
                return RedirectToAction("IncomingOrders", new { seller_id = seller_id });
            }

            // Delivery file rules: exactly 1 file, max 100MB, only .rar or .zip.
            if (deliveryFiles == null || deliveryFiles.Count != 1)
            {
                TempData["SellerIncomingOrderMessage"] = "Please upload exactly one delivery file (.rar or .zip).";
                return RedirectToAction("SelectedIncomingOrder", new { order_id = order_id, seller_id = seller_id });
            }
            IFormFile singleDeliveryFile = deliveryFiles[0];
            if (singleDeliveryFile == null || singleDeliveryFile.Length == 0)
            {
                TempData["SellerIncomingOrderMessage"] = "Please upload a delivery file (.rar or .zip).";
                return RedirectToAction("SelectedIncomingOrder", new { order_id = order_id, seller_id = seller_id });
            }
            if (singleDeliveryFile.Length > 100 * 1024 * 1024)
            {
                TempData["SellerIncomingOrderMessage"] = "The delivery file must be 100MB or smaller.";
                return RedirectToAction("SelectedIncomingOrder", new { order_id = order_id, seller_id = seller_id });
            }
            string deliveryExt = Path.GetExtension(singleDeliveryFile.FileName).ToLower();
            if (deliveryExt != ".rar" && deliveryExt != ".zip")
            {
                TempData["SellerIncomingOrderMessage"] = "Delivery file must be .rar or .zip only.";
                return RedirectToAction("SelectedIncomingOrder", new { order_id = order_id, seller_id = seller_id });
            }

            Delivery delivery = new Delivery();
            delivery.Order_id = order_id;
            delivery.Delivery_text = delivery_text ?? "";
            delivery.Delivery_file = "";

            List<Stream> fileStreams = new List<Stream>();
            List<string> fileNames = new List<string>();
            fileStreams.Add(singleDeliveryFile.OpenReadStream());
            fileNames.Add(singleDeliveryFile.FileName);

            ApiClient<Delivery> client = BuildClient<Delivery>("api/Seller/DeliverGig");

            bool response = false;
            try
            {
                response = await client.PostAsync(delivery, fileStreams, fileNames);
            }
            catch
            {
                response = false;
            }

            foreach (Stream stream in fileStreams)
            {
                stream.Dispose();
            }

            if (response)
            {
                TempData["SellerIncomingOrderMessage"] = "Delivery sent successfully.";
            }
            else
            {
                TempData["SellerIncomingOrderMessage"] = "Failed to send delivery.";
            }
            return RedirectToAction("SelectedIncomingOrder", new { order_id = order_id, seller_id = seller_id });
        }

        // Shows the delivery files (and seller-side message) attached to an order. Same view as the buyer's, with Actor = "seller".
        [HttpGet]
        public async Task<IActionResult> DeliveryDetails(string order_id, int delivery_index = 0)
        {
            if (string.IsNullOrWhiteSpace(order_id))
            {
                return RedirectToAction("ViewOrders", "Buyer");
            }

            // Load order details (for the gig info).
            CustomizeOrderViewModel orderDetails = null;
            ApiClient<CustomizeOrderViewModel> orderClient = BuildClient<CustomizeOrderViewModel>("api/Buyer/GetCustomizeOrderViewModel");
            orderClient.AddParameter("order_id", order_id);
            try
            {
                orderDetails = await orderClient.GetAsync();
            }
            catch
            {
                orderDetails = null;
            }
            if (orderDetails == null)
            {
                orderDetails = new CustomizeOrderViewModel();
                orderDetails.order = new Order();
                orderDetails.order.Order_id = order_id;
            }

            // Load the deliveries for this order.
            ApiClient<List<Delivery>> deliveriesClient = BuildClient<List<Delivery>>("api/Seller/GetDeliveriesByOrder");
            deliveriesClient.AddParameter("order_id", order_id);
            List<Delivery> deliveries = await deliveriesClient.GetAsync();
            deliveries = deliveries ?? new List<Delivery>();

            if (deliveries.Count == 0)
            {
                ViewData["Actor"] = "seller";
                ViewData["OrderId"] = order_id;
                ViewData["DeliveryCount"] = 0;
                ViewData["DeliveryIndex"] = 0;
                ViewData["SelectedDelivery"] = null;
                ViewData["DeliveryFiles"] = new List<string>();
                ViewData["Gig"] = orderDetails.gig;
                ViewData["DeliveryDetailsMessage"] = "No deliveries found for this order yet.";
                return View("~/Views/Buyer/DeliveryDetails.cshtml", orderDetails);
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

            ViewData["Actor"] = "seller";
            ViewData["OrderId"] = order_id;
            ViewData["DeliveryCount"] = deliveries.Count;
            ViewData["DeliveryIndex"] = delivery_index;
            ViewData["SelectedDelivery"] = selectedDelivery;
            ViewData["DeliveryFiles"] = files;
            ViewData["Gig"] = orderDetails.gig;
            return View("~/Views/Buyer/DeliveryDetails.cshtml", orderDetails);
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
