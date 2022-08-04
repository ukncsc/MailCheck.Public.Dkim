using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dkim.Entity.Config;
using MailCheck.Dkim.Entity.Dao;
using MailCheck.Dkim.Entity.Entity;
using MailCheck.Dkim.Entity.Entity.DomainStatus;
using MailCheck.Dkim.Entity.Entity.Notifications;
using MailCheck.Dkim.Entity.Entity.Notifiers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using MailCheck.Common.Processors.Notifiers;
using FindingsChangedNotifier = MailCheck.Common.Processors.Notifiers.FindingsChangedNotifier;
using LocalFindingsChangedNotifier = MailCheck.Dkim.Entity.Entity.Notifiers.FindingsChangedNotifier;

namespace MailCheck.Dkim.Entity.StartUp
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
                .AddTransient<DkimEntity>()
                .AddEnvironment()
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IDkimEntityConfig, DkimEntityConfig>()
                .AddTransient<IDkimEntityDao, DkimEntityDao>()
                .AddTransient<IChangeNotifier, RecordChangedNotifier>()
                .AddTransient<IChangeNotifier, AdvisoryChangedNotifier>()
                .AddTransient<IChangeNotifiersComposite, ChangeNotifiersComposite>()
                .AddTransient<IEqualityComparer<SelectorRecord>, SelectorRecordEqualityComparer>()
                .AddTransient<IEqualityComparer<SelectorMessage>, SelectorMessageEqualityComparer>()
                .AddTransient<IChangeNotifier, LocalFindingsChangedNotifier>()
                .AddTransient<IFindingsChangedNotifier, FindingsChangedNotifier>()
                .AddTransient<IChangeNotifiersComposite, ChangeNotifiersComposite>()
                .AddTransient<IDomainStatusEvaluator, DomainStatusEvaluator>()
                .AddTransient<IDomainStatusPublisher, DomainStatusPublisher>()
                .AddTransient<IClock, Clock>();
        }
    }
}