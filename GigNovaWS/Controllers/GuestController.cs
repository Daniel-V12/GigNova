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
                gigs = FilterByPrice(gigs, min_price, max_price);
                gigs = FilterByDeliveryTime(gigs, delivery_time_id);
                gigs = FilterByLanguage(gigs, language_id);
                gigs = FilterByRating(gigs, min_rating);

                UpdatePagination(catalogviewModel, gigs.Count, ref page);
                catalogviewModel.Gigs = gigs.Skip((page - 1) * catalogviewModel.GigsPerPageCount).Take(catalogviewModel.GigsPerPageCount).ToList();

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


        [HttpPost]
        public bool SignUpPage(Buyer buyer)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                bool ok = this.repositoryUOW.BuyerRepository.Create(buyer);
                return ok;
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


        [HttpGet]
        public string LogInPage(string identifier, string password)
        {
            if (identifier == null || password == null)
            {
                return null;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                if (identifier.Contains("@"))
                {
                    return this.repositoryUOW.PersonRepository.LogInByEmail(identifier, password);
                }
                return this.repositoryUOW.PersonRepository.LogIn(identifier, password);
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


    }
}
