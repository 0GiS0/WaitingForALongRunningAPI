using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading;

namespace DurableFunctions.Activities
{
    public static class LongRunningActivities
    {

        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] int wait)
        {
            Thread.Sleep(wait);

            return $"Hello me with {wait / 60000} minutes of delay";
        }
    }
}
