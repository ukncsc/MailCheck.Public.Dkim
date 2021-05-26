using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.Lambda.SQSEvents;
using MailCheck.Common.Messaging;
using MailCheck.Dkim.Contracts;
using MailCheck.Dkim.Contracts.Entity;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace MailCheck.Dkim.Scheduler
{
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(false) {Name = "Dkim"};

            commandLineApplication.Command("lambda", command =>
            {
                command.Description = "Schedule Dkim processor lambda code locally.";

                command.OnExecute(async () =>
                {
                    DkimSqsSchedulerLambdaEntryPoint selectorUpdateEventLambdaEntryPoint = new DkimSqsSchedulerLambdaEntryPoint();
                    DkimPeriodicSchedulerLambdaEntryPoint periodicSchedulerLambdaEntryPoint = new DkimPeriodicSchedulerLambdaEntryPoint();

                    while (true)
                    {
                        /*
                         * example of adding domain created locally
                         *
                         * Only domain:
                         *
                         * lambda abc.com
                         *  
                         * With selectors:
                         *
                         * lambda abc.com "selector, selector2" 
                         *
                         * With selectors and data:
                         *
                         * lambda abc.com "selector, selector2" "2018-07-11T09:01:18"
                         */
                        if (args.Length > 1)
                        {
                            List<string> selectors = args.Length >= 3
                                ? args[2].Split(',').ToList()
                                : new List<string>();

                            DateTime lastUpdated = args.Length >= 4
                                ? DateTime.Parse(args[3])
                                : DateTime.UtcNow;

                            selectorUpdateEventLambdaEntryPoint.FunctionHandler(new SQSEvent
                            {
                                Records = new List<SQSEvent.SQSMessage>
                                {
                                    new SQSEvent.SQSMessage
                                    {
                                        Attributes = new Dictionary<string, string>
                                        {
                                            { "MessageId", Guid.NewGuid().ToString()},
                                            { "SentTimestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
                                        },
                                        MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
                                        {
                                            {
                                                "Type",
                                                new SQSEvent.MessageAttribute {StringValue = nameof(DkimSelectorsUpdated)}
                                            }
                                        },
                                        Body = JsonConvert.SerializeObject(new DkimSelectorsUpdated(args[1], 1, selectors))
                                    }
                                }
                            }, null).GetAwaiter().GetResult();
                        }
                        else
                        {
                            await periodicSchedulerLambdaEntryPoint.FunctionHandler(null, LambdaContext.NonExpiringLambda);
                        }

                        Thread.Sleep(10000);
                    }
                });

            }, false);

            commandLineApplication.Execute(args);
        }
    }
}