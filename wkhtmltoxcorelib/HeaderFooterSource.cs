namespace wkpdftoxcorelib
{
    public class SourceHeaderFooter :  HeaderFooter
    {
        public SourceHeaderFooter(PdfSource source)
        {
            Source = source;
        }

        public PdfSource Source { get; set; }
    }
}