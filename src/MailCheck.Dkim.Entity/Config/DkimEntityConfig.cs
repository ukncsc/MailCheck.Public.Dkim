using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Entity.Config
{
    public interface IDkimEntityConfig
    {
        string SnsTopicArn { get; }
        string WebUrl { get; }
    }

    public class DkimEntityConfig : IDkimEntityConfig
    {
        public DkimEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            WebUrl = environmentVariables.Get("WebUrl");
        }

        public string SnsTopicArn { get; }
        public string WebUrl { get; }
    }
}