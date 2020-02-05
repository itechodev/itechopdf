using System;
using System.IO;
using System.Runtime.InteropServices;

namespace wkpdftoxcorelib
{
    // Elegant wrapper around C bindings to WkHtmlToPdf
    public class WkHtmlToPdf
    {
        
        public string GetVersion()
        {
            return Marshal.PtrToStringAnsi(WkHtmlToXBinding.wkhtmltopdf_version());
        }

        public bool ExtendedQt()
        {
            return WkHtmlToXBinding.wkhtmltopdf_extended_qt() == 1;
        }

        public void Convert()
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

            // Destroy all
            WkHtmlToXBinding.wkhtmltopdf_destroy_global_settings(globalSettings);
            WkHtmlToXBinding.wkhtmltopdf_destroy_object_settings(objectSettings);
            WkHtmlToXBinding.wkhtmltopdf_destroy_converter(converter);
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
