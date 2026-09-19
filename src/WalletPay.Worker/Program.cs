using WalletPay.Application.Interfaces;
using WalletPay.Application.UseCases.Transactions;
using WalletPay.Infrastructure.Messaging.RabbitMQ;
using WalletPay.Infrastructure.Persistence.MongoDb;
using WalletPay.Infrastructure.Repositories;
using WalletPay.Worker;

var builder = Host.CreateApplicationBuilder(args);

var mongoSettings = builder.Configuration
    .GetSection("MongoDb")
    .Get<MongoDbSettings>()
    ?? throw new InvalidOperationException(
        "MongoDB configuration was not found.");

MongoDbConfiguration.Configure();

builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped<IAccountRepository, MongoAccountRepository>();
builder.Services.AddScoped<ITransactionRepository, MongoTransactionRepository>();

var rabbitMqSettings = builder.Configuration
    .GetSection("RabbitMq")
    .Get<RabbitMqSettings>()
    ?? throw new InvalidOperationException(
        "RabbitMQ configuration was not found.");

builder.Services.AddSingleton(rabbitMqSettings);

builder.Services.AddSingleton<RabbitMqTransferConsumer>();

builder.Services.AddScoped<ProcessTransferUseCase>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
