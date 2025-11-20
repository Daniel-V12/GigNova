namespace GigNovaWS
{
    public class RepositoryUOW
    {
        GigRepository gigRepository;
        CategoryRepository categoryRepository;
        OrderRepository orderRepository;
        MessageRepository messageRepository;
        BuyerRepository buyerRepository;
        SellerRepository sellerRepository;
        PersonRepository personRepository;
        ReviewRepository reviewRepository;
        LanguageRepository languageRepository;
        Order_statusRepository order_statusRepository;
        Order_filesRepository order_filesRepository;
        Message_typeRepository message_typeRepository;
        DbHelperOledb dbHelperOledb;
        ModelCreators modelCreators;

        public RepositoryUOW()
        {
            this.dbHelperOledb = new DbHelperOledb();
            this.modelCreators = new ModelCreators();
        }

        public DbHelperOledb DbHelperOledb
        { 
            get { return this.dbHelperOledb; }

        }
        public GigRepository GigRepository
        {
            get
            {
                if (this.gigRepository == null)
                    this.gigRepository = new GigRepository(this.dbHelperOledb,this.modelCreators);
                return this.gigRepository;  
            }
        }

        public CategoryRepository CategoryRepository
        {
            get
            {
                if (this.categoryRepository == null)
                    this.categoryRepository = new CategoryRepository(this.dbHelperOledb, this.modelCreators);
                return this.categoryRepository;
            }
        }
        public OrderRepository OrderRepository
        {
            get
            {
                if (this.orderRepository == null)
                    this.orderRepository = new OrderRepository(this.dbHelperOledb, this.modelCreators);
                return this.orderRepository;
            }
        }

        public MessageRepository MessageRepository
        {
            get
            {
                if (this.messageRepository == null)
                    this.messageRepository = new MessageRepository(this.dbHelperOledb, this.modelCreators);
                return this.messageRepository;
            }
        }
        public BuyerRepository BuyerRepository
        {
            get
            {
                if (this.buyerRepository == null)
                    this.buyerRepository = new BuyerRepository(this.dbHelperOledb, this.modelCreators);
                return this.buyerRepository;
            }
        }

        public SellerRepository SellerRepository
        {
            get
            {
                if (this.sellerRepository == null)
                    this.sellerRepository = new SellerRepository(this.dbHelperOledb, this.modelCreators);
                return this.sellerRepository;
            }
        }

        public PersonRepository PersonRepository
        {
            get
            {
                if (this.personRepository == null)
                    this.personRepository = new PersonRepository(this.dbHelperOledb, this.modelCreators);
                return this.personRepository;
            }
        }
        public ReviewRepository ReviewRepository
        {
            get
            {
                if (this.reviewRepository == null)
                    this.reviewRepository = new ReviewRepository(this.dbHelperOledb, this.modelCreators);
                return this.reviewRepository;
            }
        }
        public LanguageRepository LanguageRepository
        {
            get
            {
                if (this.languageRepository == null)
                    this.languageRepository = new LanguageRepository(this.dbHelperOledb, this.modelCreators);
                return this.languageRepository;
            }
        }
        public Order_statusRepository Order_statusRepository
        {
            get
            {
                if (this.order_statusRepository == null)
                    this.order_statusRepository = new Order_statusRepository(this.dbHelperOledb, this.modelCreators);
                return this.order_statusRepository;
            }
        }
        public Order_filesRepository Order_filesRepository
        {
            get
            {
                if (this.order_filesRepository == null)
                    this.order_filesRepository = new Order_filesRepository(this.dbHelperOledb, this.modelCreators);
                return this.order_filesRepository;
            }
        }
        public Message_typeRepository Message_typeRepository
        {
            get
            {
                if (this.message_typeRepository == null)
                    this.message_typeRepository = new Message_typeRepository(this.dbHelperOledb, this.modelCreators);
                return this.message_typeRepository;
            }
        }
    }
}
