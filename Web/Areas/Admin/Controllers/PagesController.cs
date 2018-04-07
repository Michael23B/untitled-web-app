using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web.Models.Data;
using Web.Models.ViewModels.Pages;

namespace Web.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        public ActionResult Index()
        {
            List<PageViewModel> pagesList = new List<PageViewModel>();

            //using (resource) to clean up after we are done with it
            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageViewModel(x)).ToList();
            }

            return View(pagesList);
        }

        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //If we have no slug, create one from the title otherwise clean up the provided slug
                string slug = string.IsNullOrWhiteSpace(model.Slug) ? 
                    model.Title.Replace(" ", ".").ToLower() 
                    : 
                    model.Slug.Replace(" ", ".").ToLower();

                //Check if database containts a page with this title or slug
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "Title or slug already exists!");
                    return View(model);
                }

                //Create the new page
                PageDTO dto = new PageDTO
                {
                    Title = model.Title,
                    Slug = slug,
                    Body = model.Body,
                    HasSidebar = model.HasSidebar,
                    Sorting = int.MaxValue  //when you add a page, it becomes the last page
                };

                //Save changes
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["message"] = $"Page {model.Title} added successfully!";

            return RedirectToAction("Index");
        }
    }
}