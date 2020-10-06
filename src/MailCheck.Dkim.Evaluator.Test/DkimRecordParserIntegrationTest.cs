using System.Collections.Generic;
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

namespace MailCheck.Dkim.Evaluator.Test
{
    [TestFixture(Category = "Integration")]
    public class DkimRecordParserIntegrationTest
    {
        [Test]
        public void TestEvaluation()
        {
            IDkimRecordParser dkimRecordParser = Create();

            //string record = "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCxBwakMBSvmNXD9UJvNVpP3X+QanFgeep6e89Yb5yVeHE7hs7UYglXEG2a2KEGnkOqxu7IfsHtbv3ibbZVSqk8OV9n58Gzl6uGs9MCQAa0JhIodRMzjF20PhxBhKBnSp9SX1E5RlgOplNF2Bat0+2ypb+TKmEgnMkjok9YXL8ddwIDAQAB; n=1024,1462785360,1";
            string record = "p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDIl6aF27aG0BCld/lgh2vC+1Mw55QdjrYKUsI/NksVASi9XQzh05yW87rlnSnmNg13k2E+gJaQL483/jUTGKwMWLlFUUQ8WwazUpw+xsuMdksHyp+RcHizR+reXNmNikJ3xbCKxaFW3er6MiZGRkf64zf87NwEvU8r47LGPcjK6wIDAQAB";

            DkimRecord dkimRecord = dkimRecordParser.Parse(new DnsRecord(record, new List<string>()));
        }

        private IDkimRecordParser Create()
        {
            return new ServiceCollection()
                .AddTransient<IDkimRecordParser, DkimRecordParser>()
                .AddTransient<IDkimSelectorRecordsParser, DkimSelectorRecordsParser>()
                .AddTransient<IDkimEvaluatorConfig, DkimEvaluatorConfig>()
                .AddTransient<IRsaPublicKeyEvaluator, RsaPublicKeyEvaluator>()
                .AddTransient<ITagParser, TagParser>()
                .AddTransient<ITagParserStrategy, PublicKeyTypeParserStrategy>()
                .AddTransient<ITagParserStrategy, FlagsParserStrategy>()
                .AddTransient<ITagParserStrategy, HashAlgorithmParserStrategy>()
                .AddTransient<ITagParserStrategy, NotesParserStrategy>()
                .AddTransient<ITagParserStrategy, PublicKeyDataParserStrategy>()
                .AddTransient<ITagParserStrategy, VersionParserStrategy>()
                .AddTransient<ITagParserStrategy, ServiceTypeParserStrategy>()
                .AddTransient<IEvaluator<DkimRecord>, Evaluator<DkimRecord>>()
                .AddTransient<IRule<DkimRecord>, SelectorShouldBeWellConfigured>()
                .AddTransient<IRule<DkimRecord>, TagShouldHavePublicKeyData>()
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
