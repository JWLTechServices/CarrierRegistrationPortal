using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JWLApplication.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public ReportsController(IReportService reportService, ICarrierProcessingService carrierProcessingService)
        {
            _reportService = reportService;
            _carrierProcessingService = carrierProcessingService;
        }
        [HttpGet("/Reports")]
        [HttpGet("/Reports/Index")]
        public async Task<ActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            ViewData["Users"] = new SelectList(await _carrierProcessingService.ListUsers(), "userId", "name");
            return View();
        }
        //await _reportService.GetReports()
        public async Task<JsonResult> GetReports()
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Reports/Index");
            if (!isActive)
            {
                return Json("null");
            }
            return Json(await _reportService.GetReports());
        }
        public async Task<JsonResult> GetUser(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Reports/GetUser");
            if (!isActive)
            {
                return Json("null");
            }
            var users = await _reportService.GetReportsByUserID(Convert.ToInt32(id));
            return Json(users);
        }
        public async Task<JsonResult> GetByDate(string from,string to,string user)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Reports/GetByDate");
            if (!isActive)
            {
                return Json("null");
            }
            var users = await _reportService.GetReportsByDate(from,to,Convert.ToInt32(user));
            return Json(users);
        }
    }
}
