using System;
using System.Runtime.InteropServices;
using wkpdftoxcorelib.Settings;

namespace wkpdftoxcorelib
{
    class Program
    {
        static void Main(string[] args)
        {
            ExplicitHeights();
        }

        static void ExplicitHeights()
        {
            var htmltopdf = new WkHtmlToPdf();
            Console.WriteLine("WkHTML version:" + htmltopdf.GetVersion());
            // htmltopdf.PrintSettings.Margins.Set(0, 0, 0, 0, Unit.Centimeters);
            htmltopdf.PrintSettings.Margins.Unit = Unit.Millimeters;
            htmltopdf.PrintSettings.Margins.Left = 0;
            htmltopdf.PrintSettings.Margins.Right = 0;
            
            htmltopdf.PrintSettings.Margins.Top = 0;
            htmltopdf.PrintSettings.Margins.Bottom = 0;
            
            htmltopdf.PrintSettings.Header = new FileHeaderFooter("header.html");
            htmltopdf.PrintSettings.Header.Height = 25;
            htmltopdf.PrintSettings.Header.Spacing = 10; 

            htmltopdf.PrintSettings.Footer = new FileHeaderFooter("header.html");
            htmltopdf.PrintSettings.Footer.Spacing = 10; // 1 cm
            htmltopdf.PrintSettings.Footer.Height = 25; 

            var doc = htmltopdf.HtmlToPdf("<style>* {margin: 0; padding:0}</style> <body style='background-color: yellow; margin:0; padding:0;'>To PDF seems to be working just fine</div>");
            doc?.SaveToFile("output.pdf");
        }
    }
}

