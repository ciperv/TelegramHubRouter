using Microsoft.AspNetCore.SignalR;

namespace TelegramHubRouter_Server.Worker.Telegram
{
	public class TelegramAuthSession
	{
		private TaskCompletionSource<string>? _tcs;

		public Task<string> WaitForCodeAsync(IHubContext<LogHub> _hubContext)
		{
			_hubContext.Clients.All.SendAsync("AuthCode", $"Enter Code");
			_tcs = new TaskCompletionSource<string>();
			return _tcs.Task;
		}

		public bool ProvideCode(string code)
		{
			if (_tcs != null && !_tcs.Task.IsCompleted)
				return _tcs.TrySetResult(code);
			return false;
		}
	}
}