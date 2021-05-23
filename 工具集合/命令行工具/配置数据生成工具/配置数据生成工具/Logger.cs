using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public class Logger
    {
        public static void Explain(string msg, ConsoleColor color = ConsoleColor.Yellow)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
        }

        public static void Info(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(format, arg);
        }

        public static void Waring(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(format, arg);
        }

        public static void Error(string format, params object[] arg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, arg);
        }
    }
}
