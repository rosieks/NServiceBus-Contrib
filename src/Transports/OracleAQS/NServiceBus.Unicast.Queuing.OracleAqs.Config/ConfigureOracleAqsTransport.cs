namespace NServiceBus
{
    using NServiceBus.Unicast.Queuing.OracleAqs.Config;
    using NServiceBus.Unicast.Queuing.OracleAqs.Config.Installers;

    /// <summary>
    /// </summary>
    /// <remarks>
    /// Credits goes to everyone who has worked on NSB and Joseph Daigle/Andreas Ohlund
    /// who created the Service Broker transport this is based off of.
    /// </remarks>
    public static class ConfigureOracleAqsTransport
    {
        /// <summary>
        /// Use Oracle AQS for your queuing infrastruture.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>Oracle AQS configuration.</returns>
        public static ConfigOracleAqsTransport OracleAqsTransport(this Configure config)
        {
            var cfg = new ConfigOracleAqsTransport();
            cfg.Configure(config);

            EndpointInputQueueInstaller.Enabled = true;

            return cfg;
        }
    }
}
