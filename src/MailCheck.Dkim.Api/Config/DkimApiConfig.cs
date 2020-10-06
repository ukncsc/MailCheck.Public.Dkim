using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Api.Config
{
    public interface IDkimApiConfig
    {
        string SnsTopicArn { get; }
        string MicroserviceOutputSnsTopicArn { get; }
    }

    public class DkimApiConfig : IDkimApiConfig
    {
        public DkimApiConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            MicroserviceOutputSnsTopicArn = environmentVariables.Get("MicroserviceOutputSnsTopicArn");
        }

        public string SnsTopicArn { get; }
        public string MicroserviceOutputSnsTopicArn { get; }
    }
}
