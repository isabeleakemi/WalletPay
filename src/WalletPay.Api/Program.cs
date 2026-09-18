using WalletPay.Application.Interfaces;
using WalletPay.Application.UseCases.Accounts;
using WalletPay.Application.UseCases.Transactions;
using WalletPay.Infrastructure.Persistence.MongoDb;
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

// Repositories
builder.Services.AddScoped<IAccountRepository, MongoAccountRepository>();
builder.Services.AddScoped<ITransactionRepository, MongoTransactionRepository>();

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