using Microsoft.AspNetCore.Mvc;
using SavePassword.Core;
using SavePassword.Core.Entities;

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
                DataContext.GetInstance().LoadData(model.Name, model.Password);
                // await Authenticate(model.Name);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        //private async Task Authenticate(string userName)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
        //    };
        //    ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        //    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        //}

        public IActionResult Logout()
        {
            DataContext.GetInstance().Signout();
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}