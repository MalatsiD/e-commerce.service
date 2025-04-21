using Ecommerce.API.Middleware;
using Ecommerce.API.SignalR;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("StoreDbConnection"));
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddCors();
builder.Services.AddSingleton<IConnectionMultiplexer>(config =>
{
    var connString = builder.Configuration.GetConnectionString("Redis") ?? 
        throw new Exception("Cannot get redis connection string");
    var configuration = ConfigurationOptions.Parse(connString, true);

    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddSingleton<ICartService, CartService>();

builder.Services.AddAuthentication();
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<StoreContext>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

//app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

//app.UseCors(options => 
//    options.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
//    .WithOrigins("https://localhost:4200", "http://localhost:4200")
//);

app.UseCors(options =>
{
    options.WithOrigins("http://localhost:4200")
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api").MapIdentityApi<AppUser>(); // this will now use "api/login"
app.MapHub<NotificationHub>("/hub/notifications");

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context);
}
catch(Exception ex)
{
    Console.WriteLine(ex);
    throw;
}

app.Run();
