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
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ViewCatalogPage(
            string categories = null,
            int page = 0,
            double min_price = 0,
            double max_price = 0,
            int delivery_time_id = 0,
            int language_id = 0,
            string search = null,
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
            if (search != null)
            {
                client.AddParameter("search", search);
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
        public async Task<IActionResult> CustomizeOrder(string order_id)
        {
            ApiClient<CustomizeOrderViewModel> client = new ApiClient<CustomizeOrderViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/GetCustomizeOrderViewModel";
            if (order_id != null)
            {
                client.AddParameter("order_id", order_id);
            }
            CustomizeOrderViewModel customizeOrderViewModel = await client.GetAsync();
            return View(customizeOrderViewModel);
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(SignUpViewModel signUpViewModel)
        {
            if (signUpViewModel == null)
            {
                return View(signUpViewModel);
            }
            return View(signUpViewModel);
        }

        [HttpGet]
        public IActionResult LogIn()
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
