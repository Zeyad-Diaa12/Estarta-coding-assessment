using EmployeeStatus.DataAccess;
using EmployeeStatus.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text;
using NpgsqlTypes;


var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "timestamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
    { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    { "log_event", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) }
};

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("Default")!,
        tableName: "logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true)
    .CreateLogger();

builder.Host.UseSerilog();


var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var username = context.Principal?.Identity?.Name;
            Log.Information("JWT token validated for user: {Username}", username);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddFastEndpoints();

builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Employee Status API";
        s.Version = "v1";
    };

    o.EnableJWTBearerAuth = true;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("Default")!,
        name: "PostgreSQL",
        tags: new[] { "db", "postgres" })
    .AddRedis(
        redisConnectionString: builder.Configuration["Redis:Connection"]!,
        name: "Redis",
        tags: new[] { "cache", "redis" });

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var redisConnection = configuration["Redis:Connection"]!;
    return ConnectionMultiplexer.Connect(redisConnection);
});

builder.Services.AddScoped<IDataAccess, DataAccess>();
builder.Services.AddScoped<IBusinessRulesService, BusinessRulesService>();
builder.Services.AddScoped<IProcessStatusService, ProcessStatusService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

try
{
    Log.Information("Starting Employee Status API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

