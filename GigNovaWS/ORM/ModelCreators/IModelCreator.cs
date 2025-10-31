using System.Data;

namespace GigNovaWS
{
    public interface IModelCreator<T>
    {
        T CreateModel(IDataReader dataReader);


    }
}
