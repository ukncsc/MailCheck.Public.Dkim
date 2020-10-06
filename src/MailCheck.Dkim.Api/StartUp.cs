using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using FluentValidation.AspNetCore;
using MailCheck.Common.Api.Authentication;
using MailCheck.Common.Api.Authorisation.Service;
using MailCheck.Common.Api.Middleware;
using MailCheck.Common.Api.Middleware.Audit;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Common.SSM;
using MailCheck.Common.Util;
using MailCheck.Dkim.Api.Config;
using MailCheck.Dkim.Api.Dao;
using MailCheck.Dkim.Api.Domain;
using MailCheck.Dkim.Api.Mapping;
using MailCheck.Dkim.Api.Service;
using MailCheck.Dkim.Api.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Dkim.Api
{
    public class StartUp
    {
        public StartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (RunInDevMode())
            {
                services.AddCors(CorsOptions);
            }

            services
                .AddHealthChecks(checks =>
                    checks.AddValueTaskCheck("HTTP Endpoint", () =>
                        new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("Ok"))))
                .AddEnvironment()
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IDkimApiDao, DkimApiApiDao>()
                .AddTransient<IDomainValidator, DomainValidator>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>()
                .AddTransient<IDkimApiConfig, DkimApiConfig>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IValidator<DkimInfoRequest>, DkimInfoRequestValidator>()
                .AddTransient<IEntityToApiMapper, EntityToApiMapper>()
                .AddTransient<IDkimService, DkimService>()
                .AddTransient<IClock, Clock>()
                .AddAudit("Dkim-Api")
                .AddMailCheckAuthenticationClaimsPrincipleClient()
                .AddSerilogLogging()
                .AddMvc(config =>
                {
                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                .AddFluentValidation();

            services
                .AddAuthorization()
                .AddAuthentication(AuthenticationSchemes.Claims)
                .AddMailCheckClaimsAuthentication();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (RunInDevMode())
            {
                app.UseCors(CorsPolicyName);
            }

            app.UseMiddleware<AuditTimerMiddleware>()
               .UseMiddleware<OidcHeadersToClaimsMiddleware>()
               .UseMiddleware<ApiKeyToClaimsMiddleware>()
               .UseAuthentication()
               .UseMiddleware<AuditLoggingMiddleware>()
               .UseMiddleware<UnhandledExceptionMiddleware>()
               .UseMvc();
        }

        private bool RunInDevMode()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("DevMode"), out bool isDevMode);
            return isDevMode;
        }

        private static Action<CorsOptions> CorsOptions => options =>
        {
            options.AddPolicy(CorsPolicyName, builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        };

        private const string CorsPolicyName = "CorsPolicy";
    }
}
