using Amazon.Lambda.Core;
using MailCheck.Common.Messaging.CloudWatch;
using MailCheck.Dkim.Scheduler.Startup;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace MailCheck.Dkim.Scheduler
{
    public class DkimPeriodicSchedulerLambdaEntryPoint : CloudWatchTriggeredLambdaEntryPoint
    {
        public DkimPeriodicSchedulerLambdaEntryPoint() : base(new StartUpDkimPollScheduler())
        {
        }
    }
}
