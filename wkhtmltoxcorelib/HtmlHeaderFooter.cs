namespace wkpdftoxcorelib
{
    public class HtmlHeaderFooter :  HeaderFooter
    {
        public HtmlHeaderFooter(PdfSource source)
        {
            Source = source;
        }

        public PdfSource Source { get; set; }
    }
}