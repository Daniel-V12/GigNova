using GigNovaModels.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace GigNovaWS
{
    public class LanguageCreator : IModelCreator<Language>
    {
        public Language CreateModel(IDataReader dataReader)
        {
            Language language = new Language();
            language.Language_id = Convert.ToString(dataReader["language_id"]);
            language.Language_name = Convert.ToString(dataReader["language_name"]);
            return language;
        }
    }
}
