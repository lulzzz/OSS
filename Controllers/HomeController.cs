﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AiursoftBase;
using AiursoftBase.Attributes;
using AiursoftBase.Models;

namespace OSS.Controllers
{
    [AiurExceptionHandler]
    [AiurRequireHttps]
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

        public IActionResult Error()
        {
            return View();
        }
    }
}
