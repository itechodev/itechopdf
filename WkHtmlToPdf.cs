using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        public void HtmlToPdf(string html)
        {
            // 1. A wkhtmltopdf_global_settings object is creating by calling wkhtmltopdf_create_global_settings.
            //    Non web page specific Setting for the conversion are set by multiple calls to wkhtmltopdf_set_global_setting.
            // 2. A wkhtmltopdf_converter object is created by calling wkhtmltopdf_create_converter, which consumes the global_settings instance.
            //    A number of object (web pages) are added to the conversion process, this is done by
            // 3. Creating a wkhtmltopdf_object_settings instance by calling wkhtmltopdf_create_object_settings.
            //    Setting web page specific Setting by multiple calls to wkhtmltopdf_set_object_setting.
            // 4. Adding the object to the conversion process by calling wkhtmltopdf_add_object
            // 5. A number of callback function are added to the converter object.
            // 6. The conversion is performed by calling wkhtmltopdf_convert.
            // 7. The converter object is destroyed by calling wkhtmltopdf_destroy_converter.

            if (WkHtmlToXBinding.wkhtmltopdf_init(0) != 1)
            {
                throw new Exception("Could not initialize WkHtmlToPDF library");
            }
            var globalSettings =  WkHtmlToXBinding.wkhtmltopdf_create_global_settings();
            var objectSettings = WkHtmlToXBinding.wkhtmltopdf_create_object_settings();

            FillSettings(globalSettings, objectSettings);

            // Set global and object settings
            // SetGlobalSetting(globalSettings, "", "");
            // SetObjectSetting(objectSettings, "", "");

            var converter = WkHtmlToXBinding.wkhtmltopdf_create_converter(globalSettings);

            WkHtmlToXBinding.wkhtmltopdf_add_object(converter, objectSettings, "<b>All</b> is working...");
            // WkHtmlToXBinding.wkhtmltopdf_add_object(converter, objectSettings, new byte[] { .. });

            if (!WkHtmlToXBinding.wkhtmltopdf_convert(converter))
            {
                // Convert failed
            }

            byte[] ret = GetConversionResult(converter);
            File.WriteAllBytes("output.pdf", ret);

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
        }

        private string CreateTempororyFile(string content)
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, content);
            // Keep reference to file can later delete it
            _tempFiles.Add(path);
            return path;
        }

        private void FillSettings(IntPtr globalSettings, IntPtr objectSettings)
        {
            // From https://wkhtmltopdf.org/libwkhtmltox/pagesettings.html#pagePdfGlobal
            // Pdf global settings
            GlobalSetting(globalSettings, "size.width", PrintSettings.PaperSize.Width);
            GlobalSetting(globalSettings, "size.height", PrintSettings.PaperSize.Height);
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
            GlobalSetting(globalSettings, "margin.top", PrintSettings.Margins.GetMarginValue(PrintSettings.Margins.Top));
            GlobalSetting(globalSettings, "margin.bottom", PrintSettings.Margins.GetMarginValue(PrintSettings.Margins.Bottom));
            GlobalSetting(globalSettings, "margin.left", PrintSettings.Margins.GetMarginValue(PrintSettings.Margins.Left));
            GlobalSetting(globalSettings, "margin.right", PrintSettings.Margins.GetMarginValue(PrintSettings.Margins.Right));
            GlobalSetting(globalSettings, "ImageDPI", PrintSettings.ImageDPI);
            GlobalSetting(globalSettings, "ImageQuality", PrintSettings.ImageQuality);
            GlobalSetting(globalSettings, "load.cookieJar", PrintSettings.CookieJar);

            // Toc settings
            // ObjectSetting(objectSettings, "toc.useDottedLines", true);
            // ObjectSetting(objectSettings, "toc.captionText", true);
            // ObjectSetting(objectSettings, "toc.forwardLinks", true);
            // ObjectSetting(objectSettings, "toc.backLinks", true);
            // ObjectSetting(objectSettings, "toc.indentation", true);
            // ObjectSetting(objectSettings, "toc.fontScale", true);
            // ObjectSetting(objectSettings, "tocXsl", true);

            ObjectSetting(objectSettings, "page", true);
            ObjectSetting(objectSettings, "header", true);
            ObjectSetting(objectSettings, "footer", true);
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
        }

        private void HeaderFooter(IntPtr objectSettings, string prefix, HeaderFooterSettings settings)
        {
            ObjectSetting(objectSettings, prefix + ".fontSize", settings.FontSize);
            ObjectSetting(objectSettings, prefix + ".fontName", settings.FontName);
            ObjectSetting(objectSettings, prefix + ".left", settings.Left);
            ObjectSetting(objectSettings, prefix + ".center", settings.Center);
            ObjectSetting(objectSettings, prefix + ".right", settings.Right);
            ObjectSetting(objectSettings, prefix + ".line", settings.Line);
            ObjectSetting(objectSettings, prefix + ".spacing", settings.Spacing);

            if (!String.IsNullOrEmpty(settings.HtmlContent))
            {
                var file = CreateTempororyFile(settings.HtmlContent);
                ObjectSetting(objectSettings, "header.htmlUrl", file);
            }
        }
            

        private void GlobalSetting(IntPtr settings, string name, string value)
        {

        }

        private void GlobalSetting(IntPtr settings, string name, int? value)
        {

        }

        private void GlobalSetting(IntPtr settings, string name, bool? value)
        {

        }

        private void ObjectSetting(IntPtr settings, string name, string value)
        {

        }

        private void ObjectSetting(IntPtr settings, string name, int? value)
        {

        }

        private void ObjectSetting(IntPtr settings, string name, bool? value)
        {

        }
        
        private void ObjectSetting(IntPtr settings, string name, double? value)
        {

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
    }
}
