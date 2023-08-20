
using Nop.Core.Infrastructure;
using Nop.Services.SAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EcommStagingHelper.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

                        var _SAPService = EngineContext.Current.Resolve<ISAPService>();
              
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}