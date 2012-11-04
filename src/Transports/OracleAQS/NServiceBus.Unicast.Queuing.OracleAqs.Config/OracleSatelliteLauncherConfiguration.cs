namespace NServiceBus.Unicast.Queuing.OracleAqs.Config
{
    using System.Reflection;
    using NServiceBus.Config;
    using NServiceBus.Installation;

    internal class OracleSatelliteLauncherConfiguration : INeedInitialization, IWantToRunWhenConfigurationIsComplete, INeedToInstallSomething<Installation.Environments.Windows>
    {
        internal static string ConnectionString { get; set; }
        private static bool installQueue;
        private static Address queueAddress;

        public void Init()
        {
            Configure.Instance.Configurer.ConfigureComponent<OracleSatelliteTransportBuilder>(DependencyLifecycle.SingleInstance)
                .ConfigureProperty(b => b.ConnectionString, ConnectionString);
        }

        public void Run()
        {
            FieldInfo field = typeof(NServiceBus.Config.SecondLevelRetriesConfiguration).GetField("installQueue", BindingFlags.Static | BindingFlags.NonPublic);
            installQueue = (bool)field.GetValue(null);
            field.SetValue(null, false);
            queueAddress = (Address)typeof(NServiceBus.Config.SecondLevelRetriesConfiguration).GetProperty("RetriesQueueAddress", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
        }

        public void Install(System.Security.Principal.WindowsIdentity identity)
        {
            if (installQueue)
            {
                OracleAqsUtilities.CreateQueueIfNecessary(queueAddress, ConnectionString);
            }
        }
    }
}
