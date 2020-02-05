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
            
            worker.PrintSettings.Header.HtmlContent = "<!DOCTYPE html><b>First PDF header testing</b>";
            worker.PrintSettings.Footer.HtmlContent = $"<!DOCTYPE html><b>Print on {DateTime.Now.ToLongTimeString()} </b>";
            var doc = worker.HtmlToPdf("To PDF seems to be working just fine");
            doc?.SaveToFile("output.pdf");
        }
    }
}
