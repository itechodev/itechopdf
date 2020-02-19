using System;
using System.Runtime.InteropServices;

namespace ItechoPdf.Core
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void VoidCallback(IntPtr converter);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StringCallback(IntPtr converter, [MarshalAs(UnmanagedType.LPStr)] string str);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void IntCallback(IntPtr converter, int integer);

    /// <summary>
    /// This enum "extends" UnmanagedType enum from System.Runtime.InteropServices v4.1.0 which doesn't have LPUTF8Str (enum value is 48) enumartion defined
    /// </summary>
    public enum CustomUnmanagedType
    {
        LPUTF8Str = 48
    }

    unsafe static class WkHtmlToXBinding
    {
        const string LIBRARY = "libwkhtmltox";
        const CharSet CHARSET = CharSet.Unicode;
        const CallingConvention CALLING = CallingConvention.Cdecl;


        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_extended_qt();

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern IntPtr wkhtmltopdf_version();

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_init(int useGraphics);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_deinit();

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern IntPtr wkhtmltopdf_create_global_settings();

        [DllImport(LIBRARY, CharSet = CHARSET)]
        public static extern int wkhtmltopdf_set_global_setting(IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
            string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
            string value);


        [DllImport(LIBRARY, CharSet = CHARSET)]
        public static unsafe extern int wkhtmltopdf_get_global_setting(IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
            string name,
            byte* value, int valueSize);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_destroy_global_settings(IntPtr settings);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern IntPtr wkhtmltopdf_create_object_settings();

        [DllImport(LIBRARY, CharSet = CHARSET)]
        public static extern int wkhtmltopdf_set_object_setting(IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
            string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
            string value);

        [DllImport(LIBRARY, CharSet = CHARSET)]
        public static unsafe extern int wkhtmltopdf_get_object_setting(IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)]
            string name,
            byte* value, int valueSize);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_destroy_object_settings(IntPtr settings);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern void wkhtmltopdf_add_object(IntPtr converter,
            IntPtr objectSettings,
            byte[] data);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern void wkhtmltopdf_add_object(IntPtr converter,
            IntPtr objectSettings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string data);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern bool wkhtmltopdf_convert(IntPtr converter);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern void wkhtmltopdf_destroy_converter(IntPtr converter);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_set_phase_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_set_progress_changed_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_set_finished_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_set_warning_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_set_error_callback(IntPtr converter, [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_phase_count(IntPtr converter);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_current_phase(IntPtr converter);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern IntPtr wkhtmltopdf_progress_string(IntPtr converter);

        [DllImport(LIBRARY, CharSet = CHARSET, CallingConvention = CALLING)]
        public static extern int wkhtmltopdf_http_error_code(IntPtr converter);

    }
}
