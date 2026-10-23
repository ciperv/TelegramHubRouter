using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramHubRouter_App.JSON
{
	public class Config
	{
		public string API_HASH { get; set; } = "";
		public long SOURCE_ID { get; set; } = -100;
		public int MSG_DELAY { get; set; } = 400;
		public long API_ID { get; set; } = 0;
		public string PHONE_NUMBER { get; set; } = "";
		public string PASSWORD { get; set; } = "";
	}
}
