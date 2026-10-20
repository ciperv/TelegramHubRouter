namespace TelegramHubRouter_Server.Worker
{
	public static class NetworkManager
	{
		public static bool GetNetworks(ref List<string> networks, string networksPath)
		{
			if (!Directory.Exists(networksPath))
				Directory.CreateDirectory(networksPath);

			string[] files = Directory.GetFiles(networksPath, "*.netw");

			foreach (string filePath in files)
				networks.Add(Path.GetFileNameWithoutExtension(filePath));

			if (networks.Count != 0) return true;
			else return false;
		}
	}
}
