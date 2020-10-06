using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dkim.Evaluator.Config
{
    public interface IDkimEvaluatorConfig
    {
        string SnsTopicArn { get; }
    }
    public class DkimEvaluatorConfig : IDkimEvaluatorConfig
    {
        public DkimEvaluatorConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}
