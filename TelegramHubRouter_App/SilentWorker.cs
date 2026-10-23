using Newtonsoft.Json;
using TelegramHubRouter_App.JSON;
using ReaLTaiizor.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;
using WTelegram;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramHubRouter_App
{
	public class SilentWorker
	{
		public Client _client;
		private TL.ChatBase _sourceChat;
		private List<TL.InputPeer> _targetPeers = new List<InputPeer>();
		string configPath = "config.yml";
		public string sessionPath = ".session";
		JSON.Config _jsonConfig = new JSON.Config();

		public SilentWorker() {  }

		public async Task Start(string[] args, bool _isDev, bool _isLogsEnabled)
		{
		Start:
			try
			{
				_jsonConfig = Program.getConfig(configPath);
				if (_jsonConfig == null)
				{
					Console.WriteLine("Couldn't read/write config file");
					return;
				}

				if (_client == null)
				{
					if (!_isLogsEnabled)
						Helpers.Log = (lvl, msg) => { };

					_client = new Client(Config);
					TL.User user = await _client.LoginUserIfNeeded();

					Console.WriteLine($"INFO: Client is Connected: @{user.first_name}");

					List<long> _allIDs = new List<long>();
					List<string> _enabledIDs = new List<string>();

					for (int i = 1; i < args.Length; i++)
					{
						switch (args[i].ToLower())
						{
							case "-silent":
							case "-s":
							case "-dev":
							case "-d":
							case "-log":
							case "-l":
							case "-reset":
							case "-r":
								break;
							case "-w":
							case "-web":
								Console.WriteLine($"usernameForWeb:{user.first_name}");
								break;
							case "-configid":
							case "-cid":
							case "-confighash":
							case "-chash":
							case "-confignum":
							case "-cnum":
							case "-configsrc":
							case "-csrc":
							case "-configpass":
							case "-cpass":
							case "-configdelay":
							case "-cdelay":
								i++;
								break;

							default:
								_enabledIDs.Add(args[i]);
								break;
						}
					}

					if (!Directory.Exists("networks"))
						Directory.CreateDirectory("networks");

					_enabledIDs.ForEach(x =>
					{
						string fileName = $"{x.ToLower().Replace("т", "t").Replace("-", "").Trim()}.netw";
						string filePath = Path.Combine("networks", fileName);

						if (File.Exists(filePath))
						{
							Console.WriteLine($"INFO: {fileName.ToUpper()} network is initialized");

							foreach (string line in File.ReadAllLines(filePath))
								if (!string.IsNullOrWhiteSpace(line) && long.TryParse(line.Trim(), out long parsedId))
									_allIDs.Add(parsedId);
						}
					});

					if (_allIDs.Count != 0)
					{
						if (_jsonConfig.SOURCE_ID != -100 && _jsonConfig.SOURCE_ID != 0)
							await StartAutoForwarderAsync(_isDev ? -1004462603660 : _jsonConfig.SOURCE_ID, _allIDs);
						else
						{
							Console.WriteLine($"ERROR: Invalid Source ID");
							Console.WriteLine($"Enter Source ID:");
							string input = Regex.Replace(Console.ReadLine().Trim(), @"[^\d-]", "");
							long inputLong = Convert.ToInt64(input.StartsWith("-100") ? input : $"-100{input}" );
							_jsonConfig.SOURCE_ID = inputLong;
							File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
							_jsonConfig = Program.getConfig(configPath);

							await StartAutoForwarderAsync(_isDev ? -1004462603660 : inputLong, _allIDs);
						}
					}
					else
					{
						Console.WriteLine($"ERROR: No networks selected");
						if (_client != null)
						{
							await _client.DisposeAsync();
							_client = null;
						}

						_isDisposed = true;
						Environment.Exit(0);
					}

					_isDisposed = false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: {ex.Message}");
			}

			while (true)
			{
				string input = Console.ReadLine();
				if (input == "/stop" || input == "stop")
				{
					Console.WriteLine($"INFO: Disposing Client...");
					if (_client != null)
					{
						await _client.DisposeAsync();
						_client = null;
					}

					_isDisposed = true;
					Environment.Exit(0);
				}
				else if (input == "/update" || input == "update")
				{
					Console.WriteLine($"INFO: Updating Client...");
					if (_client != null)
					{
						await _client.DisposeAsync();
						_client = null;
					}

					_isDisposed = true;
					goto Start;
				}
				else if (input == null)
					Environment.Exit(0);
			}
		}

		private string Config(string what)
		{
			_jsonConfig = Program.getConfig(configPath);

			switch (what)
			{
				case "session_pathname":
					return sessionPath;
				case "api_id":
					if (_jsonConfig.API_ID != 0)
						return _jsonConfig.API_ID.ToString();
					else
					{
						Console.WriteLine("API_ID:");
						string input = Regex.Replace(Console.ReadLine().Trim(), @"[^\d-]", "");
						_jsonConfig.API_ID = Convert.ToInt64(input);
						File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
						_jsonConfig = Program.getConfig(configPath);
						return input;
					}

				case "api_hash":
					if (_jsonConfig.API_HASH != "")
						return _jsonConfig.API_HASH;
					else
					{
						Console.WriteLine("API_HASH:");
						string input = Console.ReadLine().Trim();
						_jsonConfig.API_HASH = input;
						File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
						_jsonConfig = Program.getConfig(configPath);
						return input;
					}

				case "phone_number":
					if (_jsonConfig.PHONE_NUMBER != "")
						return _jsonConfig.PHONE_NUMBER;
					else
					{
						Console.WriteLine("PHONE_NUMBER:");
						string input = Console.ReadLine().Trim();
						_jsonConfig.PHONE_NUMBER = input;
						File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
						_jsonConfig = Program.getConfig(configPath);
						return input;
					}

				case "verification_code":
					Console.WriteLine("AUTH CODE:");
					return Console.ReadLine().Trim();

				case "password":
					if (_jsonConfig.PASSWORD != "")
						return _jsonConfig.PASSWORD;
					else
					{
						Console.WriteLine("PASSWORD:");
						string input = Console.ReadLine().Trim();
						_jsonConfig.PASSWORD = input;
						File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
						_jsonConfig = Program.getConfig(configPath);
						return input;
					}

				default:
					return null;
			}
		}

		private bool _isDisposed = false;

		private async void Closing(object sender, FormClosingEventArgs e)
		{
			if (_isDisposed) return;

			e.Cancel = true;

			using (var savingForm = new AddForms.SessionSaving())
			{
				try
				{
					if (_client != null)
						await _client.DisposeAsync();
				}
				finally
				{
					_isDisposed = true;
				}
			}
		}

		private long CleanChannelId(long rawId)
		{
			if (rawId >= 0) return rawId;

			string strId = rawId.ToString();
			if (strId.StartsWith("-100"))
				return long.Parse(strId.Substring(4));

			return Math.Abs(rawId);
		}

		private async Task StartAutoForwarderAsync(long sourceId, List<long> targetIds)
		{
			try
			{
				long cleanSourceId = CleanChannelId(sourceId);

				if (_targetPeers != null)
					_targetPeers.Clear();

				var dialogs = await _client.Messages_GetAllDialogs();

				if (!dialogs.chats.TryGetValue(cleanSourceId, out _sourceChat))
				{
					Console.WriteLine($"ERROR: The source channel ({sourceId}) was not found among the account's chats! The account must be a member of this channel.");
					return;
				}

				foreach (long targetId in targetIds.Distinct())
				{
					long cleanTargetId = CleanChannelId(targetId);

					if (!dialogs.chats.TryGetValue(cleanTargetId, out var targetChat))
					{
						Console.WriteLine($"ERROR: Target channel ({targetId}) not found among the account's chats! The account must be a member of this channel.");
						continue;
					}

					_targetPeers.Add((TL.InputPeer)targetChat);
				}

				_client.OnUpdates -= Client_OnUpdate;
				_client.OnUpdates += Client_OnUpdate;

				Console.WriteLine($"INFO: Done! Channel monitoring [{_sourceChat.Title}] started!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: Channel initialization error: {ex.Message}\n{ex.StackTrace}\n{ex.ToString()}");
			}
		}

		private async Task Client_OnUpdate(TL.IObject arg)
		{
			if (arg is TL.UpdatesBase updates)
			{
				foreach (var update in updates.UpdateList)
				{
					if (update is TL.UpdateNewChannelMessage incUpdate)
					{
						if (incUpdate.message is TL.Message msg)
						{
							if (msg.peer_id is TL.PeerChannel peerChannel && peerChannel.channel_id == _sourceChat.ID)
							{
								foreach (var _targetPeer in _targetPeers)
								{
									await ProcessNewMessageAsync(msg, _targetPeer);
									await Task.Delay(_jsonConfig.MSG_DELAY);
								}
							}
						}
					}
				}
			}
		}

		private async Task ProcessNewMessageAsync(TL.Message msg, InputPeer _targetPeer)
		{
			try
			{
				if (msg == null) return;

				TL.InputMedia inputMedia = null;

				if (msg.media is TL.MessageMediaPhoto photoMedia && photoMedia.photo is TL.Photo photo)
				{
					inputMedia = new TL.InputMediaPhoto
					{
						id = new TL.InputPhoto
						{
							id = photo.id,
							access_hash = photo.access_hash,
							file_reference = photo.file_reference
						}
					};
				}
				else if (msg.media is TL.MessageMediaDocument docMedia && docMedia.document is TL.Document doc)
				{
					inputMedia = new TL.InputMediaDocument
					{
						id = new TL.InputDocument
						{
							id = doc.id,
							access_hash = doc.access_hash,
							file_reference = doc.file_reference
						}
					};
				}

				await _client.SendMessageAsync(_targetPeer, msg.message, inputMedia, entities: msg.entities);
				Console.WriteLine($"INFO[{_targetPeer.ID}]: {msg.message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ERROR: {ex.Message}");
			}
		}
	}
}
