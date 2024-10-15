namespace SharpENDEC
{
    partial class NotifyOverlay
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
            this.components = new System.ComponentModel.Container();
            this.BackPanel = new System.Windows.Forms.Panel();
            this.DataPanel = new System.Windows.Forms.Panel();
            this.AlertTitleText = new System.Windows.Forms.Label();
            this.AlertDescriptionText = new System.Windows.Forms.Label();
            this.CloseButton = new System.Windows.Forms.Button();
            this.InfoButton = new System.Windows.Forms.Button();
            this.AutoCloseTimer = new System.Windows.Forms.Timer(this.components);
            this.BackPanel.SuspendLayout();
            this.DataPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BackPanel
            // 
            this.BackPanel.BackColor = System.Drawing.Color.Red;
            this.BackPanel.Controls.Add(this.DataPanel);
            this.BackPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BackPanel.Location = new System.Drawing.Point(0, 0);
            this.BackPanel.Name = "BackPanel";
            this.BackPanel.Size = new System.Drawing.Size(400, 180);
            this.BackPanel.TabIndex = 0;
            // 
            // DataPanel
            // 
            this.DataPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DataPanel.BackColor = System.Drawing.Color.Black;
            this.DataPanel.Controls.Add(this.InfoButton);
            this.DataPanel.Controls.Add(this.CloseButton);
            this.DataPanel.Controls.Add(this.AlertDescriptionText);
            this.DataPanel.Controls.Add(this.AlertTitleText);
            this.DataPanel.Location = new System.Drawing.Point(3, 3);
            this.DataPanel.Name = "DataPanel";
            this.DataPanel.Size = new System.Drawing.Size(394, 174);
            this.DataPanel.TabIndex = 0;
            // 
            // AlertTitleText
            // 
            this.AlertTitleText.BackColor = System.Drawing.Color.Red;
            this.AlertTitleText.Dock = System.Windows.Forms.DockStyle.Top;
            this.AlertTitleText.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.AlertTitleText.Location = new System.Drawing.Point(0, 0);
            this.AlertTitleText.Name = "AlertTitleText";
            this.AlertTitleText.Size = new System.Drawing.Size(394, 50);
            this.AlertTitleText.TabIndex = 0;
            this.AlertTitleText.Text = "EMERGENCY ALERT";
            this.AlertTitleText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AlertDescriptionText
            // 
            this.AlertDescriptionText.BackColor = System.Drawing.Color.Black;
            this.AlertDescriptionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AlertDescriptionText.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.AlertDescriptionText.Location = new System.Drawing.Point(0, 50);
            this.AlertDescriptionText.Name = "AlertDescriptionText";
            this.AlertDescriptionText.Size = new System.Drawing.Size(394, 124);
            this.AlertDescriptionText.TabIndex = 1;
            this.AlertDescriptionText.Text = "!";
            this.AlertDescriptionText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.BackColor = System.Drawing.Color.Maroon;
            this.CloseButton.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.CloseButton.Location = new System.Drawing.Point(369, -3);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(28, 28);
            this.CloseButton.TabIndex = 2;
            this.CloseButton.UseVisualStyleBackColor = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // InfoButton
            // 
            this.InfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoButton.BackColor = System.Drawing.Color.Gold;
            this.InfoButton.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.InfoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.InfoButton.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.InfoButton.Location = new System.Drawing.Point(369, 26);
            this.InfoButton.Name = "InfoButton";
            this.InfoButton.Size = new System.Drawing.Size(28, 14);
            this.InfoButton.TabIndex = 3;
            this.InfoButton.UseVisualStyleBackColor = false;
            this.InfoButton.Click += new System.EventHandler(this.InfoButton_Click);
            // 
            // AutoCloseTimer
            // 
            this.AutoCloseTimer.Enabled = true;
            this.AutoCloseTimer.Interval = 30000;
            this.AutoCloseTimer.Tick += new System.EventHandler(this.AutoCloseTimer_Tick);
            // 
            // NotifyOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(400, 180);
            this.ControlBox = false;
            this.Controls.Add(this.BackPanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotifyOverlay";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SharpENDEC Notification Overlay";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.NotifyOverlay_Load);
            this.BackPanel.ResumeLayout(false);
            this.DataPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel BackPanel;
        private System.Windows.Forms.Panel DataPanel;
        private System.Windows.Forms.Label AlertTitleText;
        private System.Windows.Forms.Label AlertDescriptionText;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button InfoButton;
        private System.Windows.Forms.Timer AutoCloseTimer;
    }
}