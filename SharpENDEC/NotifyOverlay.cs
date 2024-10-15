using System;
using System.Drawing;
using System.Windows.Forms;

namespace SharpENDEC
{
    public partial class NotifyOverlay : Form
    {
        public string EventShortInfoText { get; set; }
        public string EventLongInfoText { get; set; }
        public string EventTypeText { get; set; }

        public NotifyOverlay()
        {
            InitializeComponent();
        }

        private void NotifyOverlay_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(EventShortInfoText) || 
                string.IsNullOrEmpty(EventLongInfoText) ||
                string.IsNullOrEmpty(EventTypeText))
            {
                MessageBox.Show("An emergency alert was passed with missing parameters.\r\n" +
                    "Please report this problem with your logs!");
                this.Close();
            }
            else
            {
                Rectangle workingArea = Screen.GetWorkingArea(this);

                int x = workingArea.Right - this.Width - 5;
                int y = workingArea.Top + 5;

                this.Location = new Point(x, y);

                AlertDescriptionText.Text = EventShortInfoText;
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InfoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(EventLongInfoText, $"SharpENDEC - {EventTypeText}");
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
