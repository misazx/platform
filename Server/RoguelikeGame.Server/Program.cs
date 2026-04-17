using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Services;
using RoguelikeGame.Server.Hubs;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/server-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Roguelike Game Server...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
    builder.Services.AddSignalR();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=roguelike.db"));

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IRoomService, RoomService>();
    builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();
    builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
    builder.Services.AddHostedService<RoomCleanupService>();

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key_that_should_be_changed_in_production"))
        };
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowAnyOrigin();
        });
    });

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Roguelike Game Server API",
            Version = "v1",
            Description = "多人联机游戏服务器API"
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Roguelike Game Server API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<GameHub>("/hubs/game");
    app.MapHub<LobbyHub>("/hubs/lobby");

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        try
        {
            context.Database.ExecuteSqlRaw(
                "CREATE TABLE IF NOT EXISTS Friendships (" +
                "Id TEXT PRIMARY KEY, " +
                "RequesterId TEXT NOT NULL, " +
                "AddresseeId TEXT NOT NULL, " +
                "Status INTEGER NOT NULL DEFAULT 0, " +
                "CreatedAt TEXT NOT NULL, " +
                "AcceptedAt TEXT NULL)");
        }
        catch { }
    }

    Log.Information("Server started successfully on port {Port}", 
        Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "5000");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
