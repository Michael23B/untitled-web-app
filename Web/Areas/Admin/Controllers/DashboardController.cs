using System.Web.Mvc;

namespace Web.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}