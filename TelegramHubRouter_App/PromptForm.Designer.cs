using ReaLTaiizor.Controls;
using System.Drawing;
using System.Windows.Forms;
using static ReaLTaiizor.Drawing.Poison.PoisonPaint.ForeColor;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramHubRouter_App
{
	partial class PromptForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PromptForm));
			this.SuspendLayout();
			// 
			// PromptForm
			// 
			this.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ClientSize = new System.Drawing.Size(300, 300);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PromptForm";
			this.Padding = new System.Windows.Forms.Padding(12, 90, 12, 12);
			this.ShowLeftRect = false;
			this.ShowTitle = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Style = ReaLTaiizor.Enum.Metro.Style.Dark;
			this.TextColor = System.Drawing.Color.White;
			this.ThemeName = "MetroDark";
			this.ResumeLayout(false);

		}

		#endregion
	}
}