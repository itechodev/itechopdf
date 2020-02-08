using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace wkpdftoxcorelib.Core
{

    // Elegant wrapper around C bindings to WkHtmlToPdf
    internal static class WkHtmlToPdf
    {
        public static string GetVersion()
        {
            return Marshal.PtrToStringAnsi(WkHtmlToXBinding.wkhtmltopdf_version());
        }

        public static bool ExtendedQt()
        {
            return WkHtmlToXBinding.wkhtmltopdf_extended_qt() == 1;
        }

        public static byte[] HtmlToPdf(byte[] bytes, WkHtmlToPdfSettings settings)
        {
            if (WkHtmlToXBinding.wkhtmltopdf_init(0) != 1)
            {
                throw new Exception("Could not initialize WkHtmlToPDF library");
            }
            var globalSettings = WkHtmlToXBinding.wkhtmltopdf_create_global_settings();
            var objectSettings = WkHtmlToXBinding.wkhtmltopdf_create_object_settings();

            // Set global and object settings
            FillSettings(globalSettings, objectSettings, settings);

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

            var ret = GetConversionResult(converter);

            // Destroy all
            WkHtmlToXBinding.wkhtmltopdf_destroy_global_settings(globalSettings);
            WkHtmlToXBinding.wkhtmltopdf_destroy_object_settings(objectSettings);
            WkHtmlToXBinding.wkhtmltopdf_destroy_converter(converter);

            return ret;
        }

        private static void FillSettings(IntPtr globalSettings, IntPtr objectSettings, WkHtmlToPdfSettings settings)
        {
            // From https://wkhtmltopdf.org/libwkhtmltox/pagesettings.html#pagePdfGlobal
            // Pdf global settings
            GlobalSetting(globalSettings, "size.pageSize", settings.PaperSize);
            GlobalSetting(globalSettings, "size.width", settings.PaperWidth);
            GlobalSetting(globalSettings, "size.height", settings.PaperHeight);
            GlobalSetting(globalSettings, "orientation", settings.Orientation);
            GlobalSetting(globalSettings, "colorMode", settings.ColorMode);
            GlobalSetting(globalSettings, "dpi", settings.DPI);
            GlobalSetting(globalSettings, "pageOffset", settings.PageOffset);
            GlobalSetting(globalSettings, "copies", settings.Copies);
            GlobalSetting(globalSettings, "colate", settings.Collate);
            GlobalSetting(globalSettings, "outline", settings.Outline);
            GlobalSetting(globalSettings, "outlineDepth", settings.OutlineDepth);
            GlobalSetting(globalSettings, "dumpOutline", settings.DumpOutline);
            GlobalSetting(globalSettings, "documentTitle", settings.DocumentTitle);
            GlobalSetting(globalSettings, "useCompression", settings.UseCompression);
            GlobalSetting(globalSettings, "ImageQuality", settings.ImageQuality);
            GlobalSetting(globalSettings, "load.cookieJar", settings.CookieJar);
            GlobalSetting(globalSettings, "ImageDPI", settings.ImageDPI);

            // Toc settings
            // ObjectSetting(objectSettings, "toc.useDottedLines", true);
            // ObjectSetting(objectSettings, "toc.captionText", true);
            // ObjectSetting(objectSettings, "toc.forwardLinks", true);
            // ObjectSetting(objectSettings, "toc.backLinks", true);
            // ObjectSetting(objectSettings, "toc.indentation", true);
            // ObjectSetting(objectSettings, "toc.fontScale", true);
            // ObjectSetting(objectSettings, "tocXsl", true);

            // ObjectSetting(objectSettings, "page", true);
            ObjectSetting(objectSettings, "useExternalLinks", settings.UseExternalLinks);
            // ObjectSetting(objectSettings, "replacements", true);
            ObjectSetting(objectSettings, "produceForms", settings.ProduceForms);
            ObjectSetting(objectSettings, "includeInOutline", settings.IncludeInOutline);
            ObjectSetting(objectSettings, "pagesCount", settings.PagesCount);

            // Load settings
            ObjectSetting(objectSettings, "load.username", settings.Username);
            ObjectSetting(objectSettings, "load.password", settings.Password);
            ObjectSetting(objectSettings, "load.jsdelay", settings.JSDelay);
            ObjectSetting(objectSettings, "load.blockLocalFileAccess", settings.BlockLocalFileAccess);
            ObjectSetting(objectSettings, "load.stopSlowScript", settings.StopSlowScript);
            ObjectSetting(objectSettings, "load.debugJavascript", settings.DebugJavascript);
            ObjectSetting(objectSettings, "load.loadErrorHandling", settings.LoadErrorHandling?.ToString());
            ObjectSetting(objectSettings, "load.proxy", settings.Proxy);

            // web settings
            ObjectSetting(objectSettings, "web.background", settings.PrintBackground);
            ObjectSetting(objectSettings, "web.loadImages", settings.LoadImages);
            ObjectSetting(objectSettings, "web.enableJavascript", settings.EnableJavascript);
            ObjectSetting(objectSettings, "web.enableIntelligentShrinking", settings.EnableIntelligentShrinking);
            ObjectSetting(objectSettings, "web.minimumFontSize", settings.MinimumFontSize);
            ObjectSetting(objectSettings, "web.printMediaType", settings.PrintMediaType);
            ObjectSetting(objectSettings, "web.defaultEncoding", settings.DefaultEncoding);
            // ObjectSetting(objectSettings, "web.userStyleSheet", false);
            // ObjectSetting(objectSettings, "web.enablePlugins", false);

            // headers and footers
            HeaderFooter(objectSettings, "header", settings.Header);
            HeaderFooter(objectSettings, "footer", settings.Footer);

            GlobalSetting(globalSettings, "margin.top", settings.MarginTop);
            GlobalSetting(globalSettings, "margin.bottom", settings.MarginBottom);
            GlobalSetting(globalSettings, "margin.left", settings.MarginLeft);
            GlobalSetting(globalSettings, "margin.right", settings.MarginRight);
        }

        private static void HeaderFooter(IntPtr objectSettings, string prefix, HeaderFooterSettings settings)
        {
            if (settings == null)
            {
                return;
            }
            ObjectSetting(objectSettings, prefix + ".line", settings.Line);
            ObjectSetting(objectSettings, prefix + ".spacing", settings.Spacing);
            ObjectSetting(objectSettings, prefix + ".fontSize", settings.FontSize);
            ObjectSetting(objectSettings, prefix + ".fontName", settings.FontName);
            ObjectSetting(objectSettings, prefix + ".left", settings.Left);
            ObjectSetting(objectSettings, prefix + ".center", settings.Center);
            ObjectSetting(objectSettings, prefix + ".htmlUrl", settings.Url);            
        }

        private static void GlobalSetting(IntPtr settings, string name, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }
            WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value);
        }

        private static void GlobalSetting(IntPtr settings, string name, int? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value.ToString());
            }
        }

        private static void GlobalSetting(IntPtr settings, string name, bool? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value.Value ? "true" : "false");
            }
        }

        private static void ObjectSetting(IntPtr settings, string name, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }
            WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value);
        }

        private static void ObjectSetting(IntPtr settings, string name, int? value)
        {

            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value.ToString());
            }
        }

        private static void ObjectSetting(IntPtr settings, string name, bool? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value.Value ? "true" : "false");
            }
        }

        private static void ObjectSetting(IntPtr settings, string name, double? value)
        {
            if (value.HasValue)
            {
                WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value.Value.ToString("0.##", CultureInfo.InvariantCulture));
            }
        }

        private static byte[] GetConversionResult(IntPtr converter)
        {
            IntPtr resultPointer;

            int length = WkHtmlToXBinding.wkhtmltopdf_get_output(converter, out resultPointer);
            var result = new byte[length];
            Marshal.Copy(resultPointer, result, 0, length);

            return result;
        }

        private static int SetGlobalSetting(IntPtr settings, string name, string value)
        {
            return WkHtmlToXBinding.wkhtmltopdf_set_global_setting(settings, name, value);
        }

        private static int SetObjectSetting(IntPtr settings, string name, string value)
        {
            return WkHtmlToXBinding.wkhtmltopdf_set_object_setting(settings, name, value);
        }

        private static string GetString(byte[] buffer)
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
