using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;

namespace OpenHackNelnet
{
    public static class TestEventGrid
    {
        [FunctionName("TestEventGrid")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic requestContent = JsonConvert.DeserializeObject(requestBody);

            log.LogDebug(requestBody);

            if (requestContent == null) return new BadRequestObjectResult("EventGrid message expected");

            if (requestContent[0].eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
            {
                log.LogInformation("Event Grid Validation event received.");
                var content = JsonConvert.SerializeObject(new { validationResponse = requestContent[0].data.validationCode });
                return new OkObjectResult(content);
            }

            return new OkResult();
        }
    }
}

