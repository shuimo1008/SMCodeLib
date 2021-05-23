using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCrash
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            int div = 0;

            int a = 1 / div;

            //throw new System.Exception();

            Console.ReadLine();
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {

            }
            catch
            {
                Exception error = (Exception)e.ExceptionObject;
                Console.WriteLine("MyHandler caught : " + error.Message);
            }
        }

    }
}
