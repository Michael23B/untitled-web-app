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
            if (!ModelState.IsValid) return View(model);

            using (Db db = new Db())
            {
                //If we have no slug, create one from the title otherwise clean up the provided slug
                string slug = string.IsNullOrWhiteSpace(model.Slug) ? 
                    model.Title.Replace(" ", "-").ToLower() 
                    : 
                    model.Slug.Replace(" ", "-").ToLower();

                //Check if database containts a page with this title or slug
                //TODO: Add a custom attribute to do this check instead
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
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

                //Save new page
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["message"] = $"Page '{model.Title}' added successfully!";

            return RedirectToAction("Index");
        }

        public ActionResult EditPage(int id = 0)
        {
            PageViewModel page;

            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);

                if (dto == null) return Content($"Page with id: {id} does not exist!");

                page = new PageViewModel(dto);
            }

            return View(page);
        }

        [HttpPost]
        public ActionResult EditPage(PageViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (Db db = new Db())
            {
                int id = model.Id;
                string slug = "home";   //If model is not home page, slug will change; if it is, stay defaulted to home.

                //Get slug
                //TODO: reduce duplicated code from EditPage() methods. (GetSlug() method?)
                if (model.Slug != "home")
                {
                        slug = string.IsNullOrWhiteSpace(model.Slug) ?
                        model.Title.Replace(" ", "-").ToLower()
                        :
                        model.Slug.Replace(" ", "-").ToLower();
                }

                //Check for unique title and slug excluding itself
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) || db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Title or slug already exists!");
                    return View(model);
                }

                //Update the db entry
                PageDTO dto = db.Pages.Find(id);

                dto.Title = model.Title;
                dto.Slug = (dto.Slug == "home") ? "home" : slug;    //Malicious users can edit the form even if its readonly so we check here.
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Save changes
                db.SaveChanges();
            }

            TempData["message"] = $"Page '{model.Title}' saved successfully!";

            return RedirectToAction("Index");
        }

        private PageDTO GetSlug(PageDTO dto)
        {
            return dto;
        }
    }
}