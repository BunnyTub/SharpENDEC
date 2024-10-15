using SharpENDEC.Properties;
using System;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpENDEC
{
    public partial class WeatherForm : Form
    {
        private readonly bool AllowFiringEvents = false;

        public WeatherForm()
        {
            InitializeComponent();
            AllowFiringEvents = false;
            sTest.Checked = Settings.Default.statusTest;
            sActual.Checked = Settings.Default.statusActual;
            mtAlert.Checked = Settings.Default.messageTypeAlert;
            mtUpdate.Checked = Settings.Default.messageTypeUpdate;
            mtCancel.Checked = Settings.Default.messageTypeCancel;
            mtTest.Checked = Settings.Default.messageTypeTest;
            Extreme.Checked = Settings.Default.severityExtreme;
            Severe.Checked = Settings.Default.severitySevere;
            Moderate.Checked = Settings.Default.severityModerate;
            Minor.Checked = Settings.Default.severityMinor;
            Unknown.Checked = Settings.Default.severityUnknown;
            uImmed.Checked = Settings.Default.urgencyImmediate;
            uExpect.Checked = Settings.Default.urgencyExpected;
            uFuture.Checked = Settings.Default.urgencyFuture;
            uPast.Checked = Settings.Default.urgencyPast;
            AllowFiringEvents = true;
        }

        public void Alert(DateTime datetime, int type)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void MessageText_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                Flasher.RevertAllLights(WarningText, WatchText, AdvisoryText);
            }).Start();
        }

        private void WarningText_Click(object sender, EventArgs e)
        {
            StartFlash(sender, Color.FromArgb(255, 255, 0, 0), Color.Black);
        }

        private void WatchText_Click(object sender, EventArgs e)
        {
            StartFlash(sender, Color.FromArgb(255, 255, 127, 0), Color.Black);
        }

        private void AdvisoryText_Click(object sender, EventArgs e)
        {
            StartFlash(sender, Color.FromArgb(255, 255, 255, 0), Color.Black);
        }

        private void StartFlash(object sender, Color BackColor, Color ForeColor)
        {
            IntPtr handle = this.Handle;
            Task.Run(() =>
            {
                Flasher.FlashLight(handle, (Label)sender, BackColor, ForeColor);
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Flasher.Flashing || Flasher.Resetting)
            {
                Flasher.FlashWindow(this.Handle, true);
                e.Cancel = true;
            }
        }

        private void WeatherButton_Click(object sender, EventArgs e)
        {

        }

        private void FrenchLangCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //Settings.Default.CurrentLanguage = ((CheckBox)sender).Checked;
            //Settings.Default.Save();
            //SVDictionary.IsFrench = ((CheckBox)sender).Checked;
        }

        private void sTest_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.statusTest = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void sActual_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.statusActual = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void mtAlert_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.messageTypeAlert = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void mtUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.messageTypeUpdate = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void mtCancel_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.messageTypeCancel = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void mtTest_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.messageTypeTest = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void Extreme_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.severityExtreme = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void Severe_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.severitySevere = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void Moderate_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.severityModerate = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void Minor_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.severityMinor = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void Unknown_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.severityUnknown = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void uImmed_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.urgencyImmediate = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void uExpect_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.urgencyExpected = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void uFuture_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.urgencyFuture = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void uPast_CheckedChanged(object sender, EventArgs e)
        {
            if (!AllowFiringEvents) return;
            Settings.Default.urgencyPast = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }
    }

    public static class Flasher
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_CAPTION = 1;
        public const uint FLASHW_TRAY = 2;
        public const uint FLASHW_ALL = FLASHW_CAPTION | FLASHW_TRAY;
        public const uint FLASHW_TIMER = 4;
        public const uint FLASHW_TIMERNOFG = 12;

        public static void FlashWindow(IntPtr handle, bool beep)
        {
            FLASHWINFO fw = new FLASHWINFO();

            fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(fw));
            fw.hwnd = handle;
            fw.dwFlags = FLASHW_ALL;
            fw.uCount = 6;
            if (beep)
            {
                fw.dwTimeout = 100;
            }
            else
            {
                fw.dwTimeout = 500;
            }
            fw.dwTimeout = 500;
            if (beep) SystemSounds.Beep.Play();

            FlashWindowEx(ref fw);
        }

        private static bool FlashInProgress = false;
        private static bool ResetInProgress = false;

        public static bool Flashing
        {
            get { return FlashInProgress; }
        }

        public static bool Resetting
        {
            get { return ResetInProgress; }
        }

        public static void RevertAllLights(Label Warning, Label Watch, Label Advisory)
        {
            if (ResetInProgress) return;
            ResetInProgress = true;
            while (FlashInProgress)
            {
                Thread.Sleep(100);
            }
            Warning.Font = new Font(Warning.Font, FontStyle.Regular);
            Watch.Font = new Font(Watch.Font, FontStyle.Regular);
            Advisory.Font = new Font(Advisory.Font, FontStyle.Regular);
            Warning.BackColor = Color.Black;
            Watch.BackColor = Color.Black;
            Advisory.BackColor = Color.Black;
            Warning.ForeColor = Color.FromArgb(255, 255, 0, 0);
            Watch.ForeColor = Color.FromArgb(255, 255, 127, 0);
            Advisory.ForeColor = Color.FromArgb(255, 255, 255, 0);
            ResetInProgress = false;
        }

        public static void FlashLight(IntPtr handle, Label UsedLabel,
            Color BackColor, Color ForeColor)
        {
            if (ResetInProgress || FlashInProgress) return;
            FlashInProgress = true;
            UsedLabel.Font = new Font(UsedLabel.Font, FontStyle.Bold);
            FlashWindow(handle, false);
            for (int _ = 0; _ < 6; _++)
            {
                UsedLabel.Invoke(new MethodInvoker(() =>
                {
                    UsedLabel.BackColor = Color.White;
                    UsedLabel.ForeColor = Color.Black;
                }));
                Thread.Sleep(500);
                UsedLabel.Invoke(new MethodInvoker(() =>
                {
                    UsedLabel.BackColor = BackColor;
                    UsedLabel.ForeColor = ForeColor;
                }));
                Thread.Sleep(500);
            }
            FlashInProgress = false;
        }
    }
}
