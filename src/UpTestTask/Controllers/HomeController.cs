using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace UpTestTask.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string searchString)
        {
            return View();
        }
    }
}
