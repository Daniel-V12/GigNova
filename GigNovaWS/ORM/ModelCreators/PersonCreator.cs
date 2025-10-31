using GigNovaModels.Models;
using System.Data;

namespace GigNovaWS
{
    public class PersonCreator : IModelCreator<Person>
    {
        public Person CreateModel(IDataReader dataReader)
        {
            Person person = new Person();
            person.Person_id = Convert.ToString(dataReader["person_id"]);
            person.Person_username = Convert.ToString(dataReader["person_username"]);
            person.Person_password = Convert.ToString(dataReader["person_password"]);
            person.Person_birthdate = Convert.ToString(dataReader["person_birthdate"]);
            person.Person_join_date = Convert.ToString(dataReader["person_join_date"]);
            person.Person_email = Convert.ToString(dataReader["person_email"]);
            return person;
        }
    }
}
