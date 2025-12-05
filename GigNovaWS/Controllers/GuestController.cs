using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GigNovaModels;
using GigNovaModels.ViewModels;
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


            List<string> categoriesList ;   
            CatalogViewModel catalogviewModel = new CatalogViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                catalogviewModel.Categories = this.repositoryUOW.CategoryRepository.GetAll();
                if (categories == null && page == 0)
                {
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetAll();
                }
                else if (categories != null && page == 0)
                {
                    
                    string[] strings = categories.Split(',');
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigByCategories(strings);
                }
                else if (categories == null && page != 0)
                {
                   
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsByPage(page);
                }
                else if (categories != null && page != 0)
                {
                    string[] strings = categories.Split(',');
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsByPageAndCategories(strings,page);
                }
                this.repositoryUOW.DbHelperOledb.CloseConnection();
                return catalogviewModel;
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
    }
}
