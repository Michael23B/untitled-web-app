using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Web.Models.Data;
using Web.Models.ViewModels.Pages;
using Web.Models.ViewModels.Store;

namespace Web.Controllers
{
    public class StoreController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public PartialViewResult CategoryMenuPartial()
        {
            List<CategoryViewModel> categories;

            using (Db db = new Db())
            {
                categories = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryViewModel(x))
                    .ToList();
            }

            return PartialView(categories);
        }

        public ActionResult Category(string name)
        {
            List<ProductViewModel> products;

            using (Db db = new Db())
            {
                CategoryDTO dto = db.Categories.FirstOrDefault(x => x.Slug == name);

                if (dto == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

                int categoryId = dto.Id;

                products = db.Products.ToArray().Where(x => x.CategoryId == categoryId).Select(x => new ProductViewModel(x)).ToList();

                ViewBag.CategoryName = db.Products.FirstOrDefault(x => x.CategoryId == categoryId)?.CategoryName;
            }

            return View(products);
        }


        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            ProductViewModel product;

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.FirstOrDefault(x => x.Slug == name);

                if (dto == null) return RedirectToAction("Index", "Store");

                product = new ProductViewModel(dto);
            }

            product.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + product.Id + "/Gallery/Thumbs"))
                .Select(Path.GetFileName);

            return View("ProductDetails", product);
        }
    }
}