using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GradProject.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // تنبيهات للتجربة فقط
            TempData["Success"] = "✅ Success test message.";
            TempData["Error"] = "❌ Error test message.";
            TempData["Warning"] = "⚠️ Warning test message.";
            TempData["Info"] = "ℹ️ Info test message.";

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

        //test commit
    }
}