﻿using MailCheck.Common.Messaging.Sqs;
using MailCheck.Dkim.Scheduler.Startup;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))] this is in DkimPeriodicSchedulerLambdaEntryPoint only required once per assembly
namespace MailCheck.Dkim.Scheduler
{
    public class DkimSelectorUpdateEventLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public DkimSelectorUpdateEventLambdaEntryPoint() : base(new StartupDkimDomainCreated())
        {
        }
    }
}