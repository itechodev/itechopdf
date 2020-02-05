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


            // Also in https://stackoverflow.com/questions/15299869/header-height-and-positioning-header-from-top-of-page-in-wkhtmltopdf. Height 1.3x body content. Why? I don't know
            // https://stackoverflow.com/questions/27443586/wkhtmltopdf-footer-size-issues

            htmltopdf.PrintSettings.Margins.Top = height; // Math.Ceiling(height / 1.3);
            htmltopdf.PrintSettings.Margins.Bottom = height; // Math.Ceiling(height / 1.3);
            
            htmltopdf.PrintSettings.DPI = 600;
            htmltopdf.PrintSettings.EnableIntelligentShrinking = false;
            
            
            htmltopdf.PrintSettings.Header = new HtmlHeaderFooter($"<!DOCTYPE html><body style='margin:0; background-color: lime;'><div style='display: inline-block; width: 20mm; height: {height}mm; background-color: pink'>Quite good header</div></body>");
            htmltopdf.PrintSettings.Header.Spacing = 0;
            
            htmltopdf.PrintSettings.Footer = new HtmlHeaderFooter($"<!DOCTYPE html><body style='margin:0; padding:0; background-color: red; height: {height}mm;'>Print on {DateTime.Today.ToShortDateString()} - {DateTime.Now.ToLongTimeString()}</body>");
            htmltopdf.PrintSettings.Footer.Spacing = 0;

            var doc = htmltopdf.HtmlToPdf("<body style='background-color: yellow; margin:0; padding:0;'>To PDF seems to be working just fine</div>");
            doc?.SaveToFile("output.pdf");
        }
    }
}
