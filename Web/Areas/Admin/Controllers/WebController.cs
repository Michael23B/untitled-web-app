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
                if (db.Categories.Any(x => x.Name == catName)) return "titletaken";

                CategoryDTO dto = new CategoryDTO
                {
                    Name = catName,
                    Slug = catName.Replace(" ", "-").ToLower(),
                    Sorting = int.MaxValue
                };

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString(); //Id is created by database
                //TempData["message"] = $"Category '{dto.Name}' saved!"; //(comment out when using jquery)
            }

            return id;
        }
    }
}