using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DurableFunctions.Model;
using DurableFunctions.Orchestrators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DurableFunctions.Triggers
{
    public static class HttpTrigger
    {
        [FunctionName(nameof(StartLongRunningCall))]
        public static async Task<HttpResponseMessage> StartLongRunningCall([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            var requestBody = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            //Execute long running call using an orchestrator
            string instanceId = await starter.StartNewAsync(nameof(Workflows.LongRunningWorkflow), null, data?.wait);

            //Monitor the previous workflow
            await starter.StartNewAsync(nameof(Workflows.MonitorWorkflow), null, new Info { UserName = data?.userName, WorkflowId = instanceId });

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}