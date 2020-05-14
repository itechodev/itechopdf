using System;
using System.Collections.Generic;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;

namespace testconsole
{
    public static class PdfMerge
    {

        //  case LinkType.Web:
        //   //pdf.AppendFormat("/A<</S/URI/URI{0}>>\n", PdfEncoders.EncodeAsLiteral(this.url));
        //   Elements[Keys.A] = new PdfLiteral("<</S/URI/URI{0}>>", //PdfEncoders.EncodeAsLiteral(this.url));
        //     PdfEncoders.ToStringLiteral(this.url, PdfStringEncoding.WinAnsiEncoding, writer.SecurityHandler));
        //   break;
        public static string GetUrlLink(PdfAnnotation ano)
        {
            var d = ano.Elements["/A"];
            if (d is PdfDictionary dic)
            {
                var uriElement = dic.Elements["/URI"];   
                if (uriElement is PdfString str)
                {
                    return str.Value;
                }
            }
            return null;
        }

        public static void Merge()
        {
            // Create the output document
            PdfDocument outDoc = new PdfDocument();

            PdfDocument inDoc = PdfReader.Open("1.pdf", PdfDocumentOpenMode.Import);
            XPdfForm hf = XPdfForm.FromFile("1-headerfooters.pdf");

            for (var p = 0; p < inDoc.PageCount; p++)
            {
                var page = inDoc.Pages[p];

                if (page.Annotations.Count == 1)
                {
                    var link = GetUrlLink(page.Annotations[0]);
                    if (link == "itechopdf://splitdocument")
                    {
                        Console.WriteLine(link);
                        continue;
                    }
                }

                // page.Contents.Elements.Items.Length == 1
                var newPage = outDoc.AddPage(page);

                // XGraphics gfx = XGraphics.FromPdfPage(newPage);

                // // p per milliter
                // var ppm = page.Height.Point / page.Height.Millimeter; // or page.Width.Point / page.Width.Millimeter
                //                                                       // 1 inch = 72 points
                //                                                       // 1 inch = 25.4 mm
                //                                                       // That leads to:
                //                                                       // 1 point = 0.352777778 mm

                // int headerHeight = 30;
                // int footerHeight = 10;
                // Clip(gfx, hf, newPage, new XRect(0, headerHeight * ppm, page.Width, headerHeight * ppm), new XRect(0, 0, page.Width, headerHeight * ppm));
                // Clip(gfx, hf, newPage, new XRect(0, headerHeight * 2 * ppm, page.Width, footerHeight * ppm), new XRect(0, page.Height - (footerHeight * ppm), page.Width, footerHeight * ppm));
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

