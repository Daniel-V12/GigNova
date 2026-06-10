using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using GigNovaModels;
using GigNovaModels.ViewModels;
using GigNovaModels.Models;
using System.Net.Http;
using System.Text.Json;

namespace GigNovaWS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        RepositoryUOW repositoryUOW;
        IConfiguration config;

        public GuestController(IConfiguration config)
        {
            this.repositoryUOW = new RepositoryUOW();
            this.config = config;
        }


        // ============================== Catalog (Browse Gigs) ==============================

        // Returns the gig catalog filtered by category, price, delivery time, language and rating, paginated.
        [HttpGet]
        public CatalogViewModel GetCatalogViewModel(string categories = null, int page = 1, double min_price = 0,
                                                   double max_price = 0, int delivery_time_id = 0, int language_id = 0,
                                                   double min_rating = 0)
        {
            CatalogViewModel catalogviewModel = BuildCatalogViewModel(categories, min_price, max_price, delivery_time_id, language_id, min_rating);

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                catalogviewModel.Categories = FilterUnblockedCategories(this.repositoryUOW.CategoryRepository.GetAll());
                catalogviewModel.Languages = this.repositoryUOW.LanguageRepository.GetAll();
                catalogviewModel.Delivery_Times = this.repositoryUOW.Delivery_timeRepository.GetAll();

                List<Gig> gigs = GetGigsByCategories(categories);
                gigs = FilterPublishedGigs(gigs);
                gigs = FilterUnblockedGigs(gigs);
                gigs = FilterByPrice(gigs, min_price, max_price);
                gigs = FilterByDeliveryTime(gigs, delivery_time_id);
                gigs = FilterByLanguage(gigs, language_id);
                gigs = FilterByRating(gigs, min_rating);

                UpdatePagination(catalogviewModel, gigs.Count, ref page);
                catalogviewModel.Gigs = gigs.Skip((page - 1) * catalogviewModel.GigsPerPageCount).Take(catalogviewModel.GigsPerPageCount).ToList();
                catalogviewModel.GigCategoryNames = BuildGigCategoryNames(catalogviewModel.Gigs);

                return catalogviewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return catalogviewModel;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }
        

        // Builds a new CatalogViewModel with empty lists and copies the current filter values into it.
        private CatalogViewModel BuildCatalogViewModel(string categories, double min_price, double max_price, int delivery_time_id, int language_id, double min_rating)
        {
            CatalogViewModel catalogviewModel = new CatalogViewModel();
            catalogviewModel.Categories = new List<Category>();
            catalogviewModel.Gigs = new List<Gig>();
            catalogviewModel.Languages = new List<Language>();
            catalogviewModel.Delivery_Times = new List<Delivery_time>();
            catalogviewModel.GigCategories = categories ?? "";
            catalogviewModel.min_price = min_price;
            catalogviewModel.max_price = max_price;
            catalogviewModel.delivery_time_id = delivery_time_id;
            catalogviewModel.language_id = language_id;
            catalogviewModel.min_rating = min_rating;
            return catalogviewModel;
        }

        // Returns all gigs if no categories are given, otherwise only gigs in the requested category names.
        private List<Gig> GetGigsByCategories(string categories)
        {
            if (categories == null)
            {
                return this.repositoryUOW.GigRepository.GetAll();
            }

            List<string> categoriesList = new List<string>();
            string[] splitCategories = categories.Split(',');
            foreach (string category in splitCategories)
            {
                string categoryTrim = category.Trim();
                if (categoryTrim != "")
                {
                    categoriesList.Add(categoryTrim);
                }
            }
            return this.repositoryUOW.GigRepository.GetGigByCategories(categoriesList.ToArray());
        }

        // For each gig, builds a comma-separated string of its category names (aligned with the gigs list).
        private List<string> BuildGigCategoryNames(List<Gig> gigs)
        {
            List<string> gigCategoryNames = new List<string>();
            foreach (Gig gig in gigs)
            {
                List<Category> gigCategories = this.repositoryUOW.GigRepository.GetCategoriesByGigId(gig.Gig_id);
                List<string> names = new List<string>();
                foreach (Category category in gigCategories)
                {
                    names.Add(category.Category_name);
                }
                gigCategoryNames.Add(string.Join(", ", names));
            }
            return gigCategoryNames;
        }

        // Keeps only gigs that the seller has published.
        private List<Gig> FilterPublishedGigs(List<Gig> gigs)
        {
            List<Gig> filtered = new List<Gig>();
            foreach (Gig gig in gigs)
            {
                if (gig.Is_publish)
                {
                    filtered.Add(gig);
                }
            }
            return filtered;
        }

        // Keeps only gigs that admin has not blocked.
        private List<Gig> FilterUnblockedGigs(List<Gig> gigs)
        {
            List<Gig> filtered = new List<Gig>();
            foreach (Gig gig in gigs)
            {
                if (gig.Is_blocked == false)
                {
                    filtered.Add(gig);
                }
            }
            return filtered;
        }

        // Keeps only categories that admin has not blocked.
        private List<Category> FilterUnblockedCategories(List<Category> categories)
        {
            List<Category> filtered = new List<Category>();
            foreach (Category category in categories)
            {
                if (category.Is_blocked == false)
                {
                    filtered.Add(category);
                }
            }
            return filtered;
        }

        // Keeps only gigs whose price falls inside the min/max range (a side is skipped when its value is 0).
        private List<Gig> FilterByPrice(List<Gig> gigs, double min_price, double max_price)
        {
            List<Gig> filtered = gigs;
            if (min_price > 0)
            {
                List<Gig> minPriceFiltered = new List<Gig>();
                foreach (Gig gig in filtered)
                {
                    if (gig.Gig_price >= min_price)
                    {
                        minPriceFiltered.Add(gig);
                    }
                }
                filtered = minPriceFiltered;
            }

            if (max_price > 0)
            {
                List<Gig> maxPriceFiltered = new List<Gig>();
                foreach (Gig gig in filtered)
                {
                    if (gig.Gig_price <= max_price)
                    {
                        maxPriceFiltered.Add(gig);
                    }
                }
                filtered = maxPriceFiltered;
            }
            return filtered;
        }

        // Keeps only gigs that match the selected delivery time id (0 means no filter).
        private List<Gig> FilterByDeliveryTime(List<Gig> gigs, int delivery_time_id)
        {
            if (delivery_time_id <= 0)
            {
                return gigs;
            }

            List<Gig> filtered = new List<Gig>();
            foreach (Gig gig in gigs)
            {
                if (gig.Delivery_time_id == delivery_time_id)
                {
                    filtered.Add(gig);
                }
            }
            return filtered;
        }

        // Keeps only gigs in the selected language id (0 means no filter).
        private List<Gig> FilterByLanguage(List<Gig> gigs, int language_id)
        {
            if (language_id <= 0)
            {
                return gigs;
            }

            List<Gig> filtered = new List<Gig>();
            foreach (Gig gig in gigs)
            {
                if (gig.Language_id == language_id)
                {
                    filtered.Add(gig);
                }
            }
            return filtered;
        }

        // Keeps only gigs whose average review rating is at least min_rating (0 means no filter).
        private List<Gig> FilterByRating(List<Gig> gigs, double min_rating)
        {
            if (min_rating <= 0)
            {
                return gigs;
            }

            List<Gig> filtered = new List<Gig>();
            foreach (Gig gig in gigs)
            {
                double avgRating = this.repositoryUOW.ReviewRepository.GetAverageRatingByGigId(gig.Gig_id);
                if (avgRating >= min_rating)
                {
                    filtered.Add(gig);
                }
            }
            return filtered;
        }

        // Calculates the total page count and clamps the current page to a valid value.
        private void UpdatePagination(CatalogViewModel catalogviewModel, int gigsCount, ref int page)
        {
            int perPage = catalogviewModel.GigsPerPageCount;
            if (gigsCount == 0)
            {
                catalogviewModel.TotalPages = 0;
            }
            else
            {
                catalogviewModel.TotalPages = gigsCount / perPage;
                if (gigsCount % perPage > 0)
                {
                    catalogviewModel.TotalPages++;
                }
            }

            if (page < 1)
            {
                page = 1;
            }
            if (catalogviewModel.TotalPages > 0 && page > catalogviewModel.TotalPages)
            {
                page = catalogviewModel.TotalPages;
            }
            catalogviewModel.Page = page;
        }


        // ============================== Gig Details & Reviews ==============================

        // Returns the selected gig together with its seller and its average review rating.
        [HttpGet]
        public SelectedGigViewModel GetSelectedGigViewModel(string gig_id)
        {
            SelectedGigViewModel selectedGigViewModel = new SelectedGigViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                selectedGigViewModel.gig = this.repositoryUOW.GigRepository.GetById(gig_id);
                selectedGigViewModel.seller = this.repositoryUOW.SellerRepository.GetById(selectedGigViewModel.gig.Seller_id.ToString());
                selectedGigViewModel.Review = this.repositoryUOW.ReviewRepository.GetAverageRatingByGigId(gig_id);
                return selectedGigViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Returns all reviews left on the given gig.
        [HttpGet]
        public List<Review> ViewGigReviews(string gig_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.ReviewRepository.GetReviewsByGigId(gig_id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Returns reviews for the gig enriched with each buyer's display name, bio, and username.
        // Used by the WebApp reviews page so it can show "By <buyer>" and open a buyer-info popup.
        // The WPF still uses ViewGigReviews above, which is left untouched.
        [HttpGet]
        public List<GigReviewViewModel> GetReviewsWithBuyerByGigId(string gig_id)
        {
            List<GigReviewViewModel> result = new List<GigReviewViewModel>();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                List<Review> reviews = this.repositoryUOW.ReviewRepository.GetReviewsByGigId(gig_id);
                if (reviews == null)
                {
                    return result;
                }

                foreach (Review review in reviews)
                {
                    GigReviewViewModel item = new GigReviewViewModel();
                    item.Review_id = review.Review_id;
                    item.Review_rating = review.Review_rating;
                    item.Review_comment = review.Review_comment;
                    item.Review_creation_date = review.Review_creation_date;
                    item.Buyer_id = review.Buyer_id;
                    item.Buyer_display_name = "Unknown";
                    item.Buyer_description = "";
                    item.Person_username = "";
                    item.Person_join_date = "";

                    Buyer buyer = this.repositoryUOW.BuyerRepository.GetById(review.Buyer_id.ToString());
                    if (buyer != null)
                    {
                        if (string.IsNullOrEmpty(buyer.Buyer_display_name) == false)
                        {
                            item.Buyer_display_name = buyer.Buyer_display_name;
                        }
                        if (buyer.Buyer_description != null)
                        {
                            item.Buyer_description = buyer.Buyer_description;
                        }
                    }

                    Person person = this.repositoryUOW.PersonRepository.GetById(review.Buyer_id.ToString());
                    if (person != null)
                    {
                        if (string.IsNullOrEmpty(person.Person_username) == false)
                        {
                            item.Person_username = person.Person_username;
                        }
                        if (string.IsNullOrEmpty(person.Person_join_date) == false)
                        {
                            item.Person_join_date = person.Person_join_date;
                        }
                    }

                    result.Add(item);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return result;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }


        // ============================== Seller Public Profile ==============================

        // Returns a seller's public profile: seller row, person row, their gigs, and their average rating.
        [HttpGet]
        public SellerPublicProfileViewModel GetSellerPublicProfileViewModel(string seller_id)
        {
            SellerPublicProfileViewModel viewModel = new SellerPublicProfileViewModel();
            viewModel.gigs = new List<Gig>();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                viewModel.seller = this.repositoryUOW.SellerRepository.GetById(seller_id);
                viewModel.seller_person = this.repositoryUOW.PersonRepository.GetById(seller_id);
                viewModel.gigs = this.repositoryUOW.GigRepository.GetGigsBySeller(seller_id);
                viewModel.average_rating = this.repositoryUOW.ReviewRepository.GetReviewBySeller(seller_id);
                return viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return viewModel;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }


        // ============================== Account (Sign Up / Log In) ==============================

        // Returns true if the given person id also exists as a seller.
        [HttpGet]
        public bool IsSeller(string person_id)
        {
            if (string.IsNullOrWhiteSpace(person_id))
            {
                return false;
            }

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                Seller seller = this.repositoryUOW.SellerRepository.GetById(person_id);
                return seller != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Creates a new Person row and then a Buyer row from the same data. Returns true on success.
        [HttpPost]
        public bool SignUpPage(Buyer buyer)
        {
            if (buyer == null)
            {
                return false;
            }
            buyer.Buyer_description = buyer.Buyer_description ?? "";
            buyer.Person_join_date = buyer.Person_join_date ?? DateTime.Now.ToShortDateString();

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                bool personCreated = this.repositoryUOW.PersonRepository.Create(buyer);
                if (personCreated == false)
                {
                    return false;
                }
                bool buyerCreated = this.repositoryUOW.BuyerRepository.Create(buyer);
                return buyerCreated;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Logs in by username OR email (decided by whether the identifier contains '@'). Returns the person id, 0 on failure.
        [HttpPost]
        public int LogIn(LoginRequestViewModel loginRequest)
        {
            if (loginRequest == null || loginRequest.identifier == null || loginRequest.password == null)
            {
                return 0;
            }

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                string login_result = null;
                if (loginRequest.identifier.Contains("@"))
                {
                    login_result = this.repositoryUOW.PersonRepository.LogInByEmail(loginRequest.identifier, loginRequest.password);
                }
                else
                {
                    login_result = this.repositoryUOW.PersonRepository.LogIn(loginRequest.identifier, loginRequest.password);
                }

                if (login_result == null || login_result == "")
                {
                    return 0;
                }

                return Convert.ToInt32(login_result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }


        // ============================== Currency Exchange ==============================

        // Calls the RapidAPI currency converter to get the exchange rate from 'from' to 'to'. Returns 1.0 on any failure.
        [HttpGet]
        public async Task<double> GetExchangeRate(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            {
                return 1.0;
            }
            if (from == to)
            {
                return 1.0;
            }

            try
            {
                string key = this.config["Rapidapi:CurrencyKey"];
                string host = this.config["Rapidapi:CurrencyHost"];
                string baseUrl = this.config["Rapidapi:CurrencyBaseUrl"];
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(baseUrl))
                {
                    return 1.0;
                }

                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/convert?from={from}&to={to}&amount=1"))
                    {
                        request.Headers.Add("x-rapidapi-key", key);
                        request.Headers.Add("x-rapidapi-host", host);
                        using (HttpResponseMessage response = await client.SendAsync(request))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                return 1.0;
                            }
                            string body = await response.Content.ReadAsStringAsync();
                            using (JsonDocument doc = JsonDocument.Parse(body))
                            {
                                if (doc.RootElement.TryGetProperty("result", out JsonElement resultElement))
                                {
                                    double rate = resultElement.GetDouble();
                                    if (rate > 0)
                                    {
                                        return rate;
                                    }
                                }
                                return 1.0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1.0;
            }
        }
    }
}
