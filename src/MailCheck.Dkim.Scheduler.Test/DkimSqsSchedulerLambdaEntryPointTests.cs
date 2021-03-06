﻿using System;
using NUnit.Framework;

namespace MailCheck.Dkim.Scheduler.Test
{
    [TestFixture(Category = "Integration")]
    public class DkimSqsSchedulerLambdaEntryPointTests
    {
        [Test]
        public void CreateDkimSelectorUpdateEventLambdaEntryPoint()
        {
            Environment.SetEnvironmentVariable("RemainingTimeThresholdSeconds", "10");
            Environment.SetEnvironmentVariable("SqsQueueUrl", "http://");
            Environment.SetEnvironmentVariable("TimeoutSqsSeconds", "10");
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "50");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "50");
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "50");

            DkimSqsSchedulerLambdaEntryPoint entryPoint = new DkimSqsSchedulerLambdaEntryPoint();
            Assert.That(entryPoint, Is.Not.Null);
        }
    }
}
