using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using TelegramHubRouter_Server.Worker.WorkerJSON;
using TL;
using WTelegram;

namespace TelegramHubRouter_Server.Worker.Telegram
{
	public class TelegramService
	{
		public IHubContext<LogHub> _hubContext;
		private readonly TelegramAuthSession _authSession;
		private Client _client;
		private TL.ChatBase _sourceChat;
		private List<TL.InputPeer> _targetPeers = new List<InputPeer>();
		WorkerJSON.Config _jsonConfig = new WorkerJSON.Config();


		public TelegramService(IHubContext<LogHub> hubContext, TelegramAuthSession authSession)
		{
			_hubContext = hubContext;
			_authSession = authSession;
		}

		public async Task<bool> Start(List<string> networks, string configPath, string networksPath, string _sessionPath)
		{
			try
			{
				if (_client == null)
				{
					_jsonConfig = await ConfigManager.GetConfigAsync(configPath);

					Helpers.Log = (lvl, msg) => { };

					List<long> _allIDs = new List<long>();
					List<string> _enabledIDs = new List<string>();

					Auth _auth = new(configPath, _sessionPath, _hubContext, _authSession);

					_client = new Client(_auth.Config);
					TL.User user = await _client.LoginUserIfNeeded();

					await _hubContext.Clients.All.SendAsync("SetUsername", $"@{user.username}");
					await _hubContext.Clients.All.SendAsync("ReceiveLog", $"INFO: Client is Connected: @{user.username}");

					foreach(string network in networks)
						_enabledIDs.Add(network);

					if (!Directory.Exists(networksPath))
						Directory.CreateDirectory(networksPath);

					_enabledIDs.ForEach(x =>
					{
						string fileName = $"{x}.netw";
						string filePath = Path.Combine(networksPath, fileName);

						if (File.Exists(filePath))
						{
							_hubContext.Clients.All.SendAsync("ReceiveLog", $"INFO: {fileName} network is initialized");

							foreach (string line in File.ReadAllLines(filePath))
								if (!string.IsNullOrWhiteSpace(line) && long.TryParse(line.Trim(), out long parsedId))
									_allIDs.Add(parsedId);
						}
					});

					if (_allIDs.Count != 0)
					{
						await StartAutoForwarderAsync(_jsonConfig.SOURCE_ID, _allIDs);
						_isDisposed = false;
						return true;
					}
					else
					{
						await _hubContext.Clients.All.SendAsync("ReceiveLog", $"ERROR: No networks selected");
						await Stop();
						_isDisposed = false;
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				await _hubContext.Clients.All.SendAsync("ReceiveLog", $"ERROR: {ex.Message}");
				return false;
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
					await _hubContext.Clients.All.SendAsync("ReceiveLog", $"ERROR: The source channel ({sourceId}) was not found among the account's chats! The account must be a member of this channel.");
					return;
				}

				foreach (long targetId in targetIds.Distinct())
				{
					long cleanTargetId = CleanChannelId(targetId);

					if (!dialogs.chats.TryGetValue(cleanTargetId, out var targetChat))
					{
						await _hubContext.Clients.All.SendAsync("ReceiveLog", $"ERROR: Target channel ({targetId}) not found among the account's chats! The account must be a member of this channel.");
						continue;
					}

					_targetPeers.Add((TL.InputPeer)targetChat);
				}

				_client.OnUpdates -= Client_OnUpdate;
				_client.OnUpdates += Client_OnUpdate;

				await _hubContext.Clients.All.SendAsync("ReceiveLog", $"INFO: Done! Channel monitoring [{_sourceChat.Title}] started!");
			}
			catch (Exception ex)
			{
				await _hubContext.Clients.All.SendAsync("ReceiveLog", $"ERROR: Channel initialization error: {ex.Message}\n{ex.StackTrace}\n{ex.ToString()}");
			}
		}

		private async Task Client_OnUpdate(TL.IObject arg)
		{
			if (arg is TL.UpdatesBase updates)
				foreach (var update in updates.UpdateList)
					if (update is TL.UpdateNewChannelMessage incUpdate)
						if (incUpdate.message is TL.Message msg)
							if (msg.peer_id is TL.PeerChannel peerChannel && peerChannel.channel_id == _sourceChat.ID)
								foreach (var _targetPeer in _targetPeers)
								{
									await ProcessNewMessageAsync(msg, _targetPeer);
									await Task.Delay(_jsonConfig.MSG_DELAY);
								}
		}

		private async Task ProcessNewMessageAsync(TL.Message msg, InputPeer _targetPeer)
		{
			try
			{
				if (msg == null) return;

				TL.InputMedia inputMedia = null;

				if (msg.media is TL.MessageMediaPhoto photoMedia && photoMedia.photo is TL.Photo photo)
					inputMedia = new TL.InputMediaPhoto
					{
						id = new TL.InputPhoto
						{
							id = photo.id,
							access_hash = photo.access_hash,
							file_reference = photo.file_reference
						}
					};
				else if (msg.media is TL.MessageMediaDocument docMedia && docMedia.document is TL.Document doc)
					inputMedia = new TL.InputMediaDocument
					{
						id = new TL.InputDocument
						{
							id = doc.id,
							access_hash = doc.access_hash,
							file_reference = doc.file_reference
						}
					};

				await _client.SendMessageAsync(_targetPeer, msg.message, inputMedia, entities: msg.entities);
				await _hubContext.Clients.All.SendAsync("ReceiveLog", $"INFO[{_targetPeer.ID}]: {msg.message}");
			}
			catch (Exception ex) { await _hubContext.Clients.All.SendAsync("ReceiveLog", $"ERROR: {ex.Message}"); }
		}

		private bool _isDisposed = false;
		public async Task<bool> Stop()
		{
			if (_isDisposed) return true;

			try
			{
				if (_client != null)
				{
					await _client.DisposeAsync();
					_client = null;
				}
			}
			catch (Exception ex)
			{
				await _hubContext.Clients.All.SendAsync("ReceiveLog", ex.Message);
				return false;
			}
			finally { _isDisposed = true; }

			return true;
		}
	}
}
