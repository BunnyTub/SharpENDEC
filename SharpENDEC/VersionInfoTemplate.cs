﻿namespace SharpENDEC
{
    public static class VersionInfoTemplate
    {
        // This file contains the version information. It should not be modified at runtime.
        // You can change the release, minor, and cutting edge variables.
        // ---
        // Do not change consts!
        public const int BuildNumber = 0;
        public const string BuiltOnDate = "";
        public const string BuiltOnTime = "";
        public const string BuiltTimeZone = "";
        public static readonly int ReleaseVersion = 3;
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
                    return $"SharpENDEC | Cutting Edge {ReleaseVersion}.{MinorVersion}-c (Build {BuildNumber}) | Built on {BuiltOnDate} {BuiltOnTime} ({BuiltTimeZone})";
                }
            }
        }
    }
}