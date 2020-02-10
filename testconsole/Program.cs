
using System;
using System.IO;
using System.Text;
using wkpdftoxcorelib;

namespace testconsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // No data is available for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var renderer = new PdfRenderer();
            Console.WriteLine("WkHTML version:" + renderer.GetVersion());

            var cover = new PdfDocument();
            cover.Configure(print => {
                print.DPI = 300;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.Orientation = Orientation.Portrait;
                print.PaperSize = PaperKind.A4;
            });
            cover.FromFile("res/cover.html");

            var content = new PdfDocument();
            content.Configure(print => {
                print.DPI = 300;
                print.PaperSize = PaperKind.A4;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.Orientation = Orientation.Portrait;
            });
            content.AddFileHeader("res/header.html", 25, 10);
            content.AddFileFooter("res/footer.html", 25, 10);
            content.FromFile("res/content.html");

            renderer.Add(cover);
            renderer.Add(content);

            var pdf = renderer.RenderToBytes();
            File.WriteAllBytes("output.pdf", pdf);
        }
    }
}

