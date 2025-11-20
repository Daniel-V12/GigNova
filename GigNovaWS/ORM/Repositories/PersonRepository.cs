using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class PersonRepository : Repository, IRepository<Person>
    {
        public PersonRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Person model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Person> GetAll()
        {
            throw new NotImplementedException();
        }

        public Person GetById(string id)
        {
            throw new NotImplementedException();
        }

        public bool Update(Person model)
        {
            throw new NotImplementedException();
        }

        public string LogIn(string username, string password)
        {
            string sql = @"Select person_id from Person where person_username = @person_username 
            and person_password = @person_password";
            this.dbHelperOledb.AddParameter("@person_username", username);
            this.dbHelperOledb.AddParameter("@person_password", password);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                    return reader["person_id"].ToString();
                return null;
            }
        }
    }
}
