using GigNovaModels.Models;
using GigNovaModels.ViewModels;
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


        // ============================== Categories Management ==============================

        // Creates a new category from a name.
        [HttpPost]
        public bool AddCategory(string category_name)
        {
            try
            {
                Category category = new Category
                {
                    Category_name = category_name
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

        // Renames an existing category by id.
        [HttpPost]
        public bool UpdateCategory(string category_id, string category_name)
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
                    Category_name = category_name
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

        // Marks a category as blocked (hidden from buyers).
        [HttpPost]
        public bool BlockCategory(string category_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.CategoryRepository.Block(category_id);
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

        // Unblocks a previously blocked category.
        [HttpPost]
        public bool UnblockCategory(string category_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.CategoryRepository.Unblock(category_id);
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


        // ============================== Gig & Review Moderation ==============================

        // Marks a gig as blocked (hidden from buyers).
        [HttpPost]
        public bool BlockGig(string gig_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.Block(gig_id);
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

        // Unblocks a previously blocked gig.
        [HttpPost]
        public bool UnblockGig(string gig_id)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.Unblock(gig_id);
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

        // Permanently deletes a review.
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


        // ============================== Blocked Items List ==============================

        // Returns the list of blocked gigs OR blocked categories (chosen by `type`), filtered by search text and paginated.
        [HttpGet]
        public BlockedListViewModel GetBlockedListViewModel(string type = "gig", string search = "", int page = 1)
        {
            BlockedListViewModel viewModel = new BlockedListViewModel();
            search = search ?? "";
            viewModel.Search = search;
            string searchLower = search.Trim().ToLower();

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                if (type == "gig")
                {
                    List<Gig> all = this.repositoryUOW.GigRepository.GetBlocked();
                    List<Gig> filtered = FilterGigsBySearch(all, searchLower);
                    UpdateBlockedPagination(viewModel, filtered.Count, ref page);
                    viewModel.BlockedGigs = filtered.Skip((page - 1) * viewModel.ItemsPerPageCount).Take(viewModel.ItemsPerPageCount).ToList();
                }
                else
                {
                    List<Category> all = this.repositoryUOW.CategoryRepository.GetBlocked();
                    List<Category> filtered = FilterCategoriesBySearch(all, searchLower);
                    UpdateBlockedPagination(viewModel, filtered.Count, ref page);
                    viewModel.BlockedCategories = filtered.Skip((page - 1) * viewModel.ItemsPerPageCount).Take(viewModel.ItemsPerPageCount).ToList();
                }
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

        // Keeps only the gigs whose name contains the (already lower-cased) search text.
        private List<Gig> FilterGigsBySearch(List<Gig> gigs, string searchLower)
        {
            if (searchLower == "")
            {
                return gigs;
            }
            List<Gig> result = new List<Gig>();
            foreach (Gig gig in gigs)
            {
                if (gig.Gig_name != null && gig.Gig_name.ToLower().Contains(searchLower))
                {
                    result.Add(gig);
                }
            }
            return result;
        }

        // Keeps only the categories whose name contains the (already lower-cased) search text.
        private List<Category> FilterCategoriesBySearch(List<Category> categories, string searchLower)
        {
            if (searchLower == "")
            {
                return categories;
            }
            List<Category> result = new List<Category>();
            foreach (Category category in categories)
            {
                if (category.Category_name != null && category.Category_name.ToLower().Contains(searchLower))
                {
                    result.Add(category);
                }
            }
            return result;
        }

        // Calculates total page count and clamps the current page to a valid value.
        private void UpdateBlockedPagination(BlockedListViewModel viewModel, int itemsCount, ref int page)
        {
            int perPage = viewModel.ItemsPerPageCount;
            if (itemsCount == 0)
            {
                viewModel.TotalPages = 0;
            }
            else
            {
                viewModel.TotalPages = itemsCount / perPage;
                if (itemsCount % perPage > 0)
                {
                    viewModel.TotalPages++;
                }
            }
            if (page < 1)
            {
                page = 1;
            }
            if (viewModel.TotalPages > 0 && page > viewModel.TotalPages)
            {
                page = viewModel.TotalPages;
            }
            viewModel.Page = page;
        }
    }
}
