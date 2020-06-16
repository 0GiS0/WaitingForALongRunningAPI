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
        public static async Task<HttpResponseMessage> StartLongRunningCall(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            var requestBody = await req.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            //Try to get the name of the player
            int wait = data?.wait;

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(Workflows.LongRunningWorkflow), null, wait);

            var statusResponse = starter.CreateCheckStatusResponse(req, instanceId);
            dynamic checkStatusReponse = JsonConvert.DeserializeObject(await statusResponse.Content.ReadAsStringAsync());

            await starter.StartNewAsync(nameof(Workflows.MonitorJob), null, new Info { UserName = data?.userName, StatusQueryGetUri = checkStatusReponse.statusQueryGetUri });

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            //return starter.CreateCheckStatusResponse(req, instanceId);
            return req.CreateResponse(HttpStatusCode.OK);

        }
    }
}