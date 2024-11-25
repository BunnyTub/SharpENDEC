using System.Diagnostics;

namespace SharpENDEC
{
    public class DiscordWebhook : ISharpPlugin
    {
        public void AlertBlacklisted()
        {
        }
        
        public void AlertRelayingNow(string BroadcastText)
        {
            //Process.Start("cmd.exe", $"/c start cmd.exe /k echo {BroadcastText}");
        }

        public void ProvideBroadcastText(string text)
        {
        }

        public bool RelayAlert(string info, string status, string MsgType, string severity, string urgency, string broadcastImmediately)
        {
            return true;
        }
    }
}
