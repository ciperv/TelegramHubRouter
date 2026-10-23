namespace TelegramHubRouter_App.AddForms
{
	partial class SessionSaving
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SessionSaving));
			this.metroProgressBar1 = new ReaLTaiizor.Controls.MetroProgressBar();
			this.metroLabel1 = new ReaLTaiizor.Controls.MetroLabel();
			this.SuspendLayout();
			// 
			// metroProgressBar1
			// 
			this.metroProgressBar1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
			this.metroProgressBar1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
			this.metroProgressBar1.DisabledBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
			this.metroProgressBar1.DisabledBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
			this.metroProgressBar1.DisabledProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
			this.metroProgressBar1.IsDerivedStyle = true;
			this.metroProgressBar1.Location = new System.Drawing.Point(12, 35);
			this.metroProgressBar1.Maximum = 100;
			this.metroProgressBar1.Minimum = 0;
			this.metroProgressBar1.Name = "metroProgressBar1";
			this.metroProgressBar1.Orientation = ReaLTaiizor.Enum.Metro.ProgressOrientation.Horizontal;
			this.metroProgressBar1.ProgressColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(177)))), ((int)(((byte)(225)))));
			this.metroProgressBar1.Size = new System.Drawing.Size(260, 15);
			this.metroProgressBar1.Style = ReaLTaiizor.Enum.Metro.Style.Light;
			this.metroProgressBar1.StyleManager = null;
			this.metroProgressBar1.TabIndex = 0;
			this.metroProgressBar1.Text = "Saving...";
			this.metroProgressBar1.ThemeAuthor = "Taiizor";
			this.metroProgressBar1.ThemeName = "MetroLight";
			this.metroProgressBar1.Value = 0;
			// 
			// metroLabel1
			// 
			this.metroLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.metroLabel1.IsDerivedStyle = true;
			this.metroLabel1.Location = new System.Drawing.Point(12, 9);
			this.metroLabel1.Name = "metroLabel1";
			this.metroLabel1.Size = new System.Drawing.Size(260, 23);
			this.metroLabel1.Style = ReaLTaiizor.Enum.Metro.Style.Dark;
			this.metroLabel1.StyleManager = null;
			this.metroLabel1.TabIndex = 1;
			this.metroLabel1.Text = "Saving .session file";
			this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.metroLabel1.ThemeAuthor = "Taiizor";
			this.metroLabel1.ThemeName = "MetroDark";
			// 
			// SessionSaving
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.ClientSize = new System.Drawing.Size(284, 65);
			this.Controls.Add(this.metroLabel1);
			this.Controls.Add(this.metroProgressBar1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SessionSaving";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SessionSaving";
			this.Load += new System.EventHandler(this.SessionSaving_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private ReaLTaiizor.Controls.MetroProgressBar metroProgressBar1;
		private ReaLTaiizor.Controls.MetroLabel metroLabel1;
	}
}