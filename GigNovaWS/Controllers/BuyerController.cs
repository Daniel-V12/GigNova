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
                return null;
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
            CustomizeOrderViewModel customizeOrderViewModel = new CustomizeOrderViewModel();
            customizeOrderViewModel.order = new Order();
            customizeOrderViewModel.order_file = new Order_file();
            customizeOrderViewModel.order_files = new List<Order_file>();
            customizeOrderViewModel.gig = new Gig();
            customizeOrderViewModel.seller = new Seller();
            customizeOrderViewModel.seller_person = new Person();
            customizeOrderViewModel.delivery_time = new Delivery_time();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();

                if (order_id != null)
                {
                    customizeOrderViewModel.order = this.repositoryUOW.OrderRepository.GetById(order_id);
                    customizeOrderViewModel.order_file = this.repositoryUOW.Order_filesRepository.GetByOrderId(order_id);
                    if (customizeOrderViewModel.order == null)
                    {
                        customizeOrderViewModel.order = new Order();
                    }
                    if (customizeOrderViewModel.order_file == null)
                    {
                        customizeOrderViewModel.order_file = new Order_file();
                    }
                    customizeOrderViewModel.order_files = this.repositoryUOW.Order_filesRepository.GetAllByOrderId(order_id);
                    customizeOrderViewModel.gig = this.repositoryUOW.GigRepository.GetById(customizeOrderViewModel.order.Gig_id.ToString());
                    if (customizeOrderViewModel.gig == null)
                    {
                        customizeOrderViewModel.gig = new Gig();
                    }

                    if (customizeOrderViewModel.gig.Seller_id > 0)
                    {
                        customizeOrderViewModel.seller = this.repositoryUOW.SellerRepository.GetById(customizeOrderViewModel.gig.Seller_id.ToString());
                        customizeOrderViewModel.seller_person = this.repositoryUOW.PersonRepository.GetById(customizeOrderViewModel.gig.Seller_id.ToString());
                    }

                    if (customizeOrderViewModel.seller == null)
                    {
                        customizeOrderViewModel.seller = new Seller();
                    }
                    if (customizeOrderViewModel.seller_person == null)
                    {
                        customizeOrderViewModel.seller_person = new Person();
                    }

                    if (customizeOrderViewModel.gig.Delivery_time_id > 0)
                    {
                        customizeOrderViewModel.delivery_time = this.repositoryUOW.Delivery_timeRepository.GetById(customizeOrderViewModel.gig.Delivery_time_id.ToString());
                    }
                    if (customizeOrderViewModel.delivery_time == null)
                    {
                        customizeOrderViewModel.delivery_time = new Delivery_time();
                    }

                    return customizeOrderViewModel;
                }

                if (gig_id != null)
                {
                    Gig gig = this.repositoryUOW.GigRepository.GetById(gig_id);
                    if (gig != null)
                    {
                        customizeOrderViewModel.gig = gig;
                        customizeOrderViewModel.order.Order_id = "0";
                        customizeOrderViewModel.order.Order_status_id = 1;
                        customizeOrderViewModel.order.Order_creation_date = DateTime.Now.ToShortDateString();
                        customizeOrderViewModel.order.Gig_id = Convert.ToInt32(gig.Gig_id);
                        customizeOrderViewModel.order.Seller_id = gig.Seller_id;
                        if (buyer_id != null && buyer_id != "")
                        {
                            customizeOrderViewModel.order.Buyer_id = Convert.ToInt32(buyer_id);
                        }
                        customizeOrderViewModel.order.Is_payment = false;
                        customizeOrderViewModel.order.Order_requirements = "";

                        customizeOrderViewModel.seller = this.repositoryUOW.SellerRepository.GetById(gig.Seller_id.ToString());
                        customizeOrderViewModel.seller_person = this.repositoryUOW.PersonRepository.GetById(gig.Seller_id.ToString());
                        if (gig.Delivery_time_id > 0)
                        {
                            customizeOrderViewModel.delivery_time = this.repositoryUOW.Delivery_timeRepository.GetById(gig.Delivery_time_id.ToString());
                        }

                        if (customizeOrderViewModel.seller == null)
                        {
                            customizeOrderViewModel.seller = new Seller();
                        }
                        if (customizeOrderViewModel.seller_person == null)
                        {
                            customizeOrderViewModel.seller_person = new Person();
                        }
                        if (customizeOrderViewModel.delivery_time == null)
                        {
                            customizeOrderViewModel.delivery_time = new Delivery_time();
                        }
                    }
                }

                return customizeOrderViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return customizeOrderViewModel;
            }
            finally
            {
                this.repositoryUOW.DbHelperOledb.CloseConnection();
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
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        Order_file orderFile = new Order_file();
                        orderFile.Order_id = orderId;
                        orderFile.Order_file_name = fileName;
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
        public bool BecomeASeller(Seller seller)
        {
            if (seller == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.SellerRepository.Create(seller);
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
