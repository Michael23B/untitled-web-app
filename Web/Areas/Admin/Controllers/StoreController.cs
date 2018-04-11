using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using PagedList;
using Web.Models.Data;
using Web.Models.ViewModels.Store;

namespace Web.Areas.Admin.Controllers
{
    public class StoreController : Controller
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

        public ActionResult AddProduct()
        {
            ProductViewModel product = new ProductViewModel();

            using (Db db = new Db())
            {
                product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(product);
        }

        //TODO: add the image to the viewbag/tempadata and fill it in on the addproduct view when we fail to validate
        [HttpPost]
        public ActionResult AddProduct(ProductViewModel product, HttpPostedFileBase file)
        {
            int id;

            using (Db db = new Db())
            {
                //check model is valid
                //we need to return the selectlist for the categories when we go back to the view
                if (!ModelState.IsValid)
                {
                    product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(product);
                }

                //check if slug/name is taken
                string slug = product.Name.Replace(" ", "-").ToLower();

                if (db.Products.Any(x => x.Name == product.Name || x.Slug == slug))
                {
                    product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Name or slug taken!");
                    return View(product);
                }

                ProductDTO dbEntry = new ProductDTO
                {
                    Name = product.Name,
                    Slug = slug,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    CategoryName = db.Categories.FirstOrDefault(x => x.Id == product.CategoryId).Name
                };

                db.Products.Add(dbEntry);
                db.SaveChanges();

                id = dbEntry.Id;

                TempData["message"] = $"Product '{product.Name}' added!";
            }

            //
            //Image uploading
            //

            //Create directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            List<string> paths = new List<string>
            {
                Path.Combine(originalDirectory.ToString(), "Products"),
                Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()),
                Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs"),
                Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery"),
                Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs")
            };

            foreach (string path in paths)
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }

            //Check if file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                //if the file format doesnt match, return a model error and category list to the view
                switch (ext)
                {
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":
                    case "image/gif":
                    case "image/x-png":
                    case "image/png":
                        break;

                    default:
                        using (Db db = new Db())
                        {
                            product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            TempData["warning"] = $"Image not uploaded! {ext} file format not supported!";
                            return RedirectToAction("EditProduct", new { id });
                        }
                }

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                string path = string.Format("{0}\\{1}", paths[1], imageName);
                string path2 = string.Format("{0}\\{1}", paths[2], imageName);

                file.SaveAs(path);

                //Thumbnail

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            return RedirectToAction("Products");
        }

        public ActionResult Products(int? page, int? catId)
        {
            const int productsPerPage = 5;
            List<ProductViewModel> products;

            int pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                products = db.Products.ToList().Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                      .Select(x => new ProductViewModel(x)).ToList();

                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                ViewBag.SelectedCat = catId?.ToString();
            }

            var onePageOfProducts = products.ToPagedList(pageNumber, productsPerPage);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            return View(products);
        }

        public ActionResult EditProduct(int id = 0)
        {
            ProductViewModel model;

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content($"Product with id '{id}' could not be found!");
                }

                model = new ProductViewModel(dto)
                {
                    Categories = new SelectList(db.Categories.ToList(), "Id", "Name"),
                    GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(Path.GetFileName)
                };
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductViewModel product, HttpPostedFileBase file)
        {
            int id = product.Id;

            using (Db db = new Db())
            {
                //get the gallery image list
                product.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(Path.GetFileName);

                //get select list from categories
                product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //check model is valid
                if (!ModelState.IsValid) return View(product);

                //check if slug/name is taken
                string slug = product.Name.Replace(" ", "-").ToLower();

                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == product.Name || x.Slug == slug))
                {
                    product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Name or slug taken!");
                    return View(product);
                }

                ProductDTO dbEntry = db.Products.Find(id);

                dbEntry.Name = product.Name;
                dbEntry.Slug = slug;
                dbEntry.Description = product.Description;
                dbEntry.Price = product.Price;
                dbEntry.CategoryId = product.CategoryId;
                dbEntry.CategoryName = db.Categories.FirstOrDefault(x => x.Id == product.CategoryId).Name;

                db.SaveChanges();

                TempData["message"] = $"Product '{product.Name}' saved!";
            }


            //
            //Image uploading
            //

            //Check if file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                //if the file format doesnt match, return a model error and category list to the view
                switch (ext)
                {
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/pjpeg":
                    case "image/gif":
                    case "image/x-png":
                    case "image/png":
                        break;

                    default:
                        using (Db db = new Db())
                        {
                            product.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            TempData["warning"] = $"Image not uploaded! {ext} file format not supported!";
                            return View(product);
                        }
                }

                //Get directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Delete old images from directory

                DirectoryInfo dir1 = new DirectoryInfo(pathString1);
                DirectoryInfo dir2 = new DirectoryInfo(pathString2);

                foreach (FileInfo oldFile in dir1.GetFiles()) oldFile.Delete();
                foreach (FileInfo oldFile in dir2.GetFiles()) oldFile.Delete();

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                string path = string.Format("{0}\\{1}", pathString1, imageName);
                string path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                //Thumbnail

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }

            return RedirectToAction("Products");
        }

        public ActionResult DeleteProduct(int id)
        {
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                if (dto == null) return Content($"Product with id: {id} does not exist!");

                db.Products.Remove(dto);
                db.SaveChanges();

                TempData["message"] = $"Product '{dto.Name}' removed!";
            }

            //Delete product directory
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            //Delete old images from directory

            if (Directory.Exists(pathString)) Directory.Delete(pathString, true);

            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            foreach (string fileName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[fileName];

                if (file != null && file.ContentLength > 0)
                {
                    //Get directory paths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");


                    string path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    string path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    file.SaveAs(path);

                    //Thumbnail
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullpathimage = Request.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/" + imageName);
            string fullpaththumb = Request.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullpathimage)) System.IO.File.Delete(fullpathimage);
            if (System.IO.File.Exists(fullpaththumb)) System.IO.File.Delete(fullpaththumb);
        }
    }
}