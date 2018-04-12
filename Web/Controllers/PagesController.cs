using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Web.Models.Data;
using Web.Models.ViewModels.Pages;
using Web.Models.ViewModels.Store;

namespace Web.Controllers
{
    public class PagesController : Controller
    {
        public ActionResult Index(string page = "home")
        {
            PageViewModel model;

            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.FirstOrDefault(x => x.Slug == page);
                if (dto == null) return RedirectToAction("Index", new {page = ""});

                model = new PageViewModel(dto);
            }

            ViewBag.Title = model.Title;
            ViewBag.Sidebar = model.HasSidebar;
            TempData["currentPage"] = model.Title;

            return View(model);
        }

        public PartialViewResult PagesMenuPartial()
        {
            List<PageViewModel> pages;

            using (Db db = new Db())
            {
                pages = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home")
                    .Select(x => new PageViewModel(x)).ToList();
            }

            return PartialView(pages);
        }

        public PartialViewResult SidebarPartial()
        {
            SidebarViewModel sidebar;

            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                sidebar = new SidebarViewModel(dto);
            }

            return PartialView(sidebar);
        }
    }
}