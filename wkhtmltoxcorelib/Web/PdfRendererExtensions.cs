using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace wkpdftoxcorelib.Web
{
    public static class PdfRendererExtensions
    {
        public static void AddViewPdfRenderer(this IServiceCollection services)
        {
            // Fix no data is available for encoding 1252
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddScoped<PdfViewRenderer>();
        }   
    }
}