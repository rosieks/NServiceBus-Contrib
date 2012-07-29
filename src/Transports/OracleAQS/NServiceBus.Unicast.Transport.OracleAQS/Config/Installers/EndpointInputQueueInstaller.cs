namespace NServiceBus.Unicast.Queuing.OracleAdvancedQueuing.Config.Installers
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

        public static bool Enabled { get; set; }

        public static string ConnectionString { get; set; }

        public static string QueueTable { get; set; }

        public static string InputQueue { get; set; }

        public void Install(WindowsIdentity identity)
        {
            if (!Enabled)
            {
                return;
            }

            using (OracleConnection connection = new OracleConnection(ConnectionString))
            {
                OracleCommand cmd = connection.CreateCommand();
                cmd.CommandText = InstallSql;
                cmd.Parameters.Add("queue", InputQueue);
                cmd.Parameters.Add("queueTable", QueueTable);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
