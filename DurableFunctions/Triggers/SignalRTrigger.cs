using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableFunctions.Triggers
{
    public static class SignalRTrigger
    {
        [FunctionName(nameof(Negotiate))]
        public static SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{userName}/Negotiate")] HttpRequest req, [SignalRConnectionInfo(HubName = "updates", UserId = "{userName}")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}
