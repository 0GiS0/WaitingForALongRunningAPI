using DurableFunctions.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace DurableFunctions.Activities
{
    public static class MonitorActivities
    {
        [FunctionName(nameof(CheckWorkflowStatus))]
        public static async Task<StatusResponse> CheckWorkflowStatus([ActivityTrigger] string statusQueryGetUri, [DurableClient] IDurableClient client, ILogger logger)
        {
            var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(statusQueryGetUri);
            var resultString = await result.Content.ReadAsStringAsync();

            var status = JsonConvert.DeserializeObject<StatusResponse>(resultString);

            return status;
        }
    }
}
