namespace NServiceBus.Unicast.Queuing.OracleAqs
{
    using Oracle.DataAccess.Client;

    public class OracleAqsUtilities
    {
        private const string InstallSql = "DECLARE  cnt NUMBER;BEGIN  SELECT count(*) INTO cnt FROM dba_queues WHERE name = :queue AND queue_table = :queueTable;    IF cnt = 0 THEN    DBMS_AQADM.CREATE_QUEUE_TABLE (:queueTable, 'SYS.XMLType');    DBMS_AQADM.CREATE_QUEUE (:queue, :queueTable);  END IF;    DBMS_AQADM.START_QUEUE(:queue);END;";

        public static void CreateQueueIfNecessary(Address address, string connectionString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = connection.CreateCommand(); 
                cmd.CommandText = InstallSql; 
                cmd.Parameters.Add("queue", NormalizeQueueName(address)); 
                cmd.Parameters.Add("queueTable", (NormalizeQueueName(address) + "_tab").ToUpper()); 
                connection.Open(); 
                cmd.ExecuteNonQuery();
            }
        }

        public static string NormalizeQueueName(Address address)
        {
            return address.Queue.Replace('.', '_').ToUpper();
        }
    }
}
