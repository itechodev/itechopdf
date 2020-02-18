using System;
using System.Threading.Tasks;

namespace wkpdftoxcorelib.Web
{
    public class PdfViewRenderer
    {
        private IViewRenderService _viewRenderer;

        public PdfViewRenderer(IViewRenderService viewRenderer)
        {
            _viewRenderer = viewRenderer;
        }

        public async Task<byte[]> ToBytes(string viewName, object model, string baseUrl, Action<PrintSettings> print, Action<LoadSettings> load = null)
        {
            string html = await _viewRenderer.RenderToStringAsync(viewName, model);
            var renderer = new PdfRenderer();
            var doc = new PdfDocument(PdfSource.FromHtml(html, baseUrl));
            doc.Configure(print, load);
            renderer.Add(doc);
            return renderer.RenderToBytes();
        }
    }

}