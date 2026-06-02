namespace GigNovaWS
{
    // Unit-of-Work that the WS controllers use as their single entry point to the ORM.
    // It owns one DbHelperOledb (one DB connection) and one ModelCreators factory, and lazily
    // creates each repository the first time it's asked for - so controllers never have to
    // wire DbHelperOledb and ModelCreators into each repository themselves.
    public class RepositoryUOW
    {
        // ============================== Backing fields (all start null and get filled on first access) ==============================

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
        Delivery_timeRepository delivery_timeRepository;
        DeliveryRepository deliveryRepository;

        DbHelperOledb dbHelperOledb;
        ModelCreators modelCreators;


        // ============================== Constructor ==============================

        public RepositoryUOW()
        {
            this.dbHelperOledb = new DbHelperOledb();
            this.modelCreators = new ModelCreators();
        }


        // ============================== Connection helper (used by controllers for Open/Close/Transaction) ==============================

        public DbHelperOledb DbHelperOledb
        {
            get { return this.dbHelperOledb; }
        }


        // ============================== Lazy-init repository properties ==============================
        // Each getter follows the same pattern: build the repo on first access, cache it, return it.

        public GigRepository GigRepository
        {
            get
            {
                if (this.gigRepository == null)
                {
                    this.gigRepository = new GigRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.gigRepository;
            }
        }

        public CategoryRepository CategoryRepository
        {
            get
            {
                if (this.categoryRepository == null)
                {
                    this.categoryRepository = new CategoryRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.categoryRepository;
            }
        }

        public OrderRepository OrderRepository
        {
            get
            {
                if (this.orderRepository == null)
                {
                    this.orderRepository = new OrderRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.orderRepository;
            }
        }

        public MessageRepository MessageRepository
        {
            get
            {
                if (this.messageRepository == null)
                {
                    this.messageRepository = new MessageRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.messageRepository;
            }
        }

        public BuyerRepository BuyerRepository
        {
            get
            {
                if (this.buyerRepository == null)
                {
                    this.buyerRepository = new BuyerRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.buyerRepository;
            }
        }

        public SellerRepository SellerRepository
        {
            get
            {
                if (this.sellerRepository == null)
                {
                    this.sellerRepository = new SellerRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.sellerRepository;
            }
        }

        public PersonRepository PersonRepository
        {
            get
            {
                if (this.personRepository == null)
                {
                    this.personRepository = new PersonRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.personRepository;
            }
        }

        public ReviewRepository ReviewRepository
        {
            get
            {
                if (this.reviewRepository == null)
                {
                    this.reviewRepository = new ReviewRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.reviewRepository;
            }
        }

        public LanguageRepository LanguageRepository
        {
            get
            {
                if (this.languageRepository == null)
                {
                    this.languageRepository = new LanguageRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.languageRepository;
            }
        }

        public Order_statusRepository Order_statusRepository
        {
            get
            {
                if (this.order_statusRepository == null)
                {
                    this.order_statusRepository = new Order_statusRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.order_statusRepository;
            }
        }

        public Order_filesRepository Order_filesRepository
        {
            get
            {
                if (this.order_filesRepository == null)
                {
                    this.order_filesRepository = new Order_filesRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.order_filesRepository;
            }
        }

        public Delivery_timeRepository Delivery_timeRepository
        {
            get
            {
                if (this.delivery_timeRepository == null)
                {
                    this.delivery_timeRepository = new Delivery_timeRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.delivery_timeRepository;
            }
        }

        public DeliveryRepository DeliveryRepository
        {
            get
            {
                if (this.deliveryRepository == null)
                {
                    this.deliveryRepository = new DeliveryRepository(this.dbHelperOledb, this.modelCreators);
                }
                return this.deliveryRepository;
            }
        }
    }
}
