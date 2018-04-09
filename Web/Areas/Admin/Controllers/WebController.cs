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

        //TODO: replace jquery with standard form for science purposes
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;  //for jquery

            using (Db db = new Db())
            {
                string slug = catName.Replace(" ", "-").ToLower();

                if (db.Categories.Any(x => x.Slug == slug || x.Name == catName)) return "titletaken";
                
                CategoryDTO dto = new CategoryDTO
                {
                    Name = catName,
                    Slug = slug,
                    Sorting = int.MaxValue
                };

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString(); //Id is created by database
                //TempData["message"] = $"Category '{dto.Name}' saved!"; //(comment out when using jquery)
            }

            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                CategoryDTO dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //TODO: make this not a GET method
        public ActionResult DeleteCategory(int id = 0)
        {
            using (Db db = new Db())
            {
                CategoryDTO dto = db.Categories.Find(id);

                if (dto == null) return Content($"Category with id: {id} does not exist!");

                db.Categories.Remove(dto);
                db.SaveChanges();

                TempData["message"] = $"Page '{dto.Name}' removed!";
            }

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                string slug = newCatName.Replace(" ", "-").ToLower();

                //if anything in the database matches the given name or slug excluding the entry itself
                //eg. name: cool Memes -> Cool Memes has the same slug so it wouldnt work unless you did this very advanced check
                if (db.Categories.Where(x => x.Id != id).Any(x => x.Slug == slug || x.Name == newCatName)) return "titletaken";

                CategoryDTO dto = db.Categories.Find(id);
                dto.Name = newCatName;
                dto.Slug = slug;

                db.SaveChanges();

                //TempData["message"] = $"Category '{dto.Name}' saved!"; //(comment out when using jquery)
            }

            return "ok";
        }
    }
}