using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class marketstatesController : Controller
    {
        private readonly IMarkeSstateService _markeSstateService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public marketstatesController(IMarkeSstateService markeSstateService, ICarrierProcessingService carrierProcessingService)
        {
            _markeSstateService = markeSstateService;
            _carrierProcessingService = carrierProcessingService;
        }

        // GET: marketstates
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "OperatingState/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _markeSstateService.GetMarketState());
        }


        // GET: marketstates/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "OperatingState/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: marketstates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("marketStateID,marketName,isDeleted,isActive")] marketstate marketstate)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                marketstate.isActive = true;
                await _markeSstateService.AddMarketState(marketstate, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(marketstate);
        }

        // GET: marketstates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "OperatingState/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }

            var marketstate = await _markeSstateService.GetMarketstate(id.Value);
            if (marketstate == null)
            {
                return NotFound();
            }
            return View(marketstate);
        }

        // POST: marketstates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("marketStateID,marketName,isDeleted,isActive")] marketstate marketstate)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "OperatingState/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != marketstate.marketStateID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                await _markeSstateService.EditMarketstate(marketstate, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(marketstate);
        }

        // GET: marketstates/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "OperatingState/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }
            marketstate marketstate = await _markeSstateService.GetMarketstate(Convert.ToInt32(id));
            marketstate.isDeleted = true;
            await _markeSstateService.EditMarketstate(marketstate, HttpContext.Session.GetString("UserID"));
            return Json("Operating States Deleted Successfully");
        }


    }
}
