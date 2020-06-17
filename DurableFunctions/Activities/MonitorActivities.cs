using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DurableFunctions.Activities
{
    public static class MonitorActivities
    {
        [FunctionName(nameof(CheckWorkflowStatus))]
        public static async Task<DurableOrchestrationStatus> CheckWorkflowStatus([ActivityTrigger] string instaceId, [DurableClient] IDurableClient client, ILogger logger)
        {
            return await client.GetStatusAsync(instaceId);
        }
    }
}
