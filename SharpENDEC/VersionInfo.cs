namespace SharpENDEC
{
    public static class VersionInfo
    {
        // This file contains the version information. It should not be modified at runtime.
        // You can change the release, minor, and cutting edge variables.
        // ---
        // Use VersionInfoTemplate.cs!
        public const int BuildNumber = 664;
        public const string BuiltOnDate = "2024-11-24";
        public const string BuiltOnTime = "18:54";
        public const string BuiltTimeZone = "Eastern Standard Time";
        public static readonly int ReleaseVersion = 3;
        public static readonly int MinorVersion = 0;
        public static readonly bool IsCuttingEdge = true;
        public static string FriendlyVersion
        {
            get
            {
                if (!IsCuttingEdge)
                {
                    return $"SharpENDEC | Release {ReleaseVersion}.{MinorVersion} (Build {BuildNumber}) | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})";
                }
                else
                {
                    return $"SharpENDEC | Cutting Edge {ReleaseVersion}.{MinorVersion}-c (Build {BuildNumber}) | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})";
                }
            }
        }
    }
}
