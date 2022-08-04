using Data;
using Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Data;


namespace JWLApplication.Controllers
{
    public class UsersController : Controller
    {
        private readonly ICarrierProcessingService _carrierProcessingService;
        string LogoUrl = "";
        private readonly IWebHostEnvironment _hostingEnvironment;
        private static Mailclient MailDetails;
        IConfigurationRoot configuration;
        private readonly JWLDBContext _jWLDBContext;
        private static Application Application;
        public UsersController(ICarrierProcessingService carrierProcessingService, IWebHostEnvironment hostingEnvironment, JWLDBContext jWLDBContext)
        {
            _hostingEnvironment = hostingEnvironment;
            _carrierProcessingService = carrierProcessingService;
            LogoUrl = "http://jwl.credencys.net:9089/img/jwl-logo.png";
            configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json")
          .Build();
            MailDetails = configuration.GetSection("MailClient").Get<Mailclient>();
            _jWLDBContext = jWLDBContext;
            Application = configuration.GetSection("Application").Get<Application>();
            
        }

        // GET: Users
        public async Task<IActionResult> ManageUsers()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/Manageusers");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _carrierProcessingService.ListUsers());
        }
        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/Create");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("name,email,password,confirmPassword,userType,isActive")] users users)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Index");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            users.isActive = true;
            if (ModelState.IsValid)
            {
                users.isFirstTime = true;
                await _carrierProcessingService.AddUser(users, HttpContext.Session.GetString("UserID"));
                try
                {
                    SmtpClient client = new SmtpClient(MailDetails.SmtpServer, MailDetails.Port);
                    client.Credentials = new NetworkCredential(MailDetails.UserName, MailDetails.Password);
                    client.EnableSsl = true;
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(MailDetails.Sender);
                    message.To.Add(new MailAddress(users.email));
                    message.Subject = "User Created";
                    message.IsBodyHtml = true; //to make message body as html  
                    string RegisterFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/UserCreate.html";
                    var UserHTML = string.Join("", System.IO.File.ReadAllLines(RegisterFile));
                    UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", users.name).Replace("##Password##", users.password).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId); ;
                    message.Body = UserHTML;
                    await client.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    _jWLDBContext.errortracelog.Add(new errortracelog()
                    {
                        error = ex.ToString(),
                        errorControl = "UserController/Create_SendMailAsync",
                        errorMessage = ex.Message,
                        errorName = ex.Source,
                        errorStack = ex.StackTrace,
                        errorTime = DateTime.Now
                    });
                    await _jWLDBContext.SaveChangesAsync();
                }
                return RedirectToAction("ManageUsers", "Users");

            }
            return View(users);
        }
        // GET: Users/Login
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        // POST: Users/Login
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginOld([Bind("name,email,password,userType,confirmPassword")] users users)
        {
            ModelState.Remove("confirmPassword");
            if (ModelState.IsValid)
            {
                users User = await _carrierProcessingService.IsValidDetails(users);
                if (User != null && User.userId != null && User.userId != 0)
                {
                    HttpContext.Session.SetString("UserID", User.userId.ToString());
                    string Name = User.name.Length <= 20 ? User.name : User.name.Substring(0, 20) + "...";
                    HttpContext.Session.SetString("UserName", Name);
                    HttpContext.Session.SetString("UserFullName", User.name);
                    HttpContext.Session.SetString("UserType", Convert.ToString(User.userType));
                    if (User.isFirstTime == null || User.isFirstTime == true)
                    {
                        return RedirectToAction("ChangePassword", "Users");
                    }
                    else
                    {
                        return RedirectToAction("ManageCarriers", "Carrier");
                    }
                }
                else
                {
                    ModelState.AddModelError("email", "Please enter valid email and password");
                }
            }
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("name,email,password,userType,confirmPassword")] users users)
        {
            ModelState.Remove("name");
            ModelState.Remove("password");
            ModelState.Remove("userType");
            ModelState.Remove("confirmPassword");

            if (ModelState.IsValid)
            {
                users User = await _carrierProcessingService.IsValidateSSOUser(users);
                if (User != null && User.userId != null && User.userId != 0)
                {
                    HttpContext.Session.SetString("UserID", User.userId.ToString());
                    string Name = User.name.Length <= 20 ? User.name : User.name.Substring(0, 20) + "...";
                    HttpContext.Session.SetString("UserName", Name);
                    HttpContext.Session.SetString("UserFullName", User.name);
                    HttpContext.Session.SetString("UserType", Convert.ToString(User.userType));
                    //if (User.isFirstTime == null || User.isFirstTime == true)
                    //{
                    //    return RedirectToAction("ChangePassword", "Users");
                    //}
                    //else
                    //{
                    //    return RedirectToAction("ManageCarriers", "Carrier");
                    //}
                    if (User.userType == UserType.Admin)
                    {
                        return RedirectToAction("ManageUsers", "Users");
                    }
                    else if (User.userType == UserType.CarrierUser)
                    {
                        return RedirectToAction("ManageCarriers", "Carrier");
                    }
                }
                else
                {
                    string strmessage = "Your account #emailId  is not set up in carrier registration Portal. Please get with Transportation group manager.";
                    strmessage = strmessage.Replace("#emailId", users.email);
                    ModelState.AddModelError("email", "Invalid User Or Your email is not configured");
                }
            }
            return View(users);
        }

        [HttpPost]
        public async Task<JsonResult> ValidateUserEmail(string emailId)
        {
            return Json(await _carrierProcessingService.ValidateUserEmail(emailId));
        }

        public async Task<ActionResult> ChangePassword()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/ChangePassword");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            var User = await _carrierProcessingService.GetUsers(Convert.ToInt32(HttpContext.Session.GetString("UserID")));
            return View(User);
        }
        [HttpPost]
        public async Task<ActionResult> ChangePassword([Bind("userId,password,confirmPassword")] users users)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/ChangePassword");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            users User = await _carrierProcessingService.GetUsers(users.userId);
            User.password = users.password;
            User.isFirstTime = false;
            await _carrierProcessingService.EditUser(User, HttpContext.Session.GetString("UserID"));
            return RedirectToAction("ManageCarriers", "Carrier");
        }
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _carrierProcessingService.GetUsers(id.Value));
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("userId,name,email,password,confirmPassword,userType,isActive")] users users)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/Edit");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id != users.userId)
            {
                return NotFound();
            }
            var a = await _carrierProcessingService.GetUsers(id);
            if (users.password == null || users.password == "")
            {
                ModelState.Remove("password");
                ModelState.Remove("confirmPassword");
                users.password = a.password;
            }
            users.isFirstTime = a.isFirstTime;
            if (ModelState.IsValid)
            {

                await _carrierProcessingService.EditUser(users, HttpContext.Session.GetString("UserID"));
                return RedirectToAction("ManageUsers", "Users");
            }
            return View(users);
        }
        // GET: Users/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Users/delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            if (id == null)
            {
                return NotFound();
            }
            users users = await _carrierProcessingService.GetUsers(Convert.ToInt32(id));
            users.isDeleted = true;
            await _carrierProcessingService.EditUser(users, HttpContext.Session.GetString("UserID"));
            return Json("User Deleted Successfully");
        }

    }
}
