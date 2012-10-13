namespace NServiceBus.Unicast.Queuing.OracleAqs
{
    using System;
    using System.Text;
    using System.Xml;
    using log4net;
    using NServiceBus.Unicast.Transport;
    using Oracle.DataAccess.Client;
    using Oracle.DataAccess.Types;

    /// <summary>
    /// Receives message from queue via Oracle AQS. 
    /// </summary>
    /// <remarks>
    /// Requires Oracle 10g or above and ODP.NET 11g or above.
    /// Credits goes to everyone who has worked on NSB and Joseph Daigle/Andreas Ohlund
    /// who created the Service Broker transport this is based off of.
    /// </remarks>
    public class OracleAqsMessageReceiver : IReceiveMessages
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(OracleAqsMessageReceiver));

        private string inputQueueAddress;
        private string queueTable;

        public OracleAqsMessageReceiver()
        {
            this.SecondsToWaitForMessage = 1;
        }

        /// <summary>
        /// Gets or sets connection string to the service hosting the service broker
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets value indicating how long we should wait for a message.
        /// </summary>
        public int SecondsToWaitForMessage { get; set; }

        /// <summary>
        /// Returns true if there's a message ready to be received at the address passed in the Init method.
        /// </summary>
        /// <returns></returns>
        public bool HasMessage()
        {
            return true;
        }

        /// <summary>
        /// Tries to receive a message from the address passed in Init.
        /// </summary>
        /// <returns>The first transport message available. If no message is present null will be returned.</returns>
        public TransportMessage Receive()
        {
            return this.ReceiveFromQueue();
        }

        /// <summary>
        /// Initializes the message receiver.
        /// </summary>
        /// <param name="address">The address of the message source.</param>
        /// <param name="transactional">Indicates if the receiver should be transactional.</param>
        public void Init(Address address, bool transactional)
        {
            this.inputQueueAddress = OracleAqsUtilities.NormalizeQueueName(address);

            using (OracleConnection conn = new OracleConnection(this.ConnectionString))
            {
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT queue_table FROM dba_queues WHERE name = :queue";
                cmd.Parameters.Add("queue", this.inputQueueAddress);
                conn.Open();
                this.queueTable = cmd.ExecuteScalar() as string;
            }
        }

        private TransportMessage ReceiveFromQueue()
        {
            OracleAQDequeueOptions options = new OracleAQDequeueOptions
            {
                DequeueMode = OracleAQDequeueMode.Remove,
                Wait = this.SecondsToWaitForMessage,
                ProviderSpecificType = true,
            };

            TransportMessage transportMessage;

            using (OracleConnection conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();
                OracleAQQueue queue = new OracleAQQueue(this.inputQueueAddress, conn, OracleAQMessageType.Xml);
                OracleAQMessage aqMessage = null;
                try
                {
                    aqMessage = queue.Dequeue(options);
                }
                catch (OracleException ex)
                {
                    if (ex.Number != 25228)
                    {
                        throw;
                    }
                }

                // No message? That's okay
                if (null == aqMessage)
                {
                    return null;
                }

                Guid messageGuid = new Guid(aqMessage.MessageId);

                // the serialization has to go here since Oracle needs an open connection to 
                // grab the payload from the message
                transportMessage = this.ExtractTransportMessage(aqMessage.Payload);
                transportMessage.Id = messageGuid.ToString();
                transportMessage.IdForCorrelation = aqMessage.Correlation;
            }

            Logger.DebugFormat("Received message from queue {0}", this.inputQueueAddress);

            // Set the correlation Id
            if (string.IsNullOrEmpty(transportMessage.IdForCorrelation))
            {
                transportMessage.IdForCorrelation = transportMessage.Id;
            }

            return transportMessage;
        }

        private TransportMessage ExtractTransportMessage(object payload)
        {
            OracleXmlType type = (OracleXmlType)payload;

            var bodyDoc = type.GetXmlDocument();

            var bodySection = bodyDoc.DocumentElement.SelectSingleNode("Body").FirstChild as XmlCDataSection;

            var headerDictionary = new SerializableDictionary<string, string>();
            var headerSection = bodyDoc.DocumentElement.SelectSingleNode("Headers");
            if (headerSection != null)
            {
                headerDictionary.SetXml(headerSection.InnerXml);
            }

            Address replyToAddress = Address.Undefined;
            var replyToAddressSection = bodyDoc.DocumentElement.SelectSingleNode("ReplyToAddress");
            if (replyToAddressSection != null)
            {
                replyToAddress = Address.Parse(replyToAddressSection.InnerText);
            }

            MessageIntentEnum messageIntent = default(MessageIntentEnum);
            var messageIntentSection = bodyDoc.DocumentElement.SelectSingleNode("MessageIntent");
            if (messageIntentSection != null)
            {
                messageIntent = (MessageIntentEnum)Enum.Parse(typeof(MessageIntentEnum), messageIntentSection.InnerText);
            }

            TransportMessage message = new TransportMessage
            {
                Body = Encoding.UTF8.GetBytes(bodySection.Data),
                Headers = headerDictionary,
                ReplyToAddress = replyToAddress,
                MessageIntent = messageIntent,
            };

            return message;
        }
    }
}
