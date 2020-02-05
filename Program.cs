using System;
using System.Runtime.InteropServices;

namespace wkpdftoxcorelib
{
    class Program
    {
        static void Main(string[] args)
        {
            var worker = new WkHtmlToPdf();
            Console.WriteLine("WkHTML version:" + worker.GetVersion());
            // worker.PrintSettings.UseLocalLinks = true;
            worker.HtmlToPdf("To PDF seems to be working just fine");
        }
    }
}
