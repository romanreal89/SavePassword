using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SavePassword.Core;
using SavePassword.Core.Entities;
using SavePassword.Web.Filters;
using SavePassword.Web.Models;
using System;
using System.Diagnostics;
using System.Linq;

namespace SavePassword.Web.Controllers
{
    [/*Authorize,*/ DataContextAuthorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View(DataContext.GetInstance().PasswordModel);

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(PassRecord model)
        {
            if (ModelState.IsValid)
            {
                model.Id = Guid.NewGuid();
                DataContext.GetInstance().PasswordModel.Add(model);
                DataContext.GetInstance().Save();
            }
            return View(model);
        }

        public IActionResult Edit(Guid id) => View(DataContext.GetInstance().PasswordModel.FirstOrDefault(x => x.Id == id));

        [HttpPost]
        public IActionResult Edit(PassRecord model)
        {
            if (ModelState.IsValid)
            {
                var item = DataContext.GetInstance().PasswordModel.FirstOrDefault(x => x.Id == model.Id);
                item.URL = model.URL;
                item.Login = model.Login;
                item.Password = model.Password;
                item.Details = model.Details;
                DataContext.GetInstance().Save();
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
