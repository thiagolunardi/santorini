using ModelContextProtocol.Protocol;
using Santorini.Host.Mcp;
using Santorini.Host.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IGameService, GameService>();

// Add MCP services
builder.Services.AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation { Name = "Santorini Game Server", Version = "1.0.0" };
    })
    .WithHttpTransport()
    .WithTools<GameMcpTools>();

builder.Services.AddScoped<GameMcpTools>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseAuthorization();

app.MapControllers();

// Map MCP HTTP endpoint
app.MapMcp("/mcp");

app.MapDefaultEndpoints();

app.Run();