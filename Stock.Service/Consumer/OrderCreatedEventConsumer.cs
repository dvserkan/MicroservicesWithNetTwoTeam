using MassTransit;
using ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock.Service.Consumer
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
           Console.WriteLine($"(Masstransit) Gelen Event:{context.Message.OrderId}");

            return Task.CompletedTask;
        }
    }
}
