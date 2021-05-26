using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment;
using MailCheck.Common.Environment.FeatureManagement;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Processor;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dkim.Scheduler.Startup
{
    public class StartUpDkimPollScheduler : IStartUp
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
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IProcess, DkimPollSchedulerProcessor>()
                .AddTransient<IDkimSchedulerConfig, DkimSchedulerConfig>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddEnvironment()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IDatabase, DefaultDatabase<MySqlProvider>>()
                .AddTransient<IClock, Clock>()
                .AddTransient<IDkimPeriodicSchedulerDao, DkimPeriodicSchedulerDao>();
                
        }
    }
}
