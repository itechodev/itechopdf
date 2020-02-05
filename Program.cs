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
            
            var doc = worker.HtmlToPdf("To PDF seems to be working just fine");
            doc.SaveToFile("output.pdf");
        }
    }
}
