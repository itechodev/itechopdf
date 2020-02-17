
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

            var cover = new PdfDocument(PdfSource.FromFile("res/cover.html"));
            cover.Configure(print => {
                print.DPI = 225;
                print.EnableIntelligentShrinking = false;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.Orientation = Orientation.Portrait;
                print.PaperSize = PaperKind.A4;
            });
            
            var content = new PdfDocument(PdfSource.FromFile("res/content.html"));
            content.Configure(print => {
                print.DPI = 225;
                print.EnableIntelligentShrinking = false;
                print.PaperSize = PaperKind.A4;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.Orientation = Orientation.Portrait;
            });
            content.SetHeader(HeaderFooter.Html(PdfSource.FromFile("res/header.html"), 25, 10));
            content.SetFooter(HeaderFooter.Html(PdfSource.FromFile("res/footer.html"), 25, 10));
            
            renderer.Add(cover);
            renderer.Add(content);

            var pdf = renderer.RenderToBytes();
            File.WriteAllBytes("output.pdf", pdf);
        }
    }
}

