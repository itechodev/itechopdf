using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace testconsole
{
    public static class PdfMerge
    {

        public static void DrawPdfPage(XGraphics gfx, XPdfForm form, XRect source, XRect dest)
        {
            gfx.IntersectClip(dest);
            
            gfx.DrawImage(form, new XRect(dest.Left - source.Left, dest.Top - source.Top, dest.Width, dest.Height));
        }

        public static void Merge()
        {
            // Create the output document
            PdfDocument outDoc = new PdfDocument();

            PdfDocument inDoc = PdfReader.Open("1.pdf", PdfDocumentOpenMode.Import);
            XPdfForm hf = XPdfForm.FromFile("pdf.11.pdf");
            // PdfDocument hf = PdfReader.Open("1-headerfooters.pdf", PdfDocumentOpenMode.Import);

            for (var p = 0; p < inDoc.PageCount; p++)
            {
                var page = inDoc.Pages[p];
                var newPage = outDoc.AddPage(page);

                XGraphics gfx = XGraphics.FromPdfPage(newPage);
                // DrawPdfPage(gfx, hf, , new XRect(0, 0, newPage.Width, 100));
                
                // var source = new XRect(0, 80, newPage.Width, 90);
                // var dest = new XRect(50, 50, newPage.Width, 90);

                // double rx = source.Width / dest.Width;
                // double ry = source.Height / dest.Height;

                gfx.IntersectClip(new XRect(
                    100, 
                    100,
                    180 * 2, 
                    160 * 2
                ));
                
                gfx.DrawImage(hf, new XRect(
                    (-610 * 2) + 100,
                    (-63 * 2) + 100,
                    newPage.Width * 2, 
                    newPage.Height * 2));
            }

            outDoc.Save("output.pdf");
        }
    }
}

