namespace GigNovaWS
{
    public class Repository
    {
        protected DbHelperOledb dbHelperOledb;
        protected ModelCreators modelCreators;
        public Repository(DbHelperOledb dbHelperOled, ModelCreators modelCreators)
        {
            this.dbHelperOledb = dbHelperOled;
            this.modelCreators = modelCreators;
        }
        public string GetLastId()
        {
            string sql = "Select @@Identity";
            return this.dbHelperOledb.GetLastId(sql);
            
        }
    }
}
