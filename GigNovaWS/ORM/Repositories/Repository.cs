namespace GigNovaWS
{
    public class Repository
    {
        protected DbHelperOledb dbHelperOledb;
        protected ModelCreators modelCreators;
        public Repository()
        {
            this.dbHelperOledb = new DbHelperOledb();
            this.modelCreators = new ModelCreators();
        }
    }
}
