
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.Logging;

namespace OpenHackNelnet
{
    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = null)]HttpRequest req,
            [CosmosDB(
                databaseName: "Ratingsdb",
                collectionName: "RatingsCollection",
                ConnectionStringSetting = "MyCosmosDb")] ICollector<Rating> document,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            Rating rating = new Rating();
            rating.id = Guid.NewGuid();
            rating.LocationName = "Test";
            rating.productId = Guid.NewGuid();
            rating.RatingValue = 4;
            rating.Timestamp = new DateTime();
            rating.userId = Guid.NewGuid();
            rating.UserNotes = "SampleText";

            document.Add(rating);

            return new OkResult();

        }
    }




}
