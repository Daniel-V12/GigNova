using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class LanguageRepository : Repository, IRepository<Language>
    {
        public LanguageRepository(DbHelperOledb dbHelperOledb, ModelCreators modelCreators) : base(dbHelperOledb, modelCreators)
        {

        }
        public bool Create(Language model)
        {
            string sql = @$"Insert into Languages (language_name)
            values ( @language_name)";
            this.dbHelperOledb.AddParameter("@language_name", model.Language_name);
            return this.dbHelperOledb.Insert(sql) > 0;
        }

        public bool Delete(string id)
        {
            string sql = @"Delete from Languages where language_id = @language_id";
            this.dbHelperOledb.AddParameter("@language_id", id);
            return this.dbHelperOledb.Delete(sql) > 0;
        }

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

        public bool Update(Language model)
        {
            string sql = @"Update Languages set 
            language_name = @language_name";
            this.dbHelperOledb.AddParameter("@language_name", model.Language_name);
            return this.dbHelperOledb.Update(sql) > 0;
        }
    }
}
