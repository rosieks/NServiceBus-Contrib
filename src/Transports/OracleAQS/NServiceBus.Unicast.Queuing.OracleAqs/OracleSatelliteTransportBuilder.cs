namespace NServiceBus.Unicast.Queuing.OracleAqs
{
    using NServiceBus.Faults;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.Satellites;
    using NServiceBus.Unicast.Transport;
    using NServiceBus.Unicast.Transport.Transactional;

    public class OracleSatelliteTransportBuilder : ISatelliteTransportBuilder
    {
        public IBuilder Builder { get; set; }

        public TransactionalTransport MainTransport { get; set; }

        public string ConnectionString { get; set; }

        public ITransport Build()
        {
            var nt = 1; // MainTransport != null ? MainTransport.NumberOfWorkerThreads == 0 ? 1 : MainTransport.NumberOfWorkerThreads : 1;
            var mr = MainTransport != null ? MainTransport.MaxRetries : 1;
            var tx = MainTransport != null ? MainTransport.IsTransactional : true;
            var fm = MainTransport != null
                ? Builder.Build(MainTransport.FailureManager.GetType()) as IManageMessageFailures
                : Builder.Build<IManageMessageFailures>();
            return new TransactionalTransport
            {
                MessageReceiver = new OracleAqsMessageReceiver
                {
                    ConnectionString = this.ConnectionString,
                },
                IsTransactional = tx,
                NumberOfWorkerThreads = nt,
                MaxRetries = mr,
                FailureManager = fm
            };
        }
    }
}
