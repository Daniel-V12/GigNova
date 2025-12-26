using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
        public bool DeliverGig(string order_id)
        {
            if (order_id == null)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                return this.repositoryUOW.OrderRepository.UpdateOrderStatus(order_id, 2);
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
