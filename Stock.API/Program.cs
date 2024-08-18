using MassTransit;
using ServiceBus;
using Stock.Service.Consumer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ServiceBus.IBus, ServiceBus.Bus>();
builder.Services.AddHostedService<OrderCreatedEventConsumerBackgroundService>();
builder.Services.Configure<BusOption>(builder.Configuration.GetSection(nameof(BusOption)));
builder.Services.AddMassTransit(configure =>
{
    // Consumer'ý kaydet
    configure.AddConsumer<OrderCreatedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {

        cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(60))); // Retry exception alýrsa belirtilen süre ve adet kadar tekrar gönderir. fail devam ederse error que oluþturur ve orada tutar.


        cfg.UseInMemoryOutbox(context); //memoryde bekletir data baþarýlý bir þekilde çalýþýrsa herhangi bir hata almazsa memoryden tekrar gönderir. !RAM yük bindirir.

        // BusOption yapýlandýrmasýný al
        var busOption = builder.Configuration.GetSection(nameof(BusOption)).Get<BusOption>();

        // RabbitMQ sunucusuna baðlantýyý yapýlandýr
        cfg.Host(new Uri(busOption!.Url));

        // OrderCreatedEvent tüketici için bir kuyruk endpoint'i oluþtur
        cfg.ReceiveEndpoint(BusConst.StockOrderCreatedEventQueueWithMassTransit, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });

        // Endpoint yapýlandýrmasýný yap
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
