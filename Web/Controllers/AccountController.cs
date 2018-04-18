using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Web.Models.Data;
using Web.Models.ViewModels.Account;
using Web.Models.ViewModels.Store;

namespace Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return Redirect("~/Account/Login");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username)) return RedirectToAction("user-profile");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username == model.UserName && x.Password == model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
                }
                else
                {
                    ModelState.AddModelError("", "Incorrect username or password!");
                    return View(model);
                }
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [AllowAnonymous]
        [HttpPost]
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserViewModel model)
        {
            if (!ModelState.IsValid) return View("CreateAccount", model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username == model.Username))
                {
                    ModelState.AddModelError("", $"Username {model.Username} is taken!");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                UserDTO userDTO = new UserDTO
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                db.Users.Add(userDTO);
                db.SaveChanges();

                int id = userDTO.Id;
                
                UserRoleDTO userRoleDTO = new UserRoleDTO
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            TempData["message"] = "Registration successful";

            return Redirect("~/Account/Login");
        }

        public PartialViewResult UserNavPartial()
        {
            string username = User.Identity.Name;

            UserNavPartialViewModel model;

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                model = new UserNavPartialViewModel
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            return PartialView(model);
        }

        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            string username = User.Identity.Name;

            UserProfileViewModel model;

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                model = new UserProfileViewModel(dto);
            }

            return View("UserProfile", model);
        }

        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View("UserProfile", model);

            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match!");
                return View("UserProfile", model);
            }

            using (Db db = new Db())
            {
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == model.Username))
                {
                    ModelState.AddModelError("", $"Username {model.Username} already exists");
                    return View("UserProfile", model);
                }

                UserDTO dto = db.Users.Find(model.Id);

                dto.Username = model.Username;
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;

                if (!string.IsNullOrWhiteSpace(model.Password)) dto.Password = model.Password;

                db.SaveChanges();
            }

            FormsAuthentication.SignOut();
            Session.Clear();

            TempData["message"] = "Profile changes saved! Please sign in with your new details.";

            return RedirectToAction("Login");
        }

        [Authorize(Roles="User")]
        public ActionResult Orders()
        {
            List<OrderUserViewModel> userOrders = new List<OrderUserViewModel>();

            using (Db db = new Db())
            {
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);

                List<OrderViewModel> orders = db.Orders.Where(x => x.UserId == user.Id).ToArray().Select(x => new OrderViewModel(x)).ToList();

                foreach (var order in orders)
                {
                    var productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsList =
                        db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    string username = user.Username;

                    foreach (var orderDetails in orderDetailsList)
                    {
                        ProductDTO productDto = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);
                        decimal price = productDto.Price;
                        string productname = productDto.Name;
                        int qty = orderDetails.Quantity;

                        productsAndQty.Add(productname, qty);
                        total += price * qty;
                    }

                    userOrders.Add(new OrderUserViewModel
                    {
                        OrderId = order.OrderId,
                        UserName = username,
                        Total = total,
                        CreatedAt = order.CreatedAt,
                        ProductsAndQty = productsAndQty
                    });
                }
            }

            return View(userOrders);
        }
    }
}