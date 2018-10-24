using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenHackNelnet
{
    public static class CreateRating
    {

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = null)]HttpRequest req,
            [CosmosDB(
                databaseName: "Ratingsdb",
                collectionName: "RatingsCollection",
                ConnectionStringSetting = "MyCosmosDb")] ICollector<Rating> document,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var data = JsonConvert.DeserializeObject<Rating>(requestBody);

            if (data.userId == Guid.Empty || data.productId == Guid.Empty) return new BadRequestObjectResult("User ID or product is not available");
            if((data.rating == null) || (data.rating < 0 || data.rating > 5) )
            {
                return new BadRequestObjectResult("Rating is invalid");
            }

            //Validate the userid and productid
            using (var client1 = new HttpClient())
            {
                var getUserResponse = await client1.GetAsync("https://serverlessohuser.trafficmanager.net/api/GetUser" + "?userId=" + data.userId);
                if (!getUserResponse.IsSuccessStatusCode) return new BadRequestObjectResult("User ID Invalid");
            }

            using (var client2 = new HttpClient())
            {
                var getProductResponse = await client2.GetAsync("https://serverlessohproduct.trafficmanager.net/api/GetProduct" + "?productId=" + data.productId);
                if (!getProductResponse.IsSuccessStatusCode) return new BadRequestObjectResult("Product ID Invalid");
            }

            data.id = Guid.NewGuid();
            data.timestamp = DateTime.UtcNow;

            document.Add(data);

            return new OkObjectResult(data);

        }
    }




}
