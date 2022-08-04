using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Poller;
using MailCheck.Dkim.Evaluator.Config;
using MailCheck.Dkim.Evaluator.Domain;
using MailCheck.Dkim.Evaluator.Handler;
using MailCheck.Dkim.Evaluator.Implicit;
using MailCheck.Dkim.Evaluator.Parsers;
using MailCheck.Dkim.Evaluator.Parsers.Strategy;
using MailCheck.Dkim.Evaluator.Rsa;
using MailCheck.Dkim.Evaluator.Rules;
using MailCheck.Dkim.Evaluator.Rules.Record;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using DkimRecord = MailCheck.Dkim.Evaluator.Domain.DkimRecord;

namespace MailCheck.Dkim.Evaluator.StartUp
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                serializerSetting.Converters.Add(new StringEnumConverter());

                return serializerSetting;
            };

            services
                .AddEnvironment()
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
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
                .AddTransient<IEvaluator<DkimEvaluationObject>, Evaluator<DkimEvaluationObject>>()
                .AddTransient<IRule<DkimEvaluationObject>, SelectorShouldBeWellConfigured>()
                .AddTransient<IRule<DkimEvaluationObject>, TagShouldHavePublicKeyData>()
                .AddTransient<IHandle<DkimRecordsPolled>, DkimSelectorHandler>()
                .AddTransient<IImplicitProvider<Tag>, ImplicitProvider<Tag>>()
                .AddTransient<IImplicitProviderStrategy<Tag>, FlagTypeImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, HashAlgorithImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, NotesImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, PublicKeyTypeImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, ServiceTypeImplicitProvider>()
                .AddTransient<IImplicitProviderStrategy<Tag>, FlagTypeImplicitProvider>();
        }
    }
}