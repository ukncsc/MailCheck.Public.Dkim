using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Scheduler.Config
{
    public interface IDkimSchedulerConfig
    {
        long RefreshIntervalSeconds { get; }
        int DomainsLimit { get; }
        string PublisherConnectionString { get; }
    }

    public class DkimSchedulerConfig : IDkimSchedulerConfig
    {
        public DkimSchedulerConfig(IEnvironmentVariables environmentVariables)
        {
            RefreshIntervalSeconds = environmentVariables.GetAsLong("RefreshIntervalSeconds");
            DomainsLimit = environmentVariables.GetAsInt("DomainsLimit");
            PublisherConnectionString = environmentVariables.Get("SnsTopicArn");
        }

        public long RefreshIntervalSeconds { get; }
        public int DomainsLimit { get; }
        public string PublisherConnectionString { get; }
    }
}
