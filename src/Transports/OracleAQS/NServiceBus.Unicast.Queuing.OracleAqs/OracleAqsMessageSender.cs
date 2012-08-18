namespace NServiceBus.Unicast.Queuing.OracleAqs
{
    using System.IO;
    using System.Text;
    using System.Transactions;
    using System.Xml;
    using System.Xml.Serialization;
    using NServiceBus.Unicast.Transport;
    using Oracle.DataAccess.Client;

    /// <summary>
    /// Sends a message via Oracle AQS
    /// </summary>
    public class OracleAqsMessageSender : ISendMessages
    {
        /// <summary>
        /// Gets or sets connection String to the service hosting the service broker
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// /// Sends the given message to the address.
        /// /// </summary>
        /// /// <param name="message">Message to send.</param>
        /// /// <param name="address">Message destination address.</param>
        public void Send(TransportMessage message, Address address)
        {
            using (OracleConnection conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                // Set the time from the source machine when the message was sent
                OracleAQQueue queue = new OracleAQQueue(OracleAqsUtilities.NormalizeQueueName(address), conn, OracleAQMessageType.Xml);
                queue.EnqueueOptions.Visibility = Transaction.Current == null ? OracleAQVisibilityMode.Immediate : OracleAQVisibilityMode.OnCommit;

                using (var stream = new MemoryStream())
                {
                    this.SerializeToXml(message, stream);
                    OracleAQMessage aqMessage = new OracleAQMessage(Encoding.UTF8.GetString(stream.ToArray()));
                    aqMessage.Correlation = message.CorrelationId;
                    queue.Enqueue(aqMessage);
                }
            }
        }

        private void SerializeToXml(TransportMessage transportMessage, MemoryStream stream)
        {
            var overrides = new XmlAttributeOverrides();
            var attrs = new XmlAttributes { XmlIgnore = true };

            overrides.Add(typeof(TransportMessage), "Messages", attrs);
            overrides.Add(typeof(TransportMessage), "Address", attrs);
            overrides.Add(typeof(TransportMessage), "ReplyToAddress", attrs);
            overrides.Add(typeof(TransportMessage), "Headers", attrs);
            overrides.Add(typeof(TransportMessage), "Body", attrs);
            var xs = new XmlSerializer(typeof(TransportMessage), overrides);

            var doc = new XmlDocument();

            using (var tempstream = new MemoryStream())
            {
                xs.Serialize(tempstream, transportMessage);
                tempstream.Position = 0;

                doc.Load(tempstream);
            }

            var data = transportMessage.Body != null ? Encoding.UTF8.GetString(transportMessage.Body) : string.Empty;

            var bodyElement = doc.CreateElement("Body");
            bodyElement.AppendChild(doc.CreateCDataSection(data));
            doc.DocumentElement.AppendChild(bodyElement);

            var headers = new SerializableDictionary<string, string>(transportMessage.Headers);

            var headerElement = doc.CreateElement("Headers");
            headerElement.InnerXml = headers.GetXml();
            doc.DocumentElement.AppendChild(headerElement);

            var replyToAddressElement = doc.CreateElement("ReplyToAddress");
            replyToAddressElement.InnerText = transportMessage.ReplyToAddress.ToString();
            doc.DocumentElement.AppendChild(replyToAddressElement);

            doc.Save(stream);
            stream.Position = 0;
        }
    }
}
