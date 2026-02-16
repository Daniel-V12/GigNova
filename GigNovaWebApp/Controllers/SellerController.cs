using Microsoft.AspNetCore.Mvc;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
namespace GigNovaWebApp.Controllers
{
    public class SellerController : Controller
    {
        [HttpGet]
        public IActionResult SellerHomePage()
        {
            return View();
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
            ApiClient<SellerProfileViewModel> client = new ApiClient<SellerProfileViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetSellerProfileViewModel";
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            SellerProfileViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SellerProfile(SellerProfileViewModel viewModel)
        {
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Orders(string seller_id)
        {
            ApiClient<OrdersViewModel> client = new ApiClient<OrdersViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/GetOrdersViewModel";
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            OrdersViewModel ordersViewModel = await client.GetAsync();
            return View(ordersViewModel);
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
        public async Task<IActionResult> SelectOrder(string order_id, string seller_id)
        {
            ApiClient<SelectedOrderViewModel> client = new ApiClient<SelectedOrderViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Seller/SelectOrder";
            if (order_id != null)
            {
                client.AddParameter("order_id", order_id);
            }
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            SelectedOrderViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DeliverGig(string order_id)
        {
            return View();
        }
    }
}
