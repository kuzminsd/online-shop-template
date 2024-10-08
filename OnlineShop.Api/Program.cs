using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OnlineShop.Api.Helpers;
using OnlineShop.Api.HostedServices;
using OnlineShop.Api.Options;
using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Models;
using OnlineShop.Application.Options;
using OnlineShop.Application.Services;
using OnlineShop.Persistence;
using OnlineShop.Persistence.Repositories;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .ForwardToPrometheus();

builder.Services.UseHttpClientMetrics();

builder.Services
    .AddOpenTelemetry()
        .WithMetrics(metricsBuilder =>
        {
            metricsBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();
            
            metricsBuilder.AddEventCountersInstrumentation(counters =>
            {
                counters.AddEventSources(
                    //"System.Runtime", //Not available yet.
                    "System.Net.NameResolution",
                    "System.Net.Http",
                    "Microsoft.Extensions.Diagnostics.ResourceMonitoring",
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Routing",
                    "Microsoft.AspNetCore.Diagnostics",
                    "Microsoft.AspNetCore.RateLimiting",
                    "Microsoft.AspNetCore.HeaderParsing",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "Microsoft.AspNetCore.Http.Connections",
                    "Microsoft.EntityFrameworkCore");
            });
            
            metricsBuilder.AddPrometheusExporter();
        });

// Add services to the container.
builder.Services.AddDbContext<OnlineShopDbContext>(q =>
{
    q.UseNpgsql(builder.Configuration.GetConnectionString("OnlineShop"), options =>
    {
        options.EnableRetryOnFailure(3);
    });
});

builder.Services
    .AddOptions<ExternalPaymentServiceOptions>()
    .Bind(builder.Configuration.GetSection(nameof(ExternalPaymentServiceOptions)));

builder.Services
    .AddOptions<MetricOptions>()
    .Bind(builder.Configuration.GetSection(nameof(MetricOptions)));

builder.Services.AddScoped<DbMaintenanceService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<PaymentsHostedService>();
builder.Services.AddHostedService<MetricsHostedService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapMetrics("/actuator/prometheus");
app.MapHealthChecks("/healthz");

app.MapPost("/users", (CreateUserRequest request) => new UserInfo(Guid.NewGuid(), request.Name))
    .WithTags("Users")
    .WithName("Create User")
    .WithOpenApi();

app.MapPost("/authentication", ([FromBody]AuthenticationRequest request) 
        => new AuthenticationResponse("accessToken", "refreshToken"))
    .WithTags("Authentication")
    .WithName("Authentication")
    .WithOpenApi();

app.MapPost("/orders", (IOrderService orderService, [FromQuery] Guid userId, [FromQuery] decimal price, CancellationToken cancellationToken) 
        => orderService.CreateOrder(userId, price, cancellationToken))
    .WithTags("Orders")
    .WithName("Create Order")
    .WithOpenApi();

app.MapGet("/orders/{orderId}", (IOrderService orderService, [FromRoute] Guid orderId, CancellationToken cancellationToken) 
        => orderService.GetOrderInfo(orderId, cancellationToken))
    .WithTags("Orders")
    .WithName("Get Order Info")
    .WithOpenApi();

app.MapPost("/orders/{orderId}/payment", 
        (IOrderService orderService, [FromRoute] Guid orderId, CancellationToken cancellationToken)
            => orderService.StartPayment(orderId, cancellationToken))
    .WithTags("Orders")
    .WithName("Start Payment")
    .WithOpenApi();

app.PrepareDatabase();
app.Run();