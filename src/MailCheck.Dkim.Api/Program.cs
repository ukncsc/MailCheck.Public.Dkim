﻿using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MailCheck.Dkim.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseStartup<StartUp>()
                .UseHealthChecks("/healthcheck");

            if (RunInDevMode())
            {
                webHostBuilder.UseUrls("http://localhost:5004");
            }

            return webHostBuilder;
        }

        private static bool RunInDevMode()
        {
            bool.TryParse(Environment.GetEnvironmentVariable("DevMode"), out bool isDevMode);
            return isDevMode;
        }
    }
}
