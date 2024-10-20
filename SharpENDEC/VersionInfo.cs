using System.Diagnostics;

namespace SharpENDEC
{
    public static class VersionInfo
    {
        // This file contains the version information. It should not be modified at runtime.
        // You can change the release, minor, and cutting edge variables.
        // ---
        // Use VersionInfoTemplate.cs!
        public const int BuildNumber = 596;
        public const string BuiltOnDate = "2024-10-20";
        public const string BuiltOnTime = "09:14";
        public const string BuiltTimeZone = "Eastern Standard Time";
        public static readonly int ReleaseVersion = 2;
        public static readonly int MinorVersion = 0;
        public static readonly bool IsCuttingEdge = false;
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
                    if (Debugger.IsAttached) return $"SharpENDEC | Cutting Edge {ReleaseVersion}.{MinorVersion}-c (Build {BuildNumber}) | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})";
                    else return $"SharpENDEC | Cutting Edge {ReleaseVersion}.{MinorVersion}-c (Build {BuildNumber}) | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})\r\n" +
                            $"Debugger Attached | Is Logging: {Debugger.IsLogging()}";
                }
            }
        }
    }
}
