namespace GigNovaWS
{
    // Base class every concrete repository (GigRepository, OrderRepository, etc.) inherits from.
    // Holds the DB connection helper and the model-creator factory, both passed in by RepositoryUOW.
    public class Repository
    {
        protected DbHelperOledb dbHelperOledb;
        protected ModelCreators modelCreators;

        public Repository(DbHelperOledb dbHelperOled, ModelCreators modelCreators)
        {
            this.dbHelperOledb = dbHelperOled;
            this.modelCreators = modelCreators;
        }

        // Returns the auto-increment id of the row just inserted on this connection.
        // Used right after a Create to get the new row's id (e.g. SellerController.AddGig).
        public string GetLastId()
        {
            string sql = "Select @@Identity";
            return this.dbHelperOledb.GetLastId(sql);
        }
    }
}
