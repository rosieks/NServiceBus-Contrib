namespace NServiceBus.Config
{
    using System.Configuration;

    /// <summary>
    /// </summary>
    /// <remarks>
    /// Credits goes to everyone who has worked on NSB and Joseph Daigle/Andreas Ohlund
    /// who created the Service Broker transport this is based off of.
    /// </remarks>
    public class OracleAqsTransportConfig : ConfigurationSection
    {
        /// <summary>
        /// Gets or sets name of the queue table to receive messages from in the format
        /// "[schema].[table]".
        /// </summary>
        [ConfigurationProperty("QueueTable", IsRequired = true)]
        public string QueueTable
        {
            get
            {
                return (string)this["QueueTable"];
            }

            set
            {
                this["QueueTable"] = value;
            }
        }

        /// <summary>
        /// Gets or sets name of the queue to receive messages from in the format
        /// "[schema].[queue]".
        /// </summary>
        [ConfigurationProperty("InputQueue", IsRequired = true)]
        public string InputQueue
        {
            get
            {
                return (string)this["InputQueue"];
            }

            set
            {
                this["InputQueue"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the string used to open the connection.
        /// </summary>
        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public string ConnectionString
        {
            get
            {
                return (string)this["ConnectionString"];
            }

            set
            {
                this["ConnectionString"] = value;
            }
        }
    }
}
