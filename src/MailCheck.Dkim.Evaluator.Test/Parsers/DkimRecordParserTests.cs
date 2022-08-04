using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MailCheck.Dkim.Contracts.Evaluator.Domain;
using MailCheck.Dkim.Contracts.SharedDomain;
using MailCheck.Dkim.Evaluator.Config;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Implicit;
using MailCheck.Dkim.Evaluator.Parsers;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rsa;
using MailCheck.Dkim.Evaluator.Rules;
using MailCheck.Dkim.Evaluator.Rules.Record;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using DkimRecord = MailCheck.Dkim.Evaluator.Domain.DkimRecord;

namespace MailCheck.Dkim.Evaluator.Test.Parsers
{
    [TestFixture]
    public class DkimRecordParserTests
    {
        [Test]
        public void ParseWithPublicKeyDataTagMissing()
        {
            IDkimRecordParser parser = Create();
            string record = "v=DKIM1;"
                            + "k=rsa;"
                            + "n=hello world;"
                            + "s=*;"
                            + "h=sha256;"
                            + "t=y";

            DkimRecord dkimRecord = parser.Parse("selector1", new DnsRecord(record, new List<string>()), null);

            Assert.That(dkimRecord.Errors.Count, Is.EqualTo(1));
            Assert.That(dkimRecord.Errors.First().ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(dkimRecord.Errors.First().Id, Is.EqualTo(Guid.Parse("f3895ef6-8709-499a-8c5c-40118a818087")));
        }

        [Test]
        public void ParseWithEd25519KeyDkimRecord()
        {
            IDkimRecordParser parser = Create();
            string record = "v=DKIM1;"
                            + "k=ed25519;"
                            + "n=hello world;"
                            + "s=*;"
                            + "h=sha256;"
                            + "t=y;"
                            + "p=pEd4PDHp7GcYy1yc/0ZYw/aidpwIUPL2Z4433ZnuWoA=";

            DkimRecord dkimRecord = parser.Parse("selector1", new DnsRecord(record, new List<string>()), null);

            Assert.That(dkimRecord.Errors, Is.Empty);
        }

        [Test]
        public void ParseWithRsa1985KeyDkimRecord()
        {
            IRsaPublicKeyEvaluator rsaPublicKeyEvaluator = A.Fake<IRsaPublicKeyEvaluator>();
            int anOut;
            A.CallTo(() => rsaPublicKeyEvaluator.TryGetKeyLengthSize(A<string>._, out anOut)).Returns(true).AssignsOutAndRefParameters(1985);

            IDkimRecordParser parser = Create(rsaPublicKeyEvaluator);
            string record = "v=DKIM1;"
                            + "k=rsa;"
                            + "n=hello world;"
                            + "s=*;"
                            + "h=sha256;"
                            + "t=y;"
                            + "p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvtzV5FCafF4GWCsuV0qhyDpV8pz7QrFxF8JABDl+kewpUIhTRujuKQbeAIuPzbiTe0djy1tPIyzEASChfPpSg7C9RrJlKlluyYN1H4ORJQsnAuwAy8+Eur+Zjo1o6xItxsac9RUvl4eCm8ZSkhtdu1HMRV0+rq2S1+E0cqwyO7xw2alhLRmu96Dgy2j4dz/8teguAUEaNGQECcd7h7DM" +
                            "8vo8IZgyJHJdaMQHX7zhuRUP8fjsrD5YUfOsm/kzSJfSKEk9cXwKk8p5QoiHG+W4/tc/nyl+uqz9fUF/K0d9qCvHE31mu3to2GCakABPU9XpczhIwYkGB6ASdd53PKhXJwIDAQAB";

            DkimRecord dkimRecord = parser.Parse("selector1", new DnsRecord(record, new List<string>()), null);

            Assert.That(dkimRecord.Errors.Count, Is.EqualTo(1));
            Assert.That(dkimRecord.Errors.First().ErrorType, Is.EqualTo(EvaluationErrorType.Warning));
            Assert.That(dkimRecord.Errors.First().Id, Is.EqualTo(Guid.Parse("d16be8d5-8ed6-4dd0-a8aa-c8f1380a8d3c")));
        }

        [Test]
        public void ParseWithRsa2048KeyDkimRecord()
        {
            IDkimRecordParser parser = Create();
            string record = "v=DKIM1;"
                            + "k=rsa;"
                            + "n=hello world;"
                            + "s=*;"
                            + "h=sha256;"
                            + "t=y;"
                            + "p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvtzV5FCafF4GWCsuV0qhyDpV8pz7QrFxF8JABDl+kewpUIhTRujuKQbeAIuPzbiTe0djy1tPIyzEASChfPpSg7C9RrJlKlluyYN1H4ORJQsnAuwAy8+Eur+Zjo1o6xItxsac9RUvl4eCm8ZSkhtdu1HMRV0+rq2S1+E0cqwyO7xw2alhLRmu96Dgy2j4dz/8teguAUEaNGQECcd7h7DM" +
                            "8vo8IZgyJHJdaMQHX7zhuRUP8fjsrD5YUfOsm/kzSJfSKEk9cXwKk8p5QoiHG+W4/tc/nyl+uqz9fUF/K0d9qCvHE31mu3to2GCakABPU9XpczhIwYkGB6ASdd53PKhXJwIDAQAB";

            DkimRecord dkimRecord = parser.Parse("selector1", new DnsRecord(record, new List<string>()), null);

            Assert.That(dkimRecord.Errors.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParseWithRsa1024KeyDkimRecordShouldError()
        {
            string selector = "selector1";

            string expectedTitle = $"Selector {selector}. We recommend a longer key size for this selector.";

            string expectedMarkdown = "We have detected that you are using a 1024-bit public rsa key. We recommend a stronger key size of 2048-bits. " + Environment.NewLine +
                Environment.NewLine + "For further information about setting up and maintaining DKIM please refer to our guidance:  [Create and manage a DKIM record](https://www.ncsc.gov.uk/collection/email-security-and-anti-spoofing/configure-anti-spoofing-controls-/create-and-manage-a-dkim-record)";

            IDkimRecordParser parser = Create();
            string record = "v=DKIM1;"
                            + "k=rsa;"
                            + "n=hello world;"
                            + "s=*;"
                            + "h=sha256;"
                            + "t=y;"
                            + "p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCDsRhw52Ldz6Cz4LYdSfDdCGi1x+SmR/xT+6PhGzvZfEgQN8SzRrQBnvmTW/Rizuivul+iq4bbS3Dc4S0ZoDkdG30jr4NRK35lGcDRWUV3XYsgX8Y5FzTFP73dbO9vHv3UVxmH0/giIOZ2j4xzIad7VHjn2AvDUafv2UkIwk/qlQIDAQAB";

            DkimRecord dkimRecord = parser.Parse("selector1", new DnsRecord(record, new List<string>()), null);

            Assert.That(dkimRecord.Errors.Count, Is.EqualTo(1));
            Assert.That(dkimRecord.Errors.First().ErrorType, Is.EqualTo(EvaluationErrorType.Warning));
            Assert.That(dkimRecord.Errors.First().Id, Is.EqualTo(Guid.Parse("d16be8d5-8ed6-4dd0-a8aa-c8f1380a8d3c")));
            Assert.That(dkimRecord.Errors.First().Message, Is.EqualTo(expectedTitle));
            Assert.That(dkimRecord.Errors.First().MarkDown, Is.EqualTo(expectedMarkdown));
        }

        [Test]
        public void ParseWithCorruptPublicKeyDkimRecordShouldError()
        {
            IDkimRecordParser parser = Create();
            string record = "v=DKIM1;"
                            + "k=rsa;"
                            + "n=hello world;"
                            + "s=*;"
                            + "h=sha256;"
                            + "t=y;"
                            + "p=ABCfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCDsRhw52Ldz6Cz4LYdSfDdCGi1x+SmR/xT+6PhGzvZfEgQN8SzRrQBnvmTW/Rizuivul+iq4bbS3Dc4S0ZoDkdG30jr4NRK35lGcDRWUV3XYsgX8Y5FzTFP73dbO9vHv3UVxmH0/giIOZ2j4xzIad7VHjn2AvDUafv2UkIwk/qlQIDAQAB";

            DkimRecord dkimRecord = parser.Parse("selector1", new DnsRecord(record, new List<string>()), null);

            Assert.That(dkimRecord.Errors.Count, Is.EqualTo(1));
            Assert.That(dkimRecord.Errors.First().ErrorType, Is.EqualTo(EvaluationErrorType.Error));
            Assert.That(dkimRecord.Errors.First().Id, Is.EqualTo(Guid.Parse("21B09D43-685C-4CAE-989E-7194CA093863")));
        }

        private IDkimRecordParser Create(IRsaPublicKeyEvaluator rsaPublicKeyEvaluator = null)
        {
            return new ServiceCollection()
                .AddTransient<IDkimRecordParser, DkimRecordParser>()
                .AddTransient<IDkimSelectorRecordsParser, DkimSelectorRecordsParser>()
                .AddTransient<IDkimEvaluatorConfig, DkimEvaluatorConfig>()
                .AddTransient(_ => rsaPublicKeyEvaluator ?? new RsaPublicKeyEvaluator())
                .AddTransient<ITagParser, TagParser>()
                .AddTransient<ITagParserStrategy, PublicKeyTypeParserStrategy>()
                .AddTransient<ITagParserStrategy, FlagsParserStrategy>()
                .AddTransient<ITagParserStrategy, HashAlgorithmParserStrategy>()
                .AddTransient<ITagParserStrategy, NotesParserStrategy>()
                .AddTransient<ITagParserStrategy, PublicKeyDataParserStrategy>()
                .AddTransient<ITagParserStrategy, VersionParserStrategy>()
                .AddTransient<ITagParserStrategy, ServiceTypeParserStrategy>()
                .AddTransient<IEvaluator<DkimEvaluationObject>, Evaluator<DkimEvaluationObject>>()
                .AddTransient<IRule<DkimEvaluationObject>, SelectorShouldBeWellConfigured>()
                .AddTransient<IRule<DkimEvaluationObject>, TagShouldHavePublicKeyData>()
                .AddTransient<IImplicitProvider<Tag>, ImplicitProvider<Tag>>()
                .AddTransient<IImplicitProviderStrategy<Tag>, FlagTypeImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, HashAlgorithImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, NotesImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, PublicKeyTypeImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, ServiceTypeImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, FlagTypeImplicitProvider>()
                .BuildServiceProvider()
                .GetRequiredService<IDkimRecordParser>();
        }
    }
}
