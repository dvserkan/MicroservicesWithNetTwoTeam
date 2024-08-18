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
    // Consumer'� kaydet
    configure.AddConsumer<OrderCreatedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {

        cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(60))); // Retry exception al�rsa belirtilen s�re ve adet kadar tekrar g�nderir. fail devam ederse error que olu�turur ve orada tutar.


        cfg.UseInMemoryOutbox(context); //memoryde bekletir data ba�ar�l� bir �ekilde �al���rsa herhangi bir hata almazsa memoryden tekrar g�nderir. !RAM y�k bindirir.

        // BusOption yap�land�rmas�n� al
        var busOption = builder.Configuration.GetSection(nameof(BusOption)).Get<BusOption>();

        // RabbitMQ sunucusuna ba�lant�y� yap�land�r
        cfg.Host(new Uri(busOption!.Url));

        // OrderCreatedEvent t�ketici i�in bir kuyruk endpoint'i olu�tur
        cfg.ReceiveEndpoint(BusConst.StockOrderCreatedEventQueueWithMassTransit, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });

        // Endpoint yap�land�rmas�n� yap
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
