namespace wkpdftoxcorelib.Settings
{
    public enum ColorMode
    {
        Color,
        Grayscale
    }

    public enum Orientation
    {
        Landscape,
        Portrait
    }

    public enum Unit
    {
        Inches,
        Millimeters,
        Centimeters
    }
    
    public enum SettingsType
    {
        Global,
        Object,
    }
 
    public class PrintSettings
    {
        /// <summary>
        /// Should we print the background. Default = true
        /// </summary>
        public bool? PrintBackground { get; set; }

        /// <summary>
        /// Should we load images. Default = true
        /// </summary>
        public bool? LoadImages { get; set; }

        /// <summary>
        /// Should we enable javascript. Default = true
        /// </summary>
        public bool? EnableJavascript { get; set; }

        /// <summary>
        /// Should we enable intelligent shrinkng to fit more content on one page. Has no effect for wkhtmltoimage. Default = false
        /// Should be used with caution. It mangles with your CSS units including mm, cm, inch.
        /// </summary>
        public bool? EnableIntelligentShrinking { get; set; } = false;

        /// <summary>
        /// The minimum font size allowed. Default = -1
        /// </summary>
        public int? MinimumFontSize { get; set; }

        /// <summary>
        /// Should the content be printed using the print media type instead of the screen media type. Default = false
        /// </summary>
        public bool? PrintMediaType { get; set; }

        /// <summary>
        /// What encoding should we guess content is using if they do not specify it properly. Default = ""
        /// </summary>
        public string DefaultEncoding { get; set; }

        public HeaderFooterSettings Header { get; set; } = new HeaderFooterSettings();
        public HeaderFooterSettings Footer { get; set; } = new HeaderFooterSettings();

        /// <summary>
        /// The URL or path of the web page to convert, if "-" input is read from stdin. Default = ""
        /// </summary>
        // public string Page { get; set; }

        /// <summary>
        /// Should external links in the HTML document be converted into external pdf links. Default = true
        /// </summary>
        public bool? UseExternalLinks { get; set; }

        /// <summary>
        /// Should internal links in the HTML document be converted into pdf references. Default = true
        /// </summary>
        public bool? UseLocalLinks { get; set; }

        /// <summary>
        /// Should we turn HTML forms into PDF forms. Default = false
        /// </summary>
        public bool? ProduceForms { get; set; }

        /// <summary>
        /// Should the sections from this document be included in the outline and table of content. Default = false
        /// </summary>
        public bool? IncludeInOutline { get; set; }

        /// <summary>
        /// Should we count the pages of this document, in the counter used for TOC, headers and footers. Default = false
        /// </summary>
        public bool? PagesCount { get; set; }
        
        // /// <summary>
        // /// Url or path to a user specified style sheet. Default = ""
        // /// </summary>
        // public string UserStyleSheet { get; set; }

        // /// <summary>
        // /// Should we enable NS plugins. Enabling this will have limited success. Default = false
        // /// </summary>
        // public bool? enablePlugins { get; set; }

        /// <summary>
        /// The orientation of the output document, must be either "Landscape" or "Portrait". Default = "portrait"
        /// </summary>
        public Orientation? Orientation { get; set; }

        /// <summary>
        /// Should the output be printed in color or gray scale, must be either "Color" or "Grayscale". Default = "color"
        /// </summary>
        public ColorMode? ColorMode { get; set; }

        /// <summary>
        /// Should we use loss less compression when creating the pdf file. Default = true
        /// </summary>
        public bool? UseCompression { get; set; }

        /// <summary>
        /// What dpi should we use when printing. Default = 96
        /// </summary>
        public int? DPI { get; set; }

        /// <summary>
        /// A number that is added to all page numbers when printing headers, footers and table of content. Default = 0
        /// </summary>
        public int? PageOffset { get; set; }

        /// <summary>
        /// How many copies should we print. Default = 1
        /// </summary>
        public int? Copies { get; set; }

        /// <summary>
        /// Should the copies be collated. Default = true
        /// </summary>
        public bool? Collate { get; set; }

        /// <summary>
        /// Should a outline (table of content in the sidebar) be generated and put into the PDF. Default = true
        /// </summary>
        public bool? Outline { get; set; }

        /// <summary>
        /// The maximal depth of the outline. Default = 4
        /// </summary>
        public int? OutlineDepth { get; set; }

        /// <summary>
        /// If not set to the empty string a XML representation of the outline is dumped to this file. Default = ""
        /// </summary>
        public string DumpOutline { get; set; }

        /// <summary>
        /// The path of the output file, if "-" output is sent to stdout, if empty the output is stored in a buffer. Default = ""
        /// </summary>
        // public string Out { get; set; }

        /// <summary>
        /// The title of the PDF document. Default = ""
        /// </summary>
        public string DocumentTitle { get; set; }

        /// <summary>
        /// The maximal DPI to use for images in the pdf document. Default = 600
        /// </summary>
        public int? ImageDPI { get; set; }

        /// <summary>
        /// The jpeg compression factor to use when producing the pdf document. Default = 94
        /// </summary>
        public int? ImageQuality { get; set; }

        /// <summary>
        /// Path of file used to load and store cookies. Default = ""
        /// </summary>
        public string CookieJar { get; set; }

        /// <summary>
        /// Size of output paper
        /// </summary>
        public PaperSize PaperSize { get; set; }

        /// <summary>
        /// The height of the output document
        /// </summary>
        private string PaperHeight
        {
            get {
                return PaperSize == null ? null : PaperSize.Height;
            }
        }
        
        /// <summary>
        /// The width of the output document
        /// </summary>
        private string PaperWidth
        {
            get
            {
                return PaperSize == null ? null : PaperSize.Width;
            }
        }

        public MarginSettings Margins { get; set; } = new MarginSettings();

        private string MarginTop
        {
            get
            {
                return Margins.GetMarginValue(Margins.Top);
            }
        }

        private string MarginBottom
        {
            get
            {
                return Margins.GetMarginValue(Margins.Bottom);
            }
        }

        private string MarginLeft
        {
            get
            {
                return Margins.GetMarginValue(Margins.Left);
            }
        }

        private string MarginRight
        {
            get
            {
                return Margins.GetMarginValue(Margins.Right);
            }
        }

    }
    
}
