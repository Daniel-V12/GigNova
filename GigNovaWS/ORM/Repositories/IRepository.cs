namespace GigNovaWS
{
    // The 5 standard CRUD operations every concrete repository implements.
    // Some repos (Message, Review, Order_file, Order_status, Delivery_time, Language) don't really
    // edit their rows, and just throw NotImplementedException on Update().
    public interface IRepository<T>
    {
        bool Create(T model);
        bool Update(T model);
        bool Delete(string id);
        List<T> GetAll();
        T GetById(string id);
    }
}
