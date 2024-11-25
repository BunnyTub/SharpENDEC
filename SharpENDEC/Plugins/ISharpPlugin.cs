namespace SharpENDEC
{
    internal interface ISharpPlugin
    {
        void AlertBlacklisted();
        void AlertRelayingNow(string BroadcastText);
        void ProvideBroadcastText(string text);
        bool RelayAlert(string info, string status, string MsgType, string severity, string urgency, string broadcastImmediately);
    }
}
