namespace NServiceBus.Unicast.Queuing.OracleAqs
{
    using System.IO;
    using System.Text;
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
                OracleAQQueue queue = new OracleAQQueue(address.Queue, conn, OracleAQMessageType.Xml);
                queue.EnqueueOptions.Visibility = OracleAQVisibilityMode.Immediate;

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

            var data = Encoding.UTF8.GetString(transportMessage.Body);

            var bodyElement = doc.CreateElement("Body");
            bodyElement.AppendChild(doc.CreateCDataSection(data));
            doc.DocumentElement.AppendChild(bodyElement);

            doc.Save(stream);
            stream.Position = 0;
        }
    }
}
