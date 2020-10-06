using Amazon.Lambda.Core;
using MailCheck.Common.Messaging.Sqs;
using MailCheck.Dkim.Poller.StartUp;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace MailCheck.Dkim.Poller
{
    public class DkimPollerLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public DkimPollerLambdaEntryPoint() : base(new DkimPollerStartUp()) { }
    }
}
