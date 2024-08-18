using MassTransit;
using ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService(/*ServiceBus.IBus bus,*/IPublishEndpoint publishEndpoint) :IOrderService
    {
        public async Task Create()
        {
            //create order
            //sen to queue

            var orderCreatedEvent = new OrderCreatedEvent(10, new Dictionary<int, int>()
            {
                {1,5 }, {2,6}
            });

            //Retry Mekanızması 
            CancellationTokenSource cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(50));

            //await bus.Send(orderCreatedEvent, BusConst.OrderCreatedEventExchange);
            await publishEndpoint.Publish(orderCreatedEvent,pipeline =>
            {
                pipeline.SetAwaitAck(true);
                pipeline.Durable = true;
                pipeline.TimeToLive = TimeSpan.FromMinutes(1);
            },cancellationTokenSource.Token);
        }
    }
}
