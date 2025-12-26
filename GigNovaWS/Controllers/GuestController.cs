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
        public CatalogViewModel GetCatalogViewModel(string categories = null, int page = 0, double min_price = 0, double max_price = 0, int delivery_time_id = 0, int language_id = 0, string search = null, double min_rating = 0)
        {
            string categoriesValue = categories;
            if (categoriesValue == null)
            {
                categoriesValue = null;
            }
            CatalogViewModel catalogviewModel = new CatalogViewModel
            {
                Categories = new List<Category>(),
                Gigs = new List<Gig>()
            };
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                catalogviewModel.Categories = this.repositoryUOW.CategoryRepository.GetAll();
                catalogviewModel.Languages = this.repositoryUOW.LanguageRepository.GetAll();
                catalogviewModel.Delivery_Times = this.repositoryUOW.Delivery_timeRepository.GetAll();
                if (categoriesValue == null && page == 0)
                {
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetAll();
                }
                else if (categoriesValue != null && page == 0)
                {

                    string[] strings = categoriesValue.Split(',');
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigByCategories(strings);
                }
                else if (categoriesValue == null && page != 0)
                {

                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsByPage(page);
                }
                else if (categoriesValue != null && page != 0)
                {
                    string[] strings = categoriesValue.Split(',');
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsByPageAndCategories(strings, page);
                }
                if (min_price != 0 || max_price != 0)
                {
                    List<Gig> filtered = new List<Gig>();
                    foreach (Gig gig in catalogviewModel.Gigs)
                    {
                        bool minOk = min_price == 0 || gig.Gig_price >= min_price;
                        bool maxOk = max_price == 0 || gig.Gig_price <= max_price;
                        if (minOk && maxOk)
                        {
                            filtered.Add(gig);
                        }
                    }
                    catalogviewModel.Gigs = filtered;
                }
                if (delivery_time_id != 0)
                {
                    List<Gig> filtered = new List<Gig>();
                    foreach (Gig gig in catalogviewModel.Gigs)
                    {
                        if (gig.Delivery_time_id == delivery_time_id)
                        {
                            filtered.Add(gig);
                        }
                    }
                    catalogviewModel.Gigs = filtered;
                }
                if (language_id != 0)
                {
                    List<Gig> filtered = new List<Gig>();
                    foreach (Gig gig in catalogviewModel.Gigs)
                    {
                        if (gig.Language_id == language_id)
                        {
                            filtered.Add(gig);
                        }
                    }
                    catalogviewModel.Gigs = filtered;
                }
                if (search != null)
                {
                    string searchLower = search.ToLower();
                    List<Gig> filtered = new List<Gig>();
                    foreach (Gig gig in catalogviewModel.Gigs)
                    {
                        if (gig.Gig_name != null && gig.Gig_name.ToLower().Contains(searchLower))
                        {
                            filtered.Add(gig);
                        }
                    }
                    catalogviewModel.Gigs = filtered;
                }
                if (min_rating != 0)
                {
                    List<Gig> filtered = new List<Gig>();
                    foreach (Gig gig in catalogviewModel.Gigs)
                    {
                        double avgRating = this.repositoryUOW.ReviewRepository.GetAverageRatingByGigId(gig.Gig_id);
                        if (avgRating >= min_rating)
                        {
                            filtered.Add(gig);
                        }
                    }
                    catalogviewModel.Gigs = filtered;
                }
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
        public CustomizeOrderViewModel GetCustomizeOrderViewModel(string order_id)
        {
            CustomizeOrderViewModel customizeOrderViewModel = new CustomizeOrderViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                customizeOrderViewModel.order = this.repositoryUOW.OrderRepository.GetById(order_id);
                customizeOrderViewModel.order_file = this.repositoryUOW.Order_filesRepository.GetById(order_id);
                return customizeOrderViewModel;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        [HttpPost]
        public bool SignUp(SignUpViewModel signUpViewModel)
        {
            if (signUpViewModel == null || signUpViewModel.Person == null || signUpViewModel.Buyer == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                bool personCreated = this.repositoryUOW.PersonRepository.Create(signUpViewModel.Person);
                bool buyerCreated = this.repositoryUOW.BuyerRepository.Create(signUpViewModel.Buyer);
                return personCreated && buyerCreated;
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
        public string LogIn(string identifier, string password)
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
