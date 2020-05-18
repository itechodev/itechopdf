using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ItechoPdf.Core;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;

namespace ItechoPdf
{
    public partial class PdfRenderer
    {
        private List<PdfDocument> _documents { get; set; } = new List<PdfDocument>();
        private List<string> _tempFiles = new List<string>();
        public PdfSettings Settings { get; set; } = new PdfSettings();

        public PdfRenderer(Action<PdfSettings> config = null)
        {
            config?.Invoke(Settings);
        }

        public PdfDocument AddDocument(int headerHeightmm = 0, int footerHeightmm = 0, string baseUrl = null, Action<PdfSettings> settingsAction = null)
        {
            // copy settings from render
            var settings = new PdfSettings(Settings);
            settingsAction?.Invoke(settings);
            var doc = new PdfDocument(headerHeightmm, footerHeightmm, baseUrl, settings);
            _documents.Add(doc);
            return doc;
        }

        public string GetVersion()
        {
            return WkHtmlToPdf.GetVersion() + (WkHtmlToPdf.ExtendedQt() ? " (Extended QT)" : "");
        }

        private string ResolveSource(PdfSource source)
        {
            if (source is PdfSourceHtml html)
            {
                return html.Html;
            }
            else if (source is PdfSourceFile file)
            {
                return File.ReadAllText(file.Path);
            }
            return null;
        }

        const string PageBreak = "<div style=\"page-break-after: always;\"></div>";
        const string CloseHtml = "</body></html>";
        const string StartHtml = "<!DOCTYPE html><html><head><base href=\"file://{0}\"/>{1}</head><body>";
        const string HeaderFootStart = "<div style=\"overflow: hidden; height: {0}mm\">";
        const string HeaderFootClose = "</div>";
        const string StyleLinkHtml = "<link rel=\"stylesheet\" href=\"{0}\"/>";
        const string StyleHtml = "<style>{0}</style>";
        const string ScriptLinkHtml = "<script type=\"text/javascript\" language=\"javascript\" src=\"{0}\"></script>";
        const string ScriptHtml = "<script type=\"text/javascript\" language=\"javascript\">{0}</script>";
        const string SplitDocumentUri = "itechopdf://splitdocument";
        const string AnchorHtml = "<a href=\"{0}\">{1}</a>";


        private string BuildHtml(PdfDocument doc)
        {
            var builder = new StringBuilder();
            builder.Append(String.Format(StartHtml, doc.BaseUrl, RenderResources(doc.Resources)));

            foreach (var page in doc.Pages)
            {
                // Idea here is to split all pages with a empty page
                if (page != doc.Pages.First())
                {
                    // Split document pages by empty page to identify split
                    // Pages may overflow onto next page
                    builder.Append(PageBreak);
                    builder.Append(String.Format(AnchorHtml, SplitDocumentUri, '-'));
                    builder.Append(PageBreak);
                }
                var html = ResolveSource(page.Source);
                builder.Append(html);
            }
            builder.Append(CloseHtml);
            return builder.ToString();
        }

        private void BuildHeaderFooter(StringBuilder builder, PdfSource source, int height, List<VariableReplace> vars)
        {
            builder.Append(String.Format(HeaderFootStart, height));
            var html = ResolveSource(source);
            if (!String.IsNullOrEmpty(html))
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
            builder.Append(String.Format(StartHtml, doc.BaseUrl, RenderResources(doc.Resources)));

            foreach (var c in counts)
            {
                if (c != counts.First())
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

                BuildHeaderFooter(builder, c.RenderPage.Header, doc.HeaderHeight, replace);
                builder.Append(PageBreak);
                BuildHeaderFooter(builder, c.RenderPage.Footer, doc.FooterHeight, replace);
                builder.Append(PageBreak);
            }
            builder.Append(CloseHtml);
            return builder.ToString();
        }

        //  case LinkType.Web:
        //   //pdf.AppendFormat("/A<</S/URI/URI{0}>>\n", PdfEncoders.EncodeAsLiteral(this.url));
        //   Elements[Keys.A] = new PdfLiteral("<</S/URI/URI{0}>>", //PdfEncoders.EncodeAsLiteral(this.url));
        //     PdfEncoders.ToStringLiteral(this.url, PdfStringEncoding.WinAnsiEncoding, writer.SecurityHandler));
        //   break;
        private string GetUrlLink(PdfAnnotation ano)
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

            foreach (var doc in _documents)
            {
                // Concatenate all pages into one HTML document for performance
                // Try to minimize calls to native WkHtmlToPdf because it's slowish.
                var htmlPages = BuildHtml(doc);

                var settings = ConvertToCoreSettings(doc);
                var bytes = HtmlToPdf(htmlPages, settings);

                // Now read / parse the generate PDF to determine page counts
                var pdf = PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Import);
                var overflowCount = 0;
                int renderPageIndex = 0;
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
        
        public byte[] RenderToBytes()
        {
            // 1. For all PDF documents render pages with no header or footer, but with the correct header and footer heights.
            // The final output document
            var finalPdf = new PdfSharp.Pdf.PdfDocument();
            
            var pageCounters = DocumentToPdf(finalPdf);

            // Now generate headers and footers
            foreach (var doc in _documents)
            {
                // get all pages of the generated document
                var counters = pageCounters.Where(c => c.RenderDocument == doc);
                // and genereate a header / footer pair for each page
                // one page can be rendered as multiple pages
                var html = BuildHeaderFooter(doc, counters);

                if (html != null)
                {                    
                    var height = Math.Max(doc.HeaderHeight, doc.FooterHeight);
                    // Keep the same settings as the document except the height and y - margins
                    var settings = ConvertToCoreSettings(doc);
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

                    if (hf.PageCount != counters.Count() * 2)
                    {
                        throw new Exception("Header and footer segments does not match number of pages.");
                    }
                    int pageCount = 0;
                    foreach (var c in counters)
                    {
                        XGraphics gfx = XGraphics.FromPdfPage(c.PdfPage);
                        // 1 inch = 72 points, 1 inch = 25.4 mm, 1 point = 0.352777778 mm
                        // points per millimeter
                        var ppm = c.PdfPage.Height.Point / c.PdfPage.Height.Millimeter; // or page.Width.Point / page.Width.Millimeter
                        var mt = doc.Settings.Margins.Top ?? 0;
                        var mb = doc.Settings.Margins.Bottom ?? 0;

                        hf.PageIndex = pageCount++;
                        gfx.DrawImage(hf, 0, mt * ppm);

                        hf.PageIndex = pageCount++;
                        gfx.DrawImage(hf, 0, c.PdfPage.Height - ((doc.FooterHeight - mb) * ppm));
                    }
                }
            }

            using (var ms = new MemoryStream())
            {
                finalPdf.Save(ms);
                return ms.ToArray();
            }
        }

        public void RenderToFile(string output)
        {
            System.IO.File.WriteAllBytes(output, RenderToBytes());
        }

        private byte[] HtmlToPdf(string html, WkHtmlToPdfSettings settings)
        {
            try
            {
                return WkHtmlToPdf.HtmlToPdf(System.Text.Encoding.UTF8.GetBytes(html), settings);
            }
            finally
            {
                CleanUp();
            }
        }

        private void CleanUp()
        {
            // Delete all temporiry files created
            foreach (var tempFile in _tempFiles)
            {
                System.IO.File.Delete(tempFile);
            }
        }

        private WkHtmlToPdfSettings ConvertToCoreSettings(PdfDocument document)
        {
            var settings = document.Settings;

            // 25mm header + 10mm spacing + 1mm margin top
            // Set margins. Header and footers may affect marings
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

            return new WkHtmlToPdfSettings
            {
                BlockLocalFileAccess = settings.BlockLocalFileAccess,
                DebugJavascript = settings.DebugJavascript,
                JSDelay = settings.JSDelay,
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

        private string ReplaceHtmlWithVariables(string html, List<VariableReplace> vars)
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
                if (String.IsNullOrEmpty(name))
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

        private string CreateTempFile()
        {
            // For some reason it should end in html
            var path = Path.GetTempFileName() + ".html";
            // Keep reference to file can later delete it
            _tempFiles.Add(path);
            return path;
        }

        private string RenderResources(List<PdfResource> resources)
        {
            if (resources == null)
            {
                return null;
            }
            StringBuilder content = new StringBuilder();
            foreach (var res in resources)
            {
                if (res.Type == ResourceType.Javascript)
                {
                    content.Append(CreateJavascriptResource(res.Content));
                }
                else
                {
                    content.Append(CreateCSSResource(res.Content));
                }
            }
            return content.ToString();
        }

        private string CreateCSSResource(PdfSource source)
        {
            if (source is PdfSourceFile file)
            {
                return String.Format(StyleLinkHtml, file.Path);
            }
            if (source is PdfSourceHtml html)
            {
                return String.Format(StyleHtml, html.Html);
            }
            return null;
        }

        private string CreateJavascriptResource(PdfSource source)
        {
            if (source is PdfSourceFile file)
            {
                return String.Format(ScriptLinkHtml, file.Path);
            }
            if (source is PdfSourceHtml content)
            {
                return String.Format(ScriptHtml, content.Html);
            }
            return null;
        }
    }
}
