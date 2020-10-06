using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test
{
    [TestFixture(Category = "Integration")]
    public class LambdaEntryPointTest
    {
        [Test]
        public void CanCreate()
        {
            System.Environment.SetEnvironmentVariable("TimeoutSqsSeconds", "10");
            System.Environment.SetEnvironmentVariable("SqsQueueUrl", "http://queue");
            System.Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "50");
            System.Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "50");
            System.Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "50");

            LambdaEntryPoint lambdaEntry = new LambdaEntryPoint();
            Assert.That(lambdaEntry, Is.Not.Null);
        }
    }
}
