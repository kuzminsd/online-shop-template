using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Api.Helpers;
using OnlineShop.Api.HostedServices;
using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Models;
using OnlineShop.Application.Options;
using OnlineShop.Application.Services;
using OnlineShop.Persistence;
using OnlineShop.Persistence.Repositories;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecks()
    .ForwardToPrometheus();

builder.Services.UseHttpClientMetrics();

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

builder.Services.AddScoped<DbMaintenanceService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<PaymentsHostedService>();

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

app.MapMetrics();

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