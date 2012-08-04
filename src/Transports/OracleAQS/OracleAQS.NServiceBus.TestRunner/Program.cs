using System;
using System.Threading;
using NServiceBus;
using NServiceBus.Saga;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var bus =
                NServiceBus.Configure.With()
                .DefaultBuilder()
                .XmlSerializer()
                //.Log4Net()
                .UnicastBus()
                    .DoNotAutoSubscribe()
                    .LoadMessageHandlers()
                .OracleAqsTransport("Data Source=localhost;Persist Security Info=True;User ID=slawek;Password=sasa")
                .Sagas()
                .NHibernateUnitOfWork()
                .NHibernateSagaPersister()
                .DefineEndpointName("ConsoleTest")
                .RunTimeoutManagerWithInMemoryPersistence()
                .CreateBus()
                .Start(() => Configure.Instance
                    .ForInstallationOn<NServiceBus.Installation.Environments.Windows>().Install());

            bus.Send("consoletest",new MockMessage { Data = "Hello World" });

            Thread.Sleep(TimeSpan.FromMinutes(2));
        }
    }

    public class MockMessage : IMessage
    {
        public String Data { get; set; }
    }

    public class MyTimeout
    {
        public String Data { get; set; }
    }

    public class MySagaData : IContainSagaData
    {
        // the following properties are mandatory
        public virtual Guid Id { get; set; }
        public virtual string Originator { get; set; }
        public virtual string OriginalMessageId { get; set; }

        // all other properties you want persisted
        public virtual string Data { get; set; }
    }

    public class MockMessageHandler : Saga<MySagaData>, ISagaStartedBy<MockMessage>, IHandleTimeouts<MyTimeout>
    {
        public override void ConfigureHowToFindSaga()
        {
            this.ConfigureMapping<MockMessage>(m => m.Data, m => m.Data);
        }

        public void Handle(MockMessage message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{1}: Handled message with data: {0}", message.Data, DateTime.Now);
            Console.ResetColor();

            this.RequestUtcTimeout(TimeSpan.FromSeconds(5), new MyTimeout {Data = message.Data});
        }

        public void Timeout(MyTimeout state)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{1}: Handled timeout with data: {0}", state.Data, DateTime.Now);
            Console.ResetColor();

            this.MarkAsComplete();
        }
    }
}
