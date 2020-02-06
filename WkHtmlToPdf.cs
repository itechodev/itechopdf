using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HtmlAgilityPack;
using wkpdftoxcorelib.Settings;

namespace wkpdftoxcorelib
{

    // Elegant wrapper around C bindings to WkHtmlToPdf
    public class WkHtmlToPdf
    {
        public PrintSettings PrintSettings = new PrintSettings();
        public LoadSettings LoadSettings = new LoadSettings();

        private List<string> _tempFiles = new List<string>();

        public string GetVersion()
        {
            bool extended = WkHtmlToXBinding.wkhtmltopdf_extended_qt() == 1;
            return Marshal.PtrToStringAnsi(WkHtmlToXBinding.wkhtmltopdf_version()) + (extended ? " (Extended QT)" : "");
        }

        public PdfDocument HtmlFileToPdf(string filename)
        {
            return HtmlToPdf(File.ReadAllText(filename));
        }

        public PdfDocument HtmlToPdf(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            FormatHtml(htmlDoc, Environment.CurrentDirectory);    
            using (var sw = new StringWriter())
            {
                htmlDoc.Save(sw);
                return HtmlToPdf(System.Text.Encoding.UTF8.GetBytes(sw.ToString()));
            }
        }

        public PdfDocument HtmlToPdf(byte[] bytes)
        {
            if (WkHtmlToXBinding.wkhtmltopdf_init(0) != 1)
            {
                throw new Exception("Could not initialize WkHtmlToPDF library");
            }
            var globalSettings = WkHtmlToXBinding.wkhtmltopdf_create_global_settings();
            var objectSettings = WkHtmlToXBinding.wkhtmltopdf_create_object_settings();

            // Set global and object settings
            FillSettings(globalSettings, objectSettings);

            var converter = WkHtmlToXBinding.wkhtmltopdf_create_converter(globalSettings);

            WkHtmlToXBinding.wkhtmltopdf_set_error_callback(converter, (IntPtr cc, string str) => {
                Console.WriteLine("Error: " + str);
            });

            WkHtmlToXBinding.wkhtmltopdf_add_object(converter, objectSettings, bytes);
            // WkHtmlToXBinding.wkhtmltopdf_add_object(converter, objectSettings, new byte[] { .. });

            if (!WkHtmlToXBinding.wkhtmltopdf_convert(converter))
            {
                return null;
            }

            byte[] ret = GetConversionResult(converter);

            // Clear all temp files
            foreach (string file in _tempFiles)
            {
                File.Delete(file);
            }
            _tempFiles.Clear();

            // Destroy all
            WkHtmlToXBinding.wkhtmltopdf_destroy_global_settings(globalSettings);
            WkHtmlToXBinding.wkhtmltopdf_destroy_object_settings(objectSettings);
            WkHtmlToXBinding.wkhtmltopdf_destroy_converter(converter);

            return new PdfDocument(ret);
        }

        private string CreateTempFile()
        {
            // For some reason it should end in html
            var path = Path.GetTempFileName() + ".html";
            // Keep reference to file can later delete it
            _tempFiles.Add(path);
            return path;
        }

        private void FillSettings(IntPtr globalSettings, IntPtr objectSettings)
        {
            // From https://wkhtmltopdf.org/libwkhtmltox/pagesettings.html#pagePdfGlobal
            // Pdf global settings
            GlobalSetting(globalSettings, "size.width", PrintSettings.PaperSize?.Width);
            GlobalSetting(globalSettings, "size.height", PrintSettings.PaperSize?.Height);
            GlobalSetting(globalSettings, "orientation", PrintSettings.Orientation?.ToString());
            GlobalSetting(globalSettings, "colorMode", PrintSettings.ColorMode?.ToString());
            GlobalSetting(globalSettings, "dpi", PrintSettings.DPI);
            GlobalSetting(globalSettings, "pageOffset", PrintSettings.PageOffset);
            GlobalSetting(globalSettings, "copies", PrintSettings.Copies);
            GlobalSetting(globalSettings, "colate", PrintSettings.Collate);
            GlobalSetting(globalSettings, "outline", PrintSettings.Outline);
            GlobalSetting(globalSettings, "outlineDepth", PrintSettings.OutlineDepth);
            GlobalSetting(globalSettings, "dumpOutline", PrintSettings.DumpOutline);
            GlobalSetting(globalSettings, "documentTitle", PrintSettings.DocumentTitle);
            GlobalSetting(globalSettings, "useCompression", PrintSettings.UseCompression);
            GlobalSetting(globalSettings, "ImageQuality", PrintSettings.ImageQuality);
            GlobalSetting(globalSettings, "load.cookieJar", PrintSettings.CookieJar);
            GlobalSetting(globalSettings, "ImageDPI", PrintSettings.ImageDPI);

            // Toc settings
            // ObjectSetting(objectSettings, "toc.useDottedLines", true);
            // ObjectSetting(objectSettings, "toc.captionText", true);
            // ObjectSetting(objectSettings, "toc.forwardLinks", true);
            // ObjectSetting(objectSettings, "toc.backLinks", true);
            // ObjectSetting(objectSettings, "toc.indentation", true);
            // ObjectSetting(objectSettings, "toc.fontScale", true);
            // ObjectSetting(objectSettings, "tocXsl", true);

            // ObjectSetting(objectSettings, "page", true);
            ObjectSetting(objectSettings, "useExternalLinks", PrintSettings.UseExternalLinks);
            // ObjectSetting(objectSettings, "replacements", true);
            ObjectSetting(objectSettings, "produceForms", PrintSettings.ProduceForms);
            ObjectSetting(objectSettings, "includeInOutline", PrintSettings.IncludeInOutline);
            ObjectSetting(objectSettings, "pagesCount", PrintSettings.PagesCount);

            // Load settings
            ObjectSetting(objectSettings, "load.username", LoadSettings.Username);
            ObjectSetting(objectSettings, "load.password", LoadSettings.Password);
            ObjectSetting(objectSettings, "load.jsdelay", LoadSettings.JSDelay);
            ObjectSetting(objectSettings, "load.blockLocalFileAccess", LoadSettings.BlockLocalFileAccess);
            ObjectSetting(objectSettings, "load.stopSlowScript", LoadSettings.StopSlowScript);
            ObjectSetting(objectSettings, "load.debugJavascript", LoadSettings.DebugJavascript);
            ObjectSetting(objectSettings, "load.loadErrorHandling", LoadSettings.LoadErrorHandling?.ToString());
            ObjectSetting(objectSettings, "load.proxy", LoadSettings.Proxy);

            // web settings
            ObjectSetting(objectSettings, "web.background", PrintSettings.PrintBackground);
            ObjectSetting(objectSettings, "web.loadImages", PrintSettings.LoadImages);
            ObjectSetting(objectSettings, "web.enableJavascript", PrintSettings.EnableJavascript);
            ObjectSetting(objectSettings, "web.enableIntelligentShrinking", PrintSettings.EnableIntelligentShrinking);
            ObjectSetting(objectSettings, "web.minimumFontSize", PrintSettings.MinimumFontSize);
            ObjectSetting(objectSettings, "web.printMediaType", PrintSettings.PrintMediaType);
            ObjectSetting(objectSettings, "web.defaultEncoding", PrintSettings.DefaultEncoding);
            // ObjectSetting(objectSettings, "web.userStyleSheet", false);
            // ObjectSetting(objectSettings, "web.enablePlugins", false);

            // headers and footers
            HeaderFooter(objectSettings, "header", PrintSettings.Header);
            HeaderFooter(objectSettings, "footer", PrintSettings.Footer);

            // 25mm header + 10mm spacing + 1mm margin top
            // Set margins. Header and footers may affect marings
            if (PrintSettings.Margins.Top.HasValue)
            {
                if (PrintSettings.Header?.Height == null)
                {
                    throw new Exception("Header height should be explicit when margin top is explicit.");
                }
                double value = PrintSettings.Margins.Top.Value + (PrintSettings.Header.Spacing ?? 0) + PrintSettings.Header.Height.Value;
                GlobalSetting(globalSettings, "margin.top", PrintSettings.Margins.GetMarginValue(value));
            }

            if (PrintSettings.Margins.Bottom.HasValue)
            {
                if (PrintSettings.Footer?.Height == null)
                {
                    throw new Exception("Footer height should be explicit when margin bottom is explicit.");
                }
                double value = PrintSettings.Margins.Bottom.Value + (PrintSettings.Footer.Spacing ?? 0) + PrintSettings.Footer.Height.Value;
                GlobalSetting(globalSettings, "margin.bottom", PrintSettings.Margins.GetMarginValue(value));
            }
            
            GlobalSetting(globalSettings, "margin.left", PrintSettings.Margins.GetMarginValue(PrintSettings.Margins.Left));
            GlobalSetting(globalSettings, "margin.right", PrintSettings.Margins.GetMarginValue(PrintSettings.Margins.Right));
            
        }

        private void HeaderFooter(IntPtr objectSettings, string prefix, HeaderFooter settings)
        {
            if (settings == null)
            {
                return;
            }
            ObjectSetting(objectSettings, prefix + ".line", settings.Line);
            ObjectSetting(objectSettings, prefix + ".spacing", settings.Spacing);

            if (settings is StandardHeaderFooter std)
            {
                ObjectSetting(objectSettings, prefix + ".fontSize", std.FontSize);
                ObjectSetting(objectSettings, prefix + ".fontName", std.FontName);
                ObjectSetting(objectSettings, prefix + ".left", std.Left);
                ObjectSetting(objectSettings, prefix + ".center", std.Center);
                ObjectSetting(objectSettings, prefix + ".right", std.Right);
                return;
            }

            var htmlDoc = new HtmlDocument();

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
            ObjectSetting(objectSettings, prefix + ".htmlUrl", path);
        }


        private void GlobalSetting(IntPtr settings, string name, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }
            WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value);
        }

        private void GlobalSetting(IntPtr settings, string name, int? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value.ToString());
            }
        }

        private void GlobalSetting(IntPtr settings, string name, bool? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value.Value ? "true" : "false");
            }
        }

        private void ObjectSetting(IntPtr settings, string name, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }
            WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value);
        }

        private void ObjectSetting(IntPtr settings, string name, int? value)
        {

            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value.ToString());
            }
        }

        private void ObjectSetting(IntPtr settings, string name, bool? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value.Value ? "true" : "false");
            }
        }

        private void ObjectSetting(IntPtr settings, string name, double? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value.Value.ToString("0.##", CultureInfo.InvariantCulture));
            }
        }

        private byte[] GetConversionResult(IntPtr converter)
        {
            IntPtr resultPointer;

            int length = WkHtmlToXBinding.wkhtmltopdf_get_output(converter, out resultPointer);
            var result = new byte[length];
            Marshal.Copy(resultPointer, result, 0, length);

            return result;
        }

        private int SetGlobalSetting(IntPtr settings, string name, string value)
        {
            return WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value);
        }

        private int SetObjectSetting(IntPtr settings, string name, string value)
        {
            return WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value);
        }

        private string GetString(byte[] buffer)
        {
            var walk = Array.FindIndex(buffer, a => a == 0);
            if (walk == -1)
            {
                walk = buffer.Length;
            }
            return System.Text.Encoding.UTF8.GetString(buffer, 0, walk);
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
            // Check for absolute etc.
            return Path.Join(baseUrl, url);
        }
    }
}
