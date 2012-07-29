namespace NServiceBus.Unicast.Queuing.OracleAdvancedQueuing
{
    using System;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
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

        /// <summary>
        /// Connection String to the service hosting the service broker
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets name of the table that backs the queue.
        /// </summary>
        public string QueueTable { get; set; }

        /// <summary>
        /// Gets or sets name of the Oracle queue.
        /// </summary>
        public string InputQueue { get; set; }

        /// <summary>
        /// Gets or sets value indicating how long we should wait for a message.
        /// </summary>
        public int SecondsToWaitForMessage { get; set; }

        public bool HasMessage()
        {
            return this.GetNumberOfPendingMessages() > 0;
        }

        public TransportMessage Receive()
        {
            return this.ReceiveFromQueue();
        }

        public void Init(Address address, bool transactional)
        {
        }

        private TransportMessage ReceiveFromQueue()
        {
            OracleAQDequeueOptions options = new OracleAQDequeueOptions
            {
                DequeueMode = OracleAQDequeueMode.Remove,
                Wait = this.SecondsToWaitForMessage,
                ProviderSpecificType = true
            };

            TransportMessage transportMessage = null;

            using (OracleConnection conn = new OracleConnection(this.ConnectionString))
            {
                OracleAQQueue queue = new OracleAQQueue(this.InputQueue, conn, OracleAQMessageType.Xml);
                OracleAQMessage aqMessage = queue.Dequeue(options);

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
            };

            Logger.DebugFormat("Received message from queue {0}", this.QueueTable);

            // Set the correlation Id
            if (string.IsNullOrEmpty(transportMessage.IdForCorrelation))
            {
                transportMessage.IdForCorrelation = transportMessage.Id;
            }

            return transportMessage;
        }

        private int GetNumberOfPendingMessages()
        {
            int count;

            string sql = string.Format(@"SELECT COUNT(*) FROM {0}", this.QueueTable);

            using (OracleConnection conn = new OracleConnection(this.ConnectionString))
            {
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                conn.Open();
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }

            Logger.DebugFormat("There are {0} messages in queue {1}", count, this.QueueTable);

            return count;
        }

        private TransportMessage ExtractTransportMessage(object payload)
        {
            OracleXmlType type = (OracleXmlType)payload;
            TransportMessage message = null;

            var xs = new XmlSerializer(typeof(TransportMessage));

            message = xs.Deserialize(type.GetXmlReader()) as TransportMessage;

            var bodyDoc = type.GetXmlDocument();

            var bodySection = bodyDoc.DocumentElement.SelectSingleNode("Body").FirstChild as XmlCDataSection;

            message.Body = Encoding.UTF8.GetBytes(bodySection.Data);

            return message;
        }
    }
}
