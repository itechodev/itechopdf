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
             // in mm
            double height = 40;

            var htmltopdf = new WkHtmlToPdf();
            Console.WriteLine("WkHTML version:" + htmltopdf.GetVersion());
            // htmltopdf.PrintSettings.Margins.Set(0, 0, 0, 0, Unit.Centimeters);
            htmltopdf.PrintSettings.Margins.Unit = Unit.Millimeters;
            htmltopdf.PrintSettings.Margins.Left = 0;
            htmltopdf.PrintSettings.Margins.Right = 0;

            htmltopdf.PrintSettings.Margins.Top = 25 + 10; // 25mm header + 10mm spacing + 1mm margin top
            htmltopdf.PrintSettings.Margins.Bottom = 25 + 10; 
            
            // htmltopdf.PrintSettings.Header = new HtmlHeaderFooter($"<!DOCTYPE html><html style='margin:0; padding:0;'><body style='margin:0; padding:0; background-color: green;'><div style='background-color: pink; height: 2cm;'>Quite</br> good </br>header</div></body></html");
            
            htmltopdf.PrintSettings.Header = new FileHeaderFooter("header.html");
            htmltopdf.PrintSettings.Footer = new FileHeaderFooter("header.html");

            // htmltopdf.PrintSettings.Header = new StandardHeaderFooter("Left", "Center", "Right");
            // htmltopdf.PrintSettings.Footer = new StandardHeaderFooter("Left", "Center", "Right");
            htmltopdf.PrintSettings.Footer.Spacing = 10; // 1 cm 
            htmltopdf.PrintSettings.Header.Spacing = 10; // 1 cm


            var doc = htmltopdf.HtmlToPdf("<body style='background-color: yellow; margin:0; padding:0;'>To PDF seems to be working just fine</div>");
            doc?.SaveToFile("output.pdf");
        }
    }
}

