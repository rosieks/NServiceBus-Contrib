namespace NServiceBus.Unicast.Queuing.OracleAqs.Config.Installers
{
    using System.Security.Principal;
    using NServiceBus.Installation;

    public class EndpointInputQueueInstaller : INeedToInstallSomething<Installation.Environments.Windows>
    {
        /// <summary>
        /// Gets or sets a value indicating whether Oracle AQS transport should be used.
        /// </summary>
        internal static bool Enabled { get; set; }
        
        /// <summary>
        /// Gets or sets the string used to open the connection.
        /// </summary>
        internal static string ConnectionString { get; set; }

        /// <summary>
        /// Performs the installation providing permission for the given user.
        /// </summary>
        /// <param name="identity">The user for whom permissions will be given.</param>
        public void Install(WindowsIdentity identity)
        {
            if (!Enabled)
            {
                return;
            }

            OracleAqsUtilities.CreateQueueIfNecessary(Address.Local, ConnectionString);
            OracleAqsUtilities.CreateQueueIfNecessary(Configure.Instance.GetTimeoutManagerAddress(), ConnectionString);
        }
    }
}
