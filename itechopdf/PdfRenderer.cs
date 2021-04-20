using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using ItechoPdf.Core;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;
using static System.String;

namespace ItechoPdf
{
    public class PdfRenderer
    {
        private List<PdfDocument> Documents { get; } = new List<PdfDocument>();
        private PdfSettings Settings { get; } = new PdfSettings();

        public PdfRenderer(Action<PdfSettings> config = null)
        {
            config?.Invoke(Settings);
        }

        public PdfDocument AddDocument(string baseUrl = null, Action<PdfSettings> settingsAction = null)
        {
            // copy settings from render
            var settings = new PdfSettings(Settings);
            settingsAction?.Invoke(settings);
            var doc = new PdfDocument(baseUrl, settings);
            Documents.Add(doc);
            return doc;
        }

        public string GetVersion()
        {
            return WkHtmlToPdf.GetVersion() + (WkHtmlToPdf.ExtendedQt() ? " (Extended QT)" : "");
        }

        private string ResolveSource(PdfSource source)
        {
            switch (source)
            {
                case PdfSourceHtml html:
                    return html.Html;
                case PdfSourceFile file:
                    return File.ReadAllText(file.Path);
                default:
                    return null;
            }
        }

        private const string PageBreak = "<div style=\"page-break-after: always;\"></div>";
        private const string CloseHtml = "</body></html>";
        private const string StartHtml = "<!DOCTYPE html><html><head><base href=\"file://{0}\"/>{1}</head><body>";

        private const string HeaderFootStart =
            "<div style=\"overflow: hidden; margin: 0; height:{0}mm; page-break-inside: avoid;\">";

        private const string HeaderFootClose = "</div>";
        private const string StyleLinkHtml = "<link rel=\"stylesheet\" href=\"{0}\"/>";
        private const string StyleHtml = "<style>{0}</style>";
        private const string ScriptLinkHtml = "<script type=\"text/javascript\" language=\"javascript\" src=\"{0}\"></script>";
        private const string ScriptHtml = "<script type=\"text/javascript\" language=\"javascript\">{0}</script>";
        private const string SplitDocumentUri = "itechopdf://splitdocument";
        private const string AnchorHtml = "<a href=\"{0}\">{1}</a>";

        private const string TableOverflowHeaderStyle =
            "thead { display: table-header-group; } tfoot { display: table-row-group; } tr { page-break-inside: avoid; }";


        private string BuildHtml(PdfDocument doc)
        {
            var builder = new StringBuilder();
            var res = new List<PdfResource>(doc.Resources)
            {
                new PdfResource(PdfSource.FromHtml(TableOverflowHeaderStyle), ResourcePlacement.Head,
                    ResourceType.StyleSheet)
            };
            builder.Append(Format(StartHtml, doc.BaseUrl, RenderResources(res)));

            foreach (var page in doc.Pages)
            {
                // Idea here is to split all pages with a empty page
                if (page != doc.Pages.First())
                {
                    // Split document pages by empty page to identify split
                    // Pages may overflow onto next page
                    builder.Append(PageBreak);
                    builder.Append(Format(AnchorHtml, SplitDocumentUri, '-'));
                    builder.Append(PageBreak);
                }

                var html = ResolveSource(page.Source);
                builder.Append(html);
            }

            builder.Append(CloseHtml);
            return builder.ToString();
        }

        private void BuildHeaderFooter(StringBuilder builder, PdfSource source, double height,
            IReadOnlyCollection<VariableReplace> vars)
        {
            builder.Append(Format(HeaderFootStart, height));
            var html = ResolveSource(source);
            if (!IsNullOrEmpty(html))
            {
                html = ReplaceHtmlWithVariables(html, vars);
                builder.Append(html);
            }

            builder.Append(HeaderFootClose);
        }

        private string BuildHeaderFooter(PdfDocument doc, IEnumerable<PageCount> counts)
        {
            if (doc.HeaderHeight == 0 && doc.FooterHeight == 0)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.Append(Format(StartHtml, doc.BaseUrl, RenderResources(doc.Resources)));

            var list = counts.ToList();
            foreach (var c in list)
            {
                if (c != list.First())
                {
                    builder.Append(PageBreak);
                }

                // all variables is zero based
                var replace = new List<VariableReplace>
                {
                    new VariableReplace("overflow", c.Overflow + 1),
                    new VariableReplace("overflows", c.Overflows + 1),
                    new VariableReplace("document", c.Documents + 1),
                    new VariableReplace("documents", c.Documents + 1),
                    new VariableReplace("page", c.Page + 1),
                    new VariableReplace("pages", c.Pages + 1)
                };

                var extra = doc.VariableResolver?.Invoke(c);
                if (extra != null)
                {
                    replace.AddRange(extra);
                }

                BuildHeaderFooter(builder, c.RenderPage.Header ?? doc.HeaderSource, doc.HeaderHeight, replace);
                builder.Append(PageBreak);
                BuildHeaderFooter(builder, c.RenderPage.Footer ?? doc.FooterSource, doc.FooterHeight, replace);
                if (c != list.Last())
                {
                    builder.Append(PageBreak);
                }
            }

            builder.Append(CloseHtml);
            return builder.ToString();
        }

        //  case LinkType.Web:
        //   //pdf.AppendFormat("/A<</S/URI/URI{0}>>\n", PdfEncoders.EncodeAsLiteral(this.url));
        //   Elements[Keys.A] = new PdfLiteral("<</S/URI/URI{0}>>", //PdfEncoders.EncodeAsLiteral(this.url));
        //     PdfEncoders.ToStringLiteral(this.url, PdfStringEncoding.WinAnsiEncoding, writer.SecurityHandler));
        //   break;
        private static string GetUrlLink(PdfAnnotation ano)
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

        private bool IsPageSplit(PdfSharp.Pdf.PdfPage page)
        {
            return page.Annotations.Count == 1 && GetUrlLink(page.Annotations[0]) == SplitDocumentUri;
        }

        private List<PageCount> DocumentToPdf(PdfSharp.Pdf.PdfDocument finalPdf)
        {
            var pageCounters = new List<PageCount>();
            int pageCount = 0;
            int documentCount = 0;

            foreach (var doc in Documents)
            {
                // Concatenate all pages into one HTML document for performance
                // Try to minimize calls to native WkHtmlToPdf because it's slowish.
                var htmlPages = BuildHtml(doc);

                var settings = ConvertToCoreSettings(doc);
                var bytes = HtmlToPdf(htmlPages, settings);

                // Now read / parse the generate PDF to determine page counts
                var pdf = PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Import);
                var overflowCount = 0;
                var renderPageIndex = 0;
                for (var p = 0; p < pdf.PageCount; p++)
                {
                    var page = pdf.Pages[p];
                    // DocumentSplit
                    if (IsPageSplit(page))
                    {
                        // Update the last overflow overflowCount to overflowCount
                        for (var i = 0; i < overflowCount; i++)
                        {
                            pageCounters[pageCounters.Count - i - 1].Overflows = overflowCount - 1;
                        }

                        overflowCount = 0;
                        renderPageIndex++;
                        continue;
                    }

                    var newPage = finalPdf.AddPage(page);

                    pageCounters.Add(new PageCount
                    {
                        Overflow = overflowCount,
                        Page = pageCount,
                        Document = documentCount,
                        PdfPage = newPage,

                        RenderDocument = doc,
                        RenderPage = doc.Pages.ElementAt(renderPageIndex)
                    });

                    overflowCount++;
                    pageCount++;
                }

                documentCount++;
            }

            // Update document and page count
            foreach (var c in pageCounters)
            {
                c.Documents = documentCount - 1;
                c.Pages = pageCount - 1;
            }

            return pageCounters;
        }

        public byte[] RenderToBytes(string title = null, string author = null, string subject = null,
            string keywords = null)
        {
            // 1. For all PDF documents render pages with no header or footer, but with the correct header and footer heights.
            // The final output document
            var finalPdf = new PdfSharp.Pdf.PdfDocument();
            finalPdf.Info.Title = title;
            finalPdf.Info.Author = author;
            finalPdf.Info.Subject = subject;
            finalPdf.Info.Keywords = keywords;
            finalPdf.Info.Creator = "ItechoPDF";

            var pageCounters = DocumentToPdf(finalPdf);

            // Now generate headers and footers
            foreach (var doc in Documents)
            {
                // get all pages of the generated document
                var counters = pageCounters
                    .Where(c => c.RenderDocument == doc)
                    .ToList();
                
                // and generate a header / footer pair for each page
                // one page can be rendered as multiple pages
                var html = BuildHeaderFooter(doc, counters);

                if (html == null) continue;
                // add 1mm extra to fill rounding height issues caused by WkhtmlToPdf
                var height = Math.Max(doc.HeaderHeight, doc.FooterHeight) + 1;
                // Keep the same settings as the document except the height and y - margins
                var settings = ConvertToCoreSettings(doc);
                // JSDelay and windows status in headers and footer not supported yet.
                settings.JSDelay = 0;
                settings.WindowStatus = null;
                
                settings.Orientation = Orientation.Portrait;
                if (doc.Settings.Orientation == Orientation.Landscape)
                {
                    settings.PaperHeight = height + "mm";
                    settings.PaperWidth = doc.Settings.PaperSize.Height + "mm";
                }
                else
                {
                    settings.PaperHeight = height + "mm";
                    settings.PaperWidth = doc.Settings.PaperSize.Width + "mm";
                }

                settings.MarginBottom = "0mm";
                settings.MarginTop = "0mm";

                var bytes = HtmlToPdf(html, settings);

                XPdfForm hf = XPdfForm.FromStream(new MemoryStream(bytes));

                // HF can be 1 page more than pages because of page breaks or of manual page breaking inside headers or footers
                // But should never be less.
                if (hf.PageCount < counters.Count() * 2)
                {
                    throw new Exception("Header and footer segments does not match number of pages.");
                }

                int pageCount = 0;
                foreach (var c in counters)
                {
                    XGraphics gfx = XGraphics.FromPdfPage(c.PdfPage);
                    // 1 inch = 72 points, 1 inch = 25.4 mm, 1 point = 0.352777778 mm
                    // points per millimeter
                    var ppm = c.PdfPage.Height.Point /
                              c.PdfPage.Height.Millimeter; // or page.Width.Point / page.Width.Millimeter
                    var mt = doc.Settings.Margins.Top ?? 0;
                    var mb = doc.Settings.Margins.Bottom ?? 0;

                    hf.PageIndex = pageCount++;
                    gfx.DrawImage(hf, 0, mt * ppm);

                    hf.PageIndex = pageCount++;
                    gfx.DrawImage(hf, 0, c.PdfPage.Height - ((doc.FooterHeight - mb) * ppm));
                }
            }

            using (var ms = new MemoryStream())
            {
                finalPdf.Save(ms);
                return ms.ToArray();
            }
        }

        private static byte[] HtmlToPdf(string html, WkHtmlToPdfSettings settings)
        {
            return WkHtmlToPdf.HtmlToPdf(Encoding.UTF8.GetBytes(html), settings);
        }

        private static WkHtmlToPdfSettings ConvertToCoreSettings(PdfDocument document)
        {
            var settings = document.Settings;

            // 25mm header + 10mm spacing + 1mm margin top
            // Set margins. Header and footers may affect margins
            double? marginTop = settings.Margins.Top;
            double? marginBottom = settings.Margins.Bottom;

            if (marginTop.HasValue)
            {
                marginTop = marginTop.Value + document.HeaderHeight;
            }

            if (marginBottom.HasValue)
            {
                marginBottom = marginBottom.Value + document.FooterHeight;
            }
            
            if (!settings.EnableJavascript && (settings.JSDelay.HasValue || !IsNullOrEmpty(settings.WindowStatus)))
            {
                throw new Exception("Javascript cannot be disable if JSDelay or windowStatus is set. Please set EnabledJavascript to true.");
            }
            
            if (!IsNullOrEmpty(settings.WindowStatus))
            {
                if (settings.JSDelay.HasValue)
                    throw new Exception("JSDelay cannot be used in conjunction with WindowsStatus");
                
            }

            return new WkHtmlToPdfSettings
            {
                BlockLocalFileAccess = settings.BlockLocalFileAccess,
                DebugJavascript = settings.DebugJavascript,
                JSDelay = settings.JSDelay,
                WindowStatus = settings.WindowStatus,
                LoadErrorHandling = settings.LoadErrorHandling,
                Password = settings.Password,
                Proxy = settings.Proxy,
                StopSlowScript = settings.StopSlowScript,
                Username = settings.Username,

                Collate = settings.Collate,
                ColorMode = settings.ColorMode,
                CookieJar = settings.CookieJar,
                Copies = settings.Copies,
                DefaultEncoding = settings.DefaultEncoding,
                DocumentTitle = settings.DocumentTitle,
                DPI = settings.DPI,
                DumpOutline = settings.DumpOutline,
                EnableIntelligentShrinking = settings.EnableIntelligentShrinking,
                EnableJavascript = settings.EnableJavascript,
                Footer = null, // BuildHeaderFooter(document.Footer, document.Settings),
                Header = null, // BuildHeaderFooter(document.Header, document.Settings),
                ImageDPI = settings.ImageDPI,
                ImageQuality = settings.ImageQuality,
                IncludeInOutline = settings.IncludeInOutline,
                LoadImages = settings.LoadImages,
                MarginLeft = settings.Margins.GetMarginValue(settings.Margins.Left),
                MarginRight = settings.Margins.GetMarginValue(settings.Margins.Right),
                MarginBottom = settings.Margins.GetMarginValue(marginBottom),
                MarginTop = settings.Margins.GetMarginValue(marginTop),
                MinimumFontSize = settings.MinimumFontSize,
                Orientation = settings.Orientation,
                Outline = settings.Outline,
                OutlineDepth = settings.OutlineDepth,
                PageOffset = settings.PageOffset,
                PagesCount = settings.PagesCount,
                // always in millimeters
                PaperHeight = settings.PaperSize.Height + "mm",
                PaperWidth = settings.PaperSize.Width + "mm",
                PaperSize = null,
                PrintBackground = settings.PrintBackground,
                PrintMediaType = settings.PrintMediaType,
                ProduceForms = settings.ProduceForms,
                UseCompression = settings.UseCompression,
                UseExternalLinks = settings.UseExternalLinks,
                UseLocalLinks = settings.UseExternalLinks,
            };
        }

        private static string ReplaceHtmlWithVariables(string html, IReadOnlyCollection<VariableReplace> vars)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var varNodes = doc.DocumentNode.SelectNodes("//var");
            if (varNodes == null)
            {
                return html;
            }

            foreach (var n in varNodes)
            {
                var name = n.GetAttributeValue("name", null);
                if (IsNullOrEmpty(name))
                {
                    continue;
                }

                var var = vars.FirstOrDefault(v => v.Name == name);
                if (var == null)
                {
                    continue;
                }

                var replace = doc.CreateTextNode(var.Replace);
                n.ParentNode.ReplaceChild(replace, n);
            }

            using (var text = new StringWriter())
            {
                doc.Save(text);
                return text.ToString();
            }
        }

        
        private static string RenderResources(IReadOnlyCollection<PdfResource> resources)
        {
            if (resources == null)
            {
                return null;
            }

            var content = new StringBuilder();
            foreach (var res in resources)
            {
                content.Append(res.Type == ResourceType.Javascript
                    ? CreateJavascriptResource(res.Content)
                    : CreateCssResource(res.Content));
            }

            return content.ToString();
        }

        private static string CreateCssResource(PdfSource source)
        {
            switch (source)
            {
                case PdfSourceFile file:
                    return Format(StyleLinkHtml, file.Path);
                case PdfSourceHtml html:
                    return Format(StyleHtml, html.Html);
                default:
                    return null;
            }
        }

        private static string CreateJavascriptResource(PdfSource source)
        {
            switch (source)
            {
                case PdfSourceFile file:
                    return Format(ScriptLinkHtml, file.Path);
                case PdfSourceHtml content:
                    return Format(ScriptHtml, content.Html);
                default:
                    return null;
            }
        }
    }
}