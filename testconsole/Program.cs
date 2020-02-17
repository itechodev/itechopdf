
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
                print.DPI = 300;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
            });
            
            var content = new PdfDocument(PdfSource.FromFile("res/content.html"));
            content.Configure(print => {
                print.DPI = 300;
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
            });
            content.SetHeader(PdfSource.FromFile("res/header.html"), 25, 5);
            content.SetFooter(PdfSource.FromFile("res/footer.html"), 25, 5);
            
            renderer.Add(cover);
            renderer.Add(content);

            var pdf = renderer.RenderToBytes();
            File.WriteAllBytes("output.pdf", pdf);
        }
    }
}

