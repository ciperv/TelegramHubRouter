using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using TelegramHubRouter_Server.Worker.Telegram;
using TelegramHubRouter_Server.Worker.WorkerJSON;

namespace TelegramHubRouter_Server.Worker
{
	[ApiController]
	[Route("api/worker")]
	public class WorkerController : ControllerBase
	{
		private readonly TelegramService _telegramService;
		private readonly TelegramAuthSession _authSession;
		private string _configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
		private string _networksPath = Path.Combine(AppContext.BaseDirectory, "Networks");
		private string _sessionPath = "";

		public WorkerController(TelegramService TelegramService, TelegramAuthSession authSession)
		{
			_telegramService = TelegramService;
			_authSession = authSession;
		}

		[HttpPost("start")]
		public async Task<IActionResult> Start([FromBody] StartWorkerDto dto)
		{
			bool success = await _telegramService.Start(dto.Networks, _configPath, _networksPath, _sessionPath);
			if (success) return Ok(new { message = "Worker started" });
			return BadRequest(new { message = "Worker already running" });
		}

		[HttpPost("stop")]
		public async Task<IActionResult> Stop()
		{
			bool success = await _telegramService.Stop();
			if (success) return Ok(new { message = "Worker stopped" });
			return BadRequest(new { message = "Worker is not running" });
		}

		[HttpPost("writecfg")]
		public IActionResult WriteConfig([FromBody] WriteConfigDto dto)
		{
			string response = "";
			bool success = ConfigManager.WriteConfig(ref response, _configPath, dto.apiHash, dto.sourceId, dto.messageDelay, dto.apiId, dto.phoneNumber, dto.password);
			if (success) return Ok(new { message = response });
			return BadRequest(new { message = response });
		}

		[HttpPost("resetcfg")]
		public IActionResult ResetConfig()
		{
			string response = "";
			bool success = ConfigManager.ResetConfig(ref response, _configPath);
			if (success) return Ok(new { message = "Config is empty now" });
			return BadRequest(new { message = $"Config error:{response}" });
		}

		[HttpGet("getnet")]
		public IActionResult GetNetworks()
		{
			List<string> networks = new();
			if (NetworkManager.GetNetworks(ref networks, _networksPath)) return Ok(new { message = networks });
			return BadRequest(new { message = "No networks found" });
		}

		[HttpGet("getcfg")]
		public async Task<IActionResult> GetFullConfig()
		{
			try
			{
				WorkerJSON.Config config = ConfigManager.GetConfig(_configPath);

				return Ok(new { message = new List<string>()
				{
					config.API_HASH,
					config.SOURCE_ID.ToString(),
					config.MSG_DELAY.ToString(),
					config.API_ID.ToString(),
					config.PHONE_NUMBER,
					config.PASSWORD
				}
				});
			}
			catch
			{
				return BadRequest(new { message = $"Can't read {_configPath}" });
			}
		}

		[HttpPost("submit-code")]
		public IActionResult SubmitCode([FromBody] CodeDto dto)
		{
			bool success = _authSession.ProvideCode(dto.authCode);
			if (success)
				return Ok(new { message = "Good" });

			return BadRequest(new { message = "Bad" });
		}
	}

	public class StartWorkerDto
	{
		public List<string> Networks { get; set; } = new();
	}

	public record WriteConfigDto(string apiHash, string sourceId, string messageDelay, string apiId, string phoneNumber, string password);

	public record CodeDto(string authCode);

	public class CommandDto
	{
		public string Command { get; set; } = string.Empty;
	}
}