using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Net;

namespace OpenHackNelnet
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = null)]HttpRequest req,
                    [CosmosDB(
                databaseName: "Ratingsdb",
                collectionName: "RatingsCollection",
                ConnectionStringSetting = "MyCosmosDb")] DocumentClient client,
                    ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string ratingId = req.Query["ratingId"];
            if (string.IsNullOrWhiteSpace(ratingId))
            {
                return new NotFoundResult();
            }

            try
            {
                Document doc = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri("Ratingsdb", "RatingsCollection", ratingId));
                return new OkObjectResult((Rating)(dynamic)doc);
            }
            catch (DocumentClientException de)
            {
                if(de.StatusCode == HttpStatusCode.NotFound)
                {
                    return new NotFoundResult();
                }
                return new BadRequestObjectResult("Error occured while retrieving the rating");
            }
        }
    }
}
