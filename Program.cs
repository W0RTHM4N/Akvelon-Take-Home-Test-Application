using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TakeHomeTestApp.Models;

namespace TakeHomeTestApp
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        private static string url = "https://api.appcenter.ms/";
        private static string ownerName = "WORTHMAN";
        private static string appName = "Akvelon-Take-Home-App-Center-Test-Application";
        private static string APIToken = "3ef8e8219699d57c3cd5f3439af1f70b58b33b4a";

        static async Task Main()
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-API-Token", APIToken);

            List<BranchData> branchesData = await GetBranches();

            foreach (BranchData branchData in branchesData)
            {
                Build build = await RunBuild(branchData.branch);
            }
        }

        private static async Task<List<BranchData>> GetBranches()
        {
            string response = await client.GetStringAsync($"v0.1/apps/{ownerName}/{appName}/branches");

            List<BranchData> branchesData = JsonConvert.DeserializeObject<List<BranchData>>(response);

            return branchesData;
        }

        private static async Task<Build> RunBuild(Branch branch)
        {
            string body = $"{{\"sourceVersion\": \"{branch.commit.sha}\", \"debug\": true}}";
            StringContent stringContent = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"v0.1/apps/{ownerName}/{appName}/branches/{branch.name}/builds", stringContent);
            string buildString = await response.Content.ReadAsStringAsync();

            Build build =  JsonConvert.DeserializeObject<Build>(buildString);

            return build;
        }
    }
}
