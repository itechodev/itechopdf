using System;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace testconsole
{
    public static class PdfMerge
    {
        public static void Merge()
        {
            // Create the output document
            PdfDocument outDoc = new PdfDocument();

            PdfDocument inDoc = PdfReader.Open("1.pdf", PdfDocumentOpenMode.Import);
            XPdfForm hf = XPdfForm.FromFile("1-headerfooters.pdf");
            
            for (var p = 0; p < inDoc.PageCount; p++)
            {
                var page = inDoc.Pages[p];
                var newPage = outDoc.AddPage(page);

                XGraphics gfx = XGraphics.FromPdfPage(newPage);

                // p per milliter
                var ppm = page.Height.Point / page.Height.Millimeter; // or page.Width.Point / page.Width.Millimeter
                // 1 inch = 72 points
                // 1 inch = 25.4 mm
                // That leads to:
                // 1 point = 0.352777778 mm
                
                int headerHeight = 30;
                int footerHeight = 10;
                Clip(gfx, hf, newPage, new XRect(0, headerHeight * ppm, page.Width, headerHeight * ppm), new XRect(0, 0, page.Width, headerHeight * ppm));
                Clip(gfx, hf, newPage, new XRect(0, headerHeight * 2 * ppm, page.Width, footerHeight * ppm), new XRect(0, page.Height - (footerHeight * ppm), page.Width, footerHeight * ppm));
            }

            outDoc.Save("output.pdf");
        }

        private static void Clip(XGraphics gfx, XPdfForm hf, PdfPage newPage, XRect source, XRect dest)
        {
            double rx = dest.Width / source.Width;
            double ry = dest.Height / source.Height;

            gfx.Save();
            gfx.IntersectClip(dest);
            
            gfx.DrawImage(hf, new XRect(
                (-source.Left * rx) + dest.Left,
                (-source.Top * ry) + dest.Top,
                newPage.Width * rx, 
                newPage.Height * ry));

            gfx.Restore();
        }
    }
}

