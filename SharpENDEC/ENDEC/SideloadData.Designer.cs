namespace SharpENDEC
{
    partial class SideloadData
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
            this.SideloadButton = new System.Windows.Forms.Button();
            this.SideloadInput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // SideloadButton
            // 
            this.SideloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SideloadButton.Location = new System.Drawing.Point(365, 287);
            this.SideloadButton.Name = "SideloadButton";
            this.SideloadButton.Size = new System.Drawing.Size(75, 23);
            this.SideloadButton.TabIndex = 0;
            this.SideloadButton.Text = "Sideload";
            this.SideloadButton.UseVisualStyleBackColor = true;
            this.SideloadButton.Click += new System.EventHandler(this.SideloadButton_Click);
            // 
            // SideloadInput
            // 
            this.SideloadInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SideloadInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SideloadInput.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.SideloadInput.Location = new System.Drawing.Point(12, 12);
            this.SideloadInput.MaxLength = 1000000000;
            this.SideloadInput.Multiline = true;
            this.SideloadInput.Name = "SideloadInput";
            this.SideloadInput.Size = new System.Drawing.Size(428, 269);
            this.SideloadInput.TabIndex = 1;
            // 
            // SideloadData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(452, 322);
            this.Controls.Add(this.SideloadInput);
            this.Controls.Add(this.SideloadButton);
            this.Name = "SideloadData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sideload XML Data";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SideloadButton;
        private System.Windows.Forms.TextBox SideloadInput;
    }
}