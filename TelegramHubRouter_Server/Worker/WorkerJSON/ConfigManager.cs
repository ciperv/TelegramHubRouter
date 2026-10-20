using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using TL;

namespace TelegramHubRouter_Server.Worker.WorkerJSON
{
	public static class ConfigManager
	{
		public static Config GetConfig(string configPath)
		{
			try
			{
				if (!File.Exists(configPath))
					File.WriteAllText(configPath, JsonSerializer.Serialize(new Config(), new JsonSerializerOptions { WriteIndented = true }));

				return JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
			}
			catch
			{
				return null;
			}
		}

		public async static Task<Config> GetConfigAsync(string configPath)
		{
			try
			{
				if (!File.Exists(configPath))
					await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(new Config(), new JsonSerializerOptions { WriteIndented = true }));

				return JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(configPath));
			}
			catch
			{
				return null;
			}
		}


		public static bool WriteConfig(ref string responseParameter, string configPath, string apiHash, string sourceId, string messageDelay, string apiId, string phoneNumber, string password)
		{
			try
			{
				WorkerJSON.Config config = new()
				{
					API_HASH = apiHash.Trim(),
					SOURCE_ID = Convert.ToInt64(sourceId.Trim()),
					MSG_DELAY = Convert.ToInt32(messageDelay.Trim()),
					API_ID = Convert.ToInt64(apiId.Trim()),
					PHONE_NUMBER = phoneNumber,
					PASSWORD = password
				};

				File.WriteAllText(configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

				responseParameter = $"Config saved: {configPath}";
				return true;
			}
			catch (Exception ex)
			{
				responseParameter = $"Can't write config: {ex.Message}";
				return false;
			}
		}

		public static bool ResetConfig(ref string responseParameter, string configPath)
		{
			try
			{
				WorkerJSON.Config config = new();
				File.WriteAllText(configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
				return true;
			}
			catch (Exception ex)
			{
				responseParameter = ex.Message;
				return false;
			}
		}
	}
}
