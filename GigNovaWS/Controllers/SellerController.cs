using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GigNovaWS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        RepositoryUOW repositoryUOW;

        public SellerController()
        {
            this.repositoryUOW = new RepositoryUOW();
        }


        // ============================== Profile ==============================

        // Returns the seller profile (seller row + matching person row).
        [HttpGet]
        public SellerProfileViewModel GetSellerProfileViewModel(string seller_id)
        {
            SellerProfileViewModel viewModel = new SellerProfileViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                viewModel.seller = this.repositoryUOW.SellerRepository.GetById(seller_id);
                viewModel.seller_person = this.repositoryUOW.PersonRepository.GetById(seller_id);
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

        // Changes the seller's password after the repository verifies the current password.
        [HttpPost]
        public bool ChangeSellerPassword(string seller_id, string current_password, string new_password)
        {
            if (string.IsNullOrEmpty(seller_id) || string.IsNullOrEmpty(current_password) || string.IsNullOrEmpty(new_password))
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.PersonRepository.UpdatePassword(seller_id, current_password, new_password);
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

        // Streams the seller's avatar image file by id. Used as an <img src> by the WebApp.
        [HttpGet]
        public IActionResult GetPhoto(string seller_id)
        {
            if (string.IsNullOrWhiteSpace(seller_id))
            {
                return NotFound();
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                string photo = this.repositoryUOW.SellerRepository.GetPhotoById(seller_id);
                if (string.IsNullOrWhiteSpace(photo))
                {
                    return NotFound();
                }

                string extension = Path.GetExtension(photo).TrimStart('.').ToLower();
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "SellerAvatars", photo);
                if (System.IO.File.Exists(path) == false)
                {
                    return NotFound();
                }
                FileStream stream = System.IO.File.OpenRead(path);
                return File(stream, "image/" + extension);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Image Failed To Load");
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }


        // ============================== Gigs Management ==============================

        // Returns the seller's gigs (paginated if page > 0) plus all delivery times and all categories for the edit UI.
        [HttpGet]
        public ManageGigsViewModel GetManageGigsViewModel(string seller_id, int page = 0)
        {
            ManageGigsViewModel manageGigsViewModel = new ManageGigsViewModel();
            manageGigsViewModel.Gigs = new List<Gig>();
            manageGigsViewModel.DeliveryTimes = new List<Delivery_time>();
            manageGigsViewModel.AllCategories = new List<Category>();

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                if (page == 0)
                {
                    manageGigsViewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsBySeller(seller_id);
                }
                else
                {
                    manageGigsViewModel.Gigs = this.repositoryUOW.GigRepository.GetGigsBySellerByPage(seller_id, page);
                }

                manageGigsViewModel.DeliveryTimes = this.repositoryUOW.Delivery_timeRepository.GetAll();
                manageGigsViewModel.AllCategories = this.repositoryUOW.CategoryRepository.GetAll();

                // Attach the linked category ids for each gig.
                foreach (Gig gig in manageGigsViewModel.Gigs)
                {
                    List<Category> categories = this.repositoryUOW.GigRepository.GetCategoriesByGigId(gig.Gig_id);
                    gig.Category_ids = new List<string>();
                    foreach (Category category in categories)
                    {
                        if (string.IsNullOrWhiteSpace(category.Category_id) == false)
                        {
                            gig.Category_ids.Add(category.Category_id);
                        }
                    }
                }

                return manageGigsViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return manageGigsViewModel;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Creates a new gig (with optional photo + linked categories) inside a transaction.
        [HttpPost]
        public async Task<bool> AddGig()
        {
            try
            {
                (Gig gig, IFormFile gigPhotoFile) = await this.ReadGigFromRequestAsync();
                if (gig == null || gig.Seller_id == 0)
                {
                    return false;
                }

                this.repositoryUOW.DbHelperOledb.OpenConnection();
                this.repositoryUOW.DbHelperOledb.OpenTransaction();

                bool created = this.repositoryUOW.GigRepository.Create(gig);
                if (created == false)
                {
                    this.repositoryUOW.DbHelperOledb.RollBack();
                    return false;
                }
                string gigId = this.repositoryUOW.GigRepository.GetLastId();

                if (gigPhotoFile != null)
                {
                    string photoPath = SaveGigPhoto(gigPhotoFile, gigId);
                    if (string.IsNullOrWhiteSpace(photoPath) == false)
                    {
                        this.repositoryUOW.GigRepository.UpdateGigPhoto(photoPath, gigId);
                    }
                }

                if (gig.Category_ids != null)
                {
                    foreach (string categoryId in gig.Category_ids)
                    {
                        if (string.IsNullOrWhiteSpace(categoryId) == false)
                        {
                            this.repositoryUOW.GigRepository.AddGigCategory(gigId, categoryId);
                        }
                    }
                }

                this.repositoryUOW.DbHelperOledb.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                this.repositoryUOW.DbHelperOledb.RollBack();
                return false;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Updates an existing gig (only if the gig belongs to this seller). Replaces all linked categories.
        [HttpPost]
        public async Task<bool> EditGig()
        {
            try
            {
                (Gig gig, IFormFile gigPhotoFile) = await this.ReadGigFromRequestAsync();
                if (gig == null || gig.Seller_id == 0 || string.IsNullOrWhiteSpace(gig.Gig_id))
                {
                    return false;
                }

                this.repositoryUOW.DbHelperOledb.OpenConnection();
                Gig existingGig = this.repositoryUOW.GigRepository.GetById(gig.Gig_id);
                if (existingGig == null || existingGig.Seller_id != gig.Seller_id)
                {
                    return false;
                }

                // Default to the existing photo, override only if a new photo was uploaded and saved.
                gig.Gig_photo = existingGig.Gig_photo;
                if (gigPhotoFile != null)
                {
                    string photoPath = SaveGigPhoto(gigPhotoFile, gig.Gig_id);
                    if (string.IsNullOrWhiteSpace(photoPath) == false)
                    {
                        gig.Gig_photo = photoPath;
                    }
                }

                bool updated = this.repositoryUOW.GigRepository.UpdateBySeller(gig);
                if (updated == false)
                {
                    return false;
                }

                // Replace the gig's category links with the new list.
                this.repositoryUOW.GigRepository.DeleteGigCategories(gig.Gig_id);
                if (gig.Category_ids != null)
                {
                    foreach (string categoryId in gig.Category_ids)
                    {
                        if (string.IsNullOrWhiteSpace(categoryId) == false)
                        {
                            this.repositoryUOW.GigRepository.AddGigCategory(gig.Gig_id, categoryId);
                        }
                    }
                }

                return true;
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

        // Deletes a gig (and its category links). Refuses if the gig already has orders.
        [HttpPost]
        public IActionResult DeleteGig(string seller_id, string gig_id)
        {
            if (seller_id == null || gig_id == null)
            {
                return new JsonResult("Missing seller or gig.");
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();

                bool hasOrders = this.repositoryUOW.OrderRepository.HasOrdersForGig(gig_id);
                if (hasOrders)
                {
                    return new JsonResult("Cannot delete this gig because it already has orders. You can unpublish it instead.");
                }

                this.repositoryUOW.GigRepository.DeleteGigCategories(gig_id);
                bool deleted = this.repositoryUOW.GigRepository.DeleteBySeller(gig_id, seller_id);
                if (deleted == false)
                {
                    return new JsonResult("Failed to delete gig.");
                }
                return new JsonResult("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new JsonResult("Server error.");
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Publishes the gig. Refuses if the gig has validation errors, no photo, or no categories.
        [HttpPost]
        public IActionResult PublishGig(string seller_id, string gig_id)
        {
            if (seller_id == null || gig_id == null)
            {
                return new JsonResult("Missing seller or gig.");
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();

                Gig gig = this.repositoryUOW.GigRepository.GetById(gig_id);
                if (gig == null || gig.Seller_id.ToString() != seller_id)
                {
                    return new JsonResult("Gig not found.");
                }

                gig.Validate();
                if (gig.HasErrors)
                {
                    List<string> messages = new List<string>();
                    foreach (KeyValuePair<string, List<string>> entry in gig.AllErrors())
                    {
                        if (entry.Value != null)
                        {
                            foreach (string message in entry.Value)
                            {
                                messages.Add(message);
                            }
                        }
                    }
                    return new JsonResult(string.Join(" ", messages));
                }

                if (string.IsNullOrWhiteSpace(gig.Gig_photo) || gig.Gig_photo == "none")
                {
                    return new JsonResult("Please upload a gig photo before publishing.");
                }

                List<Category> categories = this.repositoryUOW.GigRepository.GetCategoriesByGigId(gig_id);
                if (categories == null || categories.Count == 0)
                {
                    return new JsonResult("Pick at least one category before publishing.");
                }

                bool ok = this.repositoryUOW.GigRepository.SetPublishStatus(gig_id, seller_id, true);
                if (ok == false)
                {
                    return new JsonResult("Failed to publish gig.");
                }
                return new JsonResult("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new JsonResult("Server error.");
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Unpublishes the gig (sets Is_publish back to false). Only allowed for the owning seller.
        [HttpPost]
        public bool UnpublishGig(string seller_id, string gig_id)
        {
            if (seller_id == null || gig_id == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.SetPublishStatus(gig_id, seller_id, false);
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

        // Returns one gig if it belongs to the given seller, otherwise null.
        [HttpGet]
        public Gig SelectGig(string gig_id, string seller_id)
        {
            if (gig_id == null || seller_id == null)
            {
                return null;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                Gig gig = this.repositoryUOW.GigRepository.GetById(gig_id);
                if (gig == null || gig.Seller_id.ToString() != seller_id)
                {
                    return null;
                }
                return gig;
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

        // Reads either a multipart form (model JSON + optional photo file) or a plain JSON body into a Gig.
        private async Task<(Gig, IFormFile)> ReadGigFromRequestAsync()
        {
            if (Request.HasFormContentType)
            {
                IFormCollection form = await Request.ReadFormAsync();
                string modelJson = form["model"];
                if (string.IsNullOrWhiteSpace(modelJson))
                {
                    return (null, null);
                }

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                Gig formGig = JsonSerializer.Deserialize<Gig>(modelJson, options);
                IFormFile gigPhotoFile = null;
                if (form.Files.Count > 0)
                {
                    gigPhotoFile = form.Files[0];
                }
                return (formGig, gigPhotoFile);
            }

            Gig jsonGig = await Request.ReadFromJsonAsync<Gig>();
            return (jsonGig, null);
        }

        // Saves an uploaded gig photo to wwwroot/Images/Gigs. Returns the relative path, or null if rejected.
        private string SaveGigPhoto(IFormFile file, string gigId)
        {
            if (file.Length == 0)
            {
                return null;
            }

            string extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }
            extension = extension.TrimStart('.').ToLower();

            string[] allowed = new string[] { "jpg", "jpeg", "png", "gif", "webp", "bmp" };
            bool isAllowed = false;
            for (int i = 0; i < allowed.Length; i++)
            {
                if (allowed[i] == extension)
                {
                    isAllowed = true;
                    break;
                }
            }
            if (isAllowed == false)
            {
                return null;
            }

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Gigs");
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = "gig" + gigId + "." + extension;
            string fullPath = Path.Combine(folder, fileName);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                file.CopyTo(stream);
            }
            return "Gigs/" + fileName;
        }


        // ============================== Orders & Delivery ==============================

        // Returns the seller's orders and, in parallel, the buyer info for each order.
        [HttpGet]
        public OrdersViewModel GetOrdersViewModel(string seller_id)
        {
            OrdersViewModel ordersViewModel = new OrdersViewModel
            {
                Orders = new List<Order>(),
                Buyers = new List<Buyer>()
            };
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                ordersViewModel.Orders = this.repositoryUOW.OrderRepository.GetOrderBySellerId(seller_id);
                foreach (Order order in ordersViewModel.Orders)
                {
                    ordersViewModel.Buyers.Add(this.repositoryUOW.BuyerRepository.GetById(order.Buyer_id.ToString()));
                }
                return ordersViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ordersViewModel;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Returns one order (with the buyer attached) if it belongs to the given seller, otherwise null.
        [HttpGet]
        public SelectedOrderViewModel SelectOrder(string order_id, string seller_id)
        {
            if (order_id == null || seller_id == null)
            {
                return null;
            }
            SelectedOrderViewModel viewModel = new SelectedOrderViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                viewModel.Order = this.repositoryUOW.OrderRepository.GetById(order_id);
                if (viewModel.Order == null || viewModel.Order.Seller_id.ToString() != seller_id)
                {
                    return null;
                }
                viewModel.Buyer = this.repositoryUOW.BuyerRepository.GetById(viewModel.Order.Buyer_id.ToString());
                return viewModel;
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

        // Creates a delivery record, saves the uploaded delivery file, and moves the order to "delivered" (status 2). All in one transaction.
        [HttpPost]
        public async Task<IActionResult> DeliverGig()
        {
            try
            {
                IFormCollection form = await Request.ReadFormAsync();
                string modelJson = form["model"];
                if (string.IsNullOrWhiteSpace(modelJson))
                {
                    return BadRequest();
                }

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                Delivery delivery = JsonSerializer.Deserialize<Delivery>(modelJson, options);
                if (delivery == null || string.IsNullOrWhiteSpace(delivery.Order_id))
                {
                    return BadRequest();
                }

                // Delivery file rules: exactly 1 file, max 100MB, only .rar or .zip.
                if (form.Files.Count != 1)
                {
                    return BadRequest();
                }
                IFormFile deliveryFile = form.Files[0];
                if (deliveryFile.Length == 0 || deliveryFile.Length > 100 * 1024 * 1024)
                {
                    return BadRequest();
                }
                string deliveryExt = Path.GetExtension(deliveryFile.FileName).ToLower();
                if (deliveryExt != ".rar" && deliveryExt != ".zip")
                {
                    return BadRequest();
                }

                delivery.Delivery_text = delivery.Delivery_text ?? "";
                delivery.Delivery_file = delivery.Delivery_file ?? "";

                this.repositoryUOW.DbHelperOledb.OpenConnection();
                this.repositoryUOW.DbHelperOledb.OpenTransaction();

                bool deliveryCreated = this.repositoryUOW.DeliveryRepository.Create(delivery);
                if (deliveryCreated == false)
                {
                    this.repositoryUOW.DbHelperOledb.RollBack();
                    return BadRequest();
                }

                string deliveryId = this.repositoryUOW.DeliveryRepository.GetLastInsertedDeliveryId();
                if (string.IsNullOrWhiteSpace(deliveryId))
                {
                    this.repositoryUOW.DbHelperOledb.RollBack();
                    return BadRequest();
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DeliveryFiles");
                if (Directory.Exists(uploadsFolder) == false)
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string savedExtension = Path.GetExtension(deliveryFile.FileName);
                string savedFileName = deliveryId + "_1" + savedExtension;
                string savedFilePath = Path.Combine(uploadsFolder, savedFileName);
                using (FileStream stream = new FileStream(savedFilePath, FileMode.Create, FileAccess.Write))
                {
                    await deliveryFile.CopyToAsync(stream);
                }

                string deliveryFilesValue = "DeliveryFiles/" + savedFileName;
                bool fileUpdated = this.repositoryUOW.DeliveryRepository.UpdateFileById(deliveryId, deliveryFilesValue);
                if (fileUpdated == false)
                {
                    this.repositoryUOW.DbHelperOledb.RollBack();
                    return BadRequest();
                }

                bool statusUpdated = this.repositoryUOW.OrderRepository.UpdateOrderStatus(delivery.Order_id, 2);
                if (statusUpdated == false)
                {
                    this.repositoryUOW.DbHelperOledb.RollBack();
                    return BadRequest();
                }

                this.repositoryUOW.DbHelperOledb.Commit();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest();
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        // Returns all deliveries (files uploaded by the seller) attached to the given order.
        [HttpGet]
        public List<Delivery> GetDeliveriesByOrder(string order_id)
        {
            List<Delivery> deliveries = new List<Delivery>();
            if (string.IsNullOrWhiteSpace(order_id))
            {
                return deliveries;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.DeliveryRepository.GetAllByOrderId(order_id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return deliveries;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }
    }
}
