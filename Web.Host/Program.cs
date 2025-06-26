using Application.RabbitMq;
using Application.RedisCache;
using Application.Shared.RabbitMq;
using Application.Shared.RedisCache;
using Application.Shared.Users;
using Application.Users;
using Core.Shared.Common;
using DataAccess.Shared.Users;
using DataAccess.Users;
using EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PEQ.DataAccess.Interceptor;
using Serilog;
using StackExchange.Redis;
using Web.Host.Middleware;

var builder = WebApplication.CreateBuilder(args);

// logging
string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log-.txt");

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
);

// Add services to the container.
builder.Services.Configure<RabbitMqConfiguration>(c => builder.Configuration.GetSection(nameof(RabbitMqConfiguration)).Bind(c));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis").Value)
);

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Interceptors
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();
#endregion

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
           .AddInterceptors(auditInterceptor);
});


#region Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
#endregion

#region Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
builder.Services.AddSingleton<IRabbitMqPublisherService, RabbitMqPublisherService>();
builder.Services.AddHostedService<RabbitMqConsumerService>();
#endregion

var app = builder.Build();

#region Middleware
app.UseMiddleware<LoggerMiddleware>();
#endregion

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
