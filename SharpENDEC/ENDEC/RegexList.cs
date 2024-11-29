using System.Text.RegularExpressions;

namespace SharpENDEC
{
    public static partial class ENDEC
    {
        //public static readonly Regex ValueNameRegex = new Regex(
        //    @"<valueName>([^<]+)</valueName>\s*<value>([^<]+)</value>",
        //    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex SentRegex = new Regex(
            @"<sent>\s*(.*?)\s*</sent>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex StatusRegex = new Regex(
            @"<status>\s*(.*?)\s*</status>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex MessageTypeRegex = new Regex(
            @"<msgType>\s*(.*?)\s*</msgType>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex BroadcastImmediatelyRegex = new Regex(
            @"<valueName>layer:SOREM:1.0:Broadcast_Immediately</valueName>\s*<value>\s*(.*?)\s*</value>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex UrgencyRegex = new Regex(
            @"<urgency>\s*(.*?)\s*</urgency>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex SeverityRegex = new Regex(
            @"<severity>\s*(.*?)\s*</severity>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex InfoRegex = new Regex(
            @"<info>\s*(.*?)\s*</info>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex EffectiveRegex = new Regex(
            @"<effective>\s*(.*?)\s*</effective>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex ExpiresRegex = new Regex(
            @"<expires>\s*(.*?)\s*</expires>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex EventRegex = new Regex(
            @"<event>\s*(.*?)\s*</event>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        public static readonly Regex EventTypeRegex = new Regex(
            @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Name</valueName>\s*<value>\s*(.*?)\s*</value>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex LanguageRegex = new Regex(
            @"<language>\s*(.*?)\s*</language>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex SenderNameRegex = new Regex(
            @"<senderName>\s*(.*?)\s*</senderName>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex CoverageRegex = new Regex(
            @"<valueName>layer:EC-MSC-SMC:1.0:Alert_Coverage</valueName>\s*<value>\s*(.*?)\s*</value>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex DescriptionRegex = new Regex(
            @"<description>\s*(.*?)\s*</description>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex InstructionRegex = new Regex(
            @"<instruction>\s*(.*?)\s*</instruction>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex BroadcastTextRegex = new Regex(
            @"<valueName>layer:SOREM:1.0:Broadcast_Text</valueName>\s*<value>\s*(.*?)\s*</value>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex AreaDescriptionRegex = new Regex(
            @"<areaDesc>\s*(.*?)\s*</areaDesc>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex ResourceDescriptionRegex = new Regex(
            @"<resourceDesc>\s*(.*?)\s*</resourceDesc>\s*(.*?)\s*</resource>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex DerefURIRegex = new Regex(
            @"<derefUri>\s*(.*?)\s*</derefUri>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex TypeURIRegex = new Regex(
            @"<uri>\s*(.*?)\s*</uri>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex MimeRegex = new Regex(
            @"<mimeType>\s*(.*?)\s*</mimeType>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        public static readonly Regex IdentifierRegex = new Regex(
            @"<identifier>\s*(.*?)\s*</identifier>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex ReferencesRegex = new Regex(
            @"<references>\s*(.*?)\s*</references>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex LocationRegex = new Regex(
            @"<geocode>\s*<valueName>profile:CAP-CP:Location:0.3</valueName>\s*<value>\s*(.*?)\s*</value>",
            RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }
}
