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

            worker.Convert();
        }
    }
}
