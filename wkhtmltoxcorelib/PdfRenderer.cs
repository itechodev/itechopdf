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
                HtmlDocument htmlDoc = DocFromSource(doc.Source);
                pdfs.Add(HtmlDocToPdf(htmlDoc, doc.LoadSettings, doc.PrintSettings));
                // Keep track of number count.
            }
            // Merge all PDF's and return one result.
            return MergePDFBytes(pdfs);
        }

        public void  RenderToFile(string output)
        {
            File.WriteAllBytes(output, RenderToBytes());
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

        private byte[] HtmlDocToPdf(HtmlDocument doc, LoadSettings loadSettings, PrintSettings printSettings)
        {
            try
            {
                using (var sw = new StringWriter())
                {
                    doc.Save(sw);
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
                marginTop = marginTop.Value + (printSettings.Header?.Spacing ?? 0) + (printSettings.Header?.Height ?? 0);
            }

            if (marginBottom.HasValue)
            {
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

        
        private HtmlDocument DocFromSource(PdfSource pdfSource)
        {
            var htmlDoc = new HtmlDocument();
            string baseUrl = null;
            if (pdfSource is PdfSourceHtml html)
            {
                baseUrl = html.BaseUrl ?? Environment.CurrentDirectory;
                htmlDoc.LoadHtml(html.Html);
            }

            if (pdfSource is PdfSourceFile file)
            {
                baseUrl = Path.GetDirectoryName(Path.GetFullPath(file.Path)) + Path.DirectorySeparatorChar;
                htmlDoc.Load(file.Path);
            }
            
            return FormatHtml(htmlDoc, baseUrl);
        }
        
        private HeaderFooterSettings BuildHeaderFooter(HeaderFooter settings)
        {
            if (settings == null)
            {
                return null;
            }

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
            
            if (settings is HtmlHeaderFooter source)
            {
                HtmlDocument htmlDoc = DocFromSource(source.Source);
                var path = CreateTempFile();
                htmlDoc.Save(path);

                return new HeaderFooterSettings
                {
                    Spacing = settings.Spacing,
                    Line = settings.Line,
                    Url = path
                };
            }

            throw new Exception($"Unknown settings type {settings.GetType().Name}");
        }

        private string CreateTempFile()
        {
            // For some reason it should end in html
            var path = Path.GetTempFileName() + ".html";
            // Keep reference to file can later delete it
            _tempFiles.Add(path);
            return path;
        }


        private HtmlDocument FormatHtml(HtmlDocument doc, string baseUrl)
        {
            HtmlNode html = doc.DocumentNode.SelectSingleNode("html");
            if (html == null)
            {
                // html does not exists. <head> and <body> possible inside html
                // Add everything into html
                html = doc.CreateElement("html");
                html.AppendChildren(doc.DocumentNode.ChildNodes);
            }
            // head might be inside html
            HtmlNode head = html.SelectSingleNode("head");
            if (head == null)
            {
                head = doc.CreateElement("head");
                html.PrependChild(head);
            }
            // Add base for resource paths
            var baseTag =  doc.CreateElement("base");
            baseTag.SetAttributeValue("href", @"file://" + baseUrl);
            head.AppendChild(baseTag);


            HtmlNode body = html.SelectSingleNode("body");
            if (body == null)
            {
                body = doc.CreateElement("body");
                // All html.ChildNodes except head
                HtmlNodeCollection col = new HtmlNodeCollection(body);
                foreach (var child in html.ChildNodes)
                {
                    if (child != head)
                    {
                        col.Add(child);
                    }
                }
            }
            
            var newDoc = new HtmlDocument();
            // Ensure doctype
            HtmlCommentNode doctype = doc.CreateComment("<!DOCTYPE html>");
            newDoc.DocumentNode.PrependChild(doctype);
            newDoc.DocumentNode.AppendChild(html);
            
            return newDoc;
        }
    }
}
