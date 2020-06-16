using DurableFunctions.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace DurableFunctions.Activities
{
    public static class SignalRActivities
    {
        const string HUB_NAME = "updates";

        [FunctionName(nameof(SendUpdate))]
        public static Task SendUpdate([ActivityTrigger] Status status, [SignalR(HubName = HUB_NAME)] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            return signalRMessages.AddAsync(new SignalRMessage
            {
                UserId = status.UserName,
                Target = nameof(SendUpdate),
                Arguments = new[] { status }
            });
        }
    }
}
