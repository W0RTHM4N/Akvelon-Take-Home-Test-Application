using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TakeHomeTestApp
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            await GetBranches();
        }

        private static async Task GetBranches()
        {
            var url = "https://api.appcenter.ms/";
            var userName = "WORTHMAN";
            var appName = "Akvelon-Take-Home-App-Center-Test-Application";
            var APIToken = "3ef8e8219699d57c3cd5f3439af1f70b58b33b4a";

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-API-Token", APIToken);

            var response = client.GetStringAsync($"v0.1/apps/{userName}/{appName}");

            var result = await response;

            Console.WriteLine(result);
        }
    }
}
