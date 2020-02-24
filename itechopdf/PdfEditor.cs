using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.files;
using org.pdfclown.documents.contents;
using actions = org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.documents.interaction.navigation.document;
using files = org.pdfclown.files;
using org.pdfclown.objects;
using org.pdfclown.tools;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using org.pdfclown.documents.interaction.actions;

namespace ItechoPdf
{

    public class PdfEditor
    {

        public void Stuff(string filePath)
        {
            using (files::File file = new files::File(filePath))
            {

                Document document = file.Document;
                FindLinks(document);
                file.Save("other.pdf", SerializationModeEnum.Standard);
            }
        }

        private void FindLinks(Document document)
        {
            PageStamper stamper = new PageStamper(); // NOTE: Page stamper is used to draw contents on existing pages.
            
            // 2. Link extraction from the document pages.
            TextExtractor extractor = new TextExtractor();
            extractor.AreaTolerance = 2; // 2 pt tolerance on area boundary detection.

            foreach (Page page in document.Pages)
            {
                stamper.Page = page;
                PrimitiveComposer composer = stamper.Foreground;

                composer.SetStrokeColor(new DeviceRGBColor(1, 0, 0));
                composer.DrawRectangle(new RectangleF(0, 0, 100, 100));
                // composer.DrawRectangle(new RectangleF(112.12f, 686.17f, 100, 100));
                composer.Stroke();


                IDictionary<RectangleF?, IList<ITextString>> textStrings = null;

                // Get the page annotations!
                PageAnnotations annotations = page.Annotations;

                if (!annotations.Exists())
                {
                    Console.WriteLine("No annotations here.");
                    continue;
                }
                
                // page.Annotations.Remove(annotations[0]);

                // Iterating through the page annotations looking for links...
                foreach (Annotation annotation in annotations)
                {
                    if (annotation is Link link)
                    {
                        if (textStrings == null)
                        {
                            textStrings = extractor.Extract(page);
                        }
                        
                        RectangleF linkBox = link.Box;

                        composer.BeginLocalState();
                        composer.SetStrokeColor(new DeviceRGBColor(1, 0, 1));
                        // One mayor flaw in wkhtmltopdf
                        // The actual link of Anchors (annotations) is not where the text is
                        // Seems that is ignores margins. Fix: move the link box down
                        // https://github.com/wkhtmltopdf/wkhtmltopdf/issues/1692

                        // Size	72 PPI	96 PPI	150 PPI	300 PPI
                        // A4	595 x 842	794 x 1123	1240 x 1754	2480 x 3508
                        // Now convert 5mm to pixles
                        float moveDown = (25 + 5) * 2.83333333f;

                        var bb = new RectangleF(link.Box.X, link.Box.Y + moveDown, link.Box.Width, linkBox.Height);
                        composer.DrawRectangle(bb);
                        composer.Stroke();
                        composer.End();

                        // Text.
                        /*
                          Extracting text superimposed by the link...
                          NOTE: As links have no strong relation to page text but a weak location correspondence,
                          we have to filter extracted text by link area.
                        */
                        StringBuilder linkTextBuilder = new StringBuilder();
                        foreach (ITextString linkTextString in extractor.Filter(textStrings, linkBox))
                        {
                            if (linkTextString.TextChars?.Count > 0)
                            {
                                var style = linkTextString.TextChars[0].Style;
                                
                                // style.FillColor
                                // style.FillColorSpace
                                // style.Font
                                // style.FontSize
                                // style.RenderMode
                                // style.StrokeColor
                                // style.StrokeColorSpace
                            }
                            linkTextBuilder.Append(linkTextString.Text);
                            
                            // linkTextString.TextChars.Remove(linkTextString.TextChars[0]);
                        }
                         
                        Console.WriteLine("Link '" + linkTextBuilder + "' ");
                        

                        // Position.
                        Console.WriteLine(
                          "    Position: "
                            + "x:" + Math.Round(linkBox.X) + ","
                            + "y:" + Math.Round(linkBox.Y) + ","
                            + "w:" + Math.Round(linkBox.Width) + ","
                            + "h:" + Math.Round(linkBox.Height)
                            );

                        Console.Write("    Target: ");
                        PdfObjectWrapper target = link.Target;
                        if (link.Target is actions::Action action)
                        {
                            if (action is GoToURI go)
                            {
                                Console.WriteLine($"URI is {go.URI.ToString()}");
                            }
                        }

                    }
                }
                stamper.Flush();
            }
        }


    }

}
