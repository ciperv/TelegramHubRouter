using Microsoft.AspNetCore.SignalR;
using TelegramHubRouter_Server.Worker.WorkerJSON;

namespace TelegramHubRouter_Server.Worker.Telegram
{
	public class Auth
	{
		private Config _jsonConfig = new();
		private string _sessionPath = "";
		private IHubContext<LogHub> _hubContext;
		private readonly TelegramAuthSession _authSession;

		public Auth(string configPath, string sessionPath, IHubContext<LogHub> hubContext, TelegramAuthSession authSession)
		{
			_jsonConfig = ConfigManager.GetConfig(configPath);
			_sessionPath = sessionPath;
			_hubContext = hubContext;
			_authSession = authSession;
		}

		public string Config(string what)
		{
			switch (what)
			{
				case "session_pathname":
					return Path.Combine(AppContext.BaseDirectory, $"{_sessionPath}.session");
				case "api_id":
					return _jsonConfig.API_ID.ToString();
				case "api_hash":
					return _jsonConfig.API_HASH;
				case "phone_number":
					return _jsonConfig.PHONE_NUMBER;
				case "verification_code":
					return Task.Run(async () => await _authSession.WaitForCodeAsync(_hubContext)).GetAwaiter().GetResult();
				case "password":
					return _jsonConfig.PASSWORD;
				default:
					return null;
			}
		}
	}
}
