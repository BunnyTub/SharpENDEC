using System;

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
    }
}
