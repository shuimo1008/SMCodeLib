using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib.Features.SharpZip.Core;
using ZCSharpLib.Features.SharpZip.Zip;

namespace TestSharpZip
{
    class Program
    {
        static void Main(string[] args)
        {
            //CreateZip();
            ExtractZip();
        }

        public static void CreateZip()
        {
            FastZipEvents fastZipEvents = new FastZipEvents();
            fastZipEvents.Progress += OnProgressHandler;
            FastZip fastZip = new FastZip(fastZipEvents);
            fastZip.CreateZip("C:/Users/Joyan/Desktop/MOVA0069.zip", "C:/Users/Joyan/Desktop/MOVA0069", true, ".*");
        }

        public static void ExtractZip()
        {
            FastZipEvents fastZipEvents = new FastZipEvents();
            fastZipEvents.Progress += OnProgressHandler;
            FastZip fastZip = new FastZip(fastZipEvents);
            fastZip.ExtractZip("C:/Users/Joyan/Desktop/MOVA0069.zip", "C:/Users/Joyan/Desktop/MOVA0069(new)", "");
        }

        private static void OnProgressHandler(object sender, ProgressEventArgs e)
        {
            Console.WriteLine(e.PercentComplete);
        }
    }
}
