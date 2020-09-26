using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurablePatterns
{
    public static class MyFirstDurableFunction
    {
        [FunctionName("MyFirstDurableFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // --------------------------------------------------------
            // Function chaining

            //var outputs1 = new List<string>();

            //// Replace "hello" with the name of your Durable Activity Function.
            //outputs1.Add(await context.CallActivityAsync<string>("MyFirstDurableFunction_SayHello", "Tokyo"));
            //outputs1.Add(await context.CallActivityAsync<string>("MyFirstDurableFunction_SayHello", "Seattle"));
            //outputs1.Add(await context.CallActivityAsync<string>("MyFirstDurableFunction_SayHello", "London"));

            // outputs1 = ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]

            // --------------------------------------------------------
            // Fan out / fan in

            //var parallelTasks = new List<Task<int>>();

            //for (var index = 0; index < 10; index++)
            //{
            //    var task = context.CallActivityAsync<int>("MyFirstDurableFunction_ComputeRandom", index);
            //    parallelTasks.Add(task);
            //}

            //await Task.WhenAll(parallelTasks);

            //var outputs2 = parallelTasks.Select(t => t.Result).ToList();

            // Async HTTP APIs -----------------------------------

            var seconds = 60;

            await context.CallActivityAsync<string>("MyFirstDurableFunction_RunLong", seconds);

        }

        [FunctionName("MyFirstDurableFunction_RunLong")]
        public static async Task RunLong([ActivityTrigger] int durationSeconds, ILogger log)
        {
            var index = 0;

            while (index < durationSeconds)
            {
                await Task.Delay(1000);
                index++;
            }
        }

        [FunctionName("MyFirstDurableFunction_ComputeRandom")]
        public static async Task<int> ComputeRandom([ActivityTrigger] int seed, ILogger log)
        {
            log.LogInformation($"Starting random for {seed}");

            var random = new Random(seed);
            var result = random.Next(0, 250);

            await Task.Delay(result); // Simulate an operation that will take more or less time to complete

            return result;
        }

        [FunctionName("MyFirstDurableFunction_SayHello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("MyFirstDurableFunction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test-patterns")] 
            HttpRequestMessage req,
            [DurableClient] 
            IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("MyFirstDurableFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}