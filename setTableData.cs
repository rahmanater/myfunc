using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;

namespace myfunc
{
    public static class setTableData
    {
        [FunctionName("setTableData")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            int qty = Int32.Parse(req.Query["qty"]);
            bool isSale = Boolean.Parse(req.Query["sale"]);
            string partKey = req.Query["key"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            TableClient tableClient = await GetTableClient("tabledata");

            // Create new item using composite key constructor
            var prod1 = new Product()
            {
                RowKey = "68719518388",
                PartitionKey = partKey,
                Name = name,
                Quantity = qty,
                Sale = isSale
            };

            // Add new item to server-side table
            await tableClient.AddEntityAsync<Product>(prod1);

            string responseMessage = "Product: " + name + " added to table.";

            return new OkObjectResult(responseMessage);
        }
        public static async Task<TableClient> GetTableClient(string theTableName)
        {
            string connstring = Environment.GetEnvironmentVariable("connstring");
            TableServiceClient tableServiceClient = new TableServiceClient(connstring);
            TableClient tableClient = tableServiceClient.GetTableClient(tableName: theTableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
    }
}