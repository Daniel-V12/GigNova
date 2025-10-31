using System.Data;

namespace GigNovaWS
{
    public interface IDbHelper
    {
        void OpenConnection();
        void CloseConnection();

        //dataReader - אובייקט של recordset
        IDataReader Select(string sql);

        //CRUD
        int Update(string sql);
        int Delete(string sql);
        int Insert(string sql);

        void OpenTransaction();
        void Commit();
        void RollBack();
    }
}
