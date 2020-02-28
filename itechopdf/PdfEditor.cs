using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using actions = org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.annotations;
using files = org.pdfclown.files;
using org.pdfclown.tools;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;
using org.pdfclown.documents.interaction.actions;
using System.Linq;
using System.IO;
using org.pdfclown.documents.contents.colorSpaces;
using System.Web;

namespace ItechoPdf
{
    

    public class PdfEditor : IDisposable
    {
        private files.File _file;

        public PdfEditor()
        {

        }

        public Pages GetPages()
        {
            return _file?.Document.Pages;
        }

        public Document GetDocument => _file.Document;

        public void ReadFromBytes(byte[] bytes)
        {
            _file = new files::File(new org.pdfclown.bytes.Buffer(bytes));
        }

        public int Pages => _file?.Document.Pages.Count ?? 0;
        public double HeightAndSpacing { get; set; }
        
        public byte[] Save()
        {
            using (var ms = new MemoryStream())
            {
                _file.Save(new org.pdfclown.bytes.Stream(ms), SerializationModeEnum.Standard);
                return ms.ToArray();
            }
        }


        private List<ReplaceRect> FindLinks(Page page, List<VariableReplace> replace)
        {
            var ret = new List<ReplaceRect>();
            PageAnnotations annotations = page.Annotations;

            if (!annotations.Exists())
            {
                return ret;
            }

            // Iterating through the page annotations looking for links...
            foreach (Annotation annotation in annotations)
            {
                if (annotation is Link link)
                {
                    if (link.Target is actions::Action action)
                    {
                        if (action is GoToURI go)
                        {
                            if (go.URI.AbsolutePath == "/var")
                            {
                                var namevalue = HttpUtility.ParseQueryString(go.URI.Query.Substring(1));
                                var align = namevalue.GetValues("align")?.FirstOrDefault();
                                var name = namevalue.GetValues("name")?.FirstOrDefault();
                                
                                var replacement = replace.Find(r => r.Name == name);
                                if (replacement != null)
                                {
                                    var ins = new ReplaceRect(FixAnchorBox(annotation.Box), replacement, ToXAlignmentEnum(align), annotation);
                                    ret.Add(ins);
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public void ReplacePage(int pageNumber, List<VariableReplace> replace)
        {
            PageStamper stamper = new PageStamper();
            // Make sure in range
            pageNumber = Math.Max(Math.Min(Pages - 1, pageNumber), 0);
            var page = _file.Document.Pages[pageNumber];
            
            stamper.Page = page;
            PrimitiveComposer composer = stamper.Foreground;

            var links = FindLinks(page, replace);
            
            foreach (var d in links)
            {
                composer.SetStrokeColor(new DeviceRGBColor(1, 0, 1 ));
                composer.DrawRectangle(d.Rect);
                composer.Stroke();

                page.Annotations.Remove(d.Annotation);
            }

            Extract(new ContentScanner(page), composer, links);
            stamper.Flush();
            page.Contents.Flush();
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
            float moveDown = (float)HeightAndSpacing * 2.83333333f;
            return new RectangleF
            {
                Height = box.Height,
                Width = box.Width,
                X = box.X,
                Y = box.Y + moveDown
            };
        }

        private static bool RectContains(RectangleF outer, RectangleF? inner, float tollerance = 0.1f)
        {
            if (inner == null)
            {
                return false;
            }
            return
                inner.Value.X >= outer.X - tollerance &&
                inner.Value.Y >= outer.Y - tollerance &&
                inner.Value.X + inner.Value.Width <= outer.X + outer.Width + tollerance &&
                inner.Value.Y + inner.Value.Height <= outer.Y + outer.Height + tollerance;
        }

        private static void Extract(ContentScanner level, PrimitiveComposer composer, List<ReplaceRect> replaceList)
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
                    // level.CurrentWrapper
                    ContentScanner.TextWrapper wrapper = (ContentScanner.TextWrapper)level.CurrentWrapper;
                    
                    foreach (var replace in replaceList)
                    {
                        // Don't quite understand how the ContentScanner scans content
                        // I cannot delete individual TextStringWrapper elements. 
                        // Search for the first match, extract styling and delete the whole text
                        var first = wrapper.TextStrings.Find(t => RectContains(replace.Rect, t.Box));
                        if (first != null)
                        {
                            var textChar = first.TextChars.FirstOrDefault();
                            if (textChar != null)
                            {
                                // Console.WriteLine($"Real hit: {first.Text} ({wrapper.TextStrings.Count})");
                                // only draw on the first hit
                                if (!replace.AlreadyStamp) 
                                {
                                    // Console.WriteLine("First hit - need to draw");
                                    composer.SetFont(textChar.Style.Font, FontSizeToPt(textChar.Style.FontSize));
                                    composer.SetFillColor(textChar.Style.FillColor);
                                    composer.SetStrokeColor(textChar.Style.StrokeColor);
                                    composer.SetTextRenderMode(textChar.Style.RenderMode);

                                    // textChar.Style.Font.Encode("9");
                                    PointF center = new PointF
                                    {
                                        X = textChar.Box.X + (textChar.Box.Width / 2),
                                        Y = textChar.Box.Y + (textChar.Box.Height / 2)
                                    };
                                    // var bytes = textChar.Style.Font.Encode("9");
                                    composer.ShowText(replace.Replacement.Replace, center, replace.XAlignment, YAlignmentEnum.Middle, 0);
                                    replace.AlreadyStamp = true;
                                }

                                // Remote text from document
                                level.Remove();
                                level.Contents.Flush();
                            }
                        }
                    }
                }
                else if (content is ContainerObject)
                {
                    // Scan the inner level
                    Extract(level.ChildLevel, composer, replaceList);
                }
            }
        }

        private static XAlignmentEnum ToXAlignmentEnum(string align)
        {
            switch (align.ToLower().Trim())
            {
                case "left":
                    return XAlignmentEnum.Left;
                case "center":
                    return XAlignmentEnum.Center;
                default:
                case "right":
                    return XAlignmentEnum.Right;
            }
        }

        private static double FontSizeToPt(double fontSize)
        {
            // Getting font size from textString sometimes negative
            fontSize = Math.Abs(fontSize);
            // It is measured in some unit where 50 units = 1rem = 12pt = 16px
            // Now just convert to pt by 50/12 = 4.166666
            return fontSize / 4.166666666;
        }

        public void Dispose()
        {
            _file?.Dispose();
        }
    }
}
