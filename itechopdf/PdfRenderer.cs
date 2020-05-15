using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ItechoPdf.Core;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;

namespace ItechoPdf
{
    public class PdfRenderer
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

        private void BuilderAppend(StringBuilder builder, PdfSource source)
        {
             if (source is PdfSourceHtml html)
            {
                builder.Append(html.Html);
            }
            else if (source is PdfSourceFile file)
            {
                builder.Append(File.ReadAllText(file.Path));
            }
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
                BuilderAppend(builder, page.Source);
            }
            builder.Append(CloseHtml);
            return builder.ToString();
        }

        private string BuildHeaderFooter(PdfDocument doc)
        {
            if (doc.HeaderHeight == 0 && doc.FooterHeight == 0)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.Append(String.Format(StartHtml, doc.BaseUrl, RenderResources(doc.Resources)));

            // Now add all the headers and footer in abs position
            foreach (var page in doc.Pages)
            {
                if (page != doc.Pages.First())
                {
                    builder.Append(PageBreak);
                }
                if (doc.HeaderHeight != 0)
                {
                    builder.Append(String.Format(HeaderFootStart, doc.HeaderHeight));
                    BuilderAppend(builder, page.Header);
                    builder.Append(HeaderFootClose);
                }
                if (doc.FooterHeight != 0)
                {
                    builder.Append(String.Format(HeaderFootStart, doc.FooterHeight));
                    BuilderAppend(builder, page.Footer);
                    builder.Append(HeaderFootClose);
                }
            }
            builder.Append(CloseHtml);
            return builder.ToString();
        }

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
        
        public byte[] RenderToBytes()
        {
            // 1. For all PDF documents render pages with no header or footer, but with the correct header and footer heights.
            
            long total = 0;
            int count = 0;
            foreach (var doc in _documents)
            {
                var watch = new Stopwatch();
                watch.Start();
            
                var htmlPages = BuildHtml(doc);

                var settings = ConvertToCoreSettings(doc);
                var bytes = HtmlToPdf(htmlPages, settings);
                
                watch.Stop();
                Console.WriteLine($"Total elapsed time {watch.ElapsedMilliseconds}ms");
                total += watch.ElapsedMilliseconds;

                // Now read / parse the generate PDF to determine page counts
                var inDoc = PdfReader.Open(new MemoryStream(bytes), PdfDocumentOpenMode.Import);
           
                File.WriteAllBytes($"{count}.pdf", bytes);

                // var headerFooterHtml = BuildHeaderFooter(doc);
                // if (headerFooterHtml != null)
                // {
                //     var bb = HtmlToPdf(headerFooterHtml, settings);
                //     File.WriteAllBytes($"{count}-headerfooters.pdf", bb);
                // }
                
                
                count++;
            }
            Console.WriteLine($"Total time: {total}ms");

            // var replace = new List<VariableReplace>
            // {
            //     new VariableReplace("documentpage", (docpage + 1).ToString()),
            //     new VariableReplace("documentpages", edit.Pages.ToString()),
            //     new VariableReplace("page", (page + 1).ToString()),
            //     new VariableReplace("pages", totalPages.ToString()),
            // };
            
            // Merge PDF's
            return null;
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
                LoadErrorHandling = settings.LoadErrorHandling?.ToString(),
                Password = settings.Password,
                Proxy = settings.Proxy,
                StopSlowScript = settings.StopSlowScript,
                Username = settings.Username,

                Collate = settings.Collate,
                ColorMode = settings.ColorMode?.ToString(),
                CookieJar = settings.CookieJar,
                Copies = settings.Copies,
                DefaultEncoding = settings.DefaultEncoding,
                DocumentTitle = settings.DocumentTitle,
                DPI = settings.DPI,
                DumpOutline = settings.DumpOutline,
                EnableIntelligentShrinking = settings.EnableIntelligentShrinking,
                EnableJavascript = settings.EnableJavascript,
                Footer =  null, // BuildHeaderFooter(document.Footer, document.Settings),
                Header =  null, // BuildHeaderFooter(document.Header, document.Settings),
                ImageDPI = settings.ImageDPI,
                ImageQuality = settings.ImageQuality,
                IncludeInOutline = settings.IncludeInOutline,
                LoadImages = settings.LoadImages,
                MarginLeft = settings.Margins.GetMarginValue(settings.Margins.Left),
                MarginRight = settings.Margins.GetMarginValue(settings.Margins.Right),
                MarginBottom = settings.Margins.GetMarginValue(marginBottom),
                MarginTop = settings.Margins.GetMarginValue(marginTop),
                MinimumFontSize = settings.MinimumFontSize,
                Orientation = settings.Orientation?.ToString(),
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
        
        // private HtmlDocument DocFromSource(PdfSource source, List<PdfResource> resources, bool replace, PdfSettings settings)
        // {
        //     var htmlDoc = new HtmlDocument();
        //     htmlDoc.CreateElement("html");
            
        //     string baseUrl = null;
        //     if (source is PdfSourceHtml html)
        //     {
        //         baseUrl = html.BaseUrl ?? Environment.CurrentDirectory;
        //         htmlDoc.LoadHtml(html.Html);
        //     }

        //     if (source is PdfSourceFile file)
        //     {
        //         baseUrl = Path.GetDirectoryName(Path.GetFullPath(file.Path)) + Path.DirectorySeparatorChar;
        //         using (var fs = System.IO.File.OpenRead(file.Path))
        //         {
        //             htmlDoc.Load(fs);
        //         }
        //     }
            
        //     return FormatHtml(htmlDoc, baseUrl, resources, replace, settings);
        // }
        
        // private HeaderFooterSettings BuildHeaderFooter(HeaderFooter settings, PdfSettings pdf)
        // {
        //     if (settings == null)
        //     {
        //         return null;
        //     }

        //     if (settings is HtmlHeaderFooter source)
        //     {
        //         HtmlDocument htmlDoc = DocFromSource(source.Source, null, true, pdf);
        //         var path = CreateTempFile();
        //         using (var sw = System.IO.File.Create(path))
        //         {
        //             htmlDoc.Save(sw);
        //         }

        //         return new HeaderFooterSettings
        //         {
        //             Spacing = settings.Spacing,
        //             Line = settings.Line,
        //             Url = path
        //         };
        //     }

        //     throw new Exception($"Unknown settings type {settings.GetType().Name}");
        // }

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
