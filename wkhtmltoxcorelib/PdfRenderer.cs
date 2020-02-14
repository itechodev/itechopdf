using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using wkpdftoxcorelib.Core;

namespace wkpdftoxcorelib
{
    public class PdfRenderer
    {
        private List<PdfDocument> _documents { get; set; } = new List<PdfDocument>();
        private List<string> _tempFiles = new List<string>();

        public string GetVersion()
        {
            return WkHtmlToPdf.GetVersion() + (WkHtmlToPdf.ExtendedQt() ? " (Extended QT)" : "");
        }

        public void Add(PdfDocument doc)
        {
            _documents.Add(doc);
        }

        public void InsertAt(PdfDocument doc, int index)
        {
            _documents.Insert(index, doc);
        }

        public void Clear()
        {
            _documents.Clear();
        }

        public byte[] RenderToBytes()
        {
            List<byte[]> pdfs = new List<byte[]>();
            foreach (var doc in _documents)
            {
                if (doc.Source is PdfSourceFile file)
                {
                    pdfs.Add(HtmlToPdf(file.Path, doc.LoadSettings, doc.PrintSettings));
                }
                else if (doc.Source is PdfSourceHtml html)
                {
                    pdfs.Add(HtmlFileToPdf(html.Html, doc.LoadSettings, doc.PrintSettings));
                }
                // Keep track of number count.
            }
            // Merge all PDF's and return one result.
            return MergePDFBytes(pdfs);
        }

        public byte[] MergePDFBytes(List<byte[]> pdfs)
        {
            PdfSharp.Pdf.PdfDocument outputDocument = new PdfSharp.Pdf.PdfDocument();
            foreach (var pdfBytes in pdfs)
            {
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    continue;
                }
                
                var pdfDoc = PdfReader.Open(new MemoryStream(pdfBytes), PdfDocumentOpenMode.Import);
                // Iterate pages
                int count = pdfDoc.PageCount;
                for (int idx = 0; idx < count; idx++)
                {
                    // Get the page from the external document...
                    PdfPage page = pdfDoc.Pages[idx];
                    // ...and add it to the output document.
                    outputDocument.AddPage(page);
                }
            }
            using (var ms = new MemoryStream())
            {
                
                outputDocument.Save(ms);
                return ms.ToArray();
            }
        }

        private byte[] HtmlFileToPdf(string filename, LoadSettings loadSettings, PrintSettings printSettings)
        {
            return HtmlToPdf(File.ReadAllText(filename), loadSettings, printSettings);
        }

        private byte[] HtmlToPdf(string html, LoadSettings loadSettings, PrintSettings printSettings)
        {
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                FormatHtml(htmlDoc, Environment.CurrentDirectory);
                using (var sw = new StringWriter())
                {
                    htmlDoc.Save(sw);
                    return WkHtmlToPdf.HtmlToPdf(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), ConvertToCoreSettings(loadSettings, printSettings));

                }
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
                File.Delete(tempFile);
            }
        }

        private WkHtmlToPdfSettings ConvertToCoreSettings(LoadSettings loadSettings, PrintSettings printSettings)
        {
            // 25mm header + 10mm spacing + 1mm margin top
            // Set margins. Header and footers may affect marings
            double? marginTop = printSettings.Margins.Top;
            double? marginBottom = printSettings.Margins.Bottom;

            if (marginTop.HasValue)
            {
                if (printSettings.Header != null && printSettings.Header.Height == null)
                {
                    throw new Exception("Header height should be explicit when margin top is explicit.");
                }
                marginTop = marginTop.Value + (printSettings.Header?.Spacing ?? 0) + (printSettings.Header?.Height ?? 0);
            }

            if (marginBottom.HasValue)
            {
                if (printSettings.Footer != null && printSettings.Footer.Height == null)
                {
                    throw new Exception("Footer height should be explicit when margin bottom is explicit.");
                }
                marginBottom = marginBottom.Value + (printSettings.Footer?.Spacing ?? 0) + (printSettings.Footer?.Height ?? 0);
            }

            return new WkHtmlToPdfSettings
            {
                BlockLocalFileAccess = loadSettings.BlockLocalFileAccess,
                DebugJavascript = loadSettings.DebugJavascript,
                JSDelay = loadSettings.JSDelay,
                LoadErrorHandling = loadSettings.LoadErrorHandling?.ToString(),
                Password = loadSettings.Password,
                Proxy = loadSettings.Proxy,
                StopSlowScript = loadSettings.StopSlowScript,
                Username = loadSettings.Username,

                Collate = printSettings.Collate,
                ColorMode = printSettings.ColorMode?.ToString(),
                CookieJar = printSettings.CookieJar,
                Copies = printSettings.Copies,
                DefaultEncoding = printSettings.DefaultEncoding,
                DocumentTitle = printSettings.DocumentTitle,
                DPI = printSettings.DPI,
                DumpOutline = printSettings.DumpOutline,
                EnableIntelligentShrinking = printSettings.EnableIntelligentShrinking,
                EnableJavascript = printSettings.EnableJavascript,
                Footer = BuildHeaderFooter(printSettings.Footer),
                Header = BuildHeaderFooter(printSettings.Header),
                ImageDPI = printSettings.ImageDPI,
                ImageQuality = printSettings.ImageQuality,
                IncludeInOutline = printSettings.IncludeInOutline,
                LoadImages = printSettings.LoadImages,
                MarginLeft = printSettings.Margins.GetMarginValue(printSettings.Margins.Left),
                MarginRight = printSettings.Margins.GetMarginValue(printSettings.Margins.Right),
                MarginBottom = printSettings.Margins.GetMarginValue(marginBottom),
                MarginTop = printSettings.Margins.GetMarginValue(marginTop),
                MinimumFontSize = printSettings.MinimumFontSize,
                Orientation = printSettings.Orientation?.ToString(),
                Outline = printSettings.Outline,
                OutlineDepth = printSettings.OutlineDepth,
                PageOffset = printSettings.PageOffset,
                PagesCount = printSettings.PagesCount,
                PaperHeight = printSettings.PaperSize.Height,
                PaperWidth = printSettings.PaperSize.Width,
                PaperSize = null,
                PrintBackground = printSettings.PrintBackground,
                PrintMediaType = printSettings.PrintMediaType,
                ProduceForms = printSettings.ProduceForms,
                UseCompression = printSettings.UseCompression,
                UseExternalLinks = printSettings.UseExternalLinks,
                UseLocalLinks = printSettings.UseExternalLinks,
            };
        }

        private HeaderFooterSettings BuildHeaderFooter(HeaderFooter settings)
        {
            if (settings == null)
            {
                return null;
            }

            var htmlDoc = new HtmlDocument();
            if (settings is StandardHeaderFooter std)
            {
                return new HeaderFooterSettings
                {
                    Center = std.Center,
                    FontName = std.FontName,
                    FontSize = std.FontSize,
                    Left = std.Left,
                    Line = std.Line,
                    Right = std.Right,
                    Spacing = std.Spacing
                };
            }

            if (settings is HtmlHeaderFooter html)
            {
                htmlDoc.LoadHtml(html.Html);
            }

            if (settings is FileHeaderFooter file)
            {
                htmlDoc.Load(file.FilePath);
            }

            FormatHtml(htmlDoc, Environment.CurrentDirectory);
            var path = CreateTempFile();
            htmlDoc.Save(path);

            return new HeaderFooterSettings
            {
                Spacing = settings.Spacing,
                Line = settings.Line,
                Url = path
            };
        }

        private string CreateTempFile()
        {
            // For some reason it should end in html
            var path = Path.GetTempFileName() + ".html";
            // Keep reference to file can later delete it
            _tempFiles.Add(path);
            return path;
        }

        private void FormatHtml(HtmlDocument htmlDoc, string baseUrl)
        {
            // Fixed paths for resources
            FixedPath(htmlDoc, baseUrl, "//img", "src");
            // Add Doctype
            // Add javascript for injection
        }

        private void FixedPath(HtmlDocument htmlDoc, string baseUrl, string xpath, string attribute)
        {
            var nodes = htmlDoc.DocumentNode.SelectNodes(xpath);
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }
            foreach (var node in nodes)
            {
                var attrValue = node.GetAttributeValue(attribute, null);
                if (String.IsNullOrEmpty(attrValue))
                {
                    continue;
                }
                string newValue = FormatUrl(attrValue, baseUrl);
                if (newValue != attrValue)
                {
                    node.SetAttributeValue(attribute, newValue);
                }
            }
        }

        private string FormatUrl(string url, string baseUrl)
        {
            string check = url.Trim().ToLower();
            if (check.StartsWith("http://") || check.StartsWith("https://") || check.StartsWith("file://") || check.StartsWith("data:") || check.StartsWith("/"))
            {
                // Url is absolute or contains inline data. Leave as is
                return url;
            }
            return Path.Combine(baseUrl, url);
        }

    }
}
