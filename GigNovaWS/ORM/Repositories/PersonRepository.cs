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


        // ============================== Create / Update / Delete ==============================

        // Creates a Person row. The raw password is NEVER stored - we generate a random per-user salt,
        // hash (password + salt) with SHA-256, and store both the hash and the salt in the DB.
        // At login time we re-do the hash with the stored salt and compare.
        public bool Create(Person model)
        {
            string sql = "Insert into Person (person_username, person_password, person_birthdate, person_join_date, person_email, person_salt) values (@person_username, @person_password, @person_birthdate, @person_join_date, @person_email, @person_salt)";
            string salt = GetSalt(GetRandom());
            this.dbHelperOledb.AddParameter("@person_username", model.Person_username);
            this.dbHelperOledb.AddParameter("@person_password", GetHash(model.Person_password, salt));
            this.dbHelperOledb.AddParameter("@person_birthdate", model.Person_birthdate);
            this.dbHelperOledb.AddParameter("@person_join_date", model.Person_join_date);
            this.dbHelperOledb.AddParameter("@person_email", model.Person_email);
            this.dbHelperOledb.AddParameter("@person_salt", salt);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        // Update splits into two cases:
        //   1) Password field is empty - update only the editable fields (username, birthdate, email).
        //   2) Password field is filled - also re-hash the password with a fresh salt and update both.
        public bool Update(Person model)
        {
            string sql;
            if (model.Person_password == null || model.Person_password == "")
            {
                sql = @"Update Person set
                person_username = @person_username ,
                person_birthdate = @person_birthdate,
                person_email = @person_email
                where person_id = @person_id";

                this.dbHelperOledb.AddParameter("@person_username", model.Person_username);
                this.dbHelperOledb.AddParameter("@person_birthdate", model.Person_birthdate);
                this.dbHelperOledb.AddParameter("@person_email", model.Person_email);
                this.dbHelperOledb.AddParameter("@person_id", model.Person_id);
                return this.dbHelperOledb.Update(sql) > 0;
            }

            string salt = GetSalt(GetRandom());
            string hash = GetHash(model.Person_password, salt);

            sql = @"Update Person set
            person_username = @person_username ,
            person_birthdate = @person_birthdate,
            person_email = @person_email,
            person_password = @person_password,
            person_salt = @person_salt
            where person_id = @person_id";

            this.dbHelperOledb.AddParameter("@person_username", model.Person_username);
            this.dbHelperOledb.AddParameter("@person_birthdate", model.Person_birthdate);
            this.dbHelperOledb.AddParameter("@person_email", model.Person_email);
            this.dbHelperOledb.AddParameter("@person_password", hash);
            this.dbHelperOledb.AddParameter("@person_salt", salt);
            this.dbHelperOledb.AddParameter("@person_id", model.Person_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Person where person_id = @person_id";
            this.dbHelperOledb.AddParameter("@person_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

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


        // ============================== Login ==============================

        // Login by username. We DON'T compare passwords directly. We read the stored salt + stored hash,
        // hash the typed password with that same salt, and compare the two hashes.
        // Returns the person_id on success, null on failure.
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

        // Same as LogIn above but matches on email instead of username.
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


        // ============================== Change password ==============================

        // Two steps:
        //   1) Verify the current password (load the stored salt, hash the typed current password with it,
        //      compare to the stored hash). If they don't match, return false.
        //   2) Generate a fresh salt, hash the new password with it, write both into the DB.
        // Generating a NEW salt on each password change is intentional - it limits the damage if the DB
        // is ever leaked.
        public bool UpdatePassword(string person_id, string current_password, string new_password)
        {
            string sql = "Select person_salt, person_password from Person where person_id = @person_id";
            this.dbHelperOledb.AddParameter("@person_id", person_id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                if (reader.Read() == false)
                {
                    return false;
                }

                string currentSalt = reader["person_salt"].ToString();
                string currentHash = reader["person_password"].ToString();
                string calculateCurrentHash = GetHash(current_password, currentSalt);
                if (calculateCurrentHash != currentHash)
                {
                    return false;
                }
            }

            string newSalt = GetSalt(GetRandom());
            string newHash = GetHash(new_password, newSalt);

            sql = "Update Person set person_password = @person_password, person_salt = @person_salt where person_id = @person_id";
            this.dbHelperOledb.AddParameter("@person_password", newHash);
            this.dbHelperOledb.AddParameter("@person_salt", newSalt);
            this.dbHelperOledb.AddParameter("@person_id", person_id);
            return this.dbHelperOledb.Update(sql) > 0;
        }


        // ============================== Crypto helpers (salt + hash) ==============================
        // Why these exist:
        // Storing raw passwords is unsafe - if the DB is leaked, everyone's password is leaked.
        // Instead we store a SHA-256 hash of (password + per-user salt). At login we re-do the hash
        // with the stored salt and compare. Two different users with the same password end up with
        // different hashes because the salts are random and different.

        // SHA-256 hash of (password + salt), returned as base64 so it fits in a text column.
        private string GetHash(string password, string salt)
        {
            string combine = password + salt;
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(combine);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Picks a random salt length between 8 and 16 bytes (used by GetSalt below).
        private int GetRandom()
        {
            Random rnd = new Random();
            return rnd.Next(8, 16);
        }

        // Crypto-strength random bytes, returned as a base64 string.
        // RandomNumberGenerator (unlike Random) is suitable for security purposes.
        private string GetSalt(int length)
        {
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
