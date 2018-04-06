using System.Web.Mvc;

namespace Web.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}