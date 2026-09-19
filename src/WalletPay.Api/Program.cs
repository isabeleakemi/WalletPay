using StackExchange.Redis;
using WalletPay.Application.Interfaces;
using WalletPay.Application.UseCases.Accounts;
using WalletPay.Application.UseCases.Transactions;
using WalletPay.Infrastructure.Messaging.RabbitMQ;
using WalletPay.Infrastructure.Persistence.MongoDb;
using WalletPay.Infrastructure.Persistence.Redis;
using WalletPay.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB
var mongoSettings = builder.Configuration
    .GetSection("MongoDb")
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException(
        "MongoDB configuration was not found.");

MongoDbConfiguration.Configure();

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDbContext>();

// RabbitMQ
var rabbitMqSettings = builder.Configuration
    .GetSection("RabbitMq")
    .Get<RabbitMqSettings>()
    ?? throw new InvalidOperationException(
        "RabbitMQ configuration was not found.");

builder.Services.AddSingleton(rabbitMqSettings);

// Redis
var redisSettings = builder.Configuration
    .GetSection("Redis")
    .Get<RedisSettings>()
    ?? throw new InvalidOperationException(
        "Redis configuration was not found.");

builder.Services.AddSingleton(redisSettings);

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        redisSettings.ConnectionString));

builder.Services.AddSingleton<
    IIdempotencyService,
    RedisIdempotencyService>();

// Repositories
builder.Services.AddScoped<IAccountRepository, MongoAccountRepository>();
builder.Services.AddScoped<ITransactionRepository, MongoTransactionRepository>();

// Messaging
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

// Use Cases
builder.Services.AddScoped<CreateAccountUseCase>();
builder.Services.AddScoped<GetAccountUseCase>();
builder.Services.AddScoped<CreateTransferUseCase>();
builder.Services.AddScoped<GetTransferUseCase>();
builder.Services.AddScoped<DepositUseCase>();

var app = builder.Build();

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();