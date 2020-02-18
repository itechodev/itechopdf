using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using testWeb.Models;
using wkpdftoxcorelib.Web;

namespace testWeb.Controllers
{
    public class HomeController : Controller
    {
        
        public HomeController()
        {
        }

        public Task<IActionResult> Index()
        {
            return this.ViewPdf("~/Views/Home/index.cshtml", null, null, print => {
                print.DPI = 300;
            });
        }

    }
}
