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
    public class VariableReplace
    {
        // Extracted from the anchor's hash (annotation in PDF). #totalPages
        public string Name { get; set; }
        // The AABB for the replacement text
        public RectangleF Rect { get; set; }

        // All styling associated with the text inside the anchor
        public int FontSize { get; set; }
    }

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
            PageStamper stamper = new PageStamper(); 

            foreach (Page page in document.Pages)
            {
                stamper.Page = page;
                PrimitiveComposer composer = stamper.Foreground;

                // Get the page annotations!
                PageAnnotations annotations = page.Annotations;

                if (!annotations.Exists())
                {
                    continue;
                }

                List<Annotation> delete = new List<Annotation>();
                // Iterating through the page annotations looking for links...
                foreach (Annotation annotation in annotations)
                {
                    if (annotation is Link link)
                    {
                        if (link.Target is actions::Action action)
                        {
                            if (action is GoToURI go)
                            {
                                string name = go.URI.Fragment?.Length > 1 ? go.URI.Fragment.Substring(1) : null;
                                if (name == "page" || name == "total")
                                {
                                    delete.Add(annotation);
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }

                foreach (var d in delete)
                {
                    Extract(new ContentScanner(page), composer, FixAnchorBox(d.Box));
                    page.Annotations.Remove(d);
                }

                stamper.Flush();
            }
        }

        private RectangleF FixAnchorBox(RectangleF box)
        {
            // One mayor flaw in wkhtmltopdf
            // The actual link of Anchors (annotations) is not where the text is
            // Seems that is ignores margins. Fix: move the link box down
            // https://github.com/wkhtmltopdf/wkhtmltopdf/issues/1692

            // Size	72 PPI	96 PPI	150 PPI	300 PPI
            // A4	595 x 842	794 x 1123	1240 x 1754	2480 x 3508
            // Now convert 5mm to pixles
            // Footer height + spacing
            float moveDown = (25 + 5) * 2.83333333f;
            return new RectangleF
            {
                Height = box.Height,
                Width = box.Width,
                X = box.X,
                Y = box.Y + moveDown
            };
        }

        private void Extract(ContentScanner level, PrimitiveComposer composer, RectangleF rect)
        {
            if (level == null)
            {
                return;
            }

            while (level.MoveNext())
            {
                ContentObject content = level.Current;
                if (content is Text)
                {
                    ContentScanner.TextWrapper text = (ContentScanner.TextWrapper)level.CurrentWrapper;
                    
                    foreach (ContentScanner.TextStringWrapper textString in text.TextStrings)
                    {
                        if (rect.IntersectsWith(textString.Box.Value))
                        {
                            RectangleF textStringBox = textString.Box.Value;
                            Console.WriteLine(
                            "Text ["
                                + "x:" + Math.Round(textStringBox.X) + ","
                                + "y:" + Math.Round(textStringBox.Y) + ","
                                + "w:" + Math.Round(textStringBox.Width) + ","
                                + "h:" + Math.Round(textStringBox.Height)
                                + "] [font size:" + Math.Round(textString.Style.FontSize) + "]: " + textString.Text
                            );
                            foreach (TextChar textChar in textString.TextChars)
                            {
                                // composer.DrawRectangle(textChar.Box);
                                // composer.Stroke();

                                composer.SetFont(textChar.Style.Font, 16);
                                composer.SetFillColor(textChar.Style.FillColor);

                                // textChar.Style.Font.Encode("9");
                                PointF center = new PointF
                                {
                                    X = textChar.Box.X + (textChar.Box.Width / 2),
                                    Y = textChar.Box.Y + (textChar.Box.Height / 2)
                                };
                                composer.ShowText("a", center, XAlignmentEnum.Center, YAlignmentEnum.Middle, 0);
                            }
                            level.Remove();
                            level.Contents.Flush();

                        }
                    }
                }
                else if (content is ContainerObject)
                {
                    // Scan the inner level
                    Extract(level.ChildLevel, composer, rect);
                }
            }
        }

        private void GetAndDeleteText(Link link, TextExtractor extractor, Annotation annotation, IDictionary<RectangleF?, IList<ITextString>> textStrings)
        {
            // Delete the actual annotation. This is only the clickable action not the text
            RectangleF linkBox = link.Box;

            // composer.BeginLocalState();
            // composer.SetStrokeColor(new DeviceRGBColor(1, 0, 1));

            
            // var bb = new RectangleF(link.Box.X, link.Box.Y + moveDown, link.Box.Width, linkBox.Height);
            // composer.DrawRectangle(bb);
            // composer.Stroke();
            // composer.End();

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

        }
    }

}
