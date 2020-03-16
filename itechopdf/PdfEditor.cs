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
using org.pdfclown.documents.contents.fonts;
using static org.pdfclown.documents.contents.fonts.Font;
using System.Text.RegularExpressions;

namespace ItechoPdf
{

    internal class PdfEditor : IDisposable
    {
        private files.File _file;
        private PdfDocument _pdfDocument;

        public PdfEditor(PdfDocument doc)
        {
            _pdfDocument = doc;
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
            var pageSizePx = _file.Document.GetSize();
             
            // swap the width and height if in landscape mode
            var heightmm = _pdfDocument.Settings.Orientation == Orientation.Landscape
                ? _pdfDocument.Settings.PaperSize.Width
                : _pdfDocument.Settings.PaperSize.Height;
            
            // A4 is 210mm x 297mm. Which means 842px = 297mm
            // 842 / 297 = 2,835016835 px / mm
            // HeightAndSpacing is in mm. 
            float mmPerPx = pageSizePx.Height / (float)heightmm;
            float moveDown = (float)HeightAndSpacing * mmPerPx;
            return new RectangleF
            {
                Height = box.Height,
                Width = box.Width,
                X = box.X,
                Y = box.Y + moveDown
            };
        }

        private static bool RectWithin(float x, float y, RectangleF check, float tolerance)
        {
            return
                x >= check.Left - tolerance  &&
                x <= check.Right + tolerance &&
                y >= check.Top - tolerance &&
                y <= check.Bottom + tolerance;
        }

        private static bool RectContains(RectangleF a, RectangleF b, float tolerance)
        {
            return
                RectWithin(a.X, a.Y, b, tolerance) ||
                RectWithin(a.X + a.Width, a.Y, b, tolerance) ||
                RectWithin(a.X + a.Width, a.Y + a.Height, b, tolerance) ||
                RectWithin(a.X, a.Y + a.Height, b, tolerance);

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
                        bool hit = wrapper.TextStrings.Any(ts => ts.Box.HasValue && ts.TextChars.Any(c => RectContains(ts.Box.Value, replace.Rect, 1)));
                        if (!hit)
                        {
                            continue;
                        }

                        // only draw on the first hit
                        if (!replace.AlreadyStamp)
                        {
                            // Take the first match and extract styling 
                            var textChar = wrapper.TextStrings.FirstOrDefault().TextChars.FirstOrDefault();

                            // StandardType1Font font = new StandardType1Font(
                            //     _file.Document,
                            //     StandardType1Font.FamilyEnum.Helvetica,
                            //     textChar.Style.Font.Flags.HasFlag(FlagsEnum.ForceBold),
                            //     textChar.Style.Font.Flags.HasFlag(FlagsEnum.Italic)
                            // );
                            double pt = FontSizeToPt(textChar.Style.FontSize);
                            composer.SetFont(textChar.Style.Font, pt);
                            composer.SetFillColor(textChar.Style.FillColor);
                            composer.SetStrokeColor(textChar.Style.StrokeColor);
                            composer.SetTextRenderMode(textChar.Style.RenderMode);

                            var stamps = GetStampList(replace.Text, textChar.Style, pt);
                            float diffWidth = (float)stamps.Width - (float)replace.Rect.Width;
                            
                            PointF refPoint;
                            switch (replace.XAlignment)
                            {
                                case XAlignmentEnum.Left:
                                case XAlignmentEnum.Justify:
                                    refPoint = replace.Rect.Location;
                                    break;
                                case XAlignmentEnum.Center:
                                refPoint = new PointF(replace.Rect.X - (diffWidth / 2), replace.Rect.Y);
                                    break;
                                case XAlignmentEnum.Right:
                                default:
                                    refPoint = new PointF(replace.Rect.X - diffWidth, replace.Rect.Y);
                                    break;
                            }

                            foreach (var stamp in stamps.Texts)
                            {
                                composer.ShowText(stamp.Text, new PointF(refPoint.X + stamp.Point.X, refPoint.Y + stamp.Point.Y), XAlignmentEnum.Left, YAlignmentEnum.Top, 0);
                            }
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

        private StampTexts GetStampList(string text, TextStyle style, double size)
        {
            var ret = new StampTexts();
            ret.Width = 0;
            // Somethimes the space literal can be used
            float spaceWidth = (float)style.Font.GetWidth(' ', size);
            if (spaceWidth == 0)
            {
                // otherwise try the tab literal
                spaceWidth = (float) style.Font.GetWidth((char)9, size);
            }
            if (spaceWidth == 0)
            {
                // fallback half size of 1
                spaceWidth = (float)style.Font.GetWidth('1', size)  / 2;
            }
             
            var segments = Regex.Split(text, @"\s+");
            foreach (string segment in segments)
            {
                if (ret.Width != 0)
                {
                    ret.Width += spaceWidth;
                }
                ret.Add(new StampText(new PointF((float)ret.Width, 0), segment));
                ret.Width += style.Font.GetWidth(segment, size);
            }
            return ret;
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
            return newText;
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
