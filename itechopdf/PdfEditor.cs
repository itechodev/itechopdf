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
using System.Web;
using org.pdfclown.documents.contents.colorSpaces;

namespace ItechoPdf
{
    internal class PdfEditor : IDisposable
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

        private List<ReplaceRect> FindLinks(Page page, List<VariableReplace> variables)
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
                                string text = namevalue.GetValues("text")?.FirstOrDefault();
                                if (text != null)
                                {
                                    var align = namevalue.GetValues("align")?.FirstOrDefault() ?? "right";
                                    string replaceText = FormatNewText(text, variables);
                                    var ins = new ReplaceRect(FixAnchorBox(annotation.Box), replaceText, ToXAlignmentEnum(align), annotation);
                                    ret.Add(ins);
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public void ReplacePage(int pageNumber, List<VariableReplace> variables)
        {
            PageStamper stamper = new PageStamper();
            var page = _file.Document.Pages[pageNumber];
            stamper.Page = page;
            PrimitiveComposer composer = stamper.Foreground;

            var links = FindLinks(page, variables);
            
            foreach (var d in links)
            {
                // composer.SetStrokeColor(new DeviceRGBColor(1, 0, 1 ));
                // composer.SetLineWidth(0.5);
                // composer.DrawRectangle(d.Rect);
                // composer.Stroke();
                // Console.WriteLine($"Rect found: {d.Rect.Left},{d.Rect.Top}  {d.Rect.Right},{d.Rect.Bottom}  {d.Rect.Width} x {d.Rect.Height}");
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
            // Footer height + spacing header + spacing footer
            float moveDown = (float)HeightAndSpacing * 2.83333333f;
            return new RectangleF
            {
                Height = box.Height,
                Width = box.Width,
                X = box.X,
                Y = box.Y + moveDown
            };
        }

        private static bool RectWithin(float x, float y, RectangleF check)
        {
            return 
                x > check.Left && 
                x < check.Right && 
                y > check.Top && 
                y < check.Bottom;
        }

        private static bool RectContains(RectangleF a, RectangleF b)
        {
            return 
                RectWithin(a.X, a.Y, b) ||
                RectWithin(a.X + a.Width, a.Y, b) ||
                RectWithin(a.X + a.Width, a.Y + a.Height, b) ||
                RectWithin(a.X, a.Y + a.Height, b);

        }

        private void Extract(ContentScanner level, PrimitiveComposer composer, List<ReplaceRect> replaceList)
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
                    ContentScanner.TextWrapper wrapper = (ContentScanner.TextWrapper)level.CurrentWrapper;
                    // composer.SetStrokeColor(new DeviceRGBColor(0.2, 0.2, 0.2));
                    // composer.SetLineWidth(0.2);
                    // composer.DrawRectangle(wrapper.Box.Value);
                    // composer.Stroke();
                    
                    foreach (var replace in replaceList)
                    {
                        // var before = String.Join("", wrapper.TextStrings.Select(t => t.Text));
                        // Console.WriteLine($"Before hit text: {before}");
                        bool hit = wrapper.TextStrings.Any(ts => ts.Box.HasValue && ts.TextChars.Any(c => RectContains(ts.Box.Value, replace.Rect)));
                        if (!hit)
                        {
                            continue;
                        }

                        // only draw on the first hit
                        if (!replace.AlreadyStamp) 
                        {
                            // Take the first match and extract styling 
                            var textChar = wrapper.TextStrings.FirstOrDefault().TextChars.FirstOrDefault();
                            
                            composer.SetFont(textChar.Style.Font, FontSizeToPt(textChar.Style.FontSize));
                            composer.SetFillColor(textChar.Style.FillColor);
                            composer.SetStrokeColor(textChar.Style.StrokeColor);
                            composer.SetTextRenderMode(textChar.Style.RenderMode);

                            PointF refPoint;
                            switch (replace.XAlignment)
                            {
                                case XAlignmentEnum.Left:
                                case XAlignmentEnum.Justify:
                                    refPoint = replace.Rect.Location;
                                    break;
                                case XAlignmentEnum.Center:
                                    refPoint = new PointF(replace.Rect.X + (replace.Rect.Width / 2), replace.Rect.Y);
                                    break;
                                case XAlignmentEnum.Right:
                                default:
                                    refPoint = new PointF(replace.Rect.X + replace.Rect.Width, replace.Rect.Y);
                                    break; 
                            }
                            composer.ShowText(replace.Text, refPoint, replace.XAlignment, YAlignmentEnum.Top, 0);
                            replace.AlreadyStamp = true;
                        }
                        // Remove text from document
                        level.Remove();
                        level.Contents.Flush();  
                    }
                }
                else if (content is ContainerObject)
                {
                    // Scan the inner level
                    Extract(level.ChildLevel, composer, replaceList);
                }
            }
        }

        private string FormatNewText(string text, List<VariableReplace> variables)
        {
            // The whitespace charpoint 32 is not included as the embedded PDF glyphs 
            // Which makes kind of sense, because a space doesn't have any value in a PDF. 
            // Each character or sequency of characters is placed with absolute position
            // See textChar.Style.Font.codes.Values. 32 is never present
            // I also read somewhere that 32 in PDF is reserved for other purposes?
            // So we use a horizontal tab instead. Point code 9.
            // First we do variable substitution
            var newText = text;
            foreach (var variable in variables)
            {
                newText = newText.Replace($"[{variable.Name}]", variable.Replace);
            }
            // Then replace space with 9
            return newText.Replace((char)32, (char)9);
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
