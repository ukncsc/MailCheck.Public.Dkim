using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Scheduler.Config;
using MailCheck.Dkim.Scheduler.Dao;
using MailCheck.Dkim.Scheduler.Handler;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dkim.Scheduler.Startup
{
    public class StartupDkimDomainCreated : IStartUp
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
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IHandle<DkimEntityCreated>, DkimSchedulerHandler>()
                .AddTransient<IDkimSchedulerDao, DkimSchedulerDao>()
                .AddTransient<IDkimSchedulerConfig, DkimSchedulerConfig>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddEnvironment();
        }
    }
}
