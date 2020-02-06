using System;
using System.IO;
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
            var cover = new PdfDocument();
            cover.Configure(print => {
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.Orientation = Orientation.Landscape;
            });
            cover.FromFile("cover.html");

            var content = new PdfDocument();
            content.Configure(print => {
                print.Margins.Set(0, 0, 0, 0, Unit.Millimeters);
                print.Orientation = Orientation.Portrait;
            });
            content.AddFileHeader("header.html", 25, 10);
            // page.AddFileFooter("footer.html", 25, 10);
            content.FromFile("content.html");

            var renderer = new PdfRenderer();
            Console.WriteLine("WkHTML version:" + renderer.GetVersion());
            renderer.Add(cover);
            renderer.Add(content);

            var pdf = renderer.RenderToBytes();
            File.WriteAllBytes("output.pdf", pdf);
        }
    }
}

