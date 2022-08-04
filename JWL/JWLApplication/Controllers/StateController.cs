using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class StateController : Controller
    {
        private readonly IStateService _stateService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public StateController(IStateService stateService, ICarrierProcessingService carrierProcessingService)
        {
            _stateService = stateService;
            _carrierProcessingService = carrierProcessingService;
        }

        // GET: State
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "State/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _stateService.GetStates());
        }
        // GET: State/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "State/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: State/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("stateId,stateName,isDeleted,isActive")] state state)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "State/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                state.isActive = true;
                await _stateService.AddState(state, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(state);
        }

        // GET: State/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "State/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }
            var state = await _stateService.GetState(id.Value);
            if (state == null)
            {
                return NotFound();
            }
            return View(state);
        }

        // POST: State/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("stateId,stateName,isDeleted,isActive")] state state)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "State/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != state.stateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _stateService.EditState(state, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(state);
        }

        // GET: State/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "State/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }
            state state = await _stateService.GetState(Convert.ToInt32(id));
            state.isDeleted = true;
            await _stateService.EditState(state, HttpContext.Session.GetString("UserID"));
            return Json("States Deleted Successfully");
        }


    }
}
