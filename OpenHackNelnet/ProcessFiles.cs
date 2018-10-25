using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenHackNelnet.Models;

namespace OpenHackNelnet
{
    public static class ProcessFiles
    {
        [FunctionName("ProcessFiles_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic requestContent = JsonConvert.DeserializeObject(requestBody);

            //log.LogDebug(requestBody);

            //if (requestContent == null) return new BadRequestObjectResult("EventGrid message expected");

            //if (requestContent[0].eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
            //{
            //    log.LogInformation("Event Grid Validation event received.");
            //    var content = JsonConvert.SerializeObject(new { validationResponse = requestContent[0].data.validationCode });
            //    return new OkObjectResult(content);
            //}

            //Read event type from event grid
            var eventType = "BlobCreated";

            if (eventType != "BlobCreated") return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);

            //Read Blob Name
            var blobName = "20180518151300-OrderHeaderDetails.csv";

            //Get the unique ID
            var id = blobName.Substring(0, blobName.IndexOf("-", StringComparison.Ordinal));

            //Check if an orchestrator with that id exists
            var status = await starter.GetStatusAsync(id);
            if(status == null)
            {
                await starter.StartNewAsync("ProcessFiles", id, null);
                log.LogInformation($"Started orchestration with ID = '{id}'.");
            }

            //Raise event on a particular file is detected
            if (blobName.EndsWith("OrderHeaderDetails"))
            {
                await starter.RaiseEventAsync(id, "OrderHeaderDetails", blobName);
            }
            else if (blobName.EndsWith("OrderLineItems"))
            {
                await starter.RaiseEventAsync(id, "OrderLineItems", blobName);
            }
            else if (blobName.EndsWith("ProductInformation"))
            {
                await starter.RaiseEventAsync(id, "ProductInformation", blobName);
            }

            return starter.CreateCheckStatusResponse(req, id);
        }

        [FunctionName("ProcessFiles")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {

            // Wait for external events
            var orderHeaderDetailsEvent = context.WaitForExternalEvent<string>("OrderHeaderDetails");
            var productInfoEvent = context.WaitForExternalEvent<string>("ProductInformation");
            var orderLineItemsEvent = context.WaitForExternalEvent<string>("OrderLineItems");

            // When all three events are raised, process the files
            await Task.WhenAll(orderHeaderDetailsEvent, productInfoEvent, orderLineItemsEvent);

            // Query blob for the OrderHeaderDetails file

            // Process csv and map to orders

            // Query blob for the ProductInformation file

            // Process csv and map to ProductInfo

            // Query blob for the OrderLineItems file

            // Process csv and map to LineItems

            // For every order persist the data in cosmos
            var orders = new List<Order>();
            foreach (var order in orders)
            {
                await context.CallActivityAsync("ProcessFiles_SaveOrders", order);
            }
        }

        [FunctionName("ProcessFiles_SaveOrders")]
        public static async Task SaveOrders([ActivityTrigger] Order order,
                        [CosmosDB(
                databaseName: "Ordersdb",
                collectionName: "OrdersCollection",
                ConnectionStringSetting = "MyCosmosDb")] IAsyncCollector<Order> ordersOut
            , ILogger log)
        {
            await ordersOut.AddAsync(order);
        }
    }
}