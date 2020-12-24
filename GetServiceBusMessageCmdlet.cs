using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Xml.Linq;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace AzurePowerShellCmdlets
{
    [Cmdlet(VerbsCommon.Get, "ServiceBusMessage")]
    public class GetServiceBusMessageCmdlet : Cmdlet
    {
        [Parameter(Position = 0, HelpMessage = "The connection string which has access to the Service Bus entity. Note that you may need to add `;TransportType=AmqpWebSockets` to the string if the TLS AMQP port is blocked on your system.", Mandatory = true)]
        public string connectionString;

        [Parameter(Position = 1, HelpMessage = "The Service Bus topic from which messages are to be fetched.", Mandatory = true)]
        public string topicName;

        [Parameter(Position = 2, HelpMessage = "The Service Bus subscription from which messages are to be fetched.", Mandatory = true)]
        public string subscriptionName;

        [Parameter(Position = 3, HelpMessage = "Content Type to expect", Mandatory = false)]
        [ValidateSet("application/json", "application/xml", "text/plain")]
        public string contentType;

        private dynamic _parsedObject;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            _ReceiveMessagesAsync().GetAwaiter().GetResult();
            WriteObject(this._parsedObject);
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
            Environment.Exit(1);
        }

        private async Task _ReceiveMessagesAsync()
        {
            string subscriptionPath = EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName);
            IMessageReceiver subscriptionReceiver = new MessageReceiver(connectionString, subscriptionPath, ReceiveMode.ReceiveAndDelete);
            Console.WriteLine("==========================================================================");
            while (true)
            {
                Console.WriteLine($"{DateTime.Now} :: Polling for messages in {topicName}/{subscriptionName}");
                var receivedMessage = await subscriptionReceiver.ReceiveAsync();
                if (receivedMessage != null)
                {
                    if (receivedMessage.Body != null && receivedMessage.Body.Length > 0)
                    {
                        string decodedBody = Encoding.UTF8.GetString(receivedMessage.Body);
                        if (String.IsNullOrEmpty(contentType))
                        {
                            Console.WriteLine("Message ContentType not specified and automatically resolved to JSON");
                            this._parsedObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(decodedBody);
                            break;
                        }
                        string contentTypeLower = contentType.ToLower();
                        if (!receivedMessage.ContentType.Equals(contentTypeLower))
                        {
                            Console.WriteLine("Message ContentType did not match.");
                        }
                        else if (contentTypeLower.Equals("application/json"))
                        {
                            Console.WriteLine("Message ContentType is JSON");
                            this._parsedObject = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(decodedBody);
                        }
                        else if (contentTypeLower.Equals("application/xml"))
                        {
                            Console.WriteLine("Message ContentType is XML");
                            XDocument doc = XDocument.Parse(decodedBody);
                            string jsonText = JsonConvert.SerializeXNode(doc);
                            this._parsedObject = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
                        }
                        else if (contentTypeLower.Equals("text/plain"))
                        {
                            Console.WriteLine("Message ContentType is Plain");
                            this._parsedObject = decodedBody;
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Message didn't match format and is deleted.");
                    }
                    // If we reach here, it means we would have deleted a message which didn't conform with the guidelines, as expected.
                }
                Thread.Sleep(10000); // Sleep for 10 seconds
            }
        }
    }
}
