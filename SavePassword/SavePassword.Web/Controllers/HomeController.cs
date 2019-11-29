using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SavePassword.Core;
using SavePassword.Core.Entities;
using SavePassword.Web.Filters;
using System;
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
            ModelState.Remove(nameof(model.Id));
            if (ModelState.IsValid)
            {
                model.Id = Guid.NewGuid();
                DataContext.GetInstance().PasswordModel.PassRecords.Add(model);
                DataContext.GetInstance().Save();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(Guid id) => View(DataContext.GetInstance().PasswordModel.PassRecords.FirstOrDefault(x => x.Id == id));

        [HttpPost]
        public IActionResult Edit(PassRecord model)
        {
            if (ModelState.IsValid)
            {
                var item = DataContext.GetInstance().PasswordModel.PassRecords.FirstOrDefault(x => x.Id == model.Id);
                item.URL = model.URL;
                item.Login = model.Login;
                item.Password = model.Password;
                item.Details = model.Details;
                DataContext.GetInstance().Save();
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
