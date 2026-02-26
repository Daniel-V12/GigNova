using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> ManageGigs(string seller_id, int page = 0)
        {
            ApiClient<ManageGigsViewModel> client = new ApiClient<ManageGigsViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetManageGigsViewModel";
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            if (page != 0)
            {
                client.AddParameter("page", page.ToString());
            }
            ManageGigsViewModel viewModel = await client.GetAsync();
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
        public IActionResult AddGig(Gig gig)
        {
            return View(gig);
        }

        [HttpPost]
        public IActionResult EditGig(Gig gig)
        {
            return View(gig);
        }

        [HttpPost]
        public IActionResult DeleteGig(string seller_id, string gig_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult PublishGig(string seller_id, string gig_id)
        {
            return View();
        }

        [HttpPost]
        public IActionResult UnpublishGig(string seller_id, string gig_id)
        {
            return View();
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
    }
}
