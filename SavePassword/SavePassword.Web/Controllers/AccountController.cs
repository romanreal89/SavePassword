using Microsoft.AspNetCore.Mvc;
using SavePassword.Core;
using SavePassword.Core.Entities;
using System.Security.Cryptography;

namespace SavePassword.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    DataContext.GetInstance().LoadData(model.Name, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (CryptographicException exc)
                {
                    ModelState.AddModelError("Err", "Wrong credentials!");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            DataContext.GetInstance().Signout();
            return RedirectToAction("Login", "Account");
        }
    }
}