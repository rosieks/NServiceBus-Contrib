using System;
using System.Threading;
using NServiceBus;

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
                .UnicastBus()
                    .DoNotAutoSubscribe()
                    .LoadMessageHandlers()
                .OracleAqsTransport()
                    .InputQueue("TEST_Q")
                    .QueueTable("TEST_Q_TAB")
                    .ConnectionString("Data Source=localhost;Persist Security Info=True;User ID=slawek;Password=sasa")
                .CreateBus()
                .Start(() => Configure.Instance
                    .ForInstallationOn<NServiceBus.Installation.Environments.Windows>().Install());



            bus.Send("TEST_Q",new MockMessage { Data = "Hello World" });

            Thread.Sleep(TimeSpan.FromMinutes(2));
        }
    }

    public class MockMessage : IMessage
    {
        public String Data { get; set; }
    }

    public class MockMessageHandler : IHandleMessages<MockMessage>
    {
        public IBus Bus { get; set; }

        public void Handle(MockMessage message)
        {
            Console.WriteLine("Handled message with data: {0}", message.Data);
        }
    }
}
