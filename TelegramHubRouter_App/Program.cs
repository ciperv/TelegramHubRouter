using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelegramHubRouter_App
{
	internal static class Program
	{
		[DllImport("kernel32.dll")]
		private static extern bool AttachConsole(int dwProcessId);

		private const int ATTACH_PARENT_PROCESS = -1;
		/// <summary>
		/// Главная точка входа для приложения.
		/// </summary>
		[STAThread]
		static void Main()
		{
			string[] args = Environment.GetCommandLineArgs();

			bool _isSilent = false;
			bool _isDev = false;
			bool _isLogsEnabled = false;

			JSON.Config config = new JSON.Config();
			string configPath = "config.yml";

			string sessionPath = Path.Combine(Application.StartupPath, "my_user.session");

			if (!File.Exists(configPath))
				File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));

			config = getConfig(configPath);


			for (int i = 1; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-silent":
					case "-s":
						_isSilent = true;
						break;
					case "-dev":
					case "-d":
						_isDev = true;
						break;
					case "-log":
					case "-l":
						_isLogsEnabled = true;
						break;
					case "-reset":
					case "-r":
						config = new JSON.Config();
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						if (File.Exists(sessionPath))
							File.Delete(sessionPath);
						break;
					case "-configid":
					case "-cid":
						config.API_ID = Convert.ToInt64(Regex.Replace(args[i + 1].Trim(), @"[^\d-]", ""));
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						config = getConfig(configPath);
						i++;
						break;
					case "-confighash":
					case "-chash":
						config.API_HASH = args[i + 1].Trim();
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						config = getConfig(configPath);
						i++;
						break;
					case "-confignum":
					case "-cnum":
						config.PHONE_NUMBER = args[i + 1].Trim();
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						config = getConfig(configPath);
						i++;
						break;
					case "-configsrc":
					case "-csrc":
						config.SOURCE_ID = Convert.ToInt64(Regex.Replace(args[i + 1], @"[^\d-]", "").StartsWith("-100") ? Regex.Replace(args[i + 1], @"[^\d-]", "") : $"-100{Regex.Replace(args[i + 1], @"[^\d-]", "")}");
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						config = getConfig(configPath);
						i++;
						break;
					case "-configpass":
					case "-cpass":
						config.PASSWORD = args[i + 1];
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						config = getConfig(configPath);
						i++;
						break;
					case "-configdelay":
					case "-cdelay":
						config.MSG_DELAY = Convert.ToInt32(Regex.Replace(args[i + 1], @"[^\d-]", "").Trim());
						File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
						config = getConfig(configPath);
						i++;
						break;
				}
			}

			if (!_isSilent)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				MainForm mainForm = new MainForm();
				mainForm.sessionPath = sessionPath;
				Application.Run(mainForm);
			}
			else
			{
				AttachConsole(ATTACH_PARENT_PROCESS);

				var silentWorker = new SilentWorker();
				silentWorker.sessionPath = sessionPath;
				silentWorker.Start(args, _isDev, _isLogsEnabled).GetAwaiter().GetResult();
			}
		}

		public static JSON.Config getConfig(string configPath)
		{
			try
			{
				if (!File.Exists(configPath))
					File.WriteAllText(configPath, JsonConvert.SerializeObject(new JSON.Config(), Formatting.Indented));

				return JsonConvert.DeserializeObject<JSON.Config>(File.ReadAllText(configPath));
			}
			catch
			{
				return null;
			}
		}
	}
}
