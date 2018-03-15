using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;

namespace Aiursoft.OSS.Controllers
{
    [AiurExceptionHandler]
    public class HomeController : AiurController
    {
        public IActionResult Index()
        {
            return Json(new AiurProtocal
            {
                code = ErrorType.Success,
                message = $"Welcome to Aiursoft OSS system. Please View our document at: '{Values.WikiServerAddress}'."
            });
        }

        public IActionResult Test()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
