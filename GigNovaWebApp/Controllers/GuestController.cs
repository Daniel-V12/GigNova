using GigNovaModels;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Mvc;
namespace GigNovaWebApp.Controllers
{
    public class GuestController : Controller
    {
        [HttpGet]
        public IActionResult HomePage()
        {
            ViewData["HomeActor"] = "guest";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterGuestPage.cshtml";
            return View("~/Views/Shared/HomePage.cshtml");
        }

        [HttpGet]
        public IActionResult GuestHomePage()
        {
            return RedirectToAction("HomePage");
        }

        [HttpGet]
        public async Task<IActionResult> ViewCatalogPage(
            string categories = null,
            int page = 1,
            double min_price = 0,
            double max_price = 0,
            int delivery_time_id = 0,
            int language_id = 0,
            double min_rating = 0)
        {
            ApiClient<CatalogViewModel> client = new ApiClient<CatalogViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/GetCatalogViewModel";
            if (categories != null)
            {
                client.AddParameter("categories", categories);
            }
            if (page != 0)
            {
                client.AddParameter("page", page.ToString());
            }
            if (min_price != 0)
            {
                client.AddParameter("min_price", min_price.ToString());
            }
            if (max_price != 0)
            {
                client.AddParameter("max_price", max_price.ToString());
            }
            if (delivery_time_id != 0)
            {
                client.AddParameter("delivery_time_id", delivery_time_id.ToString());
            }
            if (language_id != 0)
            {
                client.AddParameter("language_id", language_id.ToString());
            }
            if (min_rating != 0)
            {
                client.AddParameter("min_rating", min_rating.ToString());
            }
            CatalogViewModel catalogViewModel = await client.GetAsync();
            return View(catalogViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewSelectedGig(string gig_id = null)
        {
            ApiClient<SelectedGigViewModel> client = new ApiClient<SelectedGigViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/GetSelectedGigViewModel";
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            SelectedGigViewModel selectedGigViewModel = await client.GetAsync();
            return View(selectedGigViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ViewGigReviews(string gig_id)
        {
            ApiClient<List<Review>> client = new ApiClient<List<Review>>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/ViewGigReviews";
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            List<Review> reviews = await client.GetAsync();
            return View(reviews);
        }

        [HttpGet]
        public IActionResult CustomizeOrder(string order_id = null, string gig_id = null)
        {
            TempData["AuthMessage"] = "Please log in or sign up as a buyer before continuing to purchase.";
            if (gig_id != null)
            {
                TempData["PurchaseGigId"] = gig_id;
            }
            return RedirectToAction("LogInPage", "Guest");
        }

        [HttpGet]
        public async Task<IActionResult> ViewSellerProfile(string seller_id)
        {
            ApiClient<SellerPublicProfileViewModel> client = new ApiClient<SellerPublicProfileViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/GetSellerPublicProfileViewModel";
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            SellerPublicProfileViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult SignUpPage(Buyer buyer = null)
        {
            return View(buyer);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(Buyer buyer)
        {
            if (ModelState.IsValid == false)
            {
                ViewBag.ErrorMessage = "The data you inserted is incorrect";
                return View("SignUpPage", buyer);
            }
            ApiClient<Buyer> client = new ApiClient<Buyer>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/SignUpPage";
            bool response = await client.PostAsync(buyer);
            if (response)
            {

                return RedirectToAction("HomePage");
            }
            ViewBag.ErrorMessage = "Server problem, try again later";
            return View("SignUpPage", buyer);
        }

        [HttpGet]
        public IActionResult LogInPage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(string identifier, string password)
        {
            ApiClient<string> client = new ApiClient<string>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/LogIn";
            if (identifier != null)
            {
                client.AddParameter("identifier", identifier);
            }
            if (password != null)
            {
                client.AddParameter("password", password);
            }
            string loginResult = await client.GetAsync();
            ViewBag.LoginResult = loginResult;
            return View();
        }

    }
}
