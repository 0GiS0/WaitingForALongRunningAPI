using DurableFunctions.Activities;
using DurableFunctions.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DurableFunctions.Orchestrators
{
    public static class Workflows
    {
        const int EXPIRY_TIME = 1; //one hour
        const int POOLING = 5; //every 5 seconds

        [FunctionName(nameof(LongRunningWorkflow))]
        public static async Task<string> LongRunningWorkflow([OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var wait = context.GetInput<int>();

            //Long running operations
            var outputs = new List<string>
            {
                await context.CallActivityAsync<string>(nameof(LongRunningActivities.SayHello), wait),
                await context.CallActivityAsync<string>(nameof(LongRunningActivities.SayHello), wait * 2),
                await context.CallActivityAsync<string>(nameof(LongRunningActivities.SayHello), wait * 3)
            };            

            return string.Join("<br/>", outputs.ToArray());
        }

        [FunctionName(nameof(MonitorWorkflow))]
        public static async Task MonitorWorkflow([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var info = context.GetInput<Info>();

            await context.CallActivityAsync(nameof(SignalRActivities.SendUpdate), new Status { UserName = info.UserName, Message = $"Start monitoring for user {info.UserName}" });

            var expiryTime = context.CurrentUtcDateTime.AddHours(EXPIRY_TIME);

            var timeout = false;

            while (context.CurrentUtcDateTime < expiryTime)
            {
                timeout = false;

                var status = await context.CallActivityAsync<DurableOrchestrationStatus>(nameof(MonitorActivities.CheckWorkflowStatus), info.WorkflowId);

                if (status.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    await context.CallActivityAsync(nameof(SignalRActivities.SendUpdate), new Status { UserName = info.UserName, Message = $"Result: <br/> {status.Output}" });
                    await context.CallActivityAsync(nameof(SignalRActivities.SendUpdate), new Status { UserName = info.UserName, Message = $"Completed! It took {(context.CurrentUtcDateTime - status.CreatedTime).TotalMinutes} minutes." });
                    break;
                }
                else
                {
                    await context.CallActivityAsync(nameof(SignalRActivities.SendUpdate), new Status { UserName = info.UserName, Message = $"The call {status.Name}'s still running since {status.CreatedTime}. Last check time: {context.CurrentUtcDateTime}" });

                    var nextCheck = context.CurrentUtcDateTime.AddSeconds(POOLING);
                    await context.CreateTimer(nextCheck, CancellationToken.None);
                    timeout = true;
                }
            }

            if (timeout)
                await context.CallActivityAsync(nameof(SignalRActivities.SendUpdate), new Status { UserName = info.UserName, Message = "Time out!" });

        }
    }
}
