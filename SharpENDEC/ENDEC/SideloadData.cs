using System;
using System.Windows.Forms;
using static SharpENDEC.ENDEC;

namespace SharpENDEC
{
    public partial class SideloadData : Form
    {
        public SideloadData()
        {
            InitializeComponent();
        }

        private void SideloadButton_Click(object sender, EventArgs e)
        {
            lock (SharpDataQueue)
            SharpDataQueue.Add(new SharpDataItem(DateTime.Now.ToString(), SideloadInput.Text));
            SideloadInput.Clear();
        }
    }
}
