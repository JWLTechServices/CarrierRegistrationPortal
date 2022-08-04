using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleTypeService _vehicleTypeService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public VehicleController(IVehicleTypeService vehicleTypeService,ICarrierProcessingService carrierProcessingService)
        {
            _vehicleTypeService = vehicleTypeService;
            _carrierProcessingService = carrierProcessingService;
        }

        // GET: Vehicle
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")),"Vehicle/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _vehicleTypeService.GetVehicleTypes());
        }


        // GET: Vehicle/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Vehicle/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: Vehicle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("vehicleId,vehicleName,isDeleted,isActive")] vehicltype vehicltype)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Vehicle/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                vehicltype.isActive = true;
                await _vehicleTypeService.AddVehicleTypes(vehicltype, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(vehicltype);
        }

        // GET: Vehicle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Vehicle/Edit"+id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }

            var vehicltype = await _vehicleTypeService.GetVehicleTypes(id.Value);
            if (vehicltype == null)
            {
                return NotFound();
            }
            return View(vehicltype);
        }

        // POST: Vehicle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("vehicleId,vehicleName,isDeleted,isActive")] vehicltype vehicltype)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Vehicle/Edit");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != vehicltype.vehicleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _vehicleTypeService.EditVehicleTypes(vehicltype, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(vehicltype);
        }

        // GET: Vehicle/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Vehicle/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }
            vehicltype vehicltype = await _vehicleTypeService.GetVehicleTypes(Convert.ToInt32(id));
            vehicltype.isDeleted = true;
            await _vehicleTypeService.EditVehicleTypes(vehicltype, HttpContext.Session.GetString("UserID"));
            return Json("Vehicle Types Deleted Successfully");
        }
    }
}
