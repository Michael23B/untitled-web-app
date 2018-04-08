using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;
using Web.Models.Data;
using Web.Models.ViewModels.Shop;

namespace Web.Areas.Admin.Controllers
{
    public class WebController : Controller
    {
        public ActionResult Categories()
        {
            List<CategoryViewModel> categoryList;

            using (Db db = new Db())
            {
                categoryList = db.Categories.ToArray()
                                            .OrderBy(x => x.Sorting)
                                            .Select(x => new CategoryViewModel(x))
                                            .ToList();
            }

            return View(categoryList);
        }
    }
}