using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TelegramHubRouter_App.AddForms
{
	public partial class SessionSaving : Form
	{
		public SessionSaving()
		{
			InitializeComponent();
		}

		private void SessionSaving_Load(object sender, EventArgs e)
		{
			Task.Run(() =>
			{
				metroProgressBar1.Value = 25;
				Thread.Sleep(50);
				metroProgressBar1.Value = 50;
				Thread.Sleep(50);
				metroProgressBar1.Value = 75;
				Thread.Sleep(50);
				metroProgressBar1.Value = 100;
				Thread.Sleep(50);
			});
		}
	}
}
