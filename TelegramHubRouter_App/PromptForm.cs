using ReaLTaiizor.Controls;
using ReaLTaiizor.Extension.Metro;
using ReaLTaiizor.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;

namespace TelegramHubRouter_App
{
	public partial class PromptForm : MetroForm
	{
		private TextBox txtInput;
		public string ResultText => txtInput.Text;

		public PromptForm(string title, bool isPassword = false)
		{
			InitializeComponent();
			Text = title;
			Size = new Size(300, 80);
			MinimizeBox = false;

			var label = new MetroLabel { Left = 20, Top = 15, Text = title, AutoSize = true, ForeColor = Color.FromArgb(255, 255, 255)};
			txtInput = new TextBox { Left = 20, Top = 40, Width = 260, UseSystemPasswordChar = isPassword, BackColor = Color.FromArgb(0, 0, 0), ForeColor = Color.FromArgb(255, 255, 255) };

			txtInput.KeyDown += txtInput_KeyDown;

			Controls.Add(label);
			Controls.Add(txtInput);
		}

		private void txtInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				DialogResult = DialogResult.OK;
		}
	}
}
