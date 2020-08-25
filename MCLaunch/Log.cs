using System.Threading;
using System;
namespace MCLaunch
{
    public static class Log
    {
        private static object _lock = new object();
        public static void Step(string message)
        {
            lock (_lock)
                if (Console.IsOutputRedirected)
                    Console.WriteLine("> " + message);
                else
                {
                    Console.Write("> ");
                    var col = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(message);
                    Console.ForegroundColor = col;
                }
        }
        public static void Error(string message)
        {
            lock (_lock)
                if (Console.IsOutputRedirected)
                    Console.WriteLine("! " + message);
                else
                {
                    Console.Write("! ");
                    var col = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(message);
                    Console.ForegroundColor = col;
                }
        }
        public static void FileNew(string message)
        {
            lock (_lock)
                if (Console.IsOutputRedirected)
                    Console.WriteLine("+ " + message);
                else
                {
                    Console.Write("+ ");
                    var col = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    Console.ForegroundColor = col;
                }
        }
        public static void FileRm(string message)
        {
            lock (_lock)
                if (Console.IsOutputRedirected)
                    Console.WriteLine("- " + message);
                else
                {
                    Console.Write("- ");
                    var col = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ForegroundColor = col;
                }
        }
    }
}