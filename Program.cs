using System;
using System.Runtime.InteropServices;

namespace wkpdftoxcorelib
{
    class Program
    {
        static void Main(string[] args)
        {
            WkHtmlToXBinding.wkhtmltopdf_init(0);

            string version =  Marshal.PtrToStringAnsi(WkHtmlToXBinding.wkhtmltopdf_version());

            Console.WriteLine("WkHTML version:" + version);   
        }
    }
}
