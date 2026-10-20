using TelegramHubRouter_Server;
using TelegramHubRouter_Server.Worker;
using TelegramHubRouter_Server.Worker.Telegram;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<TelegramService>();
builder.Services.AddSingleton<TelegramAuthSession>();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReact", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

var app = builder.Build();

app.UseCors("AllowReact");
app.MapHub<LogHub>("/hubs/logs");
app.MapControllers();

app.Run();