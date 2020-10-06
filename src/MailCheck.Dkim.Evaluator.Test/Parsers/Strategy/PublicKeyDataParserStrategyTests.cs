using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rules;
using NUnit.Framework;

namespace MailCheck.Dkim.Evaluator.Test.Parsers.Strategy
{
    [TestFixture]
    public class PublicKeyDataParserStrategyTests
    {
        [TestCase(null, null, null, TestName = "null is valid value (key revoked)")]
        [TestCase("", null, null, TestName = "empty string is valid value (key revoked)")]
        [TestCase("NOT BASE 64 STRING !!!", "Invalid value 'NOT BASE 64 STRING !!!' for p", EvaluationErrorType.Error, TestName = "non base 64 string invalid")]
        [TestCase("P7gfMA0GCSqGSIb7fQEBAQUAA4yNjDCjiQKjgQCx8wakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2K8GnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB", null, null, TestName = "base 64 string is valid")]
        [TestCase("5bbBbDkNBgkqhkbG1w0BkQEFkkOCkQ0k5bbBCkKCkQEknLs2kkRUbd1bWSo65kFgdkp12T1LXkv1bjtNTjfS55zV2QCKf/8eJjS5r8cbV5J55Fjbrebj72Q8k7Jbeb5et7zF11Uk+2j1kk51j5j5bCJ8cU+6LkXQ2Tphh15b8sL515tKKn03/QfeQsBFNCkK1z5vF0dNWChkQE7rtZSZkVbqlkku2XJbhnOfrkuxkkbRj+TB5EgSbGDxoE/dx5vkC6rBxQ5HLkEkXl5EJz2FwRNbkKtk11PToKQlbfpKF0z1g/GRbubPDpbfGcb5xSKkeE0Ojcu2b0bk7kXBkEo855q5q6xP23OdkkFrk6e8cRQ8RfVQXSNpsXtp1Do5/LjfUQbBJQ==", null, null, TestName = "base 64 string is valid with ==")]
        [TestCase("P7gfMA0GCSqGS\r\n Ib7fQEBAQUAA4yNjDCjiQKjgQCx8wakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2K8GnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB", null, null, TestName = "carriage return followed by space valid")]
        [TestCase("P7gfMA0GCSqGS Ib7fQEBAQUAA4yNjDCjiQKjgQCx8wakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2K8GnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB", null, null, TestName = "spaces valid")]
        [TestCase("P7gfMA0GCSqGS\tIb7fQEBAQUAA4yNjDCjiQKjgQCx8wakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2K8GnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB", null, null, TestName = "spaces valid")]
        [TestCase("P7gfMA0GCSqGS\t \t Ib7fQEBAQUAA4yNjDCjiQKjgQCx8wakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2K8GnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB", null, null, TestName = "tabs and spaces valid")]
        [TestCase("P7gfMA0GCSqGS\r\nIb7fQEBAQUAA4yNjDCjiQKjgQCx8wakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2K8GnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB", "Invalid value 'P7gfMA0GCSqGS", EvaluationErrorType.Error, TestName = "carriage return not followed by space invalid")]
        public void PublicKeyDataParserStrategy(string tokenValue, string errorMessageStartsWith, EvaluationErrorType? type)
        {
            PublicKeyDataParserStrategy publicKeyDataParserStrategy = new PublicKeyDataParserStrategy();

            EvaluationResult<Tag> publicKeyData = publicKeyDataParserStrategy.Parse(tokenValue);

            if (errorMessageStartsWith == null)
            {
                Assert.That(publicKeyData.Errors, Is.Empty);
                Assert.That(publicKeyData.Item.Value, Is.EqualTo(tokenValue));
            }
            else
            {
                Assert.That(publicKeyData.Errors.Count, Is.EqualTo(1));
                Assert.That(publicKeyData.Errors[0].Message.StartsWith(errorMessageStartsWith), Is.EqualTo(true));
                Assert.That(publicKeyData.Errors[0].ErrorType, Is.EqualTo(type));
                Assert.That(publicKeyData.Item.Value, Is.EqualTo(tokenValue));
            }
        }
    }
}
