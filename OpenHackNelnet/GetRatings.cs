using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Linq;

namespace OpenHackNelnet
{
    public static class GetRatings
    {
        [FunctionName("GetRatings")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = null)]HttpRequest req,
                    [CosmosDB(
                databaseName: "Ratingsdb",
                collectionName: "RatingsCollection",
                ConnectionStringSetting = "MyCosmosDb")] DocumentClient docClient,
                    ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new NotFoundResult();
            }

            //Validate the userid
            using (var client = new HttpClient())
            {
                var getUserResponse = await client.GetAsync("https://serverlessohuser.trafficmanager.net/api/GetUser" + "?userId=" + userId);
                if (!getUserResponse.IsSuccessStatusCode) return new BadRequestObjectResult("User ID Invalid");
            }

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Ratingsdb", "RatingsCollection");

            IDocumentQuery<Rating> query = docClient.CreateDocumentQuery<Rating>(collectionUri)
                .Where(p => p.userId.ToString() == userId)
                .AsDocumentQuery();

            var list = new List<Rating>();
            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<Rating>();
                list.AddRange(response);
            }

            return new OkObjectResult(list);
        }
    }
}
