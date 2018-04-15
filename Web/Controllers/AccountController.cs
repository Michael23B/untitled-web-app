using System;
using System.Collections.Generic;
using System.Configuration;
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

                //changing username has issues because the user.identity isnt updated
                //TODO: fix that
            }

            TempData["message"] = "Profile changes saved!";

            return RedirectToAction("user-profile");
        }
    }
}