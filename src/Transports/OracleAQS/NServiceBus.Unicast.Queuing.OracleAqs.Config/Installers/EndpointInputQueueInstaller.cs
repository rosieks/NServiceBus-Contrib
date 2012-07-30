namespace NServiceBus.Unicast.Queuing.OracleAqs.Config.Installers
{
    using System.Security.Principal;
    using NServiceBus.Installation;
    using Oracle.DataAccess.Client;

    public class EndpointInputQueueInstaller : INeedToInstallSomething<Installation.Environments.Windows>
    {
        private const string InstallSql = @"DECLARE
  cnt NUMBER;
BEGIN
  SELECT count(*) INTO cnt FROM dba_queues WHERE name = :queue AND queue_table = :queueTable;
  
  IF cnt = 0 THEN
    DBMS_AQADM.CREATE_QUEUE_TABLE (:queueTable, 'SYS.XMLType');
    DBMS_AQADM.CREATE_QUEUE (:queue, :queueTable);
  END IF;
  
  DBMS_AQADM.START_QUEUE(:queue);
END;";

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

            CreateQueueIfNecessary(Address.Local.Queue, ConnectionString);
        }

        private static void CreateQueueIfNecessary(string name, string connectionString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = connection.CreateCommand();
                cmd.CommandText = InstallSql;
                cmd.Parameters.Add("queue", name.ToUpper());
                cmd.Parameters.Add("queueTable", (name + "_tab").ToUpper());
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
