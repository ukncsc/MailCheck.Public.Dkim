using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Amazon.SimpleNotificationService;
using DnsClient;
using MailCheck.Common.Environment;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Poller.Config;
using MailCheck.Dkim.Poller.Dns;
using MailCheck.Dkim.Poller.Handlers;
using MailCheck.Dkim.Poller.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dkim.Poller.StartUp
{
    public class DkimPollerStartUp : IStartUp
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
                .AddTransient<IDkimDnsClient, DkimDnsClient>()
                .AddSingleton(CreateLookupClient)
                .AddTransient<IHandle<DkimPollPending>, DkimPollerHandler>()
                .AddTransient<IDnsNameServerProvider, DnsNameServerProvider>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IDkimPollerConfig, DkimPollerConfig>();
        }

        private static ILookupClient CreateLookupClient(IServiceProvider provider)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new LookupClient(NameServer.GooglePublicDns, NameServer.GooglePublicDnsIPv6)
                {
                    Timeout = provider.GetRequiredService<IDkimPollerConfig>().DnsRecordLookupTimeout
                }
                : new LookupClient(new LookupClientOptions(provider.GetService<IDnsNameServerProvider>()
                    .GetNameServers()
                    .Select(_ => new IPEndPoint(_, 53)).ToArray())
                {
                    ContinueOnEmptyResponse = false,
                    UseCache = false,
                    UseTcpOnly = true,
                    EnableAuditTrail = true,
                    Timeout = provider.GetRequiredService<IDkimPollerConfig>().DnsRecordLookupTimeout
                });
        }
    }
}
