using Microsoft.AspNetCore.Mvc;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Http;

namespace GigNovaWebApp.Controllers
{
    public class BuyerController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> HomePage()
        {
            ViewData["HomeActor"] = "buyer";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterBuyerPage.cshtml";

            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId != null)
            {
                ApiClient<BuyerProfileViewmodel> client = new ApiClient<BuyerProfileViewmodel>();
                client.Scheme = "https";
                client.Host = "localhost";
                client.Port = 7059;
                client.Path = "api/Buyer/GetBuyerProfileViewModel";
                client.AddParameter("buyer_id", buyerId);

                BuyerProfileViewmodel viewModel = await client.GetAsync();
                if (viewModel != null && viewModel.buyer != null)
                {
                    if (viewModel.buyer.Buyer_display_name != null)
                    {
                        ViewData["BuyerDisplayName"] = viewModel.buyer.Buyer_display_name;
                    }
                }
            }

            return View("~/Views/Shared/HomePage.cshtml");
        }

        [HttpGet]
        public IActionResult BuyerHomePage()
        {
            return RedirectToAction("HomePage");
        }

        [HttpGet]
        public async Task<IActionResult> OrderedGigs(string buyerId)
        {
            ApiClient<List<Order>> client = new ApiClient<List<Order>>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/GetOrderedGigsViewModel";
            if (buyerId != null)
            {
                client.AddParameter("buyerId", buyerId);
            }
            List<Order> orders = await client.GetAsync();
            return View(orders);
        }

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

            ApiClient<BuyerProfileViewmodel> client = new ApiClient<BuyerProfileViewmodel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/GetBuyerProfileViewModel";
            client.AddParameter("buyer_id", buyer_id);

            BuyerProfileViewmodel viewModel = await client.GetAsync();
            if (viewModel == null)
            {
                viewModel = new BuyerProfileViewmodel();
            }
            if (viewModel.buyer == null)
            {
                viewModel.buyer = new Buyer();
            }
            if (viewModel.buyer_person == null)
            {
                viewModel.buyer_person = new Person();
            }

            if (viewModel.buyer.Person_join_date == null || viewModel.buyer.Person_join_date == "")
            {
                viewModel.buyer.Person_join_date = viewModel.buyer_person.Person_join_date;
            }

            return View(viewModel);
        }

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
            if (viewModel.Buyer_description == null)
            {
                viewModel.Buyer_description = "";
            }

            ApiClient<BuyerProfileUpdateViewModel> client = new ApiClient<BuyerProfileUpdateViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/UpdateBuyerProfile";

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

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId == null)
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (currentPassword == null || currentPassword == "" || newPassword == null || newPassword == "")
            {
                TempData["BuyerProfileMessage"] = "Please enter current and new password.";
                return RedirectToAction("BuyerProfile", new { buyer_id = buyerId });
            }

            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/ChangeBuyerPassword";
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

        [HttpPost]
        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("person_id");
            return RedirectToAction("HomePage", "Guest");
        }

        [HttpPost]
        public IActionResult PlaceOrder(Order order)
        {
            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> CustomizeOrder(string order_id = null, string gig_id = null)
        {
            ApiClient<CustomizeOrderViewModel> client = new ApiClient<CustomizeOrderViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/GetCustomizeOrderViewModel";
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

        [HttpPost]
        public async Task<IActionResult> CustomizeOrder(CustomizeOrderViewModel viewModel, List<IFormFile> orderFiles)
        {
            string buyerId = HttpContext.Session.GetString("person_id");
            if (buyerId == null)
            {
                return RedirectToAction("HomePage", "Guest");
            }

            if (viewModel == null || viewModel.order == null)
            {
                return CustomizeOrderFailed("HomePage", null);
            }

            viewModel.order.Buyer_id = Convert.ToInt32(buyerId);
            viewModel.order.Is_payment = true;
            if (viewModel.order.Order_requirements == null)
            {
                viewModel.order.Order_requirements = "";
            }

            List<Stream> filesToSend = BuildFileStreams(orderFiles);
            bool response = await PostCustomizeOrder(viewModel, filesToSend);
            DisposeStreams(filesToSend);

            if (response == false)
            {
                return CustomizeOrderFailed("CustomizeOrder", viewModel.order.Gig_id);
            }

            return RedirectToAction("HomePage");
        }

        private IActionResult CustomizeOrderFailed(string action, int? gigId)
        {
            TempData["CustomizeOrderMessage"] = "Failed to create order.";
            if (action == "CustomizeOrder" && gigId.HasValue)
            {
                return RedirectToAction("CustomizeOrder", new { gig_id = gigId.Value });
            }
            return RedirectToAction(action);
        }

        private List<Stream> BuildFileStreams(List<IFormFile> orderFiles)
        {
            List<Stream> filesToSend = new List<Stream>();
            if (orderFiles != null)
            {
                foreach (IFormFile file in orderFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        filesToSend.Add(file.OpenReadStream());
                    }
                }
            }
            return filesToSend;
        }

        private void DisposeStreams(List<Stream> filesToSend)
        {
            foreach (Stream thisStream in filesToSend)
            {
                thisStream.Dispose();
            }
        }

        private async Task<bool> PostCustomizeOrder(CustomizeOrderViewModel viewModel, List<Stream> filesToSend)
        {
            ApiClient<CustomizeOrderViewModel> client = new ApiClient<CustomizeOrderViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/CreateOrderAndPayWithFiles";
            return await client.PostAsync(viewModel, filesToSend);
        }

        [HttpGet]
        public async Task<IActionResult> MessagingBox(string buyer_id)
        {
            ApiClient<MessagesBoxViewModel> client = new ApiClient<MessagesBoxViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/MessagingBoxViewModel";
            if (buyer_id != null)
            {
                client.AddParameter("buyer_id", buyer_id);
            }
            MessagesBoxViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Message(string message_id)
        {
            ApiClient<MessageViewModel> client = new ApiClient<MessageViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/GetMessageViewModel";
            if (message_id != null)
            {
                client.AddParameter("message_id", message_id);
            }
            MessageViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SendMessage(Message message)
        {
            return View(message);
        }

        [HttpPost]
        public IActionResult UploadGigReview(Review review)
        {
            return View(review);
        }

        [HttpPost]
        public IActionResult CommencePayment(string order_id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult BecomeASellerPage(Seller seller = null)
        {
            return View(seller);
        }

        [HttpPost]
        public async Task<IActionResult> BecomeASeller(Seller seller)
        {
            if (ModelState.IsValid == false)
            {
                ViewBag.ErrorMessage = "The data you inserted is incorrect";
                return View("BecomeASellerPage", seller);
            }
            ApiClient<Seller> client = new ApiClient<Seller>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/BecomeASeller";
            bool response = await client.PostAsync(seller);
            if (response)
            {

                return RedirectToAction("HomePage");
            }
            ViewBag.ErrorMessage = "Server problem, try again later";
            return View("BecomeASellerPage", seller);
        }
    }
}
