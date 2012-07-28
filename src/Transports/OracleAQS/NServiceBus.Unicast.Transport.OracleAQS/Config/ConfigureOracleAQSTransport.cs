namespace NServiceBus
{
    using NServiceBus.Unicast.Queuing.OracleAdvancedQueuing.Config;

    /// <summary>
    /// <remarks>
    /// Credits goes to everyone who has worked on NSB and Joseph Daigle/Andreas Ohlund
    /// who created the Service Broker transport this is based off of.
    /// </remarks>
    /// </summary>
    public static class ConfigureOracleAqsTransport
    {
        public static ConfigOracleAqsTransport OracleAqsTransport(this Configure config)
        {
            var cfg = new ConfigOracleAqsTransport();
            cfg.Configure(config);

            return cfg;
        }
    }
}
