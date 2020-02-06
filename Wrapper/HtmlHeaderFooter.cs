namespace wkpdftoxcorelib.Wrapper
{
    public class HtmlHeaderFooter : HeaderFooter
    {
        public HtmlHeaderFooter(string html)
        {
            Html = html;
        }

        public string Html { get; }
    }
    
}
