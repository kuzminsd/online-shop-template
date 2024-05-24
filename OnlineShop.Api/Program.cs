using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Models;
using OnlineShop.Application.Options;
using OnlineShop.Application.Services;
using OnlineShop.Persistence;
using OnlineShop.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);
var useHttp2 = builder.Configuration.GetValue<bool>("ApplicationSettings:UseHttp2");

if (useHttp2)
{
    builder.WebHost.ConfigureKestrel((options) =>
    {
        options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);
    });
}

// Add services to the container.
builder.Services.AddDbContext<OnlineShopDbContext>(q => q
    .UseNpgsql(builder.Configuration.GetConnectionString("OnlineShop")));

builder.Services
    .AddOptions<ExternalPaymentServiceOptions>()
    .Bind(builder.Configuration.GetSection(nameof(ExternalPaymentServiceOptions)));
    
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/users", (CreateUserRequest request) => new UserInfo(Guid.NewGuid(), request.Name))
    .WithTags("Users")
    .WithName("Create User")
    .WithOpenApi();

app.MapPost("/authentication", ([FromBody]AuthenticationRequest request) 
        => new AuthenticationResponse("accessToken", "refreshToken"))
    .WithTags("Authentication")
    .WithName("Authentication")
    .WithOpenApi();

app.MapPost("/orders", (IOrderService orderService, [FromQuery] Guid userId, [FromQuery] decimal price) 
        => orderService.CreateOrder(userId, price))
    .WithTags("Orders")
    .WithName("Create Order")
    .WithOpenApi();

app.MapGet("/orders/{orderId}", (IOrderService orderService, [FromRoute] Guid orderId) 
        => orderService.GetOrderInfo(orderId))
    .WithTags("Orders")
    .WithName("Get Order Info")
    .WithOpenApi();

app.MapPost("/orders/{orderId}/payment", 
        (IOrderService orderService, [FromRoute] Guid orderId)
            => orderService.StartPayment(orderId))
    .WithTags("Orders")
    .WithName("Start Payment")
    .WithOpenApi();

app.Run();