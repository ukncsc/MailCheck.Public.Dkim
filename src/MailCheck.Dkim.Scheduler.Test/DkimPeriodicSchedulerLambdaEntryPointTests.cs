using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace MailCheck.Dkim.Scheduler.Test
{
    [TestFixture]
    public class DkimPeriodicSchedulerLambdaEntryPointTests
    {
        [Test]
        public void CreateDkimPeriodicSchedulerLambdaEntryPoint()
        {
            Environment.SetEnvironmentVariable("RemainingTimeThresholdSeconds", "10");
            DkimPeriodicSchedulerLambdaEntryPoint entryPoint = new DkimPeriodicSchedulerLambdaEntryPoint();
            Assert.That(entryPoint, Is.Not.Null);
        }
    }
}
