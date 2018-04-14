using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
using Web.Models.Data;
using Web.Models.ViewModels.Account;

namespace Web.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("~/Account/Login");
        }

        public ActionResult Login()
        {
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username)) return RedirectToAction("user-profile");

            return View();
        }

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
            return RedirectToAction("Login");
        }

        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

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
    }
}