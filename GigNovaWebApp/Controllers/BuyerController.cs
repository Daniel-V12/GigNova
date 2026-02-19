using Microsoft.AspNetCore.Mvc;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;

namespace GigNovaWebApp.Controllers
{
    public class BuyerController : Controller
    {
        [HttpGet]
        public IActionResult HomePage()
        {
            ViewData["HomeActor"] = "buyer";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterBuyerPage.cshtml";
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
            ApiClient<BuyerProfileViewmodel> client = new ApiClient<BuyerProfileViewmodel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Buyer/GetBuyerProfileViewModel";
            if (buyer_id != null)
            {
                client.AddParameter("buyer_id", buyer_id);
            }
            BuyerProfileViewmodel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult BuyerProfile(BuyerProfileViewmodel viewModel)
        {
            return View(viewModel);
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
            CustomizeOrderViewModel customizeOrderViewModel = await client.GetAsync();
            return View(customizeOrderViewModel);
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
