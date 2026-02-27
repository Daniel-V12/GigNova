using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GigNovaModels;
using GigNovaModels.ViewModels;
using GigNovaModels.Models;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
namespace GigNovaWS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        RepositoryUOW repositoryUOW;
        public GuestController()
        {
            this.repositoryUOW = new RepositoryUOW();
        }

        [HttpGet]
        public CatalogViewModel GetCatalogViewModel(string categories = null, int page = 1, double min_price = 0,
                                                   double max_price = 0, int delivery_time_id = 0, int language_id = 0,
                                                   double min_rating = 0)
        {
            CatalogViewModel catalogviewModel = BuildCatalogViewModel(categories, min_price, max_price, delivery_time_id, language_id, min_rating);

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                catalogviewModel.Categories = this.repositoryUOW.CategoryRepository.GetAll();
                catalogviewModel.Languages = this.repositoryUOW.LanguageRepository.GetAll();
                catalogviewModel.Delivery_Times = this.repositoryUOW.Delivery_timeRepository.GetAll();
                
                List<Gig> gigs = GetGigsByCategories(categories);
                gigs = FilterPublishedGigs(gigs);
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

        private CatalogViewModel BuildCatalogViewModel(string categories, double min_price, double max_price, int delivery_time_id, int language_id, double min_rating)
        {
            CatalogViewModel catalogviewModel = new CatalogViewModel();
            catalogviewModel.Categories = new List<Category>();
            catalogviewModel.Gigs = new List<Gig>();
            catalogviewModel.Languages = new List<Language>();
            catalogviewModel.Delivery_Times = new List<Delivery_time>();
            if (categories == null)
            {
                catalogviewModel.GigCategories = "";
            }
            else
            {
                catalogviewModel.GigCategories = categories;
            }
            catalogviewModel.min_price = min_price;
            catalogviewModel.max_price = max_price;
            catalogviewModel.delivery_time_id = delivery_time_id;
            catalogviewModel.language_id = language_id;
            catalogviewModel.min_rating = min_rating;
            return catalogviewModel;
        }

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
                if (category != null)
                {
                    string categoryTrim = category.Trim();
                    if (categoryTrim != "")
                    {
                        categoriesList.Add(categoryTrim);
                    }
                }
            }
            return this.repositoryUOW.GigRepository.GetGigByCategories(categoriesList.ToArray());
        }

        private List<string> BuildGigCategoryNames(List<Gig> gigs)
        {
            List<string> gigCategoryNames = new List<string>();
            if (gigs == null)
            {
                return gigCategoryNames;
            }

            foreach (Gig gig in gigs)
            {
                List<Category> gigCategories = this.repositoryUOW.GigRepository.GetCategoriesByGigId(gig.Gig_id);
                if (gigCategories == null || gigCategories.Count == 0)
                {
                    gigCategoryNames.Add("");
                }
                else
                {
                    List<string> names = new List<string>();
                    foreach (Category category in gigCategories)
                    {
                        names.Add(category.Category_name);
                    }
                    gigCategoryNames.Add(string.Join(", ", names));
                }
            }
            return gigCategoryNames;
        }

        private List<Gig> FilterPublishedGigs(List<Gig> gigs)
        {
            List<Gig> filtered = new List<Gig>();
            if (gigs == null)
            {
                return filtered;
            }

            foreach (Gig gig in gigs)
            {
                if (gig != null && gig.Is_publish)
                {
                    filtered.Add(gig);
                }
            }

            return filtered;
        }


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
                this.repositoryUOW.DbHelperOledb.CloseConnection();
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

        [HttpPost]
        public bool SignUpPage(Buyer buyer)
        {
            if (buyer == null)
            {
                return false;
            }
            if (buyer.Buyer_description == null)
            {
                buyer.Buyer_description = "";
            }
            if (buyer.Person_join_date == null)
            {
                buyer.Person_join_date = DateTime.Now.ToShortDateString();
            }
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

        [HttpPost]
        public int LogIn(LoginRequestViewModel loginRequest)
        {
            if (loginRequest == null)
            {
                return 0;
            }
            if (loginRequest.identifier == null || loginRequest.password == null)
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
    }
}
