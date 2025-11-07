namespace GigNovaWS
{
    public interface IRepository<T>
    {
        bool Create(T model);
        bool Update(T model);
        bool Delete(string id);
        List<T> GetAll();
        T GetById(string id);
    }
}
