﻿namespace NServiceBus.Unicast.Queuing.OracleAdvancedQueuing.Config
{
    using NServiceBus.Config;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.Unicast.Queuing.OracleAdvancedQueuing.Config.Installers;

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
                this.ConnectionString(cfg.ConnectionString)
                    .InputQueue(cfg.InputQueue)
                    .QueueTable(cfg.QueueTable);
            }
        }

        /// <summary>
        /// Setup name of the queue table to receive message from.
        /// </summary>
        /// <param name="value">Name of the queue table.</param>
        /// <returns>Oracle AQS configuration.</returns>
        public ConfigOracleAqsTransport QueueTable(string value)
        {
            this.receiverConfig.ConfigureProperty(r => r.QueueTable, value);
            EndpointInputQueueInstaller.QueueTable = value;
            return this;
        }

        /// <summary>
        /// Setup transport connection string for the queue's database.
        /// </summary>
        /// <param name="value">Connection string for the queue's databes.</param>
        /// <returns>Oracle AQS configuration.</returns>
        public ConfigOracleAqsTransport ConnectionString(string value)
        {
            this.receiverConfig.ConfigureProperty(r => r.ConnectionString, value);
            this.senderConfig.ConfigureProperty(s => s.ConnectionString, value);
            EndpointInputQueueInstaller.ConnectionString = value;
            return this;
        }

        /// <summary>
        /// Setup name of the queue to receive message from.
        /// </summary>
        /// <param name="value">Name of the input queue.</param>
        /// <returns>Oracle AQS configuration.</returns>
        public ConfigOracleAqsTransport InputQueue(string value)
        {
            this.receiverConfig.ConfigureProperty(r => r.InputQueue, value);
            EndpointInputQueueInstaller.InputQueue = value;
            return this;
        }

        public ConfigOracleAqsTransport SecondsToWaitForMessage(int value)
        {
            this.receiverConfig.ConfigureProperty(r => r.SecondsToWaitForMessage, value);
            return this;
        }
    }
}
