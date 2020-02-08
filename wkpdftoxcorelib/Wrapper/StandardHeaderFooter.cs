namespace wkpdftoxcorelib.Wrapper
{
    public class StandardHeaderFooter : HeaderFooter
    {
        public StandardHeaderFooter()
        {

        }

        public StandardHeaderFooter(string left, string center, string right)
        {
            Left = left;
            Center = center;
            Right = right;
        }

        /// <summary>
        /// The font size to use for the footer. Default = 12
        /// </summary>
        public int? FontSize { get; set; }

        /// <summary>
        /// The name of the font to use for the footer. Default = "Ariel"
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// The string to print in the left part of the footer, note that some sequences are replaced in this string, see the wkhtmltopdf manual. Default = ""
        /// </summary>
        public string Left { get; set; }

        /// <summary>
        /// The text to print in the right part of the footer, note that some sequences are replaced in this string, see the wkhtmltopdf manual. Default = ""
        /// </summary>
        public string Center { get; set; }

        /// <summary>
        /// The text to print in the right part of the footer, note that some sequences are replaced in this string, see the wkhtmltopdf manual. Default = ""
        /// </summary>
        public string Right { get; set; }
   
    }
    
}
