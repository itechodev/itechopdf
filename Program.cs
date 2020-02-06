using System;
using wkpdftoxcorelib.Wrapper;

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
            var htmltopdf = new WkHtmlToPdfWrapper();
            Console.WriteLine("WkHTML version:" + htmltopdf.GetVersion());
            // htmltopdf.PrintSettings.Margins.Set(0, 0, 0, 0, Unit.Centimeters);

            htmltopdf.PrintSettings.EnableJavascript = true;
            htmltopdf.PrintSettings.Margins.Unit = Unit.Millimeters;
            htmltopdf.PrintSettings.Margins.Left = 0;
            htmltopdf.PrintSettings.Margins.Right = 0;
            
            htmltopdf.PrintSettings.Margins.Top = 0;
            htmltopdf.PrintSettings.Margins.Bottom = 0;
            
            htmltopdf.PrintSettings.Orientation = Orientation.Landscape;
            htmltopdf.PrintSettings.Header = new FileHeaderFooter("header.html");
            htmltopdf.PrintSettings.Header.Height = 25;
            htmltopdf.PrintSettings.Header.Spacing = 10; 

            htmltopdf.PrintSettings.Footer = new FileHeaderFooter("header.html");
            htmltopdf.PrintSettings.Footer.Height = 25; 
            htmltopdf.PrintSettings.Footer.Spacing = 10; // 1 cm

            var doc = htmltopdf.HtmlFileToPdf("content.html");
            doc?.SaveToFile("output.pdf");

        }
    }
}

