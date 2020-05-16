using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace testWeb.Controllers
{
    public class HomeController : Controller
    {
        
        public HomeController()
        {
        }

        public Task<IActionResult> Index()
        {   
            return this.XmlViewToPdf("~/Views/Home/index.cshtml", null);
        }
    }
}
