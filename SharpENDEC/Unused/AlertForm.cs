using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpENDEC
{
    public partial class AlertForm : Form
    {
        public Action PlayAudio = null;
        public Color EventBackColor;
        public Color EventForeColor;
        public string EventTextContent;

        public AlertForm()
        {
            InitializeComponent();
        }

        private void AlertForm_Load(object sender, EventArgs e)
        {
            EventText.BackColor = EventBackColor;
            EventText.ForeColor = EventForeColor;
            EventText.ForeColor = EventForeColor;
            EventText.Text = EventTextContent;
        }

        private void AlertForm_Shown(object sender, EventArgs e)
        {
            Task.Run(() => PlayAudio());
        }
    }
}
