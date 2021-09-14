using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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

            Console.WriteLine("Getting the branches.\n");

            List<BranchData> branchesData = await GetBranchesData();
            List<Build> activeBuilds = new List<Build>();

            Console.WriteLine("Running the builds.\n");

            foreach (BranchData branchData in branchesData)
            {
                Build build = await RunBuild(branchData.branch);

                activeBuilds.Add(build);
            }

            await MonitorBuilds(activeBuilds);
        }

        private static async Task<List<BranchData>> GetBranchesData()
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

            Build build = JsonConvert.DeserializeObject<Build>(buildString);

            return build;
        }

        private static async Task<Build> GetBuildDetails(int buildId)
        {
            string response = await client.GetStringAsync($"v0.1/apps/{ownerName}/{appName}/builds/{buildId}");

            Build updatedBuild = JsonConvert.DeserializeObject<Build>(response);

            return updatedBuild;
        }

        private static async Task<string> GetBuildLogsLink(int buildId)
        {
            string response = await client.GetStringAsync($"v0.1/apps/{ownerName}/{appName}/builds/{buildId}/downloads/logs");

            var logsModel = new { uri = "" };
            var logs = JsonConvert.DeserializeAnonymousType(response, logsModel);

            return logs.uri;
        }

        private static async Task MonitorBuilds(IEnumerable<Build> activeBuilds)
        {
            Dictionary<int, string> buildIdMessageDict = new Dictionary<int, string>();
            bool buildingCompleted = false;

            while (!buildingCompleted)
            {
                buildingCompleted = true;

                Console.WriteLine("Updating builds status: \n");

                for (int i = 0; i < activeBuilds.Count(); i++)
                {
                    Build build = activeBuilds.ElementAt(i);

                    if (!buildIdMessageDict.ContainsKey(build.id))
                    {
                        buildIdMessageDict[build.id] = "";
                    }

                    if (build.status == "completed")
                    {
                        await PrintBuildCompletionMessage(buildIdMessageDict, build);
                    }
                    else
                    {
                        build = await GetBuildDetails(build.id);

                        if (build.status == "completed")
                        {
                            await PrintBuildCompletionMessage(buildIdMessageDict, build);
                        }
                        else
                        {
                            buildingCompleted = false;

                            Console.WriteLine($"{build.sourceBranch} build is in process.");
                        }
                    }
                }

                if (!buildingCompleted)
                {
                    var nextUpdateTime = DateTime.Now.AddMinutes(2);

                    Console.WriteLine($"\nNext update at: {nextUpdateTime.ToString("T")}\n");

                    Thread.Sleep((int)new TimeSpan(0, 2, 0).TotalMilliseconds);
                }
            }
        }

        private static async Task<string> GetBuildCompletionMessage(Build build)
        {
            TimeSpan buildTime = DateTime.Parse(build.finishTime) - DateTime.Parse(build.startTime);
            string logsLink = await GetBuildLogsLink(build.id);
            string statusMessage = $"{build.sourceBranch} build {build.result} in {(int)buildTime.TotalSeconds} seconds. Link to build logs: {logsLink}";

            return statusMessage;
        }

        private static async Task PrintBuildCompletionMessage(Dictionary<int, string> buildMessageDict, Build build)
        {
            string message = buildMessageDict[build.id];

            if (message == "")
            {
                message = await GetBuildCompletionMessage(build);
                buildMessageDict[build.id] = message;
            }

            Console.WriteLine(message);
        }
    }
}
