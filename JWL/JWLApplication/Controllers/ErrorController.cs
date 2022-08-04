using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;

namespace JWLApplication.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error/404
        [HttpGet("/Error/404")]
        public IActionResult Error()
        {
            return RedirectToAction("ErrorPage");
        }
        [HttpGet("/404")]
        public IActionResult ErrorPage() {
            return View();
        }
    }
}
