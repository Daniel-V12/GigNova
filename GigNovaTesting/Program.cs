using GigNovaModels.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using GigNovaWS;
using System.Data;
namespace GigNovaTesting
{
    internal class Program
    {
        //static void CheckInsert()
        //{
        //    Console.WriteLine("Insert Language: ");
        //    string language = Console.ReadLine();
        //    DbHelperOledb dbHelperOledb = new DbHelperOledb();
        //    string sql = $"Insert into Languages (language_name) values ('{language}')";
        //    dbHelperOledb.OpenConnection();
        //    int c =dbHelperOledb.Insert(sql);
        //    dbHelperOledb.CloseConnection();
        //    if (c > 0)
        //    {
        //        Console.WriteLine("Action Successfully Worked!");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Action Failed...");
        //    }
        //}
        //static void CheckUpdate()
        //{
        //    Console.WriteLine("Which Language You Want To Update?: ");
        //    string language_id = Console.ReadLine();
        //    Console.WriteLine("Write the change: ");
        //    string language = Console.ReadLine();
        //    DbHelperOledb dbHelperOledb = new DbHelperOledb();
        //    string sql = $"UPDATE Languages SET language_name = ('{language}') WHERE language_id = ({language_id});";
        //    dbHelperOledb.OpenConnection();
        //    int c = dbHelperOledb.Update(sql);
        //    dbHelperOledb.CloseConnection();
        //    if (c > 0)
        //    {
        //        Console.WriteLine("Action Successfully Worked!");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Action Failed...");
        //    }
        //}

        static void CheckGigCreator()
        {
            string sql = "Select * from Gigs where gig_id = 5";
            DbHelperOledb dbHelperOledb = new DbHelperOledb();
            dbHelperOledb.OpenConnection();
            IDataReader datareader = dbHelperOledb.Select(sql);
            datareader.Read();
            ModelCreators modelCreators = new ModelCreators();
            Gig gig = modelCreators.GigCreator.CreateModel(datareader);
            dbHelperOledb.CloseConnection();
            Console.WriteLine($"{gig.Gig_name} - {gig.Gig_description}");
        }

        static void CheckOrderCreator()
        {
            string sql = "Select * from Orders where order_id = 1";
            DbHelperOledb dbHelperOledb = new DbHelperOledb();
            dbHelperOledb.OpenConnection();
            IDataReader datareader = dbHelperOledb.Select(sql);
            datareader.Read();
            ModelCreators modelCreators = new ModelCreators();
            Order order = modelCreators.OrderCreator.CreateModel(datareader);
            dbHelperOledb.CloseConnection();
            Console.WriteLine($"{order.Order_status_id} - {order.Order_requirements}");
        }



        static void Main(string[] args)
        {
            //CurrencyList();
            //Console.ReadLine();
            //CheckInsert();
            //Console.ReadLine();
            //CheckUpdate();
            //Console.ReadLine();
            CheckGigCreator();
            CheckOrderCreator();
        }
        //static void TestGig()
        //{
        //    Gig gig = new Gig();
        //    gig.Gig_id = "1";
        //    gig.Gig_description = "faaaaaaaaaaah";
        //    gig.Gig_price = 10000000;
        //    gig.Gig_name = "Three netherite ingots";
        //    if (gig.HasErrors == true)
        //    {
        //        foreach (KeyValuePair<string, List<string>> keyValuePair in gig.AllErrors())
        //        {
        //            Console.WriteLine(keyValuePair.Key);
        //            foreach (string str in keyValuePair.Value)
        //            {
        //                Console.WriteLine($"      {str}");
        //            }
        //            Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
        //        }
        //    }
        //    else { Console.WriteLine("There was no errors found"); }
        //}

        static async Task CurrencyList(string from = "USD", string to = "ILS", string amount = "100")
        {
            //cw
            //Console.Write(">>> Insert Currency from: ");
            //string from = Console.ReadLine();
            //Console.Write(">>> Insert Currency to: ");
            //string to = Console.ReadLine();
            //Console.Write(">>> Insert Amount: ");
            //string amount = Console.ReadLine();

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://currency-conversion-and-exchange-rates.p.rapidapi.com/convert?from={from}&to={to}& amount= {amount}"),
                Headers =
    {   
        { "x-rapidapi-key", "a42e868430mshd7fabd5578df286p189ebfjsn947f8a38777e" },
        { "x-rapidapi-host", "currency-conversion-and-exchange-rates.p.rapidapi.com" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Currency curr  = JsonSerializer.Deserialize<Currency>(body);
                Console.WriteLine($"{curr.query.amount} {curr.query.from} = {curr.result}{curr.query.to}");
            }
        }
    }
}