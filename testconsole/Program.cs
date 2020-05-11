
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ItechoPdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf;


namespace testconsole
{
    class Program
    {
        static byte[] Create(int i)
        {
            var renderer = new PdfRenderer(settings => {
                // Set global settings for all documents rendered through this service
                settings.DPI = 300;
                settings.Margins.Set(0, 0, 0, 0,Unit.Millimeters);
            });
            
            Console.WriteLine("WkHTML version:" + renderer.GetVersion());

            // var cover = renderer.AddDocument(PdfSource.FromFile("res/cover.html"));
            var doc = renderer.AddDocument(25, 15);

            // var content = renderer.AddDocument(PdfSource.FromFile("res/content.html"));
            
            // content.SetHeader(PdfSource.FromFile("res/header.html"), 25, 5);
            // content.SetFooter(PdfSource.FromFile("res/footer.html"), 15, 5);

            // var content2 = renderer.AddDocument(PdfSource.FromFile("res/content.html"));
            // content2.SetFooter(PdfSource.FromFile("res/footer.html"), 15, 5);

            return renderer.RenderToBytes();
        }

        private static void PdfClipping()
        {
            // Open the external documents as XPdfForm objects. Such objects are
            // treated like images. By default the first page of the document is
            // referenced by a new XPdfForm.
            XPdfForm form1 = XPdfForm.FromFile("input/b.pdf");

            PdfSharp.Pdf.PdfDocument outputDocument = new PdfSharp.Pdf.PdfDocument();

            PdfSharp.Pdf.PdfPage page1 = outputDocument.AddPage();

            var gfx = XGraphics.FromPdfPage(page1);
            gfx.DrawImage(form1, new XRect(0, 0, 100, 100));

            outputDocument.Save("output/b.pdf");
        }


        static void Main(string[] args)
        {
            // Console.WindowWidth;
            // No data is available for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var bytes = Create(0);
            File.WriteAllBytes("output.pdf", bytes);

            // Parallel.For(0, 2, i => {
            // });
        }
    }
}

