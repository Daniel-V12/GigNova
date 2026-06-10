using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using Microsoft.AspNetCore.Mvc;

namespace GigNovaWebApp.Controllers
{
    public class GuestController : Controller
    {
        // ============================== Home & Browsing ==============================

        // Guest landing page.
        [HttpGet]
        public IActionResult HomePage()
        {
            ViewData["HomeActor"] = "guest";
            ViewData["LayoutPath"] = "~/Views/Shared/MasterGuestPage.cshtml";
            return View("~/Views/Shared/HomePage.cshtml");
        }

        // Shows the gig catalog with filters and currency conversion. Calls the WS for gigs and (if needed) for the exchange rate.
        [HttpGet]
        public async Task<IActionResult> ViewCatalogPage(
            string categories = null,
            int page = 1,
            double min_price = 0,
            double max_price = 0,
            int delivery_time_id = 0,
            int language_id = 0,
            double min_rating = 0,
            string currency = "USD")
        {
            Dictionary<string, string> currencySymbols = new Dictionary<string, string>
            {
                { "USD", "$" },
                { "EUR", "€" },
                { "ILS", "₪" },
                { "GBP", "£" },
                { "JPY", "¥" }
            };
            if (string.IsNullOrWhiteSpace(currency) || currencySymbols.ContainsKey(currency) == false)
            {
                currency = "USD";
            }

            // If currency is not USD, fetch the exchange rate from the WS.
            double exchangeRate = 1.0;
            if (currency != "USD")
            {
                ApiClient<double> rateClient = BuildClient<double>("api/Guest/GetExchangeRate");
                rateClient.AddParameter("from", "USD");
                rateClient.AddParameter("to", currency);
                exchangeRate = await rateClient.GetAsync();
                if (exchangeRate <= 0)
                {
                    exchangeRate = 1.0;
                    currency = "USD";
                }
            }

            // Convert min/max price from the chosen currency back to USD before sending to the WS.
            double minPriceUsd = min_price;
            double maxPriceUsd = max_price;
            if (currency != "USD" && exchangeRate > 0)
            {
                if (min_price > 0)
                {
                    minPriceUsd = min_price / exchangeRate;
                }
                if (max_price > 0)
                {
                    maxPriceUsd = max_price / exchangeRate;
                }
            }

            // Call the WS catalog endpoint with whichever filters are set.
            ApiClient<CatalogViewModel> client = BuildClient<CatalogViewModel>("api/Guest/GetCatalogViewModel");
            if (categories != null)
            {
                client.AddParameter("categories", categories);
            }
            if (page != 0)
            {
                client.AddParameter("page", page.ToString());
            }
            if (minPriceUsd != 0)
            {
                client.AddParameter("min_price", minPriceUsd.ToString());
            }
            if (maxPriceUsd != 0)
            {
                client.AddParameter("max_price", maxPriceUsd.ToString());
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

            // Attach currency display info so the view can format prices correctly.
            if (catalogViewModel != null)
            {
                catalogViewModel.currency_code = currency;
                catalogViewModel.currency_symbol = currencySymbols[currency];
                catalogViewModel.exchange_rate = exchangeRate;
                catalogViewModel.min_price = min_price;
                catalogViewModel.max_price = max_price;
            }
            return View(catalogViewModel);
        }

        // Shows a single gig's full details (gig + seller + average rating).
        [HttpGet]
        public async Task<IActionResult> ViewSelectedGig(string gig_id = null)
        {
            ApiClient<SelectedGigViewModel> client = BuildClient<SelectedGigViewModel>("api/Guest/GetSelectedGigViewModel");
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            SelectedGigViewModel selectedGigViewModel = await client.GetAsync();
            return View(selectedGigViewModel);
        }

        // Shows all reviews left on a gig, enriched with each buyer's display name and bio
        // so each card can show "By <buyer>" and the popup can show their full info.
        [HttpGet]
        public async Task<IActionResult> ViewGigReviews(string gig_id)
        {
            ApiClient<List<GigReviewViewModel>> client = BuildClient<List<GigReviewViewModel>>("api/Guest/GetReviewsWithBuyerByGigId");
            if (gig_id != null)
            {
                client.AddParameter("gig_id", gig_id);
            }
            List<GigReviewViewModel> reviews = await client.GetAsync();
            ViewData["GigId"] = gig_id;
            return View(reviews);
        }

        // Shows a seller's public profile (their info + their gigs + their average rating).
        [HttpGet]
        public async Task<IActionResult> ViewSellerProfile(string seller_id)
        {
            ApiClient<SellerPublicProfileViewModel> client = BuildClient<SellerPublicProfileViewModel>("api/Guest/GetSellerPublicProfileViewModel");
            if (seller_id != null)
            {
                client.AddParameter("seller_id", seller_id);
            }
            SellerPublicProfileViewModel viewModel = await client.GetAsync();
            return View(viewModel);
        }


        // ============================== Account (Sign Up / Log In) ==============================

        // Buy gate: a guest who clicks "Buy" is bounced to the login page (we remember which gig they wanted).
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

        // Renders the sign-up form.
        [HttpGet]
        public IActionResult SignUpPage(Buyer buyer = null)
        {
            ModelState.Clear();
            return View(buyer);
        }

        // Handles the sign-up POST: validates, creates the buyer via the WS, then auto-logs them in.
        [HttpPost]
        public async Task<IActionResult> SignUp(Buyer buyer)
        {
            if (buyer == null || ModelState.IsValid == false)
            {
                ViewBag.ErrorMessage = "The data you inserted is incorrect";
                return View("SignUpPage", buyer);
            }

            buyer.Buyer_description = buyer.Buyer_description ?? "";
            buyer.Person_join_date = DateTime.Now.ToShortDateString();

            ApiClient<Buyer> client = BuildClient<Buyer>("api/Guest/SignUpPage");
            bool response = await client.PostAsync(buyer);
            if (response == false)
            {
                ViewBag.ErrorMessage = "Server problem, try again later";
                return View("SignUpPage", buyer);
            }

            // Sign-up succeeded - now auto-log the buyer in using their email + password.
            ApiClient<LoginRequestViewModel> loginClient = BuildClient<LoginRequestViewModel>("api/Guest/LogIn");
            LoginRequestViewModel loginRequest = new LoginRequestViewModel();
            loginRequest.identifier = buyer.Person_email;
            loginRequest.password = buyer.Person_password;

            int loginResult = await loginClient.PostAsyncReturn<LoginRequestViewModel, int>(loginRequest);
            if (loginResult != 0)
            {
                HttpContext.Session.SetString("person_id", loginResult.ToString());
                HttpContext.Session.SetString("actor", "buyer");
                return RedirectToAction("HomePage", "Buyer");
            }

            return RedirectToAction("LogInPage");
        }

        // Renders the login page. If a session already exists, redirect straight to the right home page.
        [HttpGet]
        public async Task<IActionResult> LogInPage()
        {
            string personId = HttpContext.Session.GetString("person_id");
            if (personId != null)
            {
                string actor = HttpContext.Session.GetString("actor");
                if (actor == "seller")
                {
                    return RedirectToAction("HomePage", "Seller");
                }
                if (actor == "buyer")
                {
                    return RedirectToAction("HomePage", "Buyer");
                }

                // Session has a person_id but no actor set yet - ask the WS what they are.
                bool isSeller = await CheckIfSeller(personId);
                if (isSeller)
                {
                    HttpContext.Session.SetString("actor", "seller");
                    return RedirectToAction("HomePage", "Seller");
                }

                HttpContext.Session.SetString("actor", "buyer");
                return RedirectToAction("HomePage", "Buyer");
            }

            // Clear the "wanted to buy gig X" remembered state if the pending-purchase flag is gone.
            if (TempData["PendingPurchase"] == null)
            {
                TempData.Remove("PurchaseGigId");
            }

            return View();
        }

        // Handles the login POST. After login, if they were trying to buy a gig, jump straight back into that flow.
        [HttpPost]
        public async Task<IActionResult> LogIn(string identifier, string password)
        {
            if (identifier == null || password == null)
            {
                ViewBag.ErrorMessage = "Please enter username/email and password.";
                return View("LogInPage");
            }

            ApiClient<LoginRequestViewModel> client = BuildClient<LoginRequestViewModel>("api/Guest/LogIn");
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

            // If they came from a "Buy" click, send them straight to CustomizeOrder for that gig.
            string pendingPurchase = TempData["PendingPurchase"] as string;
            if (pendingPurchase == "1")
            {
                string purchaseGigId = TempData["PurchaseGigId"] as string;
                if (purchaseGigId != null)
                {
                    return RedirectToAction("CustomizeOrder", "Buyer", new { gig_id = purchaseGigId });
                }
            }

            // Otherwise route them to the right home page based on whether they're a seller.
            bool sellerAfterLogin = await CheckIfSeller(loginResult.ToString());
            if (sellerAfterLogin)
            {
                HttpContext.Session.SetString("actor", "seller");
                return RedirectToAction("HomePage", "Seller");
            }

            HttpContext.Session.SetString("actor", "buyer");
            return RedirectToAction("HomePage", "Buyer");
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

        // Asks the WS whether a given person id is registered as a seller. Returns false on any error.
        private async Task<bool> CheckIfSeller(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                return false;
            }

            ApiClient<bool> client = BuildClient<bool>("api/Guest/IsSeller");
            client.AddParameter("person_id", personId);

            try
            {
                return await client.GetAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}