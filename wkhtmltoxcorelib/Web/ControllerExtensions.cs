using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace wkpdftoxcorelib.Web
{
    public static class ControllerExtensions
    {
        public static async Task<IActionResult> HtmlViewPdf(this Controller self, string viewName, object model, string baseUrl, Action<PrintSettings> print, Action<LoadSettings> load = null)
        {   
            var renderer = self.HttpContext.RequestServices.GetService<PdfViewRenderer>();
            if (renderer == null)
            {
                throw new Exception("PdfViewRenderer could be resolved. Have you register PdfViewRenderer on startup?");
            }
            var bytes = await renderer.FromViewHtml(viewName, model, baseUrl, print, load);
            return self.File(bytes, "application/pdf");
        }

        public static async Task<IActionResult> XmlViewToPdf(this Controller self, string viewName, object model)
        {   
            var renderer = self.HttpContext.RequestServices.GetService<PdfViewRenderer>();
            if (renderer == null)
            {
                throw new Exception("PdfViewRenderer could be resolved. Have you register PdfViewRenderer on startup?");
            }
            var bytes = await renderer.FromViewXml(viewName, model);
            return self.File(bytes, "application/pdf");
        }
    }

}