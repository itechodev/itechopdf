using System.Threading.Tasks;

namespace wkpdftoxcorelib.Web
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}