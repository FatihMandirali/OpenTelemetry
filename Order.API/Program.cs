using Common.Shared;
using Logging.Shared;
using MassTransit;
using MassTransit.MultiBus;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.OrderServices;
using Order.API.RedisServices;
using Order.API.StockServices;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Logging.Shared.Logging.ConfigurationLogging);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StockService>();

//OPENTELEMETRY'NİN REDİS İSTEKLERİNİ TRACE EDEBİLMESİ İÇN GEREKLİ
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{ 
    var redisService = sp.GetService<RedisService>();
    return redisService.GetConnectionMultiplexer();
});
builder.Services.AddSingleton<RedisService>(_ => new RedisService(builder.Configuration));
builder.Services.AddOpenTelemetryExt(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("SqlConnection"),npgOptions =>
        npgOptions.MigrationsAssembly("Order.API")
));

builder.Services.AddHttpClient<StockService>(options =>
{
    options.BaseAddress = new Uri(builder.Configuration.GetSection("ApiServices")["StockApi"]);
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost","/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
    });
});



var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestAndResponseActivityMiddleware>();
app.UseMiddleware<OpenTelemetryTraceIdMiddleware>();
app.UseExceptionMiddleware();

app.UseAuthorization();

app.MapControllers();

app.Run();