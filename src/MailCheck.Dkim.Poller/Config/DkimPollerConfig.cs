﻿using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Poller.Config
{
    public interface IDkimPollerConfig
    {
        string SnsTopicArn { get; }
        TimeSpan DnsRecordLookupTimeout { get; }
        string NameServer { get; }
    }

    public class DkimPollerConfig : IDkimPollerConfig
    {
        public DkimPollerConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            DnsRecordLookupTimeout = TimeSpan.FromSeconds(environmentVariables.GetAsLong("DnsRecordLookupTimeoutSeconds"));
            NameServer = environmentVariables.Get("NameServer", false);
        }

        public string SnsTopicArn { get; }
        public TimeSpan DnsRecordLookupTimeout { get; }
        public string NameServer { get; }
    }
}
