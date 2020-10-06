using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Common.Messaging;
using MailCheck.Common.Messaging.Sqs;
using MailCheck.Dkim.Contracts.Entity;
using MailCheck.Dkim.Contracts.Scheduler;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;



namespace MailCheck.Dkim.Poller
{
    public static class LocalEntryPoint
    {
        static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(false) { Name = "DkimPoller" };

            commandLineApplication.OnExecute(() =>
            {
                RunLambda().ConfigureAwait(true).GetAwaiter().GetResult();
                return 0;
            });

            commandLineApplication.Command("lambda", command =>
            {
                command.Description = "Run DKIM poller lambda locally.";

                command.OnExecute(() =>
                {
                    DkimPollerLambdaEntryPoint lambdaEntryPoint = new DkimPollerLambdaEntryPoint();

                    while (true)
                    {
                        if (args.Length > 1)
                        {
                            //Expected args
                            //args[0] lambda
                            //args[1] domain e.g abc.com
                            //args[2] selector selector1

                            List<string> selectors = args.Length >= 3
                                ? args[2].Split(',').ToList()
                                : new List<string>();

                            lambdaEntryPoint.FunctionHandler(new SQSEvent
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
                                                new SQSEvent.MessageAttribute {StringValue = nameof(DkimPollPending)}
                                            }
                                        },
                                        Body = JsonConvert.SerializeObject(new DkimPollPending(
                                            args[1],
                                           0,selectors))
                                    }
                                }
                            }, null).GetAwaiter().GetResult();
                        }

                        Thread.Sleep(10000);
                    }

                    return 0;
                });

            }, false);

            commandLineApplication.Execute(args);
        }

        private static async Task RunLambda()
        {
            AmazonSQSClient client = new AmazonSQSClient(new EnvironmentVariablesAWSCredentials());
            LambdaEntryPoint lambdaEntryPoint = new LambdaEntryPoint();
            string queueUrl = Environment.GetEnvironmentVariable("SqsQueueUrl");

            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest(queueUrl)
            {
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 20,
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" },
            };

            while (true)
            {
                Console.WriteLine($"Polling {queueUrl} for messages...");
                ReceiveMessageResponse receiveMessageResponse = await client.ReceiveMessageAsync(receiveMessageRequest);
                Console.WriteLine($"Received {receiveMessageResponse.Messages.Count} messages from {queueUrl}.");

                if (receiveMessageResponse.Messages.Any())
                {
                    try
                    {
                        Console.WriteLine($"Running Lambda...");
                        SQSEvent sqsEvent = receiveMessageResponse.Messages.ToSqsEvent();
                        await lambdaEntryPoint.FunctionHandler(sqsEvent,
                            LambdaContext.NonExpiringLambda);

                        Console.WriteLine($"Lambda completed");

                        Console.WriteLine($"Deleting messages...");
                        await client.DeleteMessageBatchAsync(queueUrl,
                            sqsEvent.Records.Select(_ => new DeleteMessageBatchRequestEntry
                            {
                                Id = _.MessageId,
                                ReceiptHandle = _.ReceiptHandle
                            }).ToList());

                        Console.WriteLine($"Deleted messages.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"An error occured running lambda {e.Message} {Environment.NewLine} {e.StackTrace}");
                    }
                }
            }
        }

        private static SQSEvent ToSqsEvent(this IEnumerable<Message> messages)
        {
            return new SQSEvent
            {
                Records = messages.Select(_ =>
                    new SQSEvent.SQSMessage
                    {
                        MessageAttributes = _.MessageAttributes.ToDictionary(a => a.Key, a =>
                            new SQSEvent.MessageAttribute
                            {
                                StringValue = a.Value.StringValue
                            }),
                        Attributes = _.Attributes,
                        Body = _.Body,
                        MessageId = _.MessageId,
                        ReceiptHandle = _.ReceiptHandle
                    }).ToList()
            };
        }
    }

    public class LambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public LambdaEntryPoint() : base(new StartUp.DkimPollerStartUp())
        {
        }
    }
}
