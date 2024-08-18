using MassTransit;
using Microsoft.VisualBasic.FileIO;
using Order.Service;
using ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ServiceBus.IBus, ServiceBus.Bus>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddMassTransit(configure =>
{
    configure.UsingRabbitMq((context, cfg) =>
    {
        // BusOption yap�land�rmas�n� al
        var busOption = builder.Configuration.GetSection(nameof(BusOption)).Get<BusOption>();

        // RabbitMQ sunucusuna ba�lant�y� yap�land�r
        cfg.Host(new Uri(busOption!.Url));

        // Endpoint yap�land�rmas�n� yap
        cfg.ConfigureEndpoints(context);
    });
});


builder.Services.Configure<BusOption>(builder.Configuration.GetSection(nameof(BusOption)));
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
