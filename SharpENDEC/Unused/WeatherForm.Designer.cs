namespace SharpENDEC
{
    partial class WeatherForm
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
            this.MessagePanel = new System.Windows.Forms.Panel();
            this.FrenchLangCheckBox = new System.Windows.Forms.CheckBox();
            this.MessageText = new System.Windows.Forms.Label();
            this.WarningPanel = new System.Windows.Forms.Panel();
            this.WarningText = new System.Windows.Forms.Label();
            this.WatchPanel = new System.Windows.Forms.Panel();
            this.WatchText = new System.Windows.Forms.Label();
            this.AdvisoryPanel = new System.Windows.Forms.Panel();
            this.AdvisoryText = new System.Windows.Forms.Label();
            this.WeatherButton = new System.Windows.Forms.Button();
            this.sTest = new System.Windows.Forms.CheckBox();
            this.sActual = new System.Windows.Forms.CheckBox();
            this.mtAlert = new System.Windows.Forms.CheckBox();
            this.mtUpdate = new System.Windows.Forms.CheckBox();
            this.mtCancel = new System.Windows.Forms.CheckBox();
            this.mtTest = new System.Windows.Forms.CheckBox();
            this.Extreme = new System.Windows.Forms.CheckBox();
            this.Severe = new System.Windows.Forms.CheckBox();
            this.Moderate = new System.Windows.Forms.CheckBox();
            this.Minor = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.uPast = new System.Windows.Forms.CheckBox();
            this.uFuture = new System.Windows.Forms.CheckBox();
            this.uExpect = new System.Windows.Forms.CheckBox();
            this.uImmed = new System.Windows.Forms.CheckBox();
            this.Unknown = new System.Windows.Forms.CheckBox();
            this.MessagePanel.SuspendLayout();
            this.WarningPanel.SuspendLayout();
            this.WatchPanel.SuspendLayout();
            this.AdvisoryPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MessagePanel
            // 
            this.MessagePanel.BackColor = System.Drawing.Color.Black;
            this.MessagePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MessagePanel.Controls.Add(this.FrenchLangCheckBox);
            this.MessagePanel.Controls.Add(this.MessageText);
            this.MessagePanel.ForeColor = System.Drawing.Color.Lime;
            this.MessagePanel.Location = new System.Drawing.Point(12, 13);
            this.MessagePanel.Name = "MessagePanel";
            this.MessagePanel.Size = new System.Drawing.Size(500, 192);
            this.MessagePanel.TabIndex = 0;
            // 
            // FrenchLangCheckBox
            // 
            this.FrenchLangCheckBox.AutoSize = true;
            this.FrenchLangCheckBox.Location = new System.Drawing.Point(426, 3);
            this.FrenchLangCheckBox.Name = "FrenchLangCheckBox";
            this.FrenchLangCheckBox.Size = new System.Drawing.Size(69, 19);
            this.FrenchLangCheckBox.TabIndex = 5;
            this.FrenchLangCheckBox.Text = "Français";
            this.FrenchLangCheckBox.UseVisualStyleBackColor = true;
            this.FrenchLangCheckBox.CheckedChanged += new System.EventHandler(this.FrenchLangCheckBox_CheckedChanged);
            // 
            // MessageText
            // 
            this.MessageText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageText.Font = new System.Drawing.Font("Segoe UI", 25F, System.Drawing.FontStyle.Bold);
            this.MessageText.ForeColor = System.Drawing.Color.White;
            this.MessageText.Location = new System.Drawing.Point(0, 0);
            this.MessageText.Margin = new System.Windows.Forms.Padding(0);
            this.MessageText.Name = "MessageText";
            this.MessageText.Size = new System.Drawing.Size(498, 190);
            this.MessageText.TabIndex = 1;
            this.MessageText.Text = "SV-200\r\n728, 382";
            this.MessageText.Click += new System.EventHandler(this.MessageText_Click);
            // 
            // WarningPanel
            // 
            this.WarningPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WarningPanel.Controls.Add(this.WarningText);
            this.WarningPanel.ForeColor = System.Drawing.Color.Red;
            this.WarningPanel.Location = new System.Drawing.Point(518, 13);
            this.WarningPanel.Name = "WarningPanel";
            this.WarningPanel.Size = new System.Drawing.Size(182, 60);
            this.WarningPanel.TabIndex = 1;
            // 
            // WarningText
            // 
            this.WarningText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WarningText.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.WarningText.ForeColor = System.Drawing.Color.Red;
            this.WarningText.Location = new System.Drawing.Point(0, 0);
            this.WarningText.Margin = new System.Windows.Forms.Padding(0);
            this.WarningText.Name = "WarningText";
            this.WarningText.Size = new System.Drawing.Size(180, 58);
            this.WarningText.TabIndex = 2;
            this.WarningText.Text = "Warning";
            this.WarningText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.WarningText.Click += new System.EventHandler(this.WarningText_Click);
            // 
            // WatchPanel
            // 
            this.WatchPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WatchPanel.Controls.Add(this.WatchText);
            this.WatchPanel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(0)))));
            this.WatchPanel.Location = new System.Drawing.Point(518, 79);
            this.WatchPanel.Name = "WatchPanel";
            this.WatchPanel.Size = new System.Drawing.Size(182, 60);
            this.WatchPanel.TabIndex = 2;
            // 
            // WatchText
            // 
            this.WatchText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WatchText.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.WatchText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(127)))), ((int)(((byte)(0)))));
            this.WatchText.Location = new System.Drawing.Point(0, 0);
            this.WatchText.Margin = new System.Windows.Forms.Padding(0);
            this.WatchText.Name = "WatchText";
            this.WatchText.Size = new System.Drawing.Size(180, 58);
            this.WatchText.TabIndex = 3;
            this.WatchText.Text = "Watch";
            this.WatchText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.WatchText.Click += new System.EventHandler(this.WatchText_Click);
            // 
            // AdvisoryPanel
            // 
            this.AdvisoryPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AdvisoryPanel.Controls.Add(this.AdvisoryText);
            this.AdvisoryPanel.ForeColor = System.Drawing.Color.Yellow;
            this.AdvisoryPanel.Location = new System.Drawing.Point(518, 145);
            this.AdvisoryPanel.Name = "AdvisoryPanel";
            this.AdvisoryPanel.Size = new System.Drawing.Size(182, 60);
            this.AdvisoryPanel.TabIndex = 3;
            // 
            // AdvisoryText
            // 
            this.AdvisoryText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdvisoryText.Font = new System.Drawing.Font("Segoe UI", 20F);
            this.AdvisoryText.ForeColor = System.Drawing.Color.Yellow;
            this.AdvisoryText.Location = new System.Drawing.Point(0, 0);
            this.AdvisoryText.Margin = new System.Windows.Forms.Padding(0);
            this.AdvisoryText.Name = "AdvisoryText";
            this.AdvisoryText.Size = new System.Drawing.Size(180, 58);
            this.AdvisoryText.TabIndex = 3;
            this.AdvisoryText.Text = "Advisory";
            this.AdvisoryText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AdvisoryText.Click += new System.EventHandler(this.AdvisoryText_Click);
            // 
            // WeatherButton
            // 
            this.WeatherButton.BackColor = System.Drawing.Color.DimGray;
            this.WeatherButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.WeatherButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.WeatherButton.Font = new System.Drawing.Font("Segoe UI", 48F, System.Drawing.FontStyle.Bold);
            this.WeatherButton.Location = new System.Drawing.Point(12, 211);
            this.WeatherButton.Name = "WeatherButton";
            this.WeatherButton.Size = new System.Drawing.Size(500, 120);
            this.WeatherButton.TabIndex = 4;
            this.WeatherButton.Text = "WEATHER";
            this.WeatherButton.UseVisualStyleBackColor = false;
            this.WeatherButton.Click += new System.EventHandler(this.WeatherButton_Click);
            // 
            // sTest
            // 
            this.sTest.AutoSize = true;
            this.sTest.Location = new System.Drawing.Point(3, 3);
            this.sTest.Name = "sTest";
            this.sTest.Size = new System.Drawing.Size(51, 19);
            this.sTest.TabIndex = 6;
            this.sTest.Text = "sTest";
            this.sTest.UseVisualStyleBackColor = true;
            this.sTest.CheckedChanged += new System.EventHandler(this.sTest_CheckedChanged);
            // 
            // sActual
            // 
            this.sActual.AutoSize = true;
            this.sActual.Location = new System.Drawing.Point(78, 3);
            this.sActual.Name = "sActual";
            this.sActual.Size = new System.Drawing.Size(65, 19);
            this.sActual.TabIndex = 7;
            this.sActual.Text = "sActual";
            this.sActual.UseVisualStyleBackColor = true;
            this.sActual.CheckedChanged += new System.EventHandler(this.sActual_CheckedChanged);
            // 
            // mtAlert
            // 
            this.mtAlert.AutoSize = true;
            this.mtAlert.Location = new System.Drawing.Point(3, 28);
            this.mtAlert.Name = "mtAlert";
            this.mtAlert.Size = new System.Drawing.Size(66, 19);
            this.mtAlert.TabIndex = 8;
            this.mtAlert.Text = "mtAlert";
            this.mtAlert.UseVisualStyleBackColor = true;
            this.mtAlert.CheckedChanged += new System.EventHandler(this.mtAlert_CheckedChanged);
            // 
            // mtUpdate
            // 
            this.mtUpdate.AutoSize = true;
            this.mtUpdate.Location = new System.Drawing.Point(78, 28);
            this.mtUpdate.Name = "mtUpdate";
            this.mtUpdate.Size = new System.Drawing.Size(79, 19);
            this.mtUpdate.TabIndex = 9;
            this.mtUpdate.Text = "mtUpdate";
            this.mtUpdate.UseVisualStyleBackColor = true;
            this.mtUpdate.CheckedChanged += new System.EventHandler(this.mtUpdate_CheckedChanged);
            // 
            // mtCancel
            // 
            this.mtCancel.AutoSize = true;
            this.mtCancel.Location = new System.Drawing.Point(3, 53);
            this.mtCancel.Name = "mtCancel";
            this.mtCancel.Size = new System.Drawing.Size(77, 19);
            this.mtCancel.TabIndex = 10;
            this.mtCancel.Text = "mtCancel";
            this.mtCancel.UseVisualStyleBackColor = true;
            this.mtCancel.CheckedChanged += new System.EventHandler(this.mtCancel_CheckedChanged);
            // 
            // mtTest
            // 
            this.mtTest.AutoSize = true;
            this.mtTest.Location = new System.Drawing.Point(78, 53);
            this.mtTest.Name = "mtTest";
            this.mtTest.Size = new System.Drawing.Size(61, 19);
            this.mtTest.TabIndex = 11;
            this.mtTest.Text = "mtTest";
            this.mtTest.UseVisualStyleBackColor = true;
            this.mtTest.CheckedChanged += new System.EventHandler(this.mtTest_CheckedChanged);
            // 
            // Extreme
            // 
            this.Extreme.AutoSize = true;
            this.Extreme.Location = new System.Drawing.Point(3, 78);
            this.Extreme.Name = "Extreme";
            this.Extreme.Size = new System.Drawing.Size(69, 19);
            this.Extreme.TabIndex = 12;
            this.Extreme.Text = "Extreme";
            this.Extreme.UseVisualStyleBackColor = true;
            this.Extreme.CheckedChanged += new System.EventHandler(this.Extreme_CheckedChanged);
            // 
            // Severe
            // 
            this.Severe.AutoSize = true;
            this.Severe.Location = new System.Drawing.Point(78, 78);
            this.Severe.Name = "Severe";
            this.Severe.Size = new System.Drawing.Size(60, 19);
            this.Severe.TabIndex = 13;
            this.Severe.Text = "Severe";
            this.Severe.UseVisualStyleBackColor = true;
            this.Severe.CheckedChanged += new System.EventHandler(this.Severe_CheckedChanged);
            // 
            // Moderate
            // 
            this.Moderate.AutoSize = true;
            this.Moderate.Location = new System.Drawing.Point(3, 103);
            this.Moderate.Name = "Moderate";
            this.Moderate.Size = new System.Drawing.Size(77, 19);
            this.Moderate.TabIndex = 14;
            this.Moderate.Text = "Moderate";
            this.Moderate.UseVisualStyleBackColor = true;
            this.Moderate.CheckedChanged += new System.EventHandler(this.Moderate_CheckedChanged);
            // 
            // Minor
            // 
            this.Minor.AutoSize = true;
            this.Minor.Location = new System.Drawing.Point(78, 103);
            this.Minor.Name = "Minor";
            this.Minor.Size = new System.Drawing.Size(58, 19);
            this.Minor.TabIndex = 15;
            this.Minor.Text = "Minor";
            this.Minor.UseVisualStyleBackColor = true;
            this.Minor.CheckedChanged += new System.EventHandler(this.Minor_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.uPast);
            this.panel1.Controls.Add(this.uFuture);
            this.panel1.Controls.Add(this.uExpect);
            this.panel1.Controls.Add(this.uImmed);
            this.panel1.Controls.Add(this.Unknown);
            this.panel1.Controls.Add(this.Minor);
            this.panel1.Controls.Add(this.sTest);
            this.panel1.Controls.Add(this.Moderate);
            this.panel1.Controls.Add(this.sActual);
            this.panel1.Controls.Add(this.Severe);
            this.panel1.Controls.Add(this.mtAlert);
            this.panel1.Controls.Add(this.Extreme);
            this.panel1.Controls.Add(this.mtUpdate);
            this.panel1.Controls.Add(this.mtTest);
            this.panel1.Controls.Add(this.mtCancel);
            this.panel1.Location = new System.Drawing.Point(518, 211);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(181, 119);
            this.panel1.TabIndex = 16;
            // 
            // uPast
            // 
            this.uPast.AutoSize = true;
            this.uPast.Location = new System.Drawing.Point(3, 178);
            this.uPast.Name = "uPast";
            this.uPast.Size = new System.Drawing.Size(55, 19);
            this.uPast.TabIndex = 20;
            this.uPast.Text = "uPast";
            this.uPast.UseVisualStyleBackColor = true;
            this.uPast.CheckedChanged += new System.EventHandler(this.uPast_CheckedChanged);
            // 
            // uFuture
            // 
            this.uFuture.AutoSize = true;
            this.uFuture.Location = new System.Drawing.Point(77, 153);
            this.uFuture.Name = "uFuture";
            this.uFuture.Size = new System.Drawing.Size(67, 19);
            this.uFuture.TabIndex = 19;
            this.uFuture.Text = "uFuture";
            this.uFuture.UseVisualStyleBackColor = true;
            this.uFuture.CheckedChanged += new System.EventHandler(this.uFuture_CheckedChanged);
            // 
            // uExpect
            // 
            this.uExpect.AutoSize = true;
            this.uExpect.Location = new System.Drawing.Point(3, 153);
            this.uExpect.Name = "uExpect";
            this.uExpect.Size = new System.Drawing.Size(68, 19);
            this.uExpect.TabIndex = 18;
            this.uExpect.Text = "uExpect";
            this.uExpect.UseVisualStyleBackColor = true;
            this.uExpect.CheckedChanged += new System.EventHandler(this.uExpect_CheckedChanged);
            // 
            // uImmed
            // 
            this.uImmed.AutoSize = true;
            this.uImmed.Location = new System.Drawing.Point(78, 128);
            this.uImmed.Name = "uImmed";
            this.uImmed.Size = new System.Drawing.Size(71, 19);
            this.uImmed.TabIndex = 17;
            this.uImmed.Text = "uImmed";
            this.uImmed.UseVisualStyleBackColor = true;
            this.uImmed.CheckedChanged += new System.EventHandler(this.uImmed_CheckedChanged);
            // 
            // Unknown
            // 
            this.Unknown.AutoSize = true;
            this.Unknown.Location = new System.Drawing.Point(3, 128);
            this.Unknown.Name = "Unknown";
            this.Unknown.Size = new System.Drawing.Size(77, 19);
            this.Unknown.TabIndex = 16;
            this.Unknown.Text = "Unknown";
            this.Unknown.UseVisualStyleBackColor = true;
            this.Unknown.CheckedChanged += new System.EventHandler(this.Unknown_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(712, 343);
            this.Controls.Add(this.WeatherButton);
            this.Controls.Add(this.AdvisoryPanel);
            this.Controls.Add(this.WatchPanel);
            this.Controls.Add(this.WarningPanel);
            this.Controls.Add(this.MessagePanel);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SV-200";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MessagePanel.ResumeLayout(false);
            this.MessagePanel.PerformLayout();
            this.WarningPanel.ResumeLayout(false);
            this.WatchPanel.ResumeLayout(false);
            this.AdvisoryPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel MessagePanel;
        private System.Windows.Forms.Label MessageText;
        private System.Windows.Forms.Panel WarningPanel;
        private System.Windows.Forms.Panel WatchPanel;
        private System.Windows.Forms.Panel AdvisoryPanel;
        private System.Windows.Forms.Label WarningText;
        private System.Windows.Forms.Label WatchText;
        private System.Windows.Forms.Label AdvisoryText;
        private System.Windows.Forms.Button WeatherButton;
        private System.Windows.Forms.CheckBox FrenchLangCheckBox;
        private System.Windows.Forms.CheckBox sTest;
        private System.Windows.Forms.CheckBox sActual;
        private System.Windows.Forms.CheckBox mtAlert;
        private System.Windows.Forms.CheckBox mtUpdate;
        private System.Windows.Forms.CheckBox mtCancel;
        private System.Windows.Forms.CheckBox mtTest;
        private System.Windows.Forms.CheckBox Extreme;
        private System.Windows.Forms.CheckBox Severe;
        private System.Windows.Forms.CheckBox Moderate;
        private System.Windows.Forms.CheckBox Minor;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox uImmed;
        private System.Windows.Forms.CheckBox Unknown;
        private System.Windows.Forms.CheckBox uFuture;
        private System.Windows.Forms.CheckBox uExpect;
        private System.Windows.Forms.CheckBox uPast;
    }
}

