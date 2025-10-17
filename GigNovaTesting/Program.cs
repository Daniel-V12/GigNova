using GigNovaModels.Models;
using System.Net.Http.Headers;
using System.Text.Json; 
namespace GigNovaTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CurrencyList();
            Console.ReadLine();
        }
        static void TestGig()
        {
            Gig gig = new Gig();
            gig.Gig_id = "1";
            gig.Gig_description = "faaaaaaaaaaah";
            gig.Gig_price = 10000000;
            gig.Gig_name = "Three netherite ingots";
            if (gig.HasErrors == true)
            {
                foreach (KeyValuePair<string, List<string>> keyValuePair in gig.AllErrors())
                {
                    Console.WriteLine(keyValuePair.Key);
                    foreach (string str in keyValuePair.Value)
                    {
                        Console.WriteLine($"      {str}");
                    }
                    Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
                }
            }
            else { Console.WriteLine("There was no errors found"); }
        }

        static async Task CurrencyList(string from = "USD", string to = "ILS", string amount = "100")
        {
            //duck
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