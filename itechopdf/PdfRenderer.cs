using System;
using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using ItechoPdf.Core;
using PdfSharp.Pdf;
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

        public PdfDocument AddDocument(PdfSource source)
        {
            var doc = new PdfDocument(source, new PdfSettings(Settings));
            Add(doc);
            return doc;
        }

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
            List<PdfEditor> editors = new List<PdfEditor>();

            int totalPages = 0;
            foreach (var doc in _documents)
            {
                HtmlDocument htmlDoc = DocFromSource(doc.Source, doc.Resources, false);
                var bytes = HtmlDocToPdf(htmlDoc, doc);

                var editor = new PdfEditor();
                editor.HeightAndSpacing = doc.Header?.Height + doc.Header?.Spacing ?? 0;
                editor.ReadFromBytes(bytes);
                totalPages += editor.Pages;

                editors.Add(editor);
            }

            // Now replace all variables
            List<byte[]> pdfs = new List<byte[]>();
            
            int page = 0;
            foreach (var edit in editors)
            {
                for (int docpage = 0; docpage < edit.Pages; docpage++)
                {
                    var replace = new List<VariableReplace>
                    {
                        new VariableReplace("documentpage", (docpage + 1).ToString()),
                        new VariableReplace("documentpages", edit.Pages.ToString()),
                        new VariableReplace("page", (page + 1).ToString()),
                        new VariableReplace("pages", totalPages.ToString()),
                    };
                    edit.ReplacePage(docpage, replace);
                    page++;
                }
                var postReplace = edit.Save();
                pdfs.Add(postReplace);
            }
            
            // Merge all PDF's and return one result.
            return MergePDFBytes(pdfs);
        }

        public void RenderToFile(string output)
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

        private byte[] HtmlDocToPdf(HtmlDocument doc, PdfDocument document)
        {
            try
            {
                using (var sw = new StringWriter())
                {
                    doc.Save(sw);
                    return WkHtmlToPdf.HtmlToPdf(System.Text.Encoding.UTF8.GetBytes(sw.ToString()), ConvertToCoreSettings(document));
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

        private WkHtmlToPdfSettings ConvertToCoreSettings(PdfDocument document)
        {
            var settings = document.Settings;

            // 25mm header + 10mm spacing + 1mm margin top
            // Set margins. Header and footers may affect marings
            double? marginTop = settings.Margins.Top;
            double? marginBottom = settings.Margins.Bottom;

            if (marginTop.HasValue)
            {
                marginTop = marginTop.Value + (document.Header?.Spacing ?? 0) + (document.Header?.Height ?? 0);
            }

            if (marginBottom.HasValue)
            {
                marginBottom = marginBottom.Value + (document.Footer?.Spacing ?? 0) + (document.Footer?.Height ?? 0);
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
                Footer = BuildHeaderFooter(document.Footer),
                Header = BuildHeaderFooter(document.Header),
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
                PaperHeight = settings.PaperSize.Height,
                PaperWidth = settings.PaperSize.Width,
                PaperSize = null,
                PrintBackground = settings.PrintBackground,
                PrintMediaType = settings.PrintMediaType,
                ProduceForms = settings.ProduceForms,
                UseCompression = settings.UseCompression,
                UseExternalLinks = settings.UseExternalLinks,
                UseLocalLinks = settings.UseExternalLinks,
            };
        }

        
        private HtmlDocument DocFromSource(PdfSource source, List<PdfResource> resources, bool replace)
        {
            var htmlDoc = new HtmlDocument();
            string baseUrl = null;
            if (source is PdfSourceHtml html)
            {
                baseUrl = html.BaseUrl ?? Environment.CurrentDirectory;
                htmlDoc.LoadHtml(html.Html);
            }

            if (source is PdfSourceFile file)
            {
                baseUrl = Path.GetDirectoryName(Path.GetFullPath(file.Path)) + Path.DirectorySeparatorChar;
                using (var fs = File.OpenRead(file.Path))
                {
                    htmlDoc.Load(fs);
                }
            }
            
            return FormatHtml(htmlDoc, baseUrl, resources, replace);
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
                HtmlDocument htmlDoc = DocFromSource(source.Source, null, true);
                var path = CreateTempFile();
                using (var sw = File.Create(path))
                {
                    htmlDoc.Save(sw);
                }

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


        private HtmlDocument FormatHtml(HtmlDocument doc, string baseUrl, List<PdfResource> resources, bool replaceVariables)
        {
            // make sure baeUrl is always ending with directory seperator
            if (!baseUrl.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseUrl += Path.DirectorySeparatorChar;
            }

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
            head.PrependChild(baseTag);


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

            AddResources(head, body, resources);

            // Replace variables
            if (replaceVariables)
            {
                string inner = body.InnerHtml;
                inner = inner.Replace("{{documentpage}}", CreateReplacementAnchor(doc, "documentpage"));
                inner = inner.Replace("{{documentpages}}", CreateReplacementAnchor(doc, "documentpages"));
                inner = inner.Replace("{{page}}", CreateReplacementAnchor(doc, "page"));
                inner = inner.Replace("{{pages}}", CreateReplacementAnchor(doc, "pages"));
                
                body.InnerHtml = inner;
            }

            return newDoc;
        }

        public string CreateReplacementAnchor(HtmlDocument doc, string fragment)
        {
            var a = doc.CreateElement("a");
            a.SetAttributeValue("style", "text-decoration: none; color:inherit; position: relative;");
            a.SetAttributeValue("href", "#" + fragment);
            a.InnerHtml = "1";
            return a.OuterHtml;
        }

        private void AddResources(HtmlNode head, HtmlNode body, List<PdfResource> resources)
        {
            if (resources == null) 
            {
                return;
            }
            foreach (var res in resources)
            {
                var node = res.Type == ResourceType.Javascript
                    ? CreateJavascriptResource(head.OwnerDocument, res.Source)
                    : CreateCSSResource(head.OwnerDocument, res.Source);

                if (res.Placement == ResourcePlacement.Head) 
                {
                    head.AppendChild(node);
                }
                else 
                {
                    body.AppendChild(node);
                }
            }            
        }

        private HtmlNode CreateCSSResource(HtmlDocument document, PdfSource source)
        {
            if (source is PdfSourceFile file)
            {
                var node = document.CreateElement("link");
                node.SetAttributeValue("rel", "stylesheet");
                node.SetAttributeValue("href", file.Path);
                return node;
            }
            if (source is PdfSourceHtml html)
            {
                var node = document.CreateElement("style");
                node.AppendChild(document.CreateTextNode(html.Html));
                return node;
            }
            throw new NotImplementedException();
        }

        private HtmlNode CreateJavascriptResource(HtmlDocument document, PdfSource source)
        {
            var node = document.CreateElement("script");
            node.SetAttributeValue("type", "text/javascript");
            node.SetAttributeValue("language", "javascript");
            if (source is PdfSourceFile file)
            {
                node.SetAttributeValue("src", file.Path);
            }
            if (source is PdfSourceHtml content)
            {
                node.AppendChild(document.CreateTextNode(content.Html));
            }
            return node;
        }
    }
}
