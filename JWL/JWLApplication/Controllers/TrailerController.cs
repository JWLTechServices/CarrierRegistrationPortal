using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class TrailerController : Controller
    {
        private readonly ITrailerService _trailerService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public TrailerController(ITrailerService trailerService, ICarrierProcessingService carrierProcessingService)
        {
            _trailerService = trailerService;
            _carrierProcessingService = carrierProcessingService;
        }

        // GET: Trailer
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Trailer/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _trailerService.GetTrailer());
        }

        // GET: Trailer/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Trailer/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: Trailer/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("trailerId,trailerName,isDeleted,isActive")] trailer trailer)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Trailer/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                trailer.isActive = true;
                await _trailerService.AddTrailer(trailer, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(trailer);
        }

        // GET: Trailer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Trailer/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }

            var trailer = await _trailerService.GetTrailer(id.Value);
            if (trailer == null)
            {
                return NotFound();
            }
            return View(trailer);
        }

        // POST: Trailer/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("trailerId,trailerName,isDeleted,isActive")] trailer trailer)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Trailer/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != trailer.trailerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _trailerService.EditTrailer(trailer, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(trailer);
        }

        // GET: Trailer/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Trailer/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }
            trailer trailer = await _trailerService.GetTrailer(Convert.ToInt32(id));
            trailer.isDeleted = true;
            await _trailerService.EditTrailer(trailer, HttpContext.Session.GetString("UserID"));
            return Json("Trailer Types Deleted Successfully");
        }
    }
}
