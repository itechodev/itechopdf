namespace wkpdftoxcorelib
{
    public class HeaderFooterSource :  HeaderFooter
    {
        public HeaderFooterSource(PdfSource source)
        {
            Source = source;
        }

        public PdfSource Source { get; set; }
    }
}