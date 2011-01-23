using System.Web.Mvc;

namespace Colombo.Samples.MvcClient.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to Colombo MVC sample!";

            return View();
        }
    }
}
