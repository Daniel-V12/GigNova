using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class LanguageRepository : Repository, IRepository<Language>
    {
        public LanguageRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {
        }


        // ============================== Create / Update / Delete ==============================

        public bool Create(Language model)
        {
            string sql = @$"Insert into Languages (language_name)
            values ( @language_name)";
            this.dbHelperOledb.AddParameter("@language_name", model.Language_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        // The Languages table is a fixed lookup - we never edit a row. Update is required by
        // IRepository<T> but unused. (The previous body had no WHERE clause which would have
        // updated every row.)
        public bool Update(Language model)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Languages where language_id = @language_id";
            this.dbHelperOledb.AddParameter("@language_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }


        // ============================== Read (single + lists) ==============================

        public List<Language> GetAll()
        {
            string sql = "Select * from Languages";
            List<Language> languages = new List<Language>();
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                while (reader.Read())
                {
                    languages.Add(this.modelCreators.LanguageCreator.CreateModel(reader));
                }
            }
            return languages;
        }

        public Language GetById(string id)
        {
            string sql = "Select * from Languages where language_id = @language_id";
            this.dbHelperOledb.AddParameter("@language_id", id);
            using (IDataReader reader = this.dbHelperOledb.Select(sql))
            {
                reader.Read();
                return this.modelCreators.LanguageCreator.CreateModel(reader);
            }
        }
    }
}
