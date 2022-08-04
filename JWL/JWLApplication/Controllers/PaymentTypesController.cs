using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Threading.Tasks;

namespace JWLApplication.Controllers
{
    public class PaymentTypesController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICarrierProcessingService _carrierProcessingService;

        public PaymentTypesController(IPaymentService paymentService, ICarrierProcessingService carrierProcessingService)
        {
            _paymentService = paymentService;
            _carrierProcessingService = carrierProcessingService;
        }

        // GET: PaymentTypes
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "PaymentTypes/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _paymentService.GetPaymentTypes());
        }


        // GET: PaymentTypes/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "PaymentTypes/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: PaymentTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("paymentTypeId,paymentName,isDeleted,isActive")] paymenttype paymenttype)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "PaymentTypes/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (ModelState.IsValid)
            {
                paymenttype.isActive = true;
                await _paymentService.Addpaymenttype(paymenttype, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(paymenttype);
        }

        // GET: PaymentTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "PaymentTypes/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }

            var paymenttype = await _paymentService.GetPaymentTypes(id.Value);
            if (paymenttype == null)
            {
                return NotFound();
            }
            return View(paymenttype);
        }

        // POST: PaymentTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("paymentTypeId,paymentName,isDeleted,isActive")] paymenttype paymenttype)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "PaymentTypes/Edit"+id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != paymenttype.paymentTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _paymentService.Editpaymenttype(paymenttype, HttpContext.Session.GetString("UserID"));
                return RedirectToAction(nameof(Index));
            }
            return View(paymenttype);
        }
        [HttpPost]
        // GET: PaymentTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "PaymentTypes/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }
            paymenttype paymenttype = await _paymentService.GetPaymentTypes(Convert.ToInt32(id));
            paymenttype.isDeleted = true;
            await _paymentService.Editpaymenttype(paymenttype, HttpContext.Session.GetString("UserID"));
            return Json("Payment Methods Deleted Successfully");
        }


    }
}
