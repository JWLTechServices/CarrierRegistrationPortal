using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class citiesController : Controller
    {
        private readonly ICityService _cityService;
        private readonly IStateService _stateService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public citiesController(ICityService cityService, IStateService stateService, ICarrierProcessingService carrierProcessingService)
        {
            _cityService = cityService;
            _stateService = stateService;
            _carrierProcessingService = carrierProcessingService;
        }
        // GET: cities
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "City/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _cityService.GetCities());
        }


        // GET: cities/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "City/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            ViewData["stateId"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            return View();
        }

        // POST: cities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("cityId,cityName,stateId,isActive")] city city)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                city.isActive = true;
                await _cityService.AddCities(city, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            ViewData["stateId"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", city.stateId);
            return View(city);
        }

        // GET: cities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "City/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }

            var city = await _cityService.GetCity(id.Value);
            if (city == null)
            {
                return NotFound();
            }
            ViewData["stateId"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", city.stateId);
            return View(city);
        }

        // POST: cities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("cityId,cityName,stateId,isActive")] city city)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != city.cityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _cityService.EditCity(city, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            ViewData["stateId"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", city.stateId);
            return View(city);
        }

        // GET: cities/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }
            city city = await _cityService.GetCity(Convert.ToInt32(id));
            city.isDeleted = true;
            await _cityService.EditCity(city, HttpContext.Session.GetString("UserID"));
            return Json("Cities Deleted Successfully");
        }
    }
}
