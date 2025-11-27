namespace GigNovaWS
{
    public class ModelCreators
    {
        GigCreator gigCreator;
        OrderCreator orderCreator;
        CategoryCreator categoryCreator;
        BuyerCreator buyerCreator;
        SellerCreator sellerCreator;
        ReviewCreator reviewCreator;
        OrderStatusCreator orderstatusCreator;
        MessageCreator messageCreator;
        MessageTypeCreator messagetypeCreator;
        PersonCreator personCreator;
        OrderFileCreator orderfileCreator;
        LanguageCreator languageCreator;

        public GigCreator GigCreator
        {
            get
            {
                if (this.gigCreator == null)
                    this.gigCreator = new GigCreator();
                return this.gigCreator;
            }
        }

        public OrderCreator OrderCreator
        {
            get
            {
                if (this.orderCreator == null)
                    this.orderCreator = new OrderCreator();
                return this.orderCreator;
            }
        }

        public CategoryCreator CategoryCreator
        {
            get
            {
                if (this.categoryCreator == null)
                    this.categoryCreator = new CategoryCreator();
                return this.categoryCreator;
            }
        }

        public BuyerCreator BuyerCreator
        {
            get
            {
                if (this.buyerCreator == null)
                    this.buyerCreator = new BuyerCreator();
                return this.buyerCreator;
            }
        }
        public SellerCreator SellerCreator
        {
            get
            {
                if (this.sellerCreator == null)
                    this.sellerCreator = new SellerCreator();
                return this.sellerCreator;
            }
        }
        public ReviewCreator ReviewCreator
        {
            get
            {
                if (this.reviewCreator == null)
                    this.reviewCreator = new ReviewCreator();
                return this.reviewCreator;
            }
        }

        public OrderStatusCreator OrderStatusCreator
        {
            get
            {
                if (this.orderstatusCreator == null)
                    this.orderstatusCreator = new OrderStatusCreator();
                return this.OrderStatusCreator;
            }
        }

        public MessageCreator MessageCreator
        {
            get
            {
                if (this.messageCreator == null)
                    this.messageCreator = new MessageCreator();
                return this.messageCreator;
            }
        }

        public MessageTypeCreator MessageTypeCreator
        {
            get
            {
                if (this.messagetypeCreator == null)
                    this.messagetypeCreator = new MessageTypeCreator();
                return this.messagetypeCreator;
            }
        }

        public PersonCreator PersonCreator
        {
            get
            {
                if (this.personCreator == null)
                    this.personCreator = new PersonCreator();
                return this.personCreator;
            }
        }

        public OrderFileCreator OrderFileCreator
        {
            get
            {
                if (this.orderfileCreator == null)
                    this.orderfileCreator = new OrderFileCreator();
                return this.orderfileCreator;
            }
        }

        public LanguageCreator LanguageCreator
        {
            get
            {
                if (this.languageCreator == null)
                    this.languageCreator = new LanguageCreator();
                return this.languageCreator;
            }
        }

    }
}
