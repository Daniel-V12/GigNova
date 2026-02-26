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

        [HttpGet]
        public ManageGigsViewModel GetManageGigsViewModel(string seller_id, int page = 0)
        {
            ManageGigsViewModel manageGigsViewModel = new ManageGigsViewModel();
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
                return manageGigsViewModel;
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

        [HttpPost]
        public bool UpdateSellerProfile(SellerProfileViewModel viewModel)
        {
            if (viewModel == null || viewModel.seller == null || viewModel.seller_person == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                bool sellerUpdated = this.repositoryUOW.SellerRepository.Update(viewModel.seller);
                bool personUpdated = this.repositoryUOW.PersonRepository.Update(viewModel.seller_person);
                return sellerUpdated && personUpdated;
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
        public bool ChangeSellerPassword(string seller_id, string current_password, string new_password)
        {
            if (seller_id == null || seller_id == "" || current_password == null || current_password == "" || new_password == null || new_password == "")
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
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Seller", "Seller_avatars", photo);
                if (System.IO.File.Exists(path) == false)
                {
                    path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", photo);
                }
                if (System.IO.File.Exists(path) == false)
                {
                    return NotFound();
                }

                FileStream stream = System.IO.File.OpenRead(path);
                return File(stream, $"image/{extension}");
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

        [HttpPost]
        public bool AddGig(Gig gig)
        {
            if (gig == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.CreateBySeller(gig);
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
        public bool EditGig(Gig gig)
        {
            if (gig == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.UpdateBySeller(gig);
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
        public bool DeleteGig(string seller_id, string gig_id)
        {
            if (seller_id == null || gig_id == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.DeleteBySeller(gig_id, seller_id);
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
        public bool PublishGig(string seller_id, string gig_id)
        {
            if (seller_id == null || gig_id == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.GigRepository.SetPublishStatus(gig_id, seller_id, true);
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

                if (delivery.Delivery_text == null)
                {
                    delivery.Delivery_text = "";
                }

                this.repositoryUOW.DbHelperOledb.OpenConnection();
                this.repositoryUOW.DbHelperOledb.OpenTransaction();

                if (delivery.Delivery_file == null)
                {
                    delivery.Delivery_file = "";
                }

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

                List<string> uploadedFileNames = new List<string>();
                if (form.Files != null && form.Files.Count > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "DeliveryFiles");
                    if (Directory.Exists(uploadsFolder) == false)
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    int fileCounter = 1;
                    foreach (IFormFile file in form.Files)
                    {
                        if (file == null || file.Length == 0)
                        {
                            continue;
                        }

                        string extension = Path.GetExtension(file.FileName);
                        string fileName = deliveryId + "_" + fileCounter + extension;
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await file.CopyToAsync(stream);
                        }

                        uploadedFileNames.Add(fileName);
                        fileCounter++;
                    }
                }

                if (uploadedFileNames.Count > 0)
                {
                    string deliveryFilesValue = string.Join("|", uploadedFileNames);
                    bool fileUpdated = this.repositoryUOW.DeliveryRepository.UpdateFileById(deliveryId, deliveryFilesValue);
                    if (fileUpdated == false)
                    {
                        this.repositoryUOW.DbHelperOledb.RollBack();
                        return BadRequest();
                    }
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
                this.repositoryUOW.DbHelperOledb.RollBack();
                return StatusCode(500, "Delivery Failed");
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }


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
