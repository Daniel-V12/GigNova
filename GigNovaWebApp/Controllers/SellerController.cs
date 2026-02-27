using Microsoft.AspNetCore.Mvc;
using System.Linq;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Http;
namespace GigNovaWebApp.Controllers
{
    public class SellerController : Controller
    {
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
                    ApiClient<SellerProfileViewModel> client = new ApiClient<SellerProfileViewModel>();
                    client.Scheme = "https";
                    client.Host = "localhost";
                    client.Port = 7059;
                    client.Path = "api/Seller/GetSellerProfileViewModel";
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

        [HttpGet]
        public IActionResult SellerHomePage()
        {
            return RedirectToAction("HomePage");
        }

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

            ApiClient<ManageGigsViewModel> client = new ApiClient<ManageGigsViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetManageGigsViewModel";
            client.AddParameter("seller_id", seller_id);
            if (page != 0)
            {
                client.AddParameter("page", page.ToString());
            }

            ManageGigsViewModel viewModel = await client.GetAsync();
            if (viewModel == null)
            {
                viewModel = new ManageGigsViewModel();
            }
            if (viewModel.Gigs == null)
            {
                viewModel.Gigs = new List<Gig>();
            }
            if (viewModel.DeliveryTimes == null)
            {
                viewModel.DeliveryTimes = new List<Delivery_time>();
            }

            int sellerIdValue = int.TryParse(seller_id, out int parsedSellerId) ? parsedSellerId : 0;
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

            Gig selectedGig = null;
            if (string.IsNullOrWhiteSpace(gig_id) == false)
            {
                selectedGig = viewModel.Gigs.FirstOrDefault(x => x != null && x.Gig_id == gig_id);
            }
            if (selectedGig == null)
            {
                selectedGig = viewModel.Gigs.FirstOrDefault(x => x != null);
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

        [HttpGet]
        public async Task<IActionResult> SellerProfile(string seller_id)
        {
            if (seller_id == null || seller_id == "")
            {
                seller_id = HttpContext.Session.GetString("person_id");
            }

            if (seller_id == null || seller_id == "")
            {
                return RedirectToAction("HomePage", "Guest");
            }

            ApiClient<SellerProfileViewModel> client = new ApiClient<SellerProfileViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetSellerProfileViewModel";
            client.AddParameter("seller_id", seller_id);

            SellerProfileViewModel viewModel = await client.GetAsync();
            if (viewModel == null)
            {
                viewModel = new SellerProfileViewModel();
            }
            if (viewModel.seller == null)
            {
                viewModel.seller = new Seller();
            }
            if (viewModel.seller_person == null)
            {
                viewModel.seller_person = new Person();
            }

            viewModel.seller.Seller_id = seller_id;
            if (viewModel.seller_person.Person_id == null || viewModel.seller_person.Person_id == "")
            {
                viewModel.seller_person.Person_id = seller_id;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SellerProfile(SellerProfileViewModel viewModel, IFormFile sellerAvatarFile)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (sellerId == null || sellerId == "")
            {
                return RedirectToAction("HomePage", "Guest");
            }

            Seller sellerToUpdate = new Seller();
            if (viewModel != null && viewModel.seller != null)
            {
                sellerToUpdate = viewModel.seller;
            }

            sellerToUpdate.Seller_id = sellerId;
            if (sellerToUpdate.Seller_display_name == null || sellerToUpdate.Seller_display_name.Trim() == "")
            {
                TempData["SellerProfileMessage"] = "Display name is required.";
                return RedirectToAction("SellerProfile", new { seller_id = sellerId });
            }

            if (sellerToUpdate.Seller_description == null)
            {
                sellerToUpdate.Seller_description = "";
            }

            List<Stream> avatarFiles = new List<Stream>();
            if (sellerAvatarFile != null && sellerAvatarFile.Length > 0)
            {
                avatarFiles.Add(sellerAvatarFile.OpenReadStream());
            }

            ApiClient<Seller> client = new ApiClient<Seller>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/BecomeASeller";

            bool response = false;
            try
            {
                response = await client.PostAsync(sellerToUpdate, avatarFiles);
            }
            catch
            {
                response = false;
            }

            foreach (Stream stream in avatarFiles)
            {
                stream.Dispose();
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

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (sellerId == null || sellerId == "")
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (currentPassword == null || currentPassword == "" || newPassword == null || newPassword == "")
            {
                TempData["SellerProfileMessage"] = "Please enter current and new password.";
                return RedirectToAction("SellerProfile", new { seller_id = sellerId });
            }

            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/ChangeSellerPassword";
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

            ApiClient<OrdersViewModel> client = new ApiClient<OrdersViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetOrdersViewModel";
            client.AddParameter("seller_id", seller_id);

            OrdersViewModel ordersViewModel = await client.GetAsync();
            if (ordersViewModel == null)
            {
                ordersViewModel = new OrdersViewModel();
            }
            if (ordersViewModel.Orders == null)
            {
                ordersViewModel.Orders = new List<Order>();
            }
            if (ordersViewModel.Buyers == null)
            {
                ordersViewModel.Buyers = new List<Buyer>();
            }

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

            notifications = notifications
                .OrderByDescending(x => DateTime.TryParse(x.Order.Order_creation_date, out DateTime d) ? d : DateTime.MinValue)
                .ThenByDescending(x => int.TryParse(x.Order.Order_id, out int id) ? id : 0)
                .ToList();

            Dictionary<string, string> gigNamesByOrderId = new Dictionary<string, string>();
            foreach (SelectedOrderViewModel notification in notifications)
            {
                string orderId = notification.Order.Order_id;
                if (string.IsNullOrWhiteSpace(orderId))
                {
                    continue;
                }

                ApiClient<CustomizeOrderViewModel> orderClient = new ApiClient<CustomizeOrderViewModel>();
                orderClient.Scheme = "https";
                orderClient.Host = "localhost";
                orderClient.Port = 7059;
                orderClient.Path = "api/Buyer/GetCustomizeOrderViewModel";
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

        [HttpPost]
        public async Task<IActionResult> AddGig(Gig gig, IFormFile gigPhotoFile)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (gig == null)
            {
                gig = new Gig();
            }

            gig.Seller_id = int.TryParse(sellerId, out int sellerIdValue) ? sellerIdValue : 0;
            if (string.IsNullOrWhiteSpace(gig.Gig_name) || string.IsNullOrWhiteSpace(gig.Gig_description) || gig.Gig_price <= 0 || gig.Delivery_time_id <= 0)
            {
                TempData["ManageGigMessage"] = "Please fill title, description, delivery time and price.";
                return RedirectToAction("ManageGigs", new { seller_id = sellerId });
            }

            if (gig.Language_id <= 0)
            {
                gig.Language_id = 1;
            }

            ApiClient<Gig> client = new ApiClient<Gig>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/AddGig";

            bool response = false;
            Stream photoStream = null;
            try
            {
                if (gigPhotoFile != null && gigPhotoFile.Length > 0)
                {
                    photoStream = gigPhotoFile.OpenReadStream();
                    response = await client.PostAsync(gig, photoStream);
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

            TempData["ManageGigMessage"] = response ? "Gig created successfully." : "Failed to create gig.";
            return RedirectToAction("ManageGigs", new { seller_id = sellerId });
        }

        [HttpPost]
        public async Task<IActionResult> EditGig(Gig gig, IFormFile gigPhotoFile)
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

            gig.Seller_id = int.TryParse(sellerId, out int sellerIdValue) ? sellerIdValue : 0;
            if (string.IsNullOrWhiteSpace(gig.Gig_name) || string.IsNullOrWhiteSpace(gig.Gig_description) || gig.Gig_price <= 0 || gig.Delivery_time_id <= 0)
            {
                TempData["ManageGigMessage"] = "Please fill title, description, delivery time and price.";
                return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig.Gig_id });
            }

            if (gig.Language_id <= 0)
            {
                gig.Language_id = 1;
            }

            ApiClient<Gig> client = new ApiClient<Gig>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/EditGig";

            bool response = false;
            Stream photoStream = null;
            try
            {
                if (gigPhotoFile != null && gigPhotoFile.Length > 0)
                {
                    photoStream = gigPhotoFile.OpenReadStream();
                    response = await client.PostAsync(gig, photoStream);
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

            TempData["ManageGigMessage"] = response ? "Gig updated successfully." : "Failed to update gig.";
            return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig.Gig_id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGig(string seller_id, string gig_id)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                sellerId = seller_id;
            }

            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/DeleteGig";
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("gig_id", gig_id);

            bool response = await client.PostAsyncReturn<string, bool>("");
            TempData["ManageGigMessage"] = response ? "Gig deleted successfully." : "Failed to delete gig.";
            return RedirectToAction("ManageGigs", new { seller_id = sellerId });
        }

        [HttpPost]
        public async Task<IActionResult> PublishGig(string seller_id, string gig_id)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                sellerId = seller_id;
            }

            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/PublishGig";
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("gig_id", gig_id);

            bool response = await client.PostAsyncReturn<string, bool>("");
            TempData["ManageGigMessage"] = response ? "Gig published to catalog." : "Failed to publish gig.";
            return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig_id });
        }

        [HttpPost]
        public async Task<IActionResult> UnpublishGig(string seller_id, string gig_id)
        {
            string sellerId = HttpContext.Session.GetString("person_id");
            if (string.IsNullOrWhiteSpace(sellerId))
            {
                sellerId = seller_id;
            }

            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/UnpublishGig";
            client.AddParameter("seller_id", sellerId);
            client.AddParameter("gig_id", gig_id);

            bool response = await client.PostAsyncReturn<string, bool>("");
            TempData["ManageGigMessage"] = response ? "Gig unpublished from catalog." : "Failed to unpublish gig.";
            return RedirectToAction("ManageGigs", new { seller_id = sellerId, gig_id = gig_id });
        }

        [HttpGet]
        public async Task<IActionResult> SelectGig(string gig_id, string seller_id)
        {
            ApiClient<Gig> client = new ApiClient<Gig>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/SelectGig";
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            Gig gig = await client.GetAsync();
            return View(gig);
        }

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

            void ConfigureLocalApi<T>(ApiClient<T> apiClient, string path)
            {
                apiClient.Scheme = "https";
                apiClient.Host = "localhost";
                apiClient.Port = 7059;
                apiClient.Path = path;
            }

            ApiClient<SelectedOrderViewModel> client = new ApiClient<SelectedOrderViewModel>();
            ConfigureLocalApi(client, "api/Seller/SelectOrder");
            client.AddParameter("order_id", order_id);
            client.AddParameter("seller_id", seller_id);
            SelectedOrderViewModel viewModel = await client.GetAsync();

            ApiClient<CustomizeOrderViewModel> orderClient = new ApiClient<CustomizeOrderViewModel>();
            ConfigureLocalApi(orderClient, "api/Buyer/GetCustomizeOrderViewModel");
            orderClient.AddParameter("order_id", order_id);
            CustomizeOrderViewModel orderDetails = await orderClient.GetAsync();

            ApiClient<List<Delivery>> deliveriesClient = new ApiClient<List<Delivery>>();
            ConfigureLocalApi(deliveriesClient, "api/Seller/GetDeliveriesByOrder");
            deliveriesClient.AddParameter("order_id", order_id);
            List<Delivery> deliveries = await deliveriesClient.GetAsync();
            if (deliveries == null)
            {
                deliveries = new List<Delivery>();
            }

            ViewData["OrderDetails"] = orderDetails;
            ViewData["SellerId"] = seller_id;
            ViewData["Deliveries"] = deliveries;
            return View("~/Views/Seller/SelectedIncomingOrder.cshtml", viewModel);
        }

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

            Delivery delivery = new Delivery();
            delivery.Order_id = order_id;
            delivery.Delivery_text = delivery_text ?? "";
            delivery.Delivery_file = "";

            List<Stream> fileStreams = new List<Stream>();
            if (deliveryFiles != null)
            {
                foreach (IFormFile file in deliveryFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        fileStreams.Add(file.OpenReadStream());
                    }
                }
            }

            ApiClient<Delivery> client = new ApiClient<Delivery>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/DeliverGig";

            bool response = false;
            try
            {
                response = await client.PostAsync(delivery, fileStreams);
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

        [HttpGet]
        public async Task<IActionResult> DeliveryDetails(string order_id, int delivery_index = 0)
        {
            if (string.IsNullOrWhiteSpace(order_id))
            {
                return RedirectToAction("ViewOrders", "Buyer");
            }

            CustomizeOrderViewModel orderDetails = null;

            ApiClient<CustomizeOrderViewModel> orderClient = new ApiClient<CustomizeOrderViewModel>();
            orderClient.Scheme = "https";
            orderClient.Host = "localhost";
            orderClient.Port = 7059;
            orderClient.Path = "api/Buyer/GetCustomizeOrderViewModel";
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

            ApiClient<List<Delivery>> deliveriesClient = new ApiClient<List<Delivery>>();
            deliveriesClient.Scheme = "https";
            deliveriesClient.Host = "localhost";
            deliveriesClient.Port = 7059;
            deliveriesClient.Path = "api/Seller/GetDeliveriesByOrder";
            deliveriesClient.AddParameter("order_id", order_id);

            List<Delivery> deliveries = await deliveriesClient.GetAsync();
            if (deliveries == null)
            {
                deliveries = new List<Delivery>();
            }

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

            if (delivery_index < 0)
            {
                delivery_index = 0;
            }
            if (delivery_index >= deliveries.Count)
            {
                delivery_index = deliveries.Count - 1;
            }

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
    }
}