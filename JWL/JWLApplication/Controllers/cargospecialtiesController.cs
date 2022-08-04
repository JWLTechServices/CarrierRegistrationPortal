using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class cargospecialtiesController : Controller
    {
        private readonly ICargoService _cargoService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public cargospecialtiesController(ICargoService cargoService,ICarrierProcessingService carrierProcessingService)
        {
            _cargoService = cargoService;
            _carrierProcessingService = carrierProcessingService;
        }

        // GET: cargospecialties
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "CargoSpecialties/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _cargoService.GetCargoSpecialities());
        }

        // GET: cargospecialties/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "CargoSpecialties/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: cargospecialties/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("cargoId,cargoName,isActive,isDeleted")] cargospecialties cargospecialties)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "CargoSpecialties/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                cargospecialties.isActive = true;
                await _cargoService.AddCargoSpecialities(cargospecialties, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(cargospecialties);
        }

        // GET: cargospecialties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "CargoSpecialties/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }

            var cargospecialties = await _cargoService.GetCargo(id.Value);
            if (cargospecialties == null)
            {
                return NotFound();
            }
            return View(cargospecialties);
        }

        // POST: cargospecialties/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("cargoId,cargoName,isActive,isDeleted")] cargospecialties cargospecialties)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "CargoSpecialties/Edit"+ id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != cargospecialties.cargoId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                await _cargoService.EditCargoSpecialities(cargospecialties, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(cargospecialties);
        }

        // GET: cargospecialties/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "CargoSpecialties/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }

            cargospecialties cargo = await _cargoService.GetCargo(Convert.ToInt32(id));
            cargo.isDeleted = true;
            await _cargoService.EditCargoSpecialities(cargo, HttpContext.Session.GetString("UserID"));
            return Json("Cargo Specialities Deleted Successfully");
        }

    }
}
