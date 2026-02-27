using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GigNovaWS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BuyerController : ControllerBase
    {
        RepositoryUOW repositoryUOW;
        public BuyerController()
        {
            this.repositoryUOW = new RepositoryUOW();
        }




        [HttpGet]
        public List<Order> GetOrderedGigsViewModel(string buyerId)
        {
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.OrderRepository.GetOrderByBuyerId(buyerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new List<Order>();
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public BuyerProfileViewmodel GetBuyerProfileViewModel(string buyer_id)
        {
            BuyerProfileViewmodel viewModel = new BuyerProfileViewmodel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                viewModel.buyer = this.repositoryUOW.BuyerRepository.GetById(buyer_id);
                viewModel.buyer_person = this.repositoryUOW.PersonRepository.GetById(buyer_id);
                if (viewModel.buyer != null && viewModel.buyer_person != null)
                {
                    viewModel.buyer.Person_id = viewModel.buyer_person.Person_id;
                    viewModel.buyer.Person_join_date = viewModel.buyer_person.Person_join_date;
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

        [HttpPost]
        public bool UpdateBuyerProfile(BuyerProfileUpdateViewModel viewModel)
        {
            if (viewModel == null || viewModel.Person_id == null || viewModel.Person_id == "")
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();

                Buyer buyer = new Buyer();
                buyer.Person_id = viewModel.Person_id;
                if (viewModel.Buyer_display_name == null)
                {
                    buyer.Buyer_display_name = "";
                }
                else
                {
                    buyer.Buyer_display_name = viewModel.Buyer_display_name;
                }

                if (viewModel.Buyer_description == null)
                {
                    buyer.Buyer_description = "";
                }
                else
                {
                    buyer.Buyer_description = viewModel.Buyer_description;
                }

                bool buyerUpdated = this.repositoryUOW.BuyerRepository.Update(buyer);
                if (buyerUpdated == true)
                {
                    return true;
                }

                Buyer currentBuyer = this.repositoryUOW.BuyerRepository.GetById(viewModel.Person_id);
                if (currentBuyer != null)
                {
                    if (currentBuyer.Buyer_display_name == buyer.Buyer_display_name &&
                        currentBuyer.Buyer_description == buyer.Buyer_description)
                    {
                        return true;
                    }
                }

                return false;
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
        public bool ChangeBuyerPassword(string buyer_id, string current_password, string new_password)
        {
            if (buyer_id == null || buyer_id == "" || current_password == null || current_password == "" || new_password == null || new_password == "")
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.PersonRepository.UpdatePassword(buyer_id, current_password, new_password);
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
        public bool PlaceOrder(Order order)
        {
            if (order == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.OrderRepository.Create(order);
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
        public CustomizeOrderViewModel GetCustomizeOrderViewModel(string order_id = null, string gig_id = null, string buyer_id = null)
        {
            CustomizeOrderViewModel viewModel = CreateEmptyCustomizeOrderViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();

                if (string.IsNullOrWhiteSpace(order_id) == false)
                {
                    FillCustomizeOrderFromOrderId(viewModel, order_id);
                    return viewModel;
                }

                if (string.IsNullOrWhiteSpace(gig_id) == false)
                {
                    FillCustomizeOrderFromGigId(viewModel, gig_id, buyer_id);
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

        [HttpGet]
        public List<CustomizeOrderViewModel> GetOrderedGigsDetailsViewModel(string buyerId, int page = 1, int pageSize = 6)
        {
            List<CustomizeOrderViewModel> viewModels = new List<CustomizeOrderViewModel>();
            if (string.IsNullOrWhiteSpace(buyerId))
            {
                return viewModels;
            }

            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                List<Order> orders = this.repositoryUOW.OrderRepository.GetOrderByBuyerId(buyerId);
                if (orders == null)
                {
                    return viewModels;
                }

                if (page < 1)
                {
                    page = 1;
                }
                if (pageSize < 1)
                {
                    pageSize = 6;
                }

                List<Order> pagedOrders = orders
                    .OrderByDescending(order => DateTime.TryParse(order.Order_creation_date, out DateTime d) ? d : DateTime.MinValue)
                    .ThenByDescending(order => int.TryParse(order.Order_id, out int id) ? id : 0)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                foreach (Order order in pagedOrders)
                {
                    if (order == null || string.IsNullOrWhiteSpace(order.Order_id))
                    {
                        continue;
                    }

                    CustomizeOrderViewModel current = CreateEmptyCustomizeOrderViewModel();
                    FillCustomizeOrderFromOrderId(current, order.Order_id);
                    viewModels.Add(current);
                }

                return viewModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return viewModels;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }


        private CustomizeOrderViewModel CreateEmptyCustomizeOrderViewModel()
        {
            CustomizeOrderViewModel viewModel = new CustomizeOrderViewModel();
            viewModel.order = new Order();
            viewModel.order_file = new Order_file();
            viewModel.order_files = new List<Order_file>();
            viewModel.gig = new Gig();
            viewModel.seller = new Seller();
            viewModel.seller_person = new Person();
            viewModel.delivery_time = new Delivery_time();
            viewModel.order_status = new Order_status();
            return viewModel;
        }

        private void FillCustomizeOrderFromOrderId(CustomizeOrderViewModel viewModel, string orderId)
        {
            Order foundOrder = this.repositoryUOW.OrderRepository.GetById(orderId);
            if (foundOrder != null)
            {
                viewModel.order = foundOrder;
            }

            Order_file firstFile = this.repositoryUOW.Order_filesRepository.GetByOrderId(orderId);
            if (firstFile != null)
            {
                viewModel.order_file = firstFile;
            }

            List<Order_file> files = this.repositoryUOW.Order_filesRepository.GetAllByOrderId(orderId);
            if (files != null)
            {
                viewModel.order_files = files;
            }

            if (viewModel.order.Order_status_id > 0)
            {
                Order_status status = this.repositoryUOW.Order_statusRepository.GetById(viewModel.order.Order_status_id.ToString());
                if (status != null)
                {
                    viewModel.order_status = status;
                }
            }

            Gig gig = this.repositoryUOW.GigRepository.GetById(viewModel.order.Gig_id.ToString());
            if (gig != null)
            {
                viewModel.gig = gig;
            }

            if (viewModel.gig.Seller_id > 0)
            {
                Seller seller = this.repositoryUOW.SellerRepository.GetById(viewModel.gig.Seller_id.ToString());
                if (seller != null)
                {
                    viewModel.seller = seller;
                }

                Person sellerPerson = this.repositoryUOW.PersonRepository.GetById(viewModel.gig.Seller_id.ToString());
                if (sellerPerson != null)
                {
                    viewModel.seller_person = sellerPerson;
                }
            }

            if (viewModel.gig.Delivery_time_id > 0)
            {
                Delivery_time deliveryTime = this.repositoryUOW.Delivery_timeRepository.GetById(viewModel.gig.Delivery_time_id.ToString());
                if (deliveryTime != null)
                {
                    viewModel.delivery_time = deliveryTime;
                }
            }
        }

        private void FillCustomizeOrderFromGigId(CustomizeOrderViewModel viewModel, string gigId, string buyerId)
        {
            Gig gig = this.repositoryUOW.GigRepository.GetById(gigId);
            if (gig == null)
            {
                return;
            }

            viewModel.gig = gig;
            viewModel.order.Order_id = "0";
            viewModel.order.Order_status_id = 1;
            viewModel.order.Order_creation_date = DateTime.Now.ToShortDateString();
            viewModel.order.Gig_id = Convert.ToInt32(gig.Gig_id);
            viewModel.order.Seller_id = gig.Seller_id;
            viewModel.order.Is_payment = false;
            viewModel.order.Order_requirements = "";

            if (string.IsNullOrWhiteSpace(buyerId) == false)
            {
                viewModel.order.Buyer_id = Convert.ToInt32(buyerId);
            }

            Order_status status = this.repositoryUOW.Order_statusRepository.GetById(viewModel.order.Order_status_id.ToString());
            if (status != null)
            {
                viewModel.order_status = status;
            }

            Seller seller = this.repositoryUOW.SellerRepository.GetById(gig.Seller_id.ToString());
            if (seller != null)
            {
                viewModel.seller = seller;
            }

            Person sellerPerson = this.repositoryUOW.PersonRepository.GetById(gig.Seller_id.ToString());
            if (sellerPerson != null)
            {
                viewModel.seller_person = sellerPerson;
            }

            if (gig.Delivery_time_id > 0)
            {
                Delivery_time deliveryTime = this.repositoryUOW.Delivery_timeRepository.GetById(gig.Delivery_time_id.ToString());
                if (deliveryTime != null)
                {
                    viewModel.delivery_time = deliveryTime;
                }
            }
        }


        [HttpPost]
        public async Task<bool> CreateOrderAndPayWithFiles()
        {
            try
            {
                IFormCollection form = await Request.ReadFormAsync();
                string modelJson = form["model"];
                if (string.IsNullOrWhiteSpace(modelJson))
                {
                    return false;
                }

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                CustomizeOrderViewModel viewModel = JsonSerializer.Deserialize<CustomizeOrderViewModel>(modelJson, options);
                if (viewModel == null || viewModel.order == null)
                {
                    return false;
                }

                this.repositoryUOW.DbHelperOledb.OpenConnection();

                if (viewModel.order.Order_requirements == null)
                {
                    viewModel.order.Order_requirements = "";
                }

                viewModel.order.Is_payment = true;
                bool orderCreated = this.repositoryUOW.OrderRepository.Create(viewModel.order);
                if (orderCreated == false)
                {
                    return false;
                }

                string orderId = this.repositoryUOW.OrderRepository.GetLastInsertedOrderId();
                if (string.IsNullOrWhiteSpace(orderId))
                {
                    return false;
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "OrderFiles");
                if (Directory.Exists(uploadsFolder) == false)
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                int fileCounter = 1;
                foreach (IFormFile file in form.Files)
                {
                    if (file != null && file.Length > 0)
                    {
                        string extension = Path.GetExtension(file.FileName);
                        string fileName = orderId + "_" + fileCounter + extension;
                        string originalFileName = Path.GetFileName(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        Order_file orderFile = new Order_file();
                        orderFile.Order_id = orderId;
                        orderFile.Order_file_name = originalFileName;
                        this.repositoryUOW.Order_filesRepository.Create(orderFile);
                        fileCounter++;
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

        [HttpGet]
        public string GetLatestOrderIdByBuyer(string buyer_id)
        {
            if (buyer_id == null || buyer_id == "")
            {
                return "";
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.OrderRepository.GetLatestOrderIdByBuyer(buyer_id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
            }
        }

        [HttpGet]
        public MessagesBoxViewModel MessagingBoxViewModel(string buyer_id, string order_id = null)
        {
            MessagesBoxViewModel viewModel = new MessagesBoxViewModel();
            viewModel.Messages = new List<Message>();
            viewModel.Senders = new List<Person>();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                if (order_id != null && order_id != "")
                {
                    viewModel.Messages = this.repositoryUOW.MessageRepository.GetByBuyerAndOrderId(buyer_id, order_id);
                }
                else
                {
                    viewModel.Messages = this.repositoryUOW.MessageRepository.GetByBuyerId(buyer_id);
                }

                foreach (Message message in viewModel.Messages)
                {
                    Person sender = this.repositoryUOW.PersonRepository.GetById(message.Sender_id.ToString());
                    if (sender != null)
                    {
                        bool exists = false;
                        foreach (Person existingSender in viewModel.Senders)
                        {
                            if (existingSender.Person_id == sender.Person_id)
                            {
                                exists = true;
                            }
                        }

                        if (exists == false)
                        {
                            viewModel.Senders.Add(sender);
                        }
                    }
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

        [HttpGet]
        public MessageViewModel GetMessageViewModel(string message_id)
        {
            MessageViewModel viewModel = new MessageViewModel();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                viewModel.message = this.repositoryUOW.MessageRepository.GetById(message_id);
                if (viewModel.message != null)
                {
                    viewModel.order = this.repositoryUOW.OrderRepository.GetById(viewModel.message.Order_id.ToString());
                    viewModel.message_type = this.repositoryUOW.Message_typeRepository.GetById(viewModel.message.Message_type_id.ToString());
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

        [HttpPost]
        public bool SendMessage(Message message)
        {
            if (message == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.MessageRepository.Create(message);
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
        public bool UploadGigReview(Review review)
        {
            if (review == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.ReviewRepository.Create(review);
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
        public bool CommencePayment(string order_id)
        {
            if (order_id == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.OrderRepository.UpdatePaymentStatus(order_id, true);
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
        public async Task<bool> BecomeASeller()
        {
            try
            {
                IFormCollection form = await Request.ReadFormAsync();
                string modelJson = form["model"];
                if (string.IsNullOrWhiteSpace(modelJson))
                {
                    return false;
                }

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                Seller seller = JsonSerializer.Deserialize<Seller>(modelJson, options);
                if (seller == null || string.IsNullOrWhiteSpace(seller.Seller_id))
                {
                    return false;
                }

                if (seller.Seller_description == null)
                {
                    seller.Seller_description = "";
                }
                if (seller.Seller_display_name == null)
                {
                    seller.Seller_display_name = "";
                }

                this.repositoryUOW.DbHelperOledb.OpenConnection();
                this.repositoryUOW.DbHelperOledb.OpenTransaction();

                Seller existingSeller = this.repositoryUOW.SellerRepository.GetById(seller.Seller_id);
                if (existingSeller == null)
                {
                    seller.Seller_is_linked = false;
                    if (this.repositoryUOW.SellerRepository.Create(seller) == false)
                    {
                        this.repositoryUOW.DbHelperOledb.RollBack();
                        return false;
                    }
                }
                else
                {
                    if (seller.Seller_avatar == null || seller.Seller_avatar == "")
                    {
                        seller.Seller_avatar = existingSeller.Seller_avatar;
                    }
                    if (this.repositoryUOW.SellerRepository.Update(seller) == false)
                    {
                        this.repositoryUOW.DbHelperOledb.RollBack();
                        return false;
                    }
                }

                if (form.Files != null && form.Files.Count > 0)
                {
                    IFormFile avatarFile = form.Files[0];
                    if (avatarFile != null && avatarFile.Length > 0)
                    {
                        string extension = Path.GetExtension(avatarFile.FileName).TrimStart('.').ToLower();
                        if (string.IsNullOrWhiteSpace(extension) == false)
                        {
                            bool avatarUpdated = this.repositoryUOW.SellerRepository.UpdatePhotoById(seller.Seller_id, extension);
                            if (avatarUpdated == false)
                            {
                                this.repositoryUOW.DbHelperOledb.RollBack();
                                return false;
                            }

                            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Seller", "Seller_avatars");
                            if (Directory.Exists(uploadsFolder) == false)
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            string avatarFileName = seller.Seller_id + "." + extension;
                            string avatarPath = Path.Combine(uploadsFolder, avatarFileName);
                            using (FileStream stream = new FileStream(avatarPath, FileMode.Create, FileAccess.Write))
                            {
                                await avatarFile.CopyToAsync(stream);
                            }
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

