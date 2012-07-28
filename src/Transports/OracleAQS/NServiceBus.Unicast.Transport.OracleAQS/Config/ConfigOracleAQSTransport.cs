namespace NServiceBus.Unicast.Queuing.OracleAdvancedQueuing.Config
{
    using NServiceBus.Config;
    using NServiceBus.ObjectBuilder;

    /// <summary>
    /// Builds the config for the Oracle Transport.
    /// </summary>
    /// <remarks>
    /// Credits goes to everyone who has worked on NSB and Joseph Daigle/Andreas Ohlund
    /// who created the Service Broker transport this is based off of.
    /// </remarks>
    public class ConfigOracleAqsTransport : Configure
    {
        private IComponentConfig<OracleAqsMessageReceiver> receiverConfig;
        private IComponentConfig<OracleAqsMessageSender> senderConfig;

        /// <summary>
        /// Wraps the given configuration object but stores the same 
        /// builder and configurer properties.
        /// </summary>
        /// <param name="config"></param>
        public void Configure(Configure config)
        {
            this.Builder = config.Builder;
            this.Configurer = config.Configurer;

            this.receiverConfig = this.Configurer.ConfigureComponent<OracleAqsMessageReceiver>(DependencyLifecycle.SingleInstance);
            this.senderConfig = this.Configurer.ConfigureComponent<OracleAqsMessageSender>(DependencyLifecycle.SingleInstance);

            var cfg = GetConfigSection<OracleAqsTransportConfig>();

            if (cfg != null)
            {
                this.receiverConfig.ConfigureProperty(t => t.InputQueue, cfg.InputQueue);
                this.receiverConfig.ConfigureProperty(t => t.QueueTable, cfg.QueueTable);
                this.ConnectionString(cfg.ConnectionString);
            }
        }

        public ConfigOracleAqsTransport QueueTable(string value)
        {
            this.receiverConfig.ConfigureProperty(t => t.QueueTable, value);
            return this;
        }

        public ConfigOracleAqsTransport ConnectionString(string value)
        {
            this.receiverConfig.ConfigureProperty(t => t.ConnectionString, value);
            this.senderConfig.ConfigureProperty(t => t.ConnectionString, value);
            return this;
        }

        public ConfigOracleAqsTransport InputQueue(string value)
        {
            this.receiverConfig.ConfigureProperty(t => t.InputQueue, value);
            return this;
        }

        public ConfigOracleAqsTransport SecondsToWaitForMessage(int value)
        {
            this.receiverConfig.ConfigureProperty(t => t.SecondsToWaitForMessage, value);
            return this;
        }
    }
}
