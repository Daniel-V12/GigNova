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
            string sql = "Insert into Person (person_username, person_password, person_birthdate, person_join_date, person_email) values (@person_username, @person_password, @person_birthdate, @person_join_date, @person_email)";
            this.dbHelperOledb.AddParameter("@person_username", model.Person_username);
            this.dbHelperOledb.AddParameter("@person_password", model.Person_password);
            this.dbHelperOledb.AddParameter("@person_birthdate", model.Person_birthdate);
            this.dbHelperOledb.AddParameter("@person_join_date", model.Person_join_date);
            this.dbHelperOledb.AddParameter("@person_email", model.Person_email);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Person where person_id = @person_id";
            this.dbHelperOledb.AddParameter("@person_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

        public List<Person> GetAll()
        {
            string sql = "Select * from Person";
            List<Person> persons = new List<Person>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    persons.Add(this.modelCreators.PersonCreator.CreateModel(reader));
                }
            }
            return persons;
        }

        public Person GetById(string id)
        {
            string sql = "Select * from Person where person_id = @person_id";
            this.dbHelperOledb.AddParameter("@person_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.PersonCreator.CreateModel(reader);
            }
        }

        public bool Update(Person model)
        {
            string sql = @"Update Gigs set 
            person_username = @person_username ,
            person_password = @person_password ,
            person_birthdate = @person_birthdate,
            person_join_date = @person_join_date
            person_email = @person_email";
            this.dbHelperOledb.AddParameter("@person_username", model.Person_username);
            this.dbHelperOledb.AddParameter("@person_password", model.Person_password);
            this.dbHelperOledb.AddParameter("@person_birthdate", model.Person_birthdate);
            this.dbHelperOledb.AddParameter("@person_join_date", model.Person_join_date);
            this.dbHelperOledb.AddParameter("@person_email", model.Person_email);
            return this.dbHelperOledb.Delete(sql) > 0;
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
