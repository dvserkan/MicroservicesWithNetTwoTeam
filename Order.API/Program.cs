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
        // BusOption yapýlandýrmasýný al
        var busOption = builder.Configuration.GetSection(nameof(BusOption)).Get<BusOption>();

        // RabbitMQ sunucusuna baðlantýyý yapýlandýr
        cfg.Host(new Uri(busOption!.Url));

        // Endpoint yapýlandýrmasýný yap
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
