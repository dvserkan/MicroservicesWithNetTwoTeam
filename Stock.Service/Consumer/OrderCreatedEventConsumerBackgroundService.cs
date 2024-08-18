using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceBus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Stock.Service.Consumer
{
    public class OrderCreatedEventConsumerBackgroundService(IBus bus) : BackgroundService
    {

        private IModel? Channel { get; set; }

        // hook methods
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Channel = bus.GetChannel();

            //create queue
            Channel.QueueDeclare(queue: BusConst.StockOrderCreatedEventQueue,
                durable: true,
                exclusive: true,
                autoDelete: false,
                arguments: null);


            Channel.QueueBind(BusConst.StockOrderCreatedEventQueue, BusConst.OrderCreatedEventExchange, "", null);

            return base.StartAsync(cancellationToken);
        }

      

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(Channel);

            Channel.BasicConsume(BusConst.StockOrderCreatedEventQueue, false, consumer);


            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageAsJson = Encoding.UTF8.GetString(body);
                var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(messageAsJson);


                Console.WriteLine($"Gelen Event:{orderCreatedEvent.OrderId}");

                // process order
                // update stock
                // send payment

                // payment
                //


                Channel!.BasicAck(ea.DeliveryTag, false);
            };

            return Task.CompletedTask;
        }
    }
}