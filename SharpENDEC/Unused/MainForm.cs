using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpENDEC
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            //SVInfo.StatusChanged += (status) => label1.Text = status.ToString();
        }
    }
}
