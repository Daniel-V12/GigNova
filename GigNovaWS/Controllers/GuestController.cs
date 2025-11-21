using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GigNovaModels;
using GigNovaModels.ViewModels;
using System.Linq.Expressions;
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
            List<string> categoriesList = new List<string>();
            if (categories != null)
            {
                categoriesList = null; //deseralization
            }
            CatalogViewModel catalogviewModel = new CatalogViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                catalogviewModel.Categories = this.repositoryUOW.CategoryRepository.GetAll();
                if (categories == null && page == 0)
                {
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetAll();
                }
                else if (categories!= null && page == 0)
                {
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigByCategories(categoriesList);
                }
                else if (categories == null && page != 0)
                {
                    catalogviewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsByPage(page);
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
    }
    
}
