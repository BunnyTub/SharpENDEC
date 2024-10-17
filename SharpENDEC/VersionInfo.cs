namespace SharpENDEC
{
    public static class VersionInfo
    {
        // This file contains the version information. It should not be modified at runtime.
        // You can change the release, minor, and cutting edge variables.
        // ---
        // Use VersionInfoTemplate.cs!
        public const string BuiltOnDate = "2024-10-17";
        public const string BuiltOnTime = "13:36";
        public const string BuiltTimeZone = "Eastern Standard Time";
        public static readonly int ReleaseVersion = 1;
        public static readonly int MinorVersion = 2;
        public static readonly bool IsCuttingEdge = true;
        public static string FriendlyVersion
        {
            get
            {
                if (!IsCuttingEdge)
                {
                    return $"SharpENDEC | Release {ReleaseVersion}.{MinorVersion} | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})";
                }
                else
                {
                    return $"SharpENDEC | Cutting Edge {ReleaseVersion}.{MinorVersion}-c | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})";
                }
            }
        }
    }
}
