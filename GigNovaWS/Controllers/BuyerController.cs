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


        // ============================== Profile ==============================

        // Returns the buyer profile (buyer row + matching person row, with join date copied across).
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

        // Updates the buyer's display name and description. Returns true on success.
        [HttpPost]
        public bool UpdateBuyerProfile(BuyerProfileUpdateViewModel viewModel)
        {
            if (viewModel == null || string.IsNullOrEmpty(viewModel.Person_id))
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();

                Buyer buyer = new Buyer();
                buyer.Person_id = viewModel.Person_id;
                buyer.Buyer_display_name = viewModel.Buyer_display_name ?? "";
                buyer.Buyer_description = viewModel.Buyer_description ?? "";

                return this.repositoryUOW.BuyerRepository.Update(buyer);
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

        // Changes the buyer's password after the repository verifies the current password.
        [HttpPost]
        public bool ChangeBuyerPassword(string buyer_id, string current_password, string new_password)
        {
            if (string.IsNullOrEmpty(buyer_id) || string.IsNullOrEmpty(current_password) || string.IsNullOrEmpty(new_password))
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


        // ============================== Orders (View / Customize / Create) ==============================

        // Returns a CustomizeOrderViewModel filled from either an existing order id, or from a gig id (for a brand new order).
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

        // Returns the buyer's orders: non-completed first, newest order id first inside each group, paginated.
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

                // Step 1: split orders into "non-completed" and "completed" groups.
                List<Order> nonCompleted = new List<Order>();
                List<Order> completed = new List<Order>();
                foreach (Order order in orders)
                {
                    if (order.Order_status_id == 3)
                    {
                        completed.Add(order);
                    }
                    else
                    {
                        nonCompleted.Add(order);
                    }
                }

                // Step 2: sort each group so the newest order id comes first.
                SortOrdersByIdDescending(nonCompleted);
                SortOrdersByIdDescending(completed);

                // Step 3: combine the groups (non-completed first, completed last).
                List<Order> sortedOrders = new List<Order>();
                foreach (Order order in nonCompleted)
                {
                    sortedOrders.Add(order);
                }
                foreach (Order order in completed)
                {
                    sortedOrders.Add(order);
                }

                // Step 4: take only the requested page using simple index math.
                int start = (page - 1) * pageSize;
                int end = start + pageSize;
                if (end > sortedOrders.Count)
                {
                    end = sortedOrders.Count;
                }
                for (int i = start; i < end; i++)
                {
                    CustomizeOrderViewModel current = CreateEmptyCustomizeOrderViewModel();
                    FillCustomizeOrderFromOrderId(current, sortedOrders[i].Order_id);
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

        // Bubble sort: arranges the list so the highest numeric Order_id comes first.
        private void SortOrdersByIdDescending(List<Order> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = 0; j < list.Count - 1 - i; j++)
                {
                    int idA = Convert.ToInt32(list[j].Order_id);
                    int idB = Convert.ToInt32(list[j + 1].Order_id);
                    if (idA < idB)
                    {
                        Order temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }
        }

        // Builds an empty CustomizeOrderViewModel with each nested object pre-initialized to avoid null references.
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

        // Fills the view model from an existing order id (loads order, files, gig, seller, status, delivery time).
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

        // Fills the view model for a brand new order built from a gig id (no order saved in DB yet).
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

        // Creates the order, validates uploaded requirement files (max 5, 50MB each, no blocked extensions), and saves them.
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
                CustomizeOrderModel dto = JsonSerializer.Deserialize<CustomizeOrderModel>(modelJson, options);
                if (dto == null || dto.Gig_id <= 0 || string.IsNullOrWhiteSpace(dto.Buyer_id))
                {
                    return false;
                }

                // File rules: max 5 files, max 50MB each, no blocked extensions.
                if (form.Files.Count > 5)
                {
                    return false;
                }

                string[] blockedExtensions = new string[] { ".exe", ".bat", ".cmd", ".sh", ".ps1", ".msi", ".dll", ".vbs" };
                long maxFileSize = 50 * 1024 * 1024;

                foreach (IFormFile uploadedFile in form.Files)
                {
                    if (uploadedFile.Length == 0)
                    {
                        continue;
                    }
                    if (uploadedFile.Length > maxFileSize)
                    {
                        return false;
                    }
                    string ext = Path.GetExtension(uploadedFile.FileName).ToLower();
                    for (int i = 0; i < blockedExtensions.Length; i++)
                    {
                        if (blockedExtensions[i] == ext)
                        {
                            return false;
                        }
                    }
                }

                this.repositoryUOW.DbHelperOledb.OpenConnection();

                Gig gig = this.repositoryUOW.GigRepository.GetById(dto.Gig_id.ToString());
                if (gig == null)
                {
                    return false;
                }

                Order order = new Order();
                order.Gig_id = dto.Gig_id;
                order.Buyer_id = int.Parse(dto.Buyer_id);
                order.Seller_id = gig.Seller_id;
                order.Order_requirements = dto.requirements ?? "";
                order.Order_creation_date = DateTime.Now.ToShortDateString();
                order.Order_status_id = 1;
                order.Is_payment = true;

                bool orderCreated = this.repositoryUOW.OrderRepository.Create(order);
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
                    if (file.Length > 0)
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
                        orderFile.Order_file_path = "OrderFiles/" + fileName;
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


        // ============================== Order Lifecycle (Complete / Deliveries) ==============================

        // Marks an order as completed (status 3). Only allowed if the order has status 2 and belongs to this buyer.
        [HttpPost]
        public bool CompleteOrder(string order_id, string buyer_id)
        {
            if (string.IsNullOrWhiteSpace(order_id) || string.IsNullOrWhiteSpace(buyer_id))
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                Order order = this.repositoryUOW.OrderRepository.GetById(order_id);
                if (order == null)
                {
                    return false;
                }
                if (order.Buyer_id.ToString() != buyer_id)
                {
                    return false;
                }
                if (order.Order_status_id != 2)
                {
                    return false;
                }
                return this.repositoryUOW.OrderRepository.UpdateOrderStatus(order_id, 3);
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


        // ============================== Messaging ==============================

        // Returns messages for a person (optionally filtered by order_id) and a unique list of senders.
        [HttpGet]
        public MessagesBoxViewModel MessagingBoxViewModel(string person_id, string order_id = null)
        {
            MessagesBoxViewModel viewModel = new MessagesBoxViewModel();
            viewModel.Messages = new List<Message>();
            viewModel.Senders = new List<Person>();
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                if (string.IsNullOrWhiteSpace(order_id) == false)
                {
                    viewModel.Messages = this.repositoryUOW.MessageRepository.GetByPersonAndOrderId(person_id, order_id);
                }
                else
                {
                    viewModel.Messages = this.repositoryUOW.MessageRepository.GetByPersonId(person_id);
                }

                // Build a list of unique senders (one entry per sender_id).
                foreach (Message message in viewModel.Messages)
                {
                    Person sender = this.repositoryUOW.PersonRepository.GetById(message.Sender_id.ToString());
                    if (sender == null)
                    {
                        continue;
                    }

                    bool exists = false;
                    foreach (Person existingSender in viewModel.Senders)
                    {
                        if (existingSender.Person_id == sender.Person_id)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists == false)
                    {
                        viewModel.Senders.Add(sender);
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

        // Sends a message; the receiver is auto-set based on whether the sender is the buyer or the seller on the order.
        [HttpPost]
        public bool SendMessage(Message message)
        {
            if (message == null || message.Sender_id <= 0 || message.Order_id <= 0)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                Order order = this.repositoryUOW.OrderRepository.GetById(message.Order_id.ToString());
                if (order == null || string.IsNullOrWhiteSpace(order.Order_id))
                {
                    return false;
                }
                if (order.Buyer_id == message.Sender_id)
                {
                    message.Reciever_id = order.Seller_id;
                }
                else if (order.Seller_id == message.Sender_id)
                {
                    message.Reciever_id = order.Buyer_id;
                }
                else
                {
                    return false;
                }
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


        // ============================== Reviews ==============================

        // Saves a 1-5 star review for a gig the buyer actually completed an order for, and not on their own gig.
        [HttpPost]
        public bool UploadGigReview(Review review)
        {
            if (review == null || review.Buyer_id <= 0 || review.Gig_id <= 0 || review.Review_rating < 1 || review.Review_rating > 5)
            {
                return false;
            }
            try
            {
                this.repositoryUOW.DbHelperOledb.OpenConnection();
                Gig gig = this.repositoryUOW.GigRepository.GetById(review.Gig_id.ToString());
                if (gig == null)
                {
                    return false;
                }

                // Gig creator cannot review own gig.
                if (gig.Seller_id.ToString() == review.Buyer_id.ToString())
                {
                    return false;
                }

                // Buyer must have at least one completed order for this gig.
                List<Order> buyerOrders = this.repositoryUOW.OrderRepository.GetOrderByBuyerId(review.Buyer_id.ToString());
                bool canReview = false;
                foreach (Order order in buyerOrders)
                {
                    if (order.Gig_id == review.Gig_id && order.Order_status_id == 3)
                    {
                        canReview = true;
                        break;
                    }
                }
                if (canReview == false)
                {
                    return false;
                }

                review.Seller_id = Convert.ToInt32(gig.Seller_id);
                review.Review_creation_date = DateTime.Now.ToShortDateString();
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


        // ============================== Become A Seller ==============================

        // Creates or updates a seller row for the current buyer and optionally saves an avatar image. Uses a transaction.
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

                seller.Seller_description = seller.Seller_description ?? "";
                seller.Seller_display_name = seller.Seller_display_name ?? "";

                this.repositoryUOW.DbHelperOledb.OpenConnection();
                this.repositoryUOW.DbHelperOledb.OpenTransaction();

                Seller existingSeller = this.repositoryUOW.SellerRepository.GetById(seller.Seller_id);
                if (existingSeller == null)
                {
                    if (this.repositoryUOW.SellerRepository.Create(seller) == false)
                    {
                        this.repositoryUOW.DbHelperOledb.RollBack();
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(seller.Seller_avatar))
                    {
                        seller.Seller_avatar = existingSeller.Seller_avatar;
                    }
                    if (this.repositoryUOW.SellerRepository.Update(seller) == false)
                    {
                        this.repositoryUOW.DbHelperOledb.RollBack();
                        return false;
                    }
                }

                // Save uploaded avatar image, if one was sent.
                if (form.Files.Count > 0)
                {
                    IFormFile avatarFile = form.Files[0];
                    if (avatarFile.Length > 0)
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

                            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "SellerAvatars");
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
    }
}
