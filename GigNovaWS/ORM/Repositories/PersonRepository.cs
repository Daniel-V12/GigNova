using GigNovaModels.Models;
using System.Data;
using System.Security.Cryptography;

namespace GigNovaWS
{
    public class PersonRepository : Repository, IRepository<Person>
    {
        public PersonRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Person model)
        {
            string sql = "Insert into Person (person_username, person_password, person_birthdate, person_join_date, person_email, person_salt) values (@person_username, @person_password, @person_birthdate, @person_join_date, @person_email, @person_salt)";
            this.dbHelperOledb.AddParameter("@person_username", model.Person_username);
            this.dbHelperOledb.AddParameter("@person_birthdate", model.Person_birthdate);
            this.dbHelperOledb.AddParameter("@person_join_date", model.Person_join_date);
            this.dbHelperOledb.AddParameter("@person_email", model.Person_email);
            string salt = GetSalt(GetRandom());
            this.dbHelperOledb.AddParameter("@person_password", GetHash(model.Person_password, salt));
            this.dbHelperOledb.AddParameter("@person_salt", salt);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        private string GetHash (string password, string salt)
        {
            string combine = password + salt;
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(combine);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private int GetRandom()
        {
            Random rnd = new Random();
            return rnd.Next(8,16);
        }
        private string GetSalt(int length)
        {
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
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
            string sql = @"Select person_salt, person_id, person_password from Person where person_username = @person_username";
            this.dbHelperOledb.AddParameter("@person_username", username);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                {
                    string salt = reader["person_salt"].ToString();
                    string hash = reader["person_password"].ToString();
                    string calculateHash = GetHash(password, salt);
                    if (hash == calculateHash)
                    {
                        return reader["person_id"].ToString();
                    }
                }
                return null;
            }
        }

        public string LogInByEmail(string email, string password)
        {
            string sql = @"Select person_salt, person_id, person_password from Person where person_email = @person_email";
            this.dbHelperOledb.AddParameter("@person_email", email);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == true)
                {
                    string salt = reader["person_salt"].ToString();
                    string hash = reader["person_password"].ToString();
                    string calculateHash = GetHash(password, salt);
                    if (hash == calculateHash)
                    {
                        return reader["person_id"].ToString();
                    }
                }
                return null;
            }
        }
    }
}
