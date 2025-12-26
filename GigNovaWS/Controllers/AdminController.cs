using GigNovaModels.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GigNovaWS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        RepositoryUOW repositoryUOW;
        public AdminController()
        {
            this.repositoryUOW = new RepositoryUOW();
        }

        [HttpPost]
        public bool RemoveGig(string gig_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.Delete(gig_id);
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
        public bool RemoveGigReview(string review_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.ReviewRepository.Delete(review_id);
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
        public bool AddCategory(string category_name, string category_photo)
        {
            try
            {
                Category category = new Category
                {
                    Category_name = category_name,
                    Category_photo = category_photo
                };
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.CategoryRepository.Create(category);
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
        public bool RemoveCategory(string category_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.CategoryRepository.Delete(category_id);
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
        [HttpPost]
        public bool UpdateCategory(string category_id, string category_name, string category_photo)
        {
            if (category_id == null)
            {
                return false;
            }
            try
            {
                Category category = new Category
                {
                    Category_id = category_id,
                    Category_name = category_name,
                    Category_photo = category_photo
                };
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.CategoryRepository.Update(category);
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
    }
}
