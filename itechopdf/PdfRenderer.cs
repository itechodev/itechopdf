using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ItechoPdf.Core;

namespace ItechoPdf
{
    public enum PdfRendererStrategy
    {
        // Determine best rendering strategy based on variables present in header / footers
        Auto,
        // Single conversion
        Single,
        // First convert to PDF to determine page counts then again with replaced variables
        Double,
        // Render header and footer in separate document and merge them back in.
        Embedding
    }

    public class PdfRenderer
    {
        private List<PdfDocument> _documents { get; set; } = new List<PdfDocument>();
        private List<string> _tempFiles = new List<string>();
        public PdfSettings Settings { get; set; } = new PdfSettings();
        public PdfRendererStrategy Strategy { get; }

        public PdfRenderer(Action<PdfSettings> config = null, PdfRendererStrategy strategy = PdfRendererStrategy.Auto)
        {
            config?.Invoke(Settings);
            Strategy = strategy;
        }

        public PdfDocument AddDocument(int headerHeightmm = 0, int footerHeightmm = 0, PdfSettings settings = null)
        {
            var doc = new PdfDocument(headerHeightmm, footerHeightmm, settings);
            _documents.Add(doc);
            return doc;
        }

        public string GetVersion()
        {
            return WkHtmlToPdf.GetVersion() + (WkHtmlToPdf.ExtendedQt() ? " (Extended QT)" : "");
        }

        public byte[] RenderToBytes()
        {
            foreach (var doc in _documents)
            {
                // build html from page sources
                foreach (var page in doc.Pages)
                {

                }
                // doc.Settings.
                // // doc.HeaderHeight
                // // doc.FooterHeight
                // HtmlDocument htmlDoc = DocFromSource(doc.pages Source, doc.Resources, false, doc.Settings);
                // var bytes = HtmlDocToPdf(htmlDoc, doc);
            }

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

        
        private HtmlDocument DocFromSource(PdfSource source, List<PdfResource> resources, bool replace, PdfSettings settings)
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
                using (var fs = System.IO.File.OpenRead(file.Path))
                {
                    htmlDoc.Load(fs);
                }
            }
            
            return FormatHtml(htmlDoc, baseUrl, resources, replace, settings);
        }
        
        private HeaderFooterSettings BuildHeaderFooter(HeaderFooter settings, PdfSettings pdf)
        {
            if (settings == null)
            {
                return null;
            }

            if (settings is HtmlHeaderFooter source)
            {
                HtmlDocument htmlDoc = DocFromSource(source.Source, null, true, pdf);
                var path = CreateTempFile();
                using (var sw = System.IO.File.Create(path))
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


        private HtmlDocument FormatHtml(HtmlDocument doc, string baseUrl, List<PdfResource> resources, bool replaceVariables, PdfSettings settings)
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
            if (!replaceVariables)
            {
                return newDoc;
            }

            var varNodes = body.SelectNodes("//var");
            if (varNodes == null)
            {
                return newDoc;
            }
            foreach (var n in varNodes)
            {
                var text = n.GetAttributeValue("text", null);
                if (String.IsNullOrEmpty(text))
                {
                    continue;
                }
                // var align = n.GetAttributeValue("text-align", "right");
                // Int32.TryParse(n.GetAttributeValue("digits", "2"),  out int digits);
                // var textReplacement = Regex.Replace(text, @"\[.*?\]", new String('5', digits));
                // // Otherwise PDF font encode will throw exception because the glyphs of the embedded font will be missing
                
                // var replace = CreateReplacementAnchor(doc, align, textReplacement, text);
                
                // n.ParentNode.ReplaceChild(replace, n);
            }
            return newDoc;
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
                    ? CreateJavascriptResource(head.OwnerDocument, res.Content)
                    : CreateCSSResource(head.OwnerDocument, res.Content);

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
