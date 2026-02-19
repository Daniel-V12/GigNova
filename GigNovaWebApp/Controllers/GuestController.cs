using GigNovaModels;
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Http;
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
            TempData["PendingPurchase"] = "1";
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
            ModelState.Clear();
            return View(buyer);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(Buyer buyer)
        {
            if (buyer != null && buyer.Buyer_description == null)
            {
                buyer.Buyer_description = "";
            }
            if (buyer != null)
            {
                buyer.Person_join_date = DateTime.Now.ToShortDateString();
            }
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
                ApiClient<LoginRequestViewModel> loginClient = new ApiClient<LoginRequestViewModel>();
                loginClient.Scheme = "https";
                loginClient.Host = "localhost";
                loginClient.Port = 7059;
                loginClient.Path = "api/Guest/LogIn";

                LoginRequestViewModel loginRequest = new LoginRequestViewModel();
                loginRequest.identifier = buyer.Person_email;
                loginRequest.password = buyer.Person_password;

                int loginResult = await loginClient.PostAsyncReturn<LoginRequestViewModel, int>(loginRequest);
                if (loginResult != 0)
                {
                    HttpContext.Session.SetString("person_id", loginResult.ToString());
                    return RedirectToAction("HomePage", "Buyer");
                }

                return RedirectToAction("LogInPage");
            }
            ViewBag.ErrorMessage = "Server problem, try again later";
            return View("SignUpPage", buyer);
        }

        [HttpGet]
        public IActionResult LogInPage()
        {
            string personId = HttpContext.Session.GetString("person_id");
            if (personId != null)
            {
                return RedirectToAction("HomePage", "Buyer");
            }

            if (TempData["PendingPurchase"] == null)
            {
                TempData.Remove("PurchaseGigId");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(string identifier, string password)
        {
            if (identifier == null || password == null)
            {
                ViewBag.ErrorMessage = "Please enter username/email and password.";
                return View("LogInPage");
            }

            ApiClient<LoginRequestViewModel> client = new ApiClient<LoginRequestViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/LogIn";

            LoginRequestViewModel loginRequest = new LoginRequestViewModel();
            loginRequest.identifier = identifier;
            loginRequest.password = password;

            int loginResult = await client.PostAsyncReturn<LoginRequestViewModel, int>(loginRequest);
            if (loginResult == 0)
            {
                ViewBag.ErrorMessage = "Invalid username/email or password.";
                return View("LogInPage");
            }

            HttpContext.Session.SetString("person_id", loginResult.ToString());

            string pendingPurchase = TempData["PendingPurchase"] as string;
            if (pendingPurchase == "1")
            {
                string purchaseGigId = TempData["PurchaseGigId"] as string;
                if (purchaseGigId != null)
                {
                    return RedirectToAction("CustomizeOrder", "Buyer", new { gig_id = purchaseGigId });
                }
            }

            return RedirectToAction("HomePage", "Buyer");
        }

    }
}
