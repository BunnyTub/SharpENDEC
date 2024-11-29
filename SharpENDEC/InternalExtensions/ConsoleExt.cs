using System;
using System.IO;

namespace SharpENDEC
{
    public static class ConsoleExt
    {
        public static ConsoleColor og = ConsoleColor.White;

        public static void WriteLine(string value = "", ConsoleColor foreground = ConsoleColor.White)
        {
            og = Console.ForegroundColor;
            Console.ForegroundColor = foreground;
            Console.WriteLine(value);
            Console.ForegroundColor = og;
        }
        
        public static void Write(string value = "", ConsoleColor foreground = ConsoleColor.White)
        {
            og = Console.ForegroundColor;
            Console.ForegroundColor = foreground;
            Console.Write(value);
            Console.ForegroundColor = og;
        }

        public static void WriteLineErr(string value = "")
        {
            File.AppendAllText("error.log", $"{DateTime.Now:F} | {value}");
            og = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(value);
            Console.ForegroundColor = og;
        }
    }
}
