using AllaganSyncServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<AllaganSyncServer.WebSocketManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
    ReceiveBufferSize = 4096
});

app.UseMiddleware<WebSocketMiddleware>();

app.Run();

