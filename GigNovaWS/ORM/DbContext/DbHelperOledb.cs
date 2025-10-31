using System.Data;
using System.Data.OleDb;

namespace GigNovaWS
{

    
    public class DbHelperOledb : IDbHelper
    {
        OleDbConnection oLeDbConnection;
        // אובייקט שאחראי לעשות קשר עם מסד נתונים - oleDbConnection

        OleDbCommand dbCommand;
        // אובייקט שאחראי לשלוח למסד נתונים ולהחזיר תשובה ממסד נתונים - dbCommand

        // אובייקט שאחראי על ביצוע יותר מפעולה אחת במסד נתונים - dbTransaction
        OleDbTransaction dbTransaction;

        public DbHelperOledb()
        {
            this.oLeDbConnection = new OleDbConnection();
            //this.oLeDbConnection.ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Directory.GetCurrentDirectory()}\App_Data\\GigNova.accdb;Persist Security Info=True";
            this.oLeDbConnection.ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\USER\source\repos\GigNova\GigNovaWS\App_Data\GigNova.accdb;Persist Security Info=True";
            this.dbCommand = new OleDbCommand();
            this.dbCommand.Connection = this.oLeDbConnection;
        }
        public void OpenConnection()
        {
            this.oLeDbConnection.Open();
        }
        public void CloseConnection()
        {
            this.oLeDbConnection.Close();
        }

        public void Commit()
        {
            this.dbTransaction.Commit();
        }

        public int Delete(string sql)
        {
            this.dbCommand.CommandText = sql;
            return this.dbCommand.ExecuteNonQuery();
        }

        public int Insert(string sql)
        {
            this.dbCommand.CommandText = sql;
            return this.dbCommand.ExecuteNonQuery();
        }

        

        public void OpenTransaction()
        {
            this.dbTransaction = this.oLeDbConnection.BeginTransaction();
        }

        public void RollBack()
        {
            this.dbTransaction?.Rollback();
        }

        public IDataReader Select(string sql)
        {
            this.dbCommand.CommandText = sql;
            return this.dbCommand.ExecuteReader();
        }

        public int Update(string sql)
        {
            this.dbCommand.CommandText = sql;
            return this.dbCommand.ExecuteNonQuery();
        }
    }
}
