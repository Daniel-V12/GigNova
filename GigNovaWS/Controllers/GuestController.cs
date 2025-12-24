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
        public CatalogViewModel GetCatalogViewModel(string categories = null, int page = 0)
        {


            string categoriesValue = categories;
            if (string.IsNullOrWhiteSpace(categoriesValue))
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
                selectedGigViewModel.Review = this.repositoryUOW.ReviewRepository.GetReviewBySeller(selectedGigViewModel.seller.Seller_id);
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
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
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
