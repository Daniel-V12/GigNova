using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
        public bool UpdateBuyerProfile(BuyerProfileViewmodel viewModel)
        {
            if (viewModel == null || viewModel.buyer == null || viewModel.buyer_person == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                bool buyerUpdated = this.repositoryUOW.BuyerRepository.Update(viewModel.buyer);
                bool personUpdated = this.repositoryUOW.PersonRepository.Update(viewModel.buyer_person);
                return buyerUpdated && personUpdated;
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

            [HttpGet]
        public MessagesBoxViewModel MessagingBoxViewModel(string buyer_id)
        {
            MessagesBoxViewModel viewModel = new MessagesBoxViewModel
            {
                Messages = new List<Message>(),
                Senders = new List<Person>()
            };
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                viewModel.Messages = this.repositoryUOW.MessageRepository.GetMessagesByReceiverId(buyer_id);
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
