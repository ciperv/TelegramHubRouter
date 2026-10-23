using Newtonsoft.Json;
using TelegramHubRouter_App.JSON;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;
using WTelegram;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TelegramHubRouter_App
{
	public partial class MainForm : MetroForm
	{
		public Client _client;
		private TL.ChatBase _sourceChat;
		private List<TL.InputPeer> _targetPeers = new List<InputPeer>();
		string configPath = "config.yml";
		public string sessionPath = ".session";
		JSON.Config _jsonConfig = new JSON.Config();

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			const int WM_SYSCOMMAND = 0x0112;
			const int SC_MAXIMIZE = 0xF030;
			const int SC_DOUBLECLICK = 0xF122;

			if (m.Msg == WM_SYSCOMMAND)
			{
				int command = m.WParam.ToInt32() & 0xFFF0;
				if (command == SC_MAXIMIZE || command == SC_DOUBLECLICK)
					return;
			}

			base.WndProc(ref m);
		}

		public MainForm()
		{
			InitializeComponent();
			this.MaximumSize = new System.Drawing.Size(800, 450);
			this.MinimumSize = new System.Drawing.Size(800, 450);

			LoadNetworkCheckboxes();
		}

		private async void MainForm_Load(object sender, EventArgs e)
		{
			_jsonConfig = Program.getConfig(configPath);

			if (_jsonConfig.SOURCE_ID != 0 && _jsonConfig.SOURCE_ID != -100)
				metroTextBox1.Text = _jsonConfig.SOURCE_ID.ToString();
			metroTextBox2.Text = _jsonConfig.MSG_DELAY.ToString();
		}

		private string Config(string what)
		{
			switch (what)
			{
				case "session_pathname":
					return sessionPath;
				case "api_id":
					return _jsonConfig.API_ID != 0 ? _jsonConfig.API_ID.ToString() : ShowPromptUI("API_ID:", isPassword: false).Trim();

				case "api_hash":
					return _jsonConfig.API_HASH!="" ? _jsonConfig.API_HASH : ShowPromptUI("API_HASH:", isPassword: false).Trim();

				case "phone_number":
					return _jsonConfig.PHONE_NUMBER!="" ? _jsonConfig.PHONE_NUMBER : ShowPromptUI("PHONE_NUMBER:", isPassword: false).Trim();

				case "verification_code":
					return ShowPromptUI("VERIFICATION CODE:", isPassword: false);

				case "password":
					return _jsonConfig.PASSWORD!="" ? _jsonConfig.PASSWORD : ShowPromptUI("PASSWORD (2FA):", isPassword: true);

				default:
					return null;
			}
		}

		private string ShowPromptUI(string title, bool isPassword)
		{
			string result = null;

			this.Invoke((MethodInvoker)delegate //UI THREAD
			{
				using (var prompt = new PromptForm(title, isPassword))
				{
					if (prompt.ShowDialog(this) == DialogResult.OK)
					{
						result = prompt.ResultText;
						if (title == "API_ID:")
						{
							string input = Regex.Replace(prompt.ResultText.Trim(), @"[^\d-]", "");
							_jsonConfig.API_ID = Convert.ToInt64(input);
							File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
							_jsonConfig = Program.getConfig(configPath);
						}
						else if (title == "API_HASH:")
						{
							_jsonConfig.API_HASH = prompt.ResultText.Trim();
							File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
							_jsonConfig = Program.getConfig(configPath);
						}
						else if (title == "PHONE_NUMBER:")
						{
							_jsonConfig.PHONE_NUMBER = prompt.ResultText.Trim();
							File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
							_jsonConfig = Program.getConfig(configPath);
						}
						else if (title == "PASSWORD (2FA):")
						{
							_jsonConfig.PASSWORD = prompt.ResultText.Trim();
							File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
							_jsonConfig = Program.getConfig(configPath);
						}
					}
				}
			});

			return result;
		}

		private bool _isDisposed = false;

		private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_isDisposed) return;

			e.Cancel = true;

			using (var savingForm = new AddForms.SessionSaving())
			{
				savingForm.Show(this);
				savingForm.Refresh();

				try
				{
					if (_client != null)
						await _client.DisposeAsync();
				}
				finally
				{
					_isDisposed = true;
					savingForm.Close();

					this.Close();
				}
			}
		}

		private async Task SendByNegativeIdAsync(long rawId, string messageText)
		{
			try
			{
				long cleanId = rawId;

				if (rawId < 0)
				{
					string strId = rawId.ToString();

					if (strId.StartsWith("-100"))
						cleanId = long.Parse(strId.Substring(4));
					else
						cleanId = Math.Abs(rawId);
				}

				var dialogs = await _client.Messages_GetAllDialogs();

				if (dialogs.chats.TryGetValue(cleanId, out var targetChat))
				{
					var sentMessage = await _client.SendMessageAsync(targetChat, messageText);
					metroTextBox3.Log($"INFO: Message successfully sent to '{targetChat.Title}'!");
				}
				else
					metroTextBox3.Log($"ERROR: The chat was not found among your conversations. Your account must be a member of this chat!");
			}
			catch (Exception ex)
			{
				metroTextBox3.Log($"ERROR: {ex.Message}");
			}
		}

		//private async void metroButton1_Click(object sender, EventArgs e)
		//{
		//	await SendByNegativeIdAsync(-1004363217933, "HUI");
		//}

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
					metroTextBox3.Log($"ERROR: The source channel ({sourceId}) was not found among the account's chats! The account must be a member of this channel.");
					return;
				}

				foreach (long targetId in targetIds.Distinct())
				{
					long cleanTargetId = CleanChannelId(targetId);

					if (!dialogs.chats.TryGetValue(cleanSourceId, out _sourceChat))
					{
						metroTextBox3.Log($"ERROR: The source channel ({sourceId}) was not found among the account's chats! The account must be a member of this channel.");
						return;
					}

					if (!dialogs.chats.TryGetValue(cleanTargetId, out var targetChat))
					{
						metroTextBox3.Log($"ERROR: Target channel ({targetId}) not found among the account's chats! The account must be a member of this channel.");
						continue;
					}

					_targetPeers.Add((TL.InputPeer)targetChat);
				}

				_client.OnUpdates -= Client_OnUpdate;
				_client.OnUpdates += Client_OnUpdate;

				metroTextBox3.Log($"INFO: Channel monitoring [{_sourceChat.Title}] started!");
			}
			catch (Exception ex)
			{
				metroTextBox3.Log($"ERROR: Channel initialization error: {ex.Message}\n{ex.StackTrace}\n{ex.ToString()}");
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
				metroTextBox3.Log($"INFO[{_targetPeer.ID}]: {msg.message}");
			}
			catch (Exception ex)
			{
				metroTextBox3.Log($"ERROR: {ex.Message}");
			}
		}

		private async void metroSwitch1_SwitchedChanged(object sender)
		{
			metroTextBox3.ResetText();

			metroSwitch1.Enabled = false;

			try
			{
				await Task.Delay(1000);

				if (metroSwitch1.Switched && _client == null)
				{
					metroTextBox1.Enabled = false;
					metroTextBox2.Enabled = false;
					metroPanel1.Enabled = false;
					_client = new Client(Config);
					TL.User user = await _client.LoginUserIfNeeded();

					Text = $"              Distribution service (Connected: @{user.first_name})";
					metroTextBox3.Log($"INFO: Client is Connected: @{user.first_name}");
					this.Invalidate();
					this.Refresh();

					List<long> _allIDs = new List<long>();
					List<string> _enabledIDs = GetSelectedNetworks();

					if (metroCheckBox1.Checked)
						_enabledIDs.Add("-t1");
					if (metroCheckBox2.Checked)
						_enabledIDs.Add("-t2");
					if (metroCheckBox3.Checked)
						_enabledIDs.Add("-t3");
					if (metroCheckBox4.Checked)
						_enabledIDs.Add("-t4");
					if (metroCheckBox5.Checked)
						_enabledIDs.Add("-t5");
					if (metroCheckBox6.Checked)
						_enabledIDs.Add("-t6");
					if (metroCheckBox7.Checked)
						_enabledIDs.Add("-t7");
					if (metroCheckBox8.Checked)
						_enabledIDs.Add("-v1");
					if (metroCheckBox9.Checked)
						_enabledIDs.Add("-s1");
					if (metroCheckBox10.Checked)
						_enabledIDs.Add("-test1");
					if (metroCheckBox11.Checked)
						_enabledIDs.Add("-test2");

					if (!Directory.Exists("networks"))
						Directory.CreateDirectory("networks");

					_enabledIDs.ForEach(x =>
					{
						string fileName = $"{x.ToLower().Replace("-", "").Trim()}.netw";
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
							await StartAutoForwarderAsync(_jsonConfig.SOURCE_ID, _allIDs);
						else
							metroTextBox3.Log($"ERROR: Invalid Source ID");
					}
					else
						metroTextBox3.Log($"ERROR: No network selected");
					_isDisposed = false;
				}
				else if (!metroSwitch1.Switched && _client != null)
				{
					metroTextBox1.Enabled = true;
					metroTextBox2.Enabled = true;
					metroPanel1.Enabled = true;
					using (var savingForm = new AddForms.SessionSaving())
					{
						savingForm.Show();

						if (_client != null)
						{
							await _client.DisposeAsync();
							_client = null;
						}

						_isDisposed = true;
						Text = "              Distribution service (Not Connected)";
						metroTextBox3.Log($"WARNING: Client is Disconnected");
					}
				}
			}
			catch (Exception ex)
			{
				metroTextBox3.Log($"ERROR: {ex.Message}");

				metroSwitch1.Switched = (_client != null);
			}
			finally
			{
				metroSwitch1.Enabled = true;
			}
		}

		private void metroTextBox1_TextChanged(object sender, EventArgs e)
		{
			string input = Regex.Replace(metroTextBox1.Text, @"[^\d-]", "");
			long inputLong = Convert.ToInt64(input.StartsWith("-100") ? input : $"-100{input}");
			_jsonConfig.SOURCE_ID = inputLong;
			File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
			_jsonConfig = Program.getConfig(configPath);
		}

		private bool _isUpdating = false;
		private void metroTextBox2_TextChanged(object sender, EventArgs e)
		{
			if (_isUpdating) return;

			if (string.IsNullOrEmpty(metroTextBox2.Text)) return;

			string cleanText = Regex.Replace(metroTextBox2.Text, @"\D", "");

			if (metroTextBox2.Text != cleanText)
			{
				try
				{
					_isUpdating = true;

					metroTextBox2.Text = cleanText;

					if (cleanText.Length > 0)
					{
						metroTextBox2.Select(cleanText.Length, 0);
					}
				}
				finally
				{
					_isUpdating = false;
				}
			}

			_jsonConfig.MSG_DELAY = Convert.ToInt32(Regex.Replace(metroTextBox2.Text, @"[^\d-]", "").Trim());
			File.WriteAllText(configPath, JsonConvert.SerializeObject(_jsonConfig, Formatting.Indented));
			_jsonConfig = Program.getConfig(configPath);
		}

		private void LoadNetworkCheckboxes()
		{
			string networksDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "networks");

			if (!Directory.Exists(networksDir))
				Directory.CreateDirectory(networksDir);

			string[] files = Directory.GetFiles(networksDir, "*.netw");

			metroPanel1.SuspendLayout();
			metroPanel1.Controls.Clear();
			metroPanel1.AutoScroll = true;

			int yOffset = 5;
			int spacing = 20;

			foreach (string filePath in files)
			{
				string fileName = Path.GetFileNameWithoutExtension(filePath);

				var chk = new MetroCheckBox();
				chk.Text = fileName.ToUpper();
				chk.Name = "chk_" + fileName;
				chk.AutoSize = true;
				chk.Location = new System.Drawing.Point(10, yOffset);
				chk.Tag = filePath;
				chk.Checked = true;
				chk.BackColor = System.Drawing.Color.Transparent;
				chk.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
				chk.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(155)))), ((int)(((byte)(155)))), ((int)(((byte)(155)))));
				chk.CheckSignColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
				chk.CheckState = ReaLTaiizor.Enum.Metro.CheckState.Checked;
				chk.Cursor = System.Windows.Forms.Cursors.Hand;
				chk.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
				chk.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
				chk.IsDerivedStyle = true;
				chk.SignStyle = ReaLTaiizor.Enum.Metro.SignStyle.Sign;
				chk.Size = new System.Drawing.Size(80, 16);
				chk.Style = ReaLTaiizor.Enum.Metro.Style.Dark;
				chk.StyleManager = null;
				chk.TabIndex = 2;
				chk.ThemeAuthor = "Taiizor";
				chk.ThemeName = "MetroDark";

				metroPanel1.Controls.Add(chk);
				yOffset += spacing;
			}

			metroPanel1.ResumeLayout(true);
			metroPanel1.PerformLayout();
			metroPanel1.Refresh();
		}

		private List<string> GetSelectedNetworks()
		{
			List<string> selectedNetworks = new List<string>();

			foreach (Control control in metroPanel1.Controls)
				if (control is ReaLTaiizor.Controls.MetroCheckBox chk && chk.Checked)
					selectedNetworks.Add($"-{chk.Text.ToLower()}");

			return selectedNetworks;
		}
	}

	public static class ControlExtensions
	{
		public static void Log(this Control control, string message, int maxCharacters = 10000)
		{
			if (control == null) return;

			if (control.InvokeRequired)
			{
				control.Invoke(new Action(() => control.Log(message, maxCharacters)));
				return;
			}

			string logLine = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

			if (control.Text.Length > maxCharacters)
				control.Text = control.Text.Substring(maxCharacters / 4);

			control.Text += logLine;

			if (control is MetroTextBox tb)
				tb.ScrollToCaret();
		}
	}
}
