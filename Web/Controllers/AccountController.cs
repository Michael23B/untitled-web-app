using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [HttpPost]
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserViewModel model)
        {
            if (!ModelState.IsValid) return View("create-account", model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match!");
                return View("create-account", model);
            }

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username == model.Username))
                {
                }
            }

            TempData["message"] = "Username or password incorrect!";

            return View("CreateAccount");
        }
    }
}