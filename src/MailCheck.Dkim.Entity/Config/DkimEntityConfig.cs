using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Entity.Config
{
    public interface IDkimEntityConfig
    {
        string SnsTopicArn { get; }
    }

    public class DkimEntityConfig : IDkimEntityConfig
    {
        public DkimEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}