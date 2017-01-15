using System.Collections.Generic;
using NUnit.Framework;

namespace Observable.Repository.Tests.Examples
{
    [TestFixture]
    public class ExampleClientOrder
    {
        public class Order
        {
            public int PkOrderId { get; set; }

            public int FkClientId { get; set; }

            public int FkCompanyId { get; set; }
        }

        public class Client
        {
            public int PkClientId { get; set; }
        }

        public class Company
        {
            public int PkCompanyId { get; set; }
        }

        public class Trade
        {
            public int PkTradeId { get; set; }

            public int FkOrderId { get; set; }
        }

        public class ExecutedOrder
        {
            public Order Order { get; set; }

            public List<Trade> Trades { get; set; }
        }

        public class ClientOrder
        {
            public ExecutedOrder ExecutedOrder { get; private set; }

            public Client Client { get; private set; }

            public Company Company { get; set; }

            public ClientOrder(ExecutedOrder order, Client client)
            {
                ExecutedOrder = order;
                Client = client;
            }
        }

        [Test]
        public void Test()
        {
            var container = new RepositoryContainer();

            container.Register<int, Client>(client => client.PkClientId);
            container.Build<int, ExecutedOrder, Order>(order => order.PkOrderId)
                .JoinMany<Trade>()
                    .DefineList(execOrder => execOrder.Trades)
                    .RightPrimaryKey(trade => trade.PkTradeId)
                    .RightLinkKey(trade => trade.FkOrderId)
                    .LeftLinkKey(order => order.PkOrderId)
                .DefineCtor(order => new ExecutedOrder { Order = order })
                .Register();
            container.Build<int, ClientOrder, ExecutedOrder>(execOrder => execOrder.Order.PkOrderId)
                .Join<Client>()
                    .RightLinkKey(client => client.PkClientId)
                    .LeftLinkKey(execOrder => execOrder.Order.FkClientId)
                .JoinUpdate<Company>()
                    .DefineUpdate(clientOrder => company => clientOrder.Company = company)
                    .RightLinkKey(company => company.PkCompanyId)
                    .LeftLinkKey(execOrder => execOrder.Order.FkCompanyId)
                .Register();

            //var clientEvent = new Subject<Client>();
            //var companyEvent = new Subject<List<Company>>();
            //var orderEvent = new Subject<RepositoryNotification<Order>>();
            //var tradeEvent = new Subject<Trade>();

            //container.AddProducer(ActionType.Add, clientEvent);
            //container.AddProducer(ActionType.Reload, companyEvent);
            //container.AddProducer(orderEvent);
            //container.AddProducer(ActionType.Add, tradeEvent);

            //clientEvent.OnNext(...);
            //companyEvent.OnNext(...);
            //orderEvent.OnNext(new RepositoryNotification<Order>(ActionType.Add, ));
            //tradeEvent.OnNext(...);

            var list = new List<ClientOrder>();
            var suscription = container.GetRepository<int, ClientOrder>()
                .Subscribe(list);
            // The list will be managed until the suscription result will be disposed
            suscription.Dispose();
        }
    }
}
