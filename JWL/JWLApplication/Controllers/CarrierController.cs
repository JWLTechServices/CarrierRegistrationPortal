using AutoMapper;
using Data;
using Interfaces;
using JWLApplication.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using EPPlus.DataExtractor;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net.Http;
using System.Globalization;

namespace JWLApplication.Controllers
{
    [ServiceFilter(typeof(MyActionFilterAttribute))]
    public class CarrierController : Controller
    {
        private readonly ICarrierService _carrierService;
        private readonly IStateService _stateService;
        private readonly ICityService _cityService;
        private readonly IMarkeSstateService _markeSstateService;
        private readonly IVehicleTypeService _vehicleTypeService;
        private readonly ICargoService _cargoService;
        private readonly IPaymentService _paymentService;
        private readonly ITrailerService _trailerService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly GoogleRecaptchaService _googleRecaptchaService;
        private readonly IMapper _mapper;
        private readonly ICarrierProcessingService _carrierProcessingService;
        string LogoUrl = "";
        private static Mailclient MailDetails;
        private static RecaptchaSettings RecaptchaSettings;
        IConfigurationRoot configuration;
        private readonly JWLDBContext _jWLDBContext;
        private static CarrierUserAuth CarrierUserAuth;
        private static Application Application;
        private readonly DatatracService _datatracService;
        public CarrierController(ICarrierService carrierService, IStateService stateService
            , ICityService cityService, IMarkeSstateService markeSstateService
            , IVehicleTypeService vehicleTypeService, ICargoService cargoService
            , IPaymentService paymentService, IWebHostEnvironment hostingEnvironment
            , GoogleRecaptchaService googleRecaptchaService, ITrailerService trailerService
            , IMapper mapper, ICarrierProcessingService carrierProcessingService, JWLDBContext jWLDBContext, DatatracService datatracService)
        {
            _carrierService = carrierService;
            _stateService = stateService;
            _cityService = cityService;
            _markeSstateService = markeSstateService;
            _vehicleTypeService = vehicleTypeService;
            _cargoService = cargoService;
            _paymentService = paymentService;
            _hostingEnvironment = hostingEnvironment;
            _trailerService = trailerService;
            _mapper = mapper;
            _carrierProcessingService = carrierProcessingService;
            LogoUrl = "http://jwl.credencys.net:9089/img/jwl-logo.png";
            _googleRecaptchaService = googleRecaptchaService;
            configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json")
             .Build();

            MailDetails = configuration.GetSection("MailClient").Get<Mailclient>();
            RecaptchaSettings = configuration.GetSection("RecaptchaSettings").Get<RecaptchaSettings>();
            _jWLDBContext = jWLDBContext;
            CarrierUserAuth = configuration.GetSection("CarrierUserAuth").Get<CarrierUserAuth>();
            Application = configuration.GetSection("Application").Get<Application>();
            _datatracService = datatracService;
        }

        // GET: CarrierController/Create
        public async Task<ActionResult> Create()
        {
            HttpContext.Session.Clear();
            ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName");
            ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName");
            ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName");
            ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName");
            ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName");
            return View(new carrierusers());
        }
        // POST: CarrierController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("cuName,cuEmail,agreementDate,MC,authorizedPerson,title,legalCompanyName,DBA,physicalAddress,city,state,zipcode,telephone,fax,factoryCompanyName,factoryContactName,factoryPhysicalAddress,factoryCity,factoryState,factoryZipCode,factoryTelephone,factoryFax,additionalPersonName,addtionalPersonTelephone,addtionalAfterHoursPersonName,addtionalAfterHoursPersonTelephone,addtionalDot,additionalScac,additionalFedaralID,additionalHazmatCertified,additinalHazmatExpiryDate,additionalPreferredLanes,additionalMajorMakets,authorizedSignaturePath,authorizedSignature,authorizedDocuments,majorMarkets,isMinorityBusiness,businessYear,ethnicIdentification,milesPerYear,cargoSpecification,paymentMethods,twic,CarrierwiseVehicle,CarrierDocument,ca,isFemaleOwned,isVeteranOwned,Token,serviceArea,brokerOptions,CarrierwiseTrailer")] carrierusers carrierUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // To check the cuEmail is already exist
                    var CheckEmail = await _carrierService.CheckEmail(carrierUser.cuEmail, false, 0);
                    if (CheckEmail == "True")
                    {
                        ModelState.AddModelError("cuEmail", "Email already Exists");
                    }
                    else
                    {

                        // To check the additionalScac is already exist
                        if (!string.IsNullOrEmpty(carrierUser.additionalScac))
                        {
                            var CheckscacCode = await _carrierService.CheckSCACCode(carrierUser.additionalScac, false, 0);
                            if (CheckscacCode == "True")
                            {
                                ModelState.AddModelError("additionalScac", "SCAC Code already exists");
                                ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.state);
                                ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.factoryState);
                                ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.majorMarkets);
                                ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierUser.CarrierwiseTrailer);
                                ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierUser.CarrierwiseVehicle);
                                ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierUser.cargoSpecification);
                                ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierUser.paymentMethods);
                                return View(carrierUser);
                            }
                        }

                        carrierUser.status = StatusEnum.New;
                        carrierUser.createdByUserName = carrierUser.authorizedPerson;

                        byte[] salt = CreateSalt();
                        byte[] hash = HashPassword(CarrierUserAuth.DefaultPassword, salt);
                        string bas64Passwordhash = Convert.ToBase64String(hash);
                        string bas64PasswordSalt = Convert.ToBase64String(salt);

                        await _carrierService.AddCarrierUser(carrierUser, HttpContext.Session.GetString("UserID"), bas64Passwordhash, bas64PasswordSalt);
                        await SendMail(carrierUser.authorizedPerson, carrierUser.cuEmail, carrierUser.cuId);
                        //Generate PDF link
                        MemoryStream ms = new MemoryStream();
                        string FileLocation = $@"{_hostingEnvironment.WebRootPath}/\PDFTemplate\/JWLNDA.htm";
                        var htmlString = string.Join("", System.IO.File.ReadAllLines(FileLocation));
                        //var htmlString = $@"{_hostingEnvironment.WebRootPath}/\PDFTemplate\/JWLNDA.htm";
                        string compayName = string.Empty;
                        if (carrierUser.legalCompanyName != null)
                        {
                            compayName = carrierUser.legalCompanyName;
                        }
                        else if (carrierUser.DBA != null)
                            compayName = carrierUser.DBA;
                        else
                        {
                            compayName = @"<br/><span style='color:#ccc;text-decoration: underline;'>Enter Carrier/ Broker Company NameType a message</span>";
                        }
                        var jwlNDAhtml = htmlString.Replace("##DATE##", DateTime.Now.ToString("MM/dd/yyyy")).Replace("##NAME##", carrierUser.authorizedPerson).Replace("##ADDRESS##", carrierUser.physicalAddress).
                            Replace("##COMPANYNAME##", compayName).Replace("##AUTHORIZEDSIGNATURE##", carrierUser.authorizedSignature);
                        var jwNDAPDF = GetPDF(jwlNDAhtml);
                        ms = new MemoryStream(jwNDAPDF);

                        string fileNameJWLNDA = $@"/\PDFAttachment/\_{carrierUser.cuId}_{DateTime.Now.ToString("MMddyyyy")}_{"JWLNDA.pdf"}";
                        string FilePath2 = _hostingEnvironment.WebRootPath + fileNameJWLNDA;
                        using (FileStream file = new FileStream(FilePath2, FileMode.Create, FileAccess.Write))
                        {
                            ms.WriteTo(file);
                            file.Close();
                        }
                        TempData["JWLNDAPDF"] = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileNameJWLNDA;
                        string FileLocation2 = $@"{_hostingEnvironment.WebRootPath}/\PDFTemplate\/MasterBrokerCarrier.html";
                        var MasterBrokerCarrierHTML = string.Join("", System.IO.File.ReadAllLines(FileLocation2));
                        string space = new string(' ', 3);
                        var MasterBrokerCarrierPDF2 = MasterBrokerCarrierHTML.Replace("##DATE##", DateTime.Now.ToString("MM/dd/yyyy")).Replace("##NAME##", carrierUser.authorizedPerson).Replace("##MC##", carrierUser != null ? carrierUser.MC : space).Replace("##DOT##", carrierUser.addtionalDot).Replace("##FEDERALTAXID##", carrierUser.additionalFedaralID)
                            .Replace("##AUTHORIZEDSIGNATURE##", carrierUser.authorizedSignature).Replace("##TITLE##", carrierUser.title).Replace("##CUEMAIL##", carrierUser.cuEmail).Replace("##PHONE##", carrierUser.telephone);

                        var MasterBrokerCarrierPDF = GetPDF(MasterBrokerCarrierPDF2);
                        ms = new MemoryStream(MasterBrokerCarrierPDF);

                        string fileMasterBrokerCarrier = $@"/\PDFAttachment/\_{carrierUser.cuId}_{DateTime.Now.ToString("MMddyyyy")}_{"MasterBrokerCarrier.pdf"}";
                        string FilePath3 = _hostingEnvironment.WebRootPath + fileMasterBrokerCarrier;

                        using (FileStream file = new FileStream(FilePath3, FileMode.Create, FileAccess.Write))
                        {
                            ms.WriteTo(file);
                            file.Close();
                            ms.Close();
                        }
                        TempData["MasterBrokerCarrierPDF"] = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileMasterBrokerCarrier;
                        TempData.Keep("JWLNDAPDF");
                        TempData.Keep("MasterBrokerCarrierPDF");
                        carrierUser.ndaUrl = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileNameJWLNDA;
                        carrierUser.mbcaUrl = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileMasterBrokerCarrier;
                        await _carrierService.UpdateURL(carrierUser, "");

                        return RedirectToAction(nameof(Thankyou));
                    }
                }
                ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.state);
                ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.factoryState);
                ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.majorMarkets);
                ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierUser.CarrierwiseTrailer);
                ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierUser.CarrierwiseVehicle);
                ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierUser.cargoSpecification);
                ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierUser.paymentMethods);
                return View(carrierUser);
            }
            catch (Exception ex)
            {
                ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.state);
                ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.factoryState);
                ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.majorMarkets);
                ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierUser.CarrierwiseTrailer);
                ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierUser.CarrierwiseVehicle);
                ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierUser.cargoSpecification);
                ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierUser.paymentMethods);
                return View(carrierUser);
            }
        }

        public byte[] GetPDF(string pHTML)
        {
            byte[] bPDF = null;

            MemoryStream ms = new MemoryStream();
            TextReader txtReader = new StringReader(pHTML);
            Document doc = new Document(PageSize.A4);
            PdfWriter oPdfWriter = PdfWriter.GetInstance(doc, ms);
            HTMLWorker htmlWorker = new HTMLWorker(doc);
            doc.Open();
            htmlWorker.StartDocument();
            htmlWorker.Parse(txtReader);
            htmlWorker.EndDocument();
            htmlWorker.Close();
            doc.Close();
            bPDF = ms.ToArray();
            return bPDF;
        }
        // GET: Carrier/Register
        public async Task<ActionResult> Register()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Register");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName");
            ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName");
            ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName");
            ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName");
            ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName");
            return View(new carrierusers() { status = StatusEnum.New });
        }

        // POST: Carrier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register([Bind("cuId,cuName,cuEmail,agreementDate,MC,authorizedPerson,title,legalCompanyName,DBA,physicalAddress,city,state,zipcode,telephone,fax,factoryCompanyName,factoryContactName,factoryPhysicalAddress,factoryCity,factoryState,factoryZipCode,factoryTelephone,factoryFax,additionalPersonName,addtionalPersonTelephone,addtionalAfterHoursPersonName,addtionalAfterHoursPersonTelephone,addtionalDot,additionalScac,additionalFedaralID,additionalHazmatCertified,additinalHazmatExpiryDate,additionalPreferredLanes,additionalMajorMakets,authorizedSignaturePath,authorizedSignature,authorizedDocuments,majorMarkets,isMinorityBusiness,businessYear,ethnicIdentification,milesPerYear,cargoSpecification,paymentMethods,twic,CarrierwiseVehicle,CarrierDocument,completedMBCA,reportSortKey,ndaReturned,onboardingCompleted,onboardingFileLink,insuranceType,paymentTerms,CreatedDate,createdBy,ca,isFemaleOwned,isVeteranOwned,status,serviceArea,brokerOptions,CarrierwiseTrailer,Ins1MGeneral,Ins1MAuto")] carrierusers carrierUser)
        {
            try
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Register");
                if (!isActive)
                {
                    return RedirectToAction("Login", "Users");
                }
                if (ModelState.IsValid)
                {
                    if (carrierUser.insuranceType == "Ins100KCargo")
                    {
                        carrierUser.Ins100KCargo = true;
                    }
                    if (carrierUser.insuranceType == "Ins250KCargo")
                    {
                        carrierUser.Ins250KCargo = true;
                    }
                    carrierUser.createdBy = carrierUser.modifiedBy = carrierUser.assignee = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    carrierUser.createdByUserName = HttpContext.Session.GetString("UserFullName");
                    carrierUser.status = StatusEnum.Inprocess;
                    await _carrierService.AddCarrierUser(carrierUser, HttpContext.Session.GetString("UserID"));
                    SmtpClient client = new SmtpClient(MailDetails.SmtpServer, MailDetails.Port);
                    client.Credentials = new NetworkCredential(MailDetails.UserName, MailDetails.Password);
                    client.EnableSsl = true;
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress(MailDetails.Sender);
                    message.To.Add(new MailAddress(carrierUser.cuEmail));
                    message.Subject = "Registration Request Received";
                    message.IsBodyHtml = true; //to make message body as html  
                    string RegisterFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/Registration.html";
                    var RegisterMailHTML = string.Join("", System.IO.File.ReadAllLines(RegisterFile));
                    RegisterMailHTML = RegisterMailHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Person##", carrierUser.authorizedPerson);
                    message.Body = RegisterMailHTML;
                    await client.SendMailAsync(message);

                    SmtpClient clients = new SmtpClient(MailDetails.SmtpServer, MailDetails.Port);
                    clients.Credentials = new NetworkCredential(MailDetails.UserName, MailDetails.Password);
                    clients.EnableSsl = true;
                    MailMessage messages = new MailMessage();
                    messages.From = new MailAddress(MailDetails.Sender);
                    messages.To.Add(new MailAddress(carrierUser.cuEmail));
                    messages.IsBodyHtml = true; //to make message body as html  
                    if (carrierUser.status == StatusEnum.Inprocess)
                    {

                        messages.Subject = "Onboarding Started";
                        string OnboardingFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/Onboarding.html";
                        var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingFile));
                        UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", carrierUser.authorizedPerson);
                        messages.Body = UserHTML;
                        await clients.SendMailAsync(messages);
                    }
                    //Generate PDF link
                    MemoryStream ms = new MemoryStream();
                    string FileLocation = $@"{_hostingEnvironment.WebRootPath}/\PDFTemplate\/JWLNDA.htm";
                    var htmlString = string.Join("", System.IO.File.ReadAllLines(FileLocation));
                    //var htmlString = $@"{_hostingEnvironment.WebRootPath}/\PDFTemplate\/JWLNDA.htm";
                    string compayName = string.Empty;
                    if (carrierUser.legalCompanyName != null)
                    {
                        compayName = carrierUser.legalCompanyName;
                    }
                    else if (carrierUser.DBA != null)
                        compayName = carrierUser.DBA;
                    else
                    {
                        compayName = @"<br/><span style='color:#ccc;text-decoration: underline;'>Enter Carrier/ Broker Company NameType a message</span>";
                    }
                    var jwlNDAhtml = htmlString.Replace("##DATE##", DateTime.Now.ToString("MM/dd/yyyy")).Replace("##NAME##", carrierUser.authorizedPerson).Replace("##ADDRESS##", carrierUser.physicalAddress).
                        Replace("##COMPANYNAME##", compayName);
                    var jwNDAPDF = GetPDF(jwlNDAhtml);
                    ms = new MemoryStream(jwNDAPDF);

                    string fileNameJWLNDA = $@"/\PDFAttachment/\_{carrierUser.cuId}_{DateTime.Now.ToString("MMddyyyy")}_{"JWLNDA.pdf"}";
                    string FilePath2 = _hostingEnvironment.WebRootPath + fileNameJWLNDA;
                    using (FileStream file = new FileStream(FilePath2, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(file);
                        file.Close();
                    }
                    TempData["JWLNDAPDF"] = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileNameJWLNDA;
                    string FileLocation2 = $@"{_hostingEnvironment.WebRootPath}/\PDFTemplate\/MasterBrokerCarrier.html";
                    var MasterBrokerCarrierHTML = string.Join("", System.IO.File.ReadAllLines(FileLocation2));
                    string space = new string(' ', 3);
                    var MasterBrokerCarrierPDF2 = MasterBrokerCarrierHTML.Replace("##DATE##", DateTime.Now.ToString("MM/dd/yyyy")).Replace("##NAME##", carrierUser.authorizedPerson).Replace("##MC##", carrierUser != null ? carrierUser.MC : space).Replace("##DOT##", carrierUser.addtionalDot).Replace("##FEDERALTAXID##", carrierUser.additionalFedaralID);

                    var MasterBrokerCarrierPDF = GetPDF(MasterBrokerCarrierPDF2);
                    ms = new MemoryStream(MasterBrokerCarrierPDF);

                    string fileMasterBrokerCarrier = $@"/\PDFAttachment/\_{carrierUser.cuId}_{DateTime.Now.ToString("MMddyyyy")}_{"MasterBrokerCarrier.pdf"}";
                    string FilePath3 = _hostingEnvironment.WebRootPath + fileMasterBrokerCarrier;

                    using (FileStream file = new FileStream(FilePath3, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(file);
                        file.Close();
                        ms.Close();
                    }
                    TempData["MasterBrokerCarrierPDF"] = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileMasterBrokerCarrier;
                    TempData.Keep("JWLNDAPDF");
                    TempData.Keep("MasterBrokerCarrierPDF");
                    carrierUser.ndaUrl = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileNameJWLNDA;
                    carrierUser.mbcaUrl = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + fileMasterBrokerCarrier;
                    await _carrierService.UpdateURL(carrierUser, HttpContext.Session.GetString("UserID"));
                    return RedirectToAction(nameof(ManageCarriers));
                }
                ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.state);
                ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.factoryState);
                ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.majorMarkets);
                ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierUser.CarrierwiseTrailer);
                ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierUser.CarrierwiseVehicle);
                ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierUser.cargoSpecification);
                ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierUser.paymentMethods);
                return View(carrierUser);
            }
            catch (Exception ex)
            {
                ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.state);
                ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.factoryState);
                ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.majorMarkets);
                ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierUser.CarrierwiseTrailer);
                ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierUser.CarrierwiseVehicle);
                ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierUser.cargoSpecification);
                ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierUser.paymentMethods);
                return RedirectToAction("ManageCarriers", "Carrier");
            }
        }

        public ActionResult Thankyou()
        {
            return View();
        }
        // GET: CarrierController/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Edit" + id);
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                return NotFound();
            }
            carrierusers carrierusers = await _carrierService.GetCarrierusersById(id.Value);
            if (carrierusers == null)
            {
                return NotFound();
            }
            TempData["JWLNDAPDF"] = "";
            TempData["MasterBrokerCarrierPDF"] = "";
            ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierusers.state);
            ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierusers.factoryState);
            ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierusers.majorMarkets);
            ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierusers.CarrierwiseTrailer);
            ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierusers.CarrierwiseVehicle);
            ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierusers.cargoSpecification);
            ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierusers.paymentMethods);
            return View(carrierusers);
        }
        // GET: Carrier/ManageCarriers
        public async Task<ActionResult> ManageCarriers()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Login", "Users");
            }
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ManageCarriers");
            if (!isActive)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(await _carrierService.GetCarrierUsers());
        }
        // POST: CarrierController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [Bind("cuId,cuName,cuEmail,agreementDate,MC,authorizedPerson,title,legalCompanyName,DBA,physicalAddress,city,state,zipcode,telephone,fax,factoryCompanyName,factoryContactName,factoryPhysicalAddress,factoryCity,factoryState,factoryZipCode,factoryTelephone,factoryFax,additionalPersonName,addtionalPersonTelephone,addtionalAfterHoursPersonName,addtionalAfterHoursPersonTelephone,addtionalDot,additionalScac,additionalFedaralID,additionalHazmatCertified,additinalHazmatExpiryDate,additionalPreferredLanes,additionalMajorMakets,authorizedSignaturePath,authorizedSignature,authorizedDocuments,majorMarkets,isMinorityBusiness,businessYear,ethnicIdentification,milesPerYear,cargoSpecification,paymentMethods,twic,CarrierwiseVehicle,CarrierDocument,completedMBCA,reportSortKey,ndaReturned,onboardingCompleted,onboardingFileLink,insuranceType,paymentTerms,CreatedDate,createdBy,ca,isFemaleOwned,isVeteranOwned,status,assignee,serviceArea,brokerOptions,CarrierwiseTrailer,createdByUserName,Ins1MGeneral,Ins1MAuto,COIExpiryDate,carriertype,nametoprintoncheck,dx_vendor_id,dtuid")] carrierusers carrierUser)
        {
            try
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Register");
                if (!isActive)
                {
                    return RedirectToAction("Login", "Users");
                }
                if (carrierUser.COIExpiryDate == null)
                {
                    ModelState.AddModelError("COIExpiryDate", "Please enter COI Expiry Date.");
                }
                if (string.IsNullOrEmpty(carrierUser.carriertype))
                {
                    ModelState.AddModelError("carriertype", "Please select carrier type.");
                }
                if (string.IsNullOrEmpty(carrierUser.nametoprintoncheck))
                {
                    ModelState.AddModelError("nametoprintoncheck", "Name to print on check is required.");
                }
                if (string.IsNullOrEmpty(carrierUser.dx_vendor_id))
                {
                    ModelState.AddModelError("dx_vendor_id", "Dynamics vendor id is required.");
                }

                //if (string.IsNullOrEmpty(carrierUser.additionalScac))
                //{
                //    ModelState.AddModelError("additionalScac", "Additional Scac is required.");
                //}

                // get carrier exiting details to compare 
                carrierusers carrierusers = await _carrierService.GetCarrierusersById(Convert.ToInt32(id));

                if (ModelState.IsValid)
                {
                    if (carrierUser.insuranceType == "Ins100KCargo")
                    {
                        carrierUser.Ins100KCargo = true;
                    }
                    if (carrierUser.insuranceType == "Ins250KCargo")
                    {
                        carrierUser.Ins250KCargo = true;
                    }
                    carrierUser.modifiedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    await _carrierService.EditCarrierUser(carrierUser, HttpContext.Session.GetString("UserID"));

                    // update dx_vendor_id to datatrack if existing dx_vendor_id changed
                    if (carrierUser.status == StatusEnum.Approved && carrierusers.dx_vendor_id != carrierUser.dx_vendor_id)
                    {
                        // update dx_vendor_id at data trac
                        await UpdateCarrierInDataTrac(carrierUser);
                    }

                    return RedirectToAction("ManageCarriers");
                }

                ViewData["States"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.state);
                ViewData["FactoryStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.factoryState);
                ViewData["MarketStates"] = new SelectList(await _stateService.GetActiveStates(), "stateId", "stateName", carrierUser.majorMarkets);
                ViewData["TrailerType"] = new SelectList(await _trailerService.GetActiveTrailer(), "trailerId", "trailerName", carrierUser.CarrierwiseTrailer);
                ViewData["VehicleTypes"] = new SelectList(await _vehicleTypeService.GetActiveVehicleTypes(), "vehicleId", "vehicleName", carrierUser.CarrierwiseVehicle);
                ViewData["CargoSpecialities"] = new SelectList(await _cargoService.GetActiveCargoSpecialities(), "cargoId", "cargoName", carrierUser.cargoSpecification);
                ViewData["PaymentTypes"] = new SelectList(await _paymentService.GetActivePaymentTypes(), "paymentTypeId", "paymentName", carrierUser.paymentMethods);
                return View(carrierUser);
            }
            catch
            {
                return View();
            }
        }

        // POST: CarrierController/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/Delete" + id);
            if (!isActive)
            {
                return Json("delete");
            }
            carrierusers carrierusers = await _carrierService.GetCarrierusersById(Convert.ToInt32(id));
            carrierusers.isDeleted = true;
            await _carrierService.EditCarrierUser(carrierusers, HttpContext.Session.GetString("UserID"));
            return Json("Carrier deleted successfully");
        }
        [HttpPost]
        public async Task<ActionResult> FillCity(string stateId)
        {
            return Json(await _cityService.GetCitiesById(Convert.ToInt32(stateId)));
        }
        [HttpPost]
        public async Task<ActionResult> ChangeStatus(string id, string status)
        {
            try
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ChangeStatus" + id);
                if (!isActive)
                {
                    return Json("null");
                }
                carrierusers carrierusers = await _carrierService.GetCarrierusersById(Convert.ToInt32(id));
                carrierusers.modifiedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));

                int OldStatus = Convert.ToInt32(carrierusers.status);
                string tempExistingstatus = Convert.ToString((StatusEnum)OldStatus);

                int tempStatus = Convert.ToInt32(status);
                carrierusers.status = (StatusEnum)tempStatus;
                if (carrierusers.status == StatusEnum.Inprocess)
                {
                    carrierusers.assignee = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                }

                // start added to check the carriertype and nametoprintoncheck and COIExpiryDate is updated or not.
                if (carrierusers.status == StatusEnum.Approved)
                {
                    if (string.IsNullOrEmpty(carrierusers.carriertype))
                    {
                        return Json("Status cannot be Approved. The carrier type is required. please update the values in edit mode and then try," + tempExistingstatus.ToString());
                    }
                    if (string.IsNullOrEmpty(carrierusers.nametoprintoncheck))
                    {
                        return Json("Status cannot be Approved. The name to print on check is required. please update the values in edit mode and then try," + tempExistingstatus.ToString());
                    }
                    if (carrierusers.COIExpiryDate == null)
                    {
                        return Json("Status cannot be Approved. The COI Expiry Date is required. please update the values in edit mode and then try," + tempExistingstatus.ToString());
                    }
                    //if (carrierusers.additionalScac == null)
                    //{
                    //    return Json("Status cannot be Approved. The SCAC Code is required. please update the values in edit mode and then try," + tempExistingstatus.ToString());
                    //}

                    var today = DateTime.Today;
                    // DateTime todaysDate = DateTime.ParseExact(today.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    if (carrierusers.COIExpiryDate < today)
                    {
                        return Json("Status cannot be Approved. The COI Expiry Date is expired/should be greater than current date. please update the values in edit mode and then try," + tempExistingstatus.ToString());
                    }
                    if (string.IsNullOrEmpty(carrierusers.dx_vendor_id))
                    {
                        return Json("Status cannot be Approved. The dynamics vendor id required. please update the values in edit mode and then try," + tempExistingstatus.ToString());
                    }
                    if (carrierusers.dtuid == null)
                    {
                        bool GenerateDatatracUIDResponse = await GenerateDatatrac_UID(carrierusers);
                        if (!GenerateDatatracUIDResponse)
                        {
                            return Json("Status cannot be Approved. The Datatrac UID is required. Unable to generate Datatrac UID,Please try again after some time" + tempExistingstatus.ToString());
                        }
                    }
                }
                // end

                await _carrierService.EditCarrierUser(carrierusers, HttpContext.Session.GetString("UserID"));
                SmtpClient client = new SmtpClient(MailDetails.SmtpServer, MailDetails.Port);
                client.Credentials = new NetworkCredential(MailDetails.UserName, MailDetails.Password);
                client.EnableSsl = true;
                MailMessage message = new MailMessage();
                message.From = new MailAddress(MailDetails.Sender);
                message.To.Add(new MailAddress(carrierusers.cuEmail));
                message.IsBodyHtml = true; //to make message body as html  

                try
                {
                    if (carrierusers.status == StatusEnum.Inprocess)
                    {
                        message.Subject = "Onboarding Started";
                        string OnboardingFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/Onboarding.html";
                        var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingFile));
                        UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", carrierusers.authorizedPerson).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId);
                        message.Body = UserHTML;
                        //  await client.SendMailAsync(message);

                    }
                    else if (carrierusers.status == StatusEnum.Complete)
                    {
                        message.Subject = "Onboarding Completed";
                        string OnboardingCompleteFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/OnboardingComplete.html";
                        var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingCompleteFile));
                        UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", carrierusers.authorizedPerson).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId); ;
                        message.Body = UserHTML;
                        // await client.SendMailAsync(message);

                    }
                    else if (carrierusers.status == StatusEnum.Approved)
                    {

                        message.Subject = "Registration  Approved";
                        string OnboardingFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/Approved.html";
                        var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingFile));
                        UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", carrierusers.authorizedPerson).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId); ;

                        //Start -- To send set password link to carrier for the first time
                        Guid AuthToken = Guid.NewGuid();
                        string SetPasswordlink = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + @"/Carrier/SetPassword?AuthToken=" + AuthToken.ToString();
                        UserHTML = UserHTML.Replace("##SetPasswordlink##", SetPasswordlink);

                        int otp = Convert.ToInt32(await _carrierService.GenerateNumericOTP(Convert.ToInt32(CarrierUserAuth.OtpLength)));
                        OtpLogs objOtpLogs = new OtpLogs();
                        objOtpLogs.UserId = carrierusers.cuId;
                        objOtpLogs.OTP = otp;
                        objOtpLogs.Token = AuthToken.ToString();
                        objOtpLogs.EmailId = carrierusers.cuEmail;
                        objOtpLogs.CreatedDate = DateTime.Now;
                        objOtpLogs.IsActive = true;
                        await _carrierService.InsertOtp(objOtpLogs);

                        // end 
                        message.Body = UserHTML;
                        // await client.SendMailAsync(message);
                    }
                    else if (carrierusers.status == StatusEnum.Rejected)
                    {
                        message.Subject = "Registration Rejected";
                        string OnboardingFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/Rejected.html";
                        var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingFile));
                        UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", carrierusers.authorizedPerson).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId); ;
                        message.Body = UserHTML;
                        //await client.SendMailAsync(message);
                    }
                    else if (carrierusers.status == StatusEnum.Onhold)
                    {
                        message.Subject = "Registration On-hold";
                        string OnboardingFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/OnHold.html";
                        var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingFile));
                        UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", carrierusers.authorizedPerson).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId); ;
                        message.Body = UserHTML;
                        // await client.SendMailAsync(message);
                    }

                    try
                    {
                        // to call SendMailAsync method in common and commented in all the conditions above
                        await client.SendMailAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _jWLDBContext.errortracelog.Add(new errortracelog()
                        {
                            error = ex.ToString(),
                            errorControl = "CarrierController/ChangeStatus_SendMailAsync",
                            errorMessage = ex.Message,
                            errorName = ex.Source,
                            errorStack = ex.StackTrace,
                            errorTime = DateTime.Now
                        });
                        await _jWLDBContext.SaveChangesAsync();
                    }

                    if (tempExistingstatus.ToLower() == "complete" && carrierusers.status == StatusEnum.Approved)
                    {
                        // create data at department
                        await CreateCarrierInDynamicsAndDataTrac(carrierusers);
                    }
                    if (tempExistingstatus.ToLower() == "approved" && (carrierusers.status == StatusEnum.Onhold || carrierusers.status == StatusEnum.Terminated))
                    {
                        // update data at department
                        await UpdateCarrierInDynamicsAndDataTrac(carrierusers);

                    }
                    if (tempExistingstatus.ToLower() == "on-hold" && carrierusers.status == StatusEnum.Approved)
                    {
                        // update data at department
                        await UpdateCarrierInDynamicsAndDataTrac(carrierusers);

                    }
                    if (tempExistingstatus.ToLower() == "on-hold" && carrierusers.status == StatusEnum.Terminated)
                    {
                        // update data at departemnt
                        await UpdateCarrierInDynamicsAndDataTrac(carrierusers);

                    }

                }
                catch (Exception ex)
                {
                    _jWLDBContext.errortracelog.Add(new errortracelog()
                    {
                        error = ex.ToString(),
                        errorControl = "CarrierController/ChangeStatus",
                        errorMessage = ex.Message,
                        errorName = ex.Source,
                        errorStack = ex.StackTrace,
                        errorTime = DateTime.Now
                    });
                    await _jWLDBContext.SaveChangesAsync();
                }
                string stringValue = carrierusers.status.ToString();
                return Json("Status changed successfully," + stringValue);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> GenerateSCACCode(string id)
        {
            string returnRespone = "F";
            string stradditionalScac = "";
            try
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ChangeStatus" + id);
                if (!isActive)
                {
                    return Json("null");
                }
                carrierusers carrierusers = await _carrierService.GetCarrierusersById(Convert.ToInt32(id));
                carrierusers.modifiedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                int i = 0;
                do
                {
                    Random random = new Random();
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    stradditionalScac = "*" + new string(Enumerable.Repeat(chars, 3)
                        .Select(s => s[random.Next(s.Length)]).ToArray());

                    var CheckscacCode = await _carrierService.CheckSCACCode(stradditionalScac, false, 0);
                    if (CheckscacCode == "False")
                    {
                        carrierusers.additionalScac = stradditionalScac;
                        await _carrierService.EditCarrierUser(carrierusers, HttpContext.Session.GetString("UserID"));
                        returnRespone = "S";
                        break;
                    }
                    i++;
                }
                while (i < 500);
                return Json("SCAC Code Updated successfully," + returnRespone + "," + stradditionalScac);
            }
            catch (Exception ex)
            {
                _jWLDBContext.errortracelog.Add(new errortracelog()
                {
                    error = ex.ToString(),
                    errorControl = "CarrierController/GenerateSCACCode",
                    errorMessage = ex.Message,
                    errorName = ex.Source,
                    errorStack = ex.StackTrace,
                    errorTime = DateTime.Now
                });
                await _jWLDBContext.SaveChangesAsync();

                returnRespone = "E";
                return Json("SCAC Code Updation Failed," + returnRespone);
                //  return Json(ex.Message +","+ returnRespone);
            }
        }

        [HttpPost]
        public async Task<bool> GenerateDatatrac_UID(carrierusers carrierusers)
        {
            string dtuid;
            bool response = false;
            int i = 0;
            int j = 1000;
            do
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                dtuid = new string(Enumerable.Repeat(chars, 4)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var Checkdtuid = await _carrierService.CheckDataTrac_UID(dtuid, false, 0);
                if (Checkdtuid == "False")
                {
                    carrierusers.dtuid = dtuid;
                    await _carrierService.EditCarrierUser(carrierusers, HttpContext.Session.GetString("UserID"));
                    response = true;
                    break;
                }
                i++;

            }
            while (i < j);
            return response;
        }

        [HttpPost]
        public string UploadSignature()
        {
            string result = string.Empty;
            try
            {
                long size = 0;
                var file = Request.Form.Files;
                var filename = ContentDispositionHeaderValue.Parse(file[0].ContentDisposition).FileName.Trim('"');
                string name = @"/\signature/\sig_" + DateTime.Now.ToString("MM-dd-yyyy") + filename;
                string FilePath = _hostingEnvironment.WebRootPath + name;
                size += file[0].Length;
                using (FileStream fs = System.IO.File.Create(FilePath))
                {
                    file[0].CopyTo(fs);
                    fs.Flush();
                }

                result = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + name;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        [HttpPost]
        public async Task<string> UploadAttachment()
        {
            string result = string.Empty;
            try
            {
                if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
                {
                    bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/RemoveFile");
                    if (!isActive)
                    {
                        return "null";
                    }
                }

                long size = 0;
                IFormFileCollection file = Request.Form.Files;
                string selectedVal = Request.Form["selectedVal"].ToString();
                var filename = ContentDispositionHeaderValue.Parse(file[0].ContentDisposition).FileName.Trim('"');

                string ret = Regex.Replace(filename.Trim(), "[^A-Za-z0-9_. ]+", "");
                ret.Replace(" ", String.Empty);

                string name = $@"/\Document/\att_{DateTime.Now.ToString("MM-dd-yyyy")} {Guid.NewGuid()} {ret}";
                string FilePath = _hostingEnvironment.WebRootPath + name;
                size += file[0].Length;
                using (FileStream fs = System.IO.File.Create(FilePath))
                {
                    file[0].CopyTo(fs);
                    fs.Flush();
                }
                result = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + name;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
        [HttpPost]
        public async Task<JsonResult> RemoveFile(string file)
        {
            string result = string.Empty;
            try
            {
                if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
                {
                    bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/RemoveFile");
                    if (!isActive)
                    {
                        return Json("null");
                    }
                }
                string FileName = Path.GetFileName(file);
                System.IO.File.Delete(_hostingEnvironment.WebRootPath + FileName);
                await _carrierService.DeleteAttachment(FileName, HttpContext.Session.GetString("UserID"));
                result = "Success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result);
        }
        [HttpPost]
        public async Task<JsonResult> RemoveVehicle(string vehicleId)
        {
            string result = string.Empty;
            try
            {
                if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
                {
                    bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/RemoveVehicle");
                    if (!isActive)
                    {
                        return Json("null");
                    }
                    await _carrierService.DeleteVehicle(Convert.ToInt32(vehicleId), HttpContext.Session.GetString("UserID"));
                }

                result = "Success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result);
        }
        [HttpPost]
        public async Task<JsonResult> RemoveTrailer(string trailerId)
        {
            string result = string.Empty;
            try
            {
                if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
                {
                    bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/RemoveTrailer");
                    if (!isActive)
                    {
                        return Json("null");
                    }
                    await _carrierService.DeleteTrailer(Convert.ToInt32(trailerId), HttpContext.Session.GetString("UserID"));
                }

                result = "Success";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return Json(result);
        }
        [HttpPost]
        public async Task<JsonResult> IsHuman(string token)
        {
            var googlerecaptcha = await _googleRecaptchaService.Verification(token);
            if (googlerecaptcha.success && googlerecaptcha.score >= 0.5)
            {
                return Json(true);
            }
            return Json(false);
        }
        [HttpPost]
        public async Task<JsonResult> CheckEmailDOT(string email, string dot, string isEdit, string id = "0")
        {
            return Json(await _carrierService.CheckEmailDOT(email, dot, Convert.ToBoolean(isEdit), Convert.ToInt32(id)));
        }
        [HttpPost]
        public async Task<JsonResult> CheckDOT(string dot, string isEdit, string id = "0")
        {
            if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/CheckDOT" + id);
                if (!isActive)
                {
                    return Json("null");
                }
            }
            return Json(await _carrierService.CheckDOT(dot, Convert.ToBoolean(isEdit), Convert.ToInt32(id)));
        }
        [HttpPost]
        public async Task<JsonResult> CheckEmail(string email, string isEdit, string id = "0")
        {
            if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/CheckEmail" + id);
                if (!isActive)
                {
                    return Json("null");
                }
            }
            return Json(await _carrierService.CheckEmail(email, Convert.ToBoolean(isEdit), Convert.ToInt32(id)));
        }

        [HttpPost]
        public async Task<JsonResult> CheckSCACCode(string additionalScac, string isEdit, string id = "0")
        {
            if (HttpContext.Session.GetString("UserID") != null && HttpContext.Session.GetString("UserID") != "")
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/CheckSCACCode" + id);
                if (!isActive)
                {
                    return Json("null");
                }
            }
            return Json(await _carrierService.CheckSCACCode(additionalScac, Convert.ToBoolean(isEdit), Convert.ToInt32(id)));
        }

        [HttpPost]
        public async Task SendMail(string Name, string Email, int cuId)
        {
            try
            {
                SmtpClient client = new SmtpClient(MailDetails.SmtpServer, MailDetails.Port);
                client.Credentials = new NetworkCredential(MailDetails.UserName, MailDetails.Password);
                client.EnableSsl = true;
                MailMessage message = new MailMessage();
                message.From = new MailAddress(MailDetails.Sender);
                message.To.Add(new MailAddress(Email));
                message.Subject = "Registration Request Received";
                message.IsBodyHtml = true; //to make message body as html  
                string RegisterFile = $@"{_hostingEnvironment.WebRootPath}/\MailTemplate\/Registration.html";
                var RegisterMailHTML = string.Join("", System.IO.File.ReadAllLines(RegisterFile));
                RegisterMailHTML = RegisterMailHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Person##", Name).Replace("##LogoUrlInTemplate##", Application.LogoUrlInTemplate).Replace("##DisclaimerReplyEmailId##", Application.DisclaimerReplyEmailId);

                ////Start -- To send set password link to carrier for the first time
                //Guid AuthToken = Guid.NewGuid();
                //string SetPasswordlink = this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + @"/Carrier/SetPassword?AuthToken=" + AuthToken.ToString();
                //RegisterMailHTML = RegisterMailHTML.Replace("##SetPasswordlink##", SetPasswordlink);

                //int otp = Convert.ToInt32(await _carrierService.GenerateNumericOTP(Convert.ToInt32(CarrierUserAuth.OtpLength)));
                //OtpLogs objOtpLogs = new OtpLogs();
                //objOtpLogs.UserId = cuId;
                //objOtpLogs.OTP = otp;
                //objOtpLogs.Token = AuthToken.ToString();
                //objOtpLogs.EmailId = Email;
                //objOtpLogs.CreatedDate = DateTime.Now;
                //objOtpLogs.IsActive = true;
                //await _carrierService.InsertOtp(objOtpLogs);

                //// end 

                message.Body = RegisterMailHTML;
                await client.SendMailAsync(message);

            }
            catch (Exception ex)
            {
                _jWLDBContext.errortracelog.Add(new errortracelog()
                {
                    error = ex.ToString(),
                    errorControl = "CarrierController/SendMail",
                    errorMessage = ex.Message,
                    errorName = ex.Source,
                    errorStack = ex.StackTrace,
                    errorTime = DateTime.Now
                });
                await _jWLDBContext.SaveChangesAsync();
            }
        }
        [HttpGet]
        //Carrier/ExportToExcel
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                bool isActive = await _carrierProcessingService.IsActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ExportToExcel");
                if (!isActive)
                {
                    return RedirectToAction("Login", "Users");
                }
                List<carrierusers> carrier = await _carrierService.ExportCarrierUsers();
                List<state> marketstates = await _stateService.GetStates();
                List<paymenttype> paymenttypes = await _paymentService.GetPaymentTypes();
                List<cargospecialties> cargo = await _cargoService.GetCargoSpecialities();
                List<CarrierViewModel> carrierViewModels = new List<CarrierViewModel>();
                foreach (var item in carrier)
                {
                    List<string> SelectedMarket = item.additionalMajorMakets != null ? item.additionalMajorMakets.Split(',').ToList() : new List<string>();
                    List<string> GivenMarket = marketstates.Where(t => SelectedMarket.Contains(t.stateId.ToString())).Select(t => t.stateName).ToList();
                    List<string> SelectedPayment = item.paymentMethods != null ? item.paymentMethods.Split(',').ToList() : new List<string>();
                    List<string> GivenPayment = paymenttypes.Where(t => SelectedPayment.Contains(t.paymentTypeId.ToString())).Select(t => t.paymentName).ToList();
                    List<string> SelectedCargo = item.cargoSpecification != null ? item.cargoSpecification.Split(',').ToList() : new List<string>();
                    List<string> GivenMethod = cargo.Where(t => SelectedCargo.Contains(t.cargoId.ToString())).Select(t => t.cargoName).ToList();
                    string Identification = string.Empty;
                    if (item.ethnicIdentification == "1")
                        Identification = "American Indian or Alaska Native";
                    else if (item.ethnicIdentification == "2")
                        Identification = "Asian";
                    else if (item.ethnicIdentification == "3")
                        Identification = "Black or African American";
                    else if (item.ethnicIdentification == "4")
                        Identification = "Hispanic or Latino";
                    else if (item.ethnicIdentification == "5")
                        Identification = "Native Hawaiian or Other Pacific Islander";
                    else if (item.ethnicIdentification == "6")
                        Identification = "White";
                    else if (item.ethnicIdentification == "7")
                        Identification = "Other";
                    else if (item.ethnicIdentification == "8")
                        Identification = "I prefer not to say";

                    List<vehicles> vehicles = JsonConvert.DeserializeObject<List<vehicles>>(item.CarrierwiseVehicle);

                    List<tempTrailer> trailers = JsonConvert.DeserializeObject<List<tempTrailer>>(item.CarrierwiseTrailer);
                    List<authorizedpath> authorizedpaths = JsonConvert.DeserializeObject<List<authorizedpath>>(item.CarrierDocument);

                    carrierViewModels.Add(new CarrierViewModel()
                    {
                        cuId = item.cuId.ToString(),
                        cuEmail = item.cuEmail != null ? item.cuEmail : string.Empty,
                        agreementDate = item.agreementDate != null ? item.agreementDate.ToString("MM/dd/yyyy") : string.Empty,
                        MC = item.MC != null ? item.MC : string.Empty,
                        authorizedPerson = item.authorizedPerson != null ? item.authorizedPerson : string.Empty,
                        title = item.title != null ? item.title : string.Empty,
                        legalCompanyName = item.legalCompanyName != null ? item.legalCompanyName : string.Empty,
                        DBA = item.DBA != null ? item.DBA : string.Empty,
                        physicalAddress = item.physicalAddress != null ? item.physicalAddress : string.Empty,
                        city = item.city != null ? item.city : string.Empty,
                        state = item.States.stateName,
                        zipcode = item.zipcode != null ? item.zipcode : string.Empty,
                        telephone = item.telephone != null ? item.telephone : string.Empty,
                        fax = item.fax != null ? item.fax : string.Empty,
                        status = item.status.ToString(),
                        factoryCompanyName = item.factoryCompanyName != null ? item.factoryCompanyName : string.Empty,
                        factoryContactName = item.factoryContactName != null ? item.factoryContactName : string.Empty,
                        factoryPhysicalAddress = item.factoryPhysicalAddress != null ? item.factoryPhysicalAddress : string.Empty,
                        factoryCity = item.factoryCity != null ? item.factoryCity : string.Empty,
                        factoryState = item.FactoryStates != null && item.FactoryStates.stateName != null ? item.FactoryStates.stateName : string.Empty,
                        factoryZipCode = item.factoryZipCode != null ? item.factoryZipCode : string.Empty,
                        factoryTelephone = item.factoryTelephone != null ? item.factoryTelephone : string.Empty,
                        factoryFax = item.factoryFax != null ? item.factoryFax : string.Empty,
                        additionalPersonName = item.additionalPersonName != null ? item.additionalPersonName : string.Empty,
                        addtionalPersonTelephone = item.addtionalPersonTelephone != null ? item.addtionalPersonTelephone : string.Empty,
                        addtionalAfterHoursPersonName = item.addtionalAfterHoursPersonName != null ? item.addtionalAfterHoursPersonName : string.Empty,
                        addtionalAfterHoursPersonTelephone = item.addtionalAfterHoursPersonTelephone != null ? item.addtionalAfterHoursPersonTelephone : string.Empty,
                        addtionalDot = item.addtionalDot != null ? item.addtionalDot : string.Empty,
                        additionalScac = item.additionalScac != null ? item.additionalScac : string.Empty,
                        additionalFedaralID = item.additionalFedaralID != null ? item.additionalFedaralID : string.Empty,
                        additionalHazmatCertified = item.additionalHazmatCertified == null ? string.Empty : item.additionalHazmatCertified == false ? "No" : "Yes",
                        additinalHazmatExpiryDate = item.additinalHazmatExpiryDate != null ? item.additinalHazmatExpiryDate.Value.ToString("MM/dd/yyyy") : string.Empty,
                        additionalPreferredLanes = item.additionalPreferredLanes,
                        additionalMajorMakets = string.Join(",", GivenMarket),
                        authorizedSignature = item.authorizedSignature != null ? item.authorizedSignature : string.Empty,
                        completedMBCA = item.completedMBCA == null ? string.Empty : item.completedMBCA == false ? "No" : "Yes",
                        reportSortKey = item.reportSortKey != null ? item.reportSortKey : string.Empty,
                        ndaReturned = item.ndaReturned == null ? string.Empty : item.ndaReturned == false ? "No" : "Yes",
                        onboardingCompleted = item.onboardingCompleted == null ? string.Empty : item.onboardingCompleted == false ? "No" : "Yes",
                        Ins1MGeneral = item.Ins1MGeneral == null ? string.Empty : item.Ins1MGeneral == false ? "No" : "Yes",
                        Ins1MAuto = item.Ins1MAuto == null ? string.Empty : item.Ins1MAuto == false ? "No" : "Yes",
                        Ins100KCargo = item.Ins100KCargo == null ? string.Empty : item.Ins100KCargo == false ? "No" : "Yes",
                        Ins250KCargo = item.Ins250KCargo == null ? string.Empty : item.Ins250KCargo == false ? "No" : "Yes",
                        paymentTerms = item.paymentTerms != null ? item.paymentTerms : string.Empty,
                        CreatedDate = item.CreatedDate != null ? item.CreatedDate.ToString("MM/dd/yyyy") : string.Empty,
                        createdBy = item.cuId == item.createdBy ? item.authorizedPerson : item.CreatedUserName,//item.createdByUserName
                        ModifiedDate = item.modifiedBy != null ? item.ModifiedDate.ToString("MM/dd/yyy") : string.Empty,
                        twic = item.twic == null ? string.Empty : item.twic == false ? "No" : "Yes",
                        ca = item.ca != null ? item.ca : string.Empty,
                        isFemaleOwned = item.isFemaleOwned == null ? string.Empty : item.isFemaleOwned == false ? "No" : "Yes",
                        isVeteranOwned = item.isVeteranOwned == null ? string.Empty : item.isVeteranOwned == false ? "No" : "Yes",
                        modifiedBy = item.modifiedByUser != null && item.modifiedByUser.name != null ? item.modifiedByUser.name : string.Empty,
                        assignee = item.Users != null && item.Users.name != null ? item.Users.name : string.Empty,
                        isMinorityBusiness = item.isMinorityBusiness == null ? string.Empty : item.isMinorityBusiness == false ? "No" : "Yes",
                        businessYear = item.businessYear != null ? item.businessYear : string.Empty,
                        milesPerYear = item.milesPerYear != null ? item.milesPerYear : string.Empty,
                        cargoSpecification = string.Join(",", GivenMethod),
                        paymentMethods = string.Join(",", GivenPayment),
                        serviceArea = item.serviceArea != null ? item.serviceArea : string.Empty,
                        brokerOptions = item.brokerOptions != null ? item.brokerOptions : string.Empty,
                        ethnicIdentification = Identification,
                        ExportVehicle = vehicles != null && vehicles.Count > 0 ? vehicles[0].vehicltype.vehicleName : string.Empty, //VehicleType != null && VehicleType != "" ? VehicleType.Remove(VehicleType.Length - 1, 1) : string.Empty,
                        FleetVehicle = vehicles != null && vehicles.Count > 0 ? vehicles[0].numberOfVehicle : string.Empty,
                        ExportTrailer = trailers != null && trailers.Count > 0 ? trailers[0].trailer.trailerName : string.Empty,//TrailerType != null && TrailerType != "" ? TrailerType.Remove(TrailerType.Length - 1, 1) : string.Empty,
                        ExportTrailerCount = trailers != null && trailers.Count > 0 ? trailers[0].numberOfVehicle : string.Empty,
                        CarrierDocumentUrl = authorizedpaths != null && authorizedpaths.Count > 0 ? authorizedpaths[0].documentPath.Replace(@"/\", @"//") : string.Empty,
                        CarrierDocument = authorizedpaths != null && authorizedpaths.Count > 0 ? authorizedpaths[0].selectedOptions.Replace("_txt", "Other-") : string.Empty
                    });

                    int max = 0; //vehicles.Count > trailers.Count ? vehicles.Count : trailers.Count;

                    if (vehicles != null && trailers != null && vehicles.Count > trailers.Count)
                    {
                        if (authorizedpaths != null)
                        {
                            if (vehicles.Count > authorizedpaths.Count)
                            {
                                max = vehicles.Count;
                            }
                            else
                            {
                                max = authorizedpaths.Count;
                            }
                        }
                        else
                        {
                            max = vehicles.Count;
                        }
                    }
                    else if (vehicles != null && trailers != null && trailers.Count > authorizedpaths.Count)
                    {
                        if (authorizedpaths != null)
                        {
                            if (trailers.Count > authorizedpaths.Count)
                            {
                                max = trailers.Count;
                            }
                            else
                            {
                                max = authorizedpaths.Count;
                            }
                        }
                        else
                        {
                            max = trailers.Count;
                        }
                        max = trailers.Count;
                    }
                    else if (vehicles != null && trailers == null && authorizedpaths == null && vehicles.Count > 0)
                    {
                        max = vehicles.Count;
                    }
                    else if (trailers != null && vehicles == null && authorizedpaths == null && trailers.Count > 0)
                    {
                        max = trailers.Count;
                    }
                    else
                    {
                        max = authorizedpaths.Count;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        bool isTrailer = i < trailers.Count;
                        bool isVehicle = i < vehicles.Count;
                        bool isPath = i < authorizedpaths.Count;
                        if (isTrailer || isVehicle || isPath)
                        {
                            carrierViewModels.Add(new CarrierViewModel()
                            {
                                ExportVehicle = isVehicle && vehicles[i] != null ? vehicles[i].vehicltype.vehicleName : string.Empty,
                                FleetVehicle = isVehicle && vehicles[i] != null ? vehicles[i].numberOfVehicle : string.Empty,
                                ExportTrailer = isTrailer && trailers[i] != null ? trailers[i].trailer.trailerName : string.Empty,
                                ExportTrailerCount = isTrailer && trailers[i] != null ? trailers[i].numberOfVehicle : string.Empty,
                                CarrierDocumentUrl = isPath && authorizedpaths[i] != null ? authorizedpaths[i].documentPath.Replace(@"/\", @"//") : string.Empty,
                                CarrierDocument = isPath && authorizedpaths[i] != null ? authorizedpaths[i].selectedOptions.Replace("_txt", "Other-") : string.Empty,
                            });
                        }
                    }
                }
                MemoryStream memory = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(memory))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Carrier");
                    worksheet.Cells.LoadFromCollection(carrierViewModels, true);
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    package.Save();

                }
                memory.Position = 0;
                string FileName = $"Carrier{DateTime.Now.ToString("MMddyyyy hh:mm tt")}.xlsx";
                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName);
            }


            catch (Exception ex)
            {
                return RedirectToAction("managecarriers");

            }
        }

        // GET: Carrier/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Carrier/SignIn
        public IActionResult SignIn()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([Bind("name,email,password,confirmPassword")] cuusers objcuusers)
        {
            ModelState.Remove("confirmPassword");
            if (ModelState.IsValid)
            {

                cuusers objcuuser = await _carrierService.GetCuuserByEmailId(objcuusers.email);
                if (objcuuser != null && objcuuser.userId != null && objcuuser.userId != 0)
                {
                    byte[] salt = System.Convert.FromBase64String(objcuuser.passwordSalt);
                    byte[] hash = HashPassword(objcuusers.password, salt);
                    objcuusers.password = Convert.ToBase64String(hash); ;
                    cuusers objuser = await _carrierService.IsValidCarrierUserDetails(objcuusers);
                    if (objuser != null && objuser.userId != null && objuser.userId != 0)
                    {
                        HttpContext.Session.SetString("UserID", objuser.userId.ToString());
                        HttpContext.Session.SetString("CuID", objuser.cuId.ToString());
                        string Name = objuser.name.Length <= 20 ? objuser.name : objuser.name.Substring(0, 20) + "...";
                        HttpContext.Session.SetString("UserName", Name);
                        HttpContext.Session.SetString("UserFullName", objuser.name);
                        HttpContext.Session.SetString("UserEmail", objuser.email);
                        //HttpContext.Session.SetString("UserType", Convert.ToString(User.userType));
                        if (objuser.isFirstTime == null || objuser.isFirstTime == true)
                        {
                            return RedirectToAction("ChangePassword", "Carrier");
                        }
                        else
                        {
                            if (CarrierUserAuth.EmailTwoFactAuthRequired == "Y")
                            {
                                return RedirectToAction("AuthenticateCarrierUser", "Carrier");
                            }
                            else
                            {
                                return RedirectToAction("CarrierHome", "Carrier");
                            }
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("email", "Invalid Credentials");
                    }
                }
                else
                {
                    ModelState.AddModelError("email", "Please enter valid email and password");
                }


            }
            // return View(signIn);
            return View(objcuusers);
        }

        public async Task<ActionResult> ChangePassword()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }

            bool isActive = await _carrierService.IsCuuserActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ChangePassword");
            if (!isActive)
            {
                return RedirectToAction("Signin", "Carrier");
            }
            var User = await _carrierService.GetCuuser(Convert.ToInt32(HttpContext.Session.GetString("UserID")));
            return View(User);
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword([Bind("userId,password,confirmPassword")] cuusers users)
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }

            bool isActive = await _carrierService.IsCuuserActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ChangePassword");
            if (!isActive)
            {
                return RedirectToAction("Signin", "Carrier");
            }

            var User = await _carrierService.GetCuuser(Convert.ToInt32(HttpContext.Session.GetString("UserID")));

            if (User != null && User.userId != null && User.userId != 0)
            {
                HttpContext.Session.SetString("UserID", User.userId.ToString());
                HttpContext.Session.SetString("CuID", User.cuId.ToString());
                string Name = User.name.Length <= 20 ? User.name : User.name.Substring(0, 20) + "...";
                HttpContext.Session.SetString("UserName", Name);
                HttpContext.Session.SetString("UserFullName", User.name);
                HttpContext.Session.SetString("UserEmail", User.email);
            }

            byte[] salt = System.Convert.FromBase64String(User.passwordSalt);
            byte[] hash = HashPassword(users.password, salt);
            User.password = Convert.ToBase64String(hash);
            User.isFirstTime = false;
            await _carrierService.EditCuuser(User, HttpContext.Session.GetString("UserID"));
            //return RedirectToAction("AuthenticateCarrierUser", "Carrier");

            if (CarrierUserAuth.EmailTwoFactAuthRequired == "Y")
            {
                return RedirectToAction("AuthenticateCarrierUser", "Carrier");
            }
            else
            {
                return RedirectToAction("CarrierHome", "Carrier");
            }
        }

        public async Task<ActionResult> SetPassword(string AuthToken)
        {

            OtpLogs objOtpLogs = await _carrierService.GetOtpLogsTokenDetails(AuthToken);
            cuusers objcuusers = new cuusers();
            if (objOtpLogs != null && objOtpLogs.UserId != null && objOtpLogs.UserId != 0)
            {
                objcuusers = await _carrierService.GetCuuserByCuId(Convert.ToInt32(objOtpLogs.UserId));

                if (objcuusers != null && objcuusers.userId != null && objcuusers.userId != 0)
                {
                    HttpContext.Session.SetString("UserID", objcuusers.userId.ToString());
                    if (objcuusers.isFirstTime == false)
                    {
                        objcuusers.userId = 0;
                        ModelState.AddModelError("password", "This link is already used and expired now.");
                    }
                }
                else
                {
                    objcuusers = new cuusers();
                    objcuusers.userId = 0;
                    ModelState.AddModelError("password", "Invalid Link");
                }
            }
            else
            {
                ModelState.AddModelError("password", "Invalid Link");
                objcuusers.userId = 0;
            }

            return View("~/Views/Carrier/ChangePassword.cshtml", objcuusers);

        }

        // GET: Carrier/AuthenticateCarrierUser
        public async Task<ActionResult> AuthenticateCarrierUser()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            bool isActive = await _carrierService.IsCuuserActive(Convert.ToInt32(HttpContext.Session.GetString("UserID")), "Carrier/ChangePassword");
            if (!isActive)
            {
                return RedirectToAction("Signin", "Carrier");
            }

            var User = await _carrierService.GetCuuser(Convert.ToInt32(HttpContext.Session.GetString("UserID")));
            int otp = Convert.ToInt32(await _carrierService.GenerateNumericOTP(Convert.ToInt32(CarrierUserAuth.OtpLength)));
            OtpLogs objOtpLogs = new OtpLogs();
            Guid AuthToken = Guid.NewGuid();
            objOtpLogs.UserId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            objOtpLogs.OTP = otp;
            objOtpLogs.Token = AuthToken.ToString();
            objOtpLogs.EmailId = HttpContext.Session.GetString("UserEmail");
            objOtpLogs.CreatedDate = DateTime.Now;
            objOtpLogs.ExpiryDateTime = DateTime.Now.AddMinutes(CarrierUserAuth.OtpexpiryInMin);
            objOtpLogs.IsActive = true;
            //  await _carrierService.inser(User, HttpContext.Session.GetString("UserID"));
            await _carrierService.InsertOtp(objOtpLogs);
            string strBody = "Your Sign In One Time Password is : " + objOtpLogs.OTP + ", Will be valid for " + CarrierUserAuth.OtpexpiryInMin + " Minutes.";
            await SendEmail(User.email, "Carrier User Authenticate", strBody);
            TwoFactorAuth objTwoFactorAuth = new TwoFactorAuth();
            objTwoFactorAuth.UserId = objOtpLogs.UserId;
            objTwoFactorAuth.Token = objOtpLogs.Token;
            objTwoFactorAuth.ActualOtp = objOtpLogs.OTP;
            objTwoFactorAuth.OtpExpired = false;
            objTwoFactorAuth.OtpResend = false;
            ModelState.AddModelError("Otp", "Please Enter OTP Received on Email");
            return View(objTwoFactorAuth);
        }


        [HttpPost]
        public async Task<string> SendEmail(string email, string strSubject, string strBody)
        {
            try
            {
                SmtpClient client = new SmtpClient(MailDetails.SmtpServer, MailDetails.Port);
                client.Credentials = new NetworkCredential(MailDetails.UserName, MailDetails.Password);
                client.EnableSsl = true;
                MailMessage message = new MailMessage();
                message.From = new MailAddress(MailDetails.Sender);
                message.To.Add(new MailAddress(email));
                message.Subject = strSubject;
                message.IsBodyHtml = true;
                message.Body = strBody;
                await client.SendMailAsync(message);
                return true.ToString();
            }
            catch (Exception ex)
            {

                _jWLDBContext.errortracelog.Add(new errortracelog()
                {
                    error = ex.ToString(),
                    errorControl = "CarrierController/SendEmail",
                    errorMessage = ex.Message,
                    errorName = ex.Source,
                    errorStack = ex.StackTrace,
                    errorTime = DateTime.Now
                });
                await _jWLDBContext.SaveChangesAsync();
                return false.ToString();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateOtp([Bind("Otp,Token")] TwoFactorAuth objtwoFactorAuth)
        {
            ModelState.Remove("ActualOtp");
            ModelState.Remove("OtpResend");
            ModelState.Remove("UserId");
            ModelState.Remove("ActualOtp");

            if (ModelState.IsValid)
            {
                OtpLogs objOtpLogs = await _carrierService.GetOtpLogsDetails(Convert.ToInt32(objtwoFactorAuth.Otp), objtwoFactorAuth.Token);
                if (objOtpLogs != null && objOtpLogs.UserId != null && objOtpLogs.UserId != 0)
                {

                    if (objOtpLogs.ExpiryDateTime >= DateTime.Now)
                    {
                        return RedirectToAction("CarrierHome", "Carrier");
                    }
                    else
                    {
                        ModelState.AddModelError("Otp", "Entered Otp is expired, Please Resend Otp.");
                        objtwoFactorAuth.OtpExpired = true;
                        objtwoFactorAuth.OtpResend = true;
                    }
                }
                else
                {
                    ModelState.AddModelError("Otp", "Invalid Otp, Please Enter OTP Received on Email");
                }
            }
            // return View(signIn);
            return View("~/Views/Carrier/AuthenticateCarrierUser.cshtml", objtwoFactorAuth);
        }

        private static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2id.Salt = salt;
            // argon2id.DegreeOfParallelism = DEGREE_OF_PARALLELISM;
            // argon2id.Iterations = NUMBER_OF_ITERATIONS;
            // argon2id.MemorySize = MEMORY_TO_USE_IN_KB;

            // No. of CPU Cores x 2.
            // private const int DEGREE_OF_PARALLELISM = 16;

            // Recommended minimum value.
            //  private const int NUMBER_OF_ITERATIONS = 4;

            // 600 MB.
            //  private const int MEMORY_TO_USE_IN_KB = 600000;

            argon2id.DegreeOfParallelism = 8; // four cores
            argon2id.Iterations = 4;
            argon2id.MemorySize = 600000; //     1024 * 1024; // 1 GB
            return argon2id.GetBytes(16);
        }

        private static bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash);
        }

        // GET: Carrier/CarrierHome
        public IActionResult CarrierHome()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            if (HttpContext.Session.GetString("CuID") == null || HttpContext.Session.GetString("CuID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }

            int CuId = Convert.ToInt32(HttpContext.Session.GetString("CuID"));
            carrierDashBoard objcarrierDashBoard = new carrierDashBoard();
            //  List<carrierKPISummary> carrierKPISummaryList = new List<carrierKPISummary>();
            //   List<carrierKPIDetails> carrierKPIDetailsList = new List<carrierKPIDetails>();

            string constr = configuration.GetConnectionString("DefaultConnection");

            DataSet ds = new DataSet();

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("USP_S_CarrierDashBoard", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CuId", CuId);
                    using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                objcarrierDashBoard.cuId = Convert.ToInt32(ds.Tables[0].Rows[0]["Cu_Id"]);
                ViewBag.CarrierName = Convert.ToString(ds.Tables[0].Rows[0]["Carrier_Name"]);
                objcarrierDashBoard.WeekNumber = Convert.ToString(ds.Tables[0].Rows[0]["Week_Number"]);
                HttpContext.Session.SetString("RecentWeekNumber", objcarrierDashBoard.WeekNumber);
                objcarrierDashBoard.CarrierName = Convert.ToString(ds.Tables[0].Rows[0]["Carrier_Name"]);
                objcarrierDashBoard.DeliveryScanPercentage = Convert.ToString(ds.Tables[0].Rows[0]["Delivery_Scan_Percentage"]);
                objcarrierDashBoard.LoadScanPercentage = Convert.ToString(ds.Tables[0].Rows[0]["Load_Scan_Percentage"]);
                objcarrierDashBoard.PhotoCompliancePercentage = Convert.ToString(ds.Tables[0].Rows[0]["Photo_Compliance_Percentage"]);
                objcarrierDashBoard.GPSCompliancePercentage = Convert.ToString(ds.Tables[0].Rows[0]["GPSCompliance_Percentage"]);
            }
            // objcarrierKPI.carrierKPISummary = carrierKPISummaryList;
            // objcarrierKPI.carrierKPIDetails = carrierKPIDetailsList;
            return View(objcarrierDashBoard);
            //return View();
        }
        // GET: Carrier/ForgotPassword
        public IActionResult ForgotPassword()
        {
            ForgotPassword objForgotPassword = new ForgotPassword();
            objForgotPassword.OtpSent = false;
            ModelState.AddModelError("Email", "Please Enter Your Email");
            objForgotPassword.Email = "";
            objForgotPassword.OtpExpired = false;
            objForgotPassword.OtpResend = false;
            return View(objForgotPassword);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword([Bind("Email,Otp,Token,OtpSent,OtpExpired,OtpResend")] ForgotPassword objForgotPassword)
        {

            if (objForgotPassword.Email != null)
            {
                var User = await _carrierService.GetCuuserByEmailId(objForgotPassword.Email);

                if (User != null && User.userId != null && User.userId != 0)
                {
                    string ResendOtp = Request.Form["ResendOtp"];
                    string Token = Request.Form["AuthToken"];


                    if (objForgotPassword.Otp != null)
                    {

                        OtpLogs objOtpLogs = await _carrierService.GetOtpLogsDetails(Convert.ToInt32(objForgotPassword.Otp), Token);
                        if (objOtpLogs != null && objOtpLogs.UserId != null && objOtpLogs.UserId != 0)
                        {

                            if (objOtpLogs.ExpiryDateTime > DateTime.Now)
                            {
                                int otp = Convert.ToInt32(await _carrierService.GenerateNumericOTP(Convert.ToInt32(CarrierUserAuth.OtpLength)));
                                objOtpLogs = new OtpLogs();
                                Guid AuthToken = Guid.NewGuid();
                                objOtpLogs.UserId = Convert.ToInt32(User.userId);
                                objOtpLogs.OTP = otp;
                                objOtpLogs.Token = AuthToken.ToString();
                                objOtpLogs.EmailId = User.email;
                                objOtpLogs.CreatedDate = DateTime.Now;
                                objOtpLogs.ExpiryDateTime = DateTime.Now.AddMinutes(CarrierUserAuth.TokenexpiryInMin);
                                objOtpLogs.IsActive = true;
                                //  await _carrierService.inser(User, HttpContext.Session.GetString("UserID"));
                                await _carrierService.InsertOtp(objOtpLogs);

                                // Send Reset Password link 
                                string strBody = "Please click on the below link to Reset your Password " + System.Environment.NewLine + ", Will be valid for " + CarrierUserAuth.TokenexpiryInMin + " Minutes.";
                                strBody = strBody + System.Environment.NewLine + this.Request.Scheme + @"://" + this.Request.Host + this.Request.PathBase + @"/Carrier/ResetPassword?AuthToken=" + AuthToken.ToString();

                                await SendEmail(objForgotPassword.Email, "Carrier User Reset Password", strBody);
                                ModelState.AddModelError("Email", "Please Reset your Password by the link send to your email");
                                objForgotPassword.ResetPassLinkSent = true;
                            }
                            else
                            {

                                objForgotPassword.OtpExpired = true;
                                objForgotPassword.OtpResend = true;
                                objForgotPassword.OtpSent = false;
                                objForgotPassword.Otp = "";
                                objForgotPassword.Token = Token;
                                ModelState.AddModelError("Email", "Entered Otp is expired, Please Resend Otp.");

                            }
                        }
                        else
                        {

                            objForgotPassword.OtpExpired = false;
                            objForgotPassword.OtpResend = false;
                            objForgotPassword.OtpSent = true;
                            ModelState.AddModelError("Otp", "Invalid Otp, Please Enter OTP Received on Email");
                            objForgotPassword.OtpSent = true;
                            objForgotPassword.Token = Token;
                        }

                    }
                    else
                    {
                        int otp = Convert.ToInt32(await _carrierService.GenerateNumericOTP(Convert.ToInt32(CarrierUserAuth.OtpLength)));
                        OtpLogs objOtpLogs = new OtpLogs();
                        Guid AuthToken = Guid.NewGuid();
                        objOtpLogs.UserId = Convert.ToInt32(User.userId);
                        objOtpLogs.OTP = otp;
                        objOtpLogs.Token = AuthToken.ToString();
                        objOtpLogs.EmailId = User.email;
                        objOtpLogs.CreatedDate = DateTime.Now;
                        objOtpLogs.ExpiryDateTime = DateTime.Now.AddMinutes(CarrierUserAuth.OtpexpiryInMin);
                        objOtpLogs.IsActive = true;
                        //  await _carrierService.inser(User, HttpContext.Session.GetString("UserID"));
                        await _carrierService.InsertOtp(objOtpLogs);

                        string strBody = "Your Forgot Password One Time Password is : " + objOtpLogs.OTP + ", Will be valid for " + CarrierUserAuth.OtpexpiryInMin + " Minutes.";

                        await SendEmail(User.email, "Carrier User Authenticate", strBody);


                        objForgotPassword.UserId = objOtpLogs.UserId;
                        objForgotPassword.Token = objOtpLogs.Token;
                        objForgotPassword.OtpExpired = false;
                        objForgotPassword.OtpResend = false;
                        objForgotPassword.OtpSent = true;
                        ModelState.AddModelError("Otp", "Please Enter OTP Received on Email");

                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "Please Enter valid email registed with us");
                    objForgotPassword.OtpSent = false;

                }
            }
            else
            {
                ModelState.AddModelError("Email", "Please Enter email registed with us");
                objForgotPassword.OtpSent = false;
            }

            return View("~/Views/Carrier/ForgotPassword.cshtml", objForgotPassword);
            //return View(objForgotPassword);
        }

        public async Task<ActionResult> ResetPassword(string AuthToken)
        {

            OtpLogs objOtpLogs = await _carrierService.GetOtpLogsTokenDetails(AuthToken);
            cuusers objcuusers = new cuusers();
            if (objOtpLogs != null && objOtpLogs.UserId != null && objOtpLogs.UserId != 0)
            {
                objcuusers = await _carrierService.GetCuuser(Convert.ToInt32(objOtpLogs.UserId));

                if (objcuusers != null && objcuusers.userId != null && objcuusers.userId != 0)
                {
                    HttpContext.Session.SetString("UserID", objcuusers.userId.ToString());
                    if (DateTime.Now > objOtpLogs.ExpiryDateTime)
                    {
                        objcuusers.userId = 0;
                        ModelState.AddModelError("password", "This link is expired.");
                    }
                }
                else
                {
                    objcuusers = new cuusers();
                    objcuusers.userId = 0;
                    ModelState.AddModelError("password", "Invalid Link");
                }
            }
            else
            {
                ModelState.AddModelError("password", "Invalid Link");
                objcuusers.userId = 0;
            }
            return View("~/Views/Carrier/ChangePassword.cshtml", objcuusers);
        }

        public IActionResult CarrierKPI()
        {
            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            if (HttpContext.Session.GetString("CuID") == null || HttpContext.Session.GetString("CuID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }

            int CuId = Convert.ToInt32(HttpContext.Session.GetString("CuID"));
            carrierKPI objcarrierKPI = new carrierKPI();
            List<carrierKPISummary> carrierKPISummaryList = new List<carrierKPISummary>();
            List<carrierKPIDetails> carrierKPIDetailsList = new List<carrierKPIDetails>();

            string constr = configuration.GetConnectionString("DefaultConnection");

            DataSet ds = new DataSet();

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("USP_S_CarrierKPIS", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CuId", CuId);
                    using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                objcarrierKPI.cuId = Convert.ToInt32(ds.Tables[0].Rows[0]["Cu_Id"]);
                ViewBag.CarrierName = Convert.ToString(ds.Tables[0].Rows[0]["Carrier_Name"]);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    carrierKPISummaryList.Add(new carrierKPISummary
                    {
                        cuId = Convert.ToInt32(dr["Cu_Id"]),
                        WeekNumber = Convert.ToString(dr["WEEK_NUMBER"]),
                        Carrier = Convert.ToString(dr["CARRIER"]),
                        WeeklyCount = Convert.ToInt32(dr["Weekly_Count"]),
                        ExpectedPieces = Convert.ToInt32(dr["Expected_Pieces"]),
                        LoadScanCount = Convert.ToInt32(dr["Load_Scan_Count"]),
                        DeliveryScanCount = Convert.ToInt32(dr["Delivery_Scan_Count"]),
                        PhotoComplianceCount = Convert.ToInt32(dr["Photo_Compliance_Count"]),
                        GPSComplianceCount = Convert.ToInt32(dr["GPSCompliance_Count"]),
                        DeliveryScanPercentage = Convert.ToString(dr["Delivery_Scan_Percentage"]),
                        LoadScanPercentage = Convert.ToString(dr["Load_Scan_Percentage"]),
                        PhotoCompliancePercentage = Convert.ToString(dr["Photo_Compliance_Percentage"]),
                        GPSCompliancePercentage = Convert.ToString(dr["GPSCompliance_Percentage"]),
                        LoadScanBonusOrDeduction = Convert.ToString(dr["Load_Scan_Bonus_Or_Deduction"]),
                        DeliveryScanBonusOrDeduction = Convert.ToString(dr["Delivery_Scan_Bonus_Or_Deduction"]),
                        PhotoBonusOrDeduction = Convert.ToString(dr["Photo_Bonus_Or_Deduction"]),
                        GPSComplianceBonusOrDeduction = Convert.ToString(dr["GPSCompliance_Bonus_Or_Deduction"]),
                        TotalBonusOrDeduction = Convert.ToString(dr["Total_Bonus_Or_Deduction"]),
                        TotalBonusOrDeductionToBeApplied = Convert.ToString(dr["Total_Bonus_Or_Deduction_To_Be_Applied"]),

                    });
                }
            }
            objcarrierKPI.carrierKPISummary = carrierKPISummaryList;
            objcarrierKPI.carrierKPIDetails = carrierKPIDetailsList;
            return View(objcarrierKPI);
        }

        public ActionResult CarrierKPIDetails(string param)
        {

            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            if (HttpContext.Session.GetString("CuID") == null || HttpContext.Session.GetString("CuID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            int CuId = Convert.ToInt32(HttpContext.Session.GetString("CuID"));
            if (param == null)
            {
                param = HttpContext.Session.GetString("RecentWeekNumber");
            }
            ViewBag.RecentWeekNumber = param;
            ViewData["WeekNumbers"] = new SelectList(GetWeekNumberList(CuId), "WeekNumber", "WeekNumber");
            return View();

            //return View("~/Views/Carrier/CarrierKPIDetails.cshtml");
        }

        public ActionResult ViewCarrierKPIDetails(string param)
        {

            if (HttpContext.Session.GetString("UserID") == null || HttpContext.Session.GetString("UserID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            if (HttpContext.Session.GetString("CuID") == null || HttpContext.Session.GetString("CuID") == "")
            {
                return RedirectToAction("Signin", "Carrier");
            }
            if (param == null)
            {
                param = HttpContext.Session.GetString("RecentWeekNumber");
            }

            int CuId = Convert.ToInt32(HttpContext.Session.GetString("CuID"));

            carrierKPI objcarrierKPI = new carrierKPI();
            List<carrierKPISummary> carrierKPISummaryList = new List<carrierKPISummary>();
            List<carrierKPIDetails> carrierKPIDetailsList = new List<carrierKPIDetails>();

            // List<carrierKPIDetails> carrierKPIDetailsList = new List<carrierKPIDetails>();

            string constr = configuration.GetConnectionString("DefaultConnection");

            DataSet ds = new DataSet();

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("USP_S_CarrierKPIS", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CuId", CuId);
                    cmd.Parameters.AddWithValue("@WeekNumber", param);
                    using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                objcarrierKPI.cuId = Convert.ToInt32(ds.Tables[0].Rows[0]["Cu_Id"]);
                ViewBag.CarrierName = Convert.ToString(ds.Tables[0].Rows[0]["Carrier_Name"]);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    carrierKPISummaryList.Add(new carrierKPISummary
                    {
                        cuId = Convert.ToInt32(dr["Cu_Id"]),
                        WeekNumber = Convert.ToString(dr["WEEK_NUMBER"]),
                        Carrier = Convert.ToString(dr["CARRIER"]),
                        WeeklyCount = Convert.ToInt32(dr["Weekly_Count"]),
                        ExpectedPieces = Convert.ToInt32(dr["Expected_Pieces"]),
                        LoadScanCount = Convert.ToInt32(dr["Load_Scan_Count"]),
                        DeliveryScanCount = Convert.ToInt32(dr["Delivery_Scan_Count"]),
                        PhotoComplianceCount = Convert.ToInt32(dr["Photo_Compliance_Count"]),
                        GPSComplianceCount = Convert.ToInt32(dr["GPSCompliance_Count"]),
                        DeliveryScanPercentage = Convert.ToString(dr["Delivery_Scan_Percentage"]),
                        LoadScanPercentage = Convert.ToString(dr["Load_Scan_Percentage"]),
                        PhotoCompliancePercentage = Convert.ToString(dr["Photo_Compliance_Percentage"]),
                        GPSCompliancePercentage = Convert.ToString(dr["GPSCompliance_Percentage"]),
                        LoadScanBonusOrDeduction = Convert.ToString(dr["Load_Scan_Bonus_Or_Deduction"]),
                        DeliveryScanBonusOrDeduction = Convert.ToString(dr["Delivery_Scan_Bonus_Or_Deduction"]),
                        PhotoBonusOrDeduction = Convert.ToString(dr["Photo_Bonus_Or_Deduction"]),
                        GPSComplianceBonusOrDeduction = Convert.ToString(dr["GPSCompliance_Bonus_Or_Deduction"]),
                        TotalBonusOrDeduction = Convert.ToString(dr["Total_Bonus_Or_Deduction"]),
                        TotalBonusOrDeductionToBeApplied = Convert.ToString(dr["Total_Bonus_Or_Deduction_To_Be_Applied"]),
                    });
                }
            }
            if (ds != null && ds.Tables[1].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    carrierKPIDetailsList.Add(new carrierKPIDetails
                    {
                        cuId = Convert.ToInt32(dr["Cu_Id"]),
                        WeekNumber = Convert.ToString(dr["WEEK_NUMBER"]),
                        Carrier = Convert.ToString(dr["CARRIER"]),
                        CompanyNumber = Convert.ToInt32(dr["COMPANY_NUMBER"]),
                        CustomerNumber = Convert.ToInt32(dr["CUSTOMER_NUMBER"]),
                        Market = Convert.ToString(dr["MARKET"]),
                        Region = Convert.ToString(dr["REGION"]),
                        UniqueIdNo = Convert.ToInt32(dr["UNIQUE_ID_NO"]),
                        LoadId = Convert.ToString(dr["LOAD_ID"]),
                        BranchId = Convert.ToString(dr["BRANCH_ID"]),
                        RouteCode = Convert.ToString(dr["ROUTE_CODE"]),
                        RouteDate = Convert.ToString(dr["ROUTE_DATE"]),
                        StopExpectedPieces = Convert.ToInt32(dr["STOP_EXPECTED_PIECES"]),
                        ReceivingScanDateTime = Convert.ToString(dr["RECEIVING_SCAN_DATETIME"]),
                        ReceivingScanPieces = Convert.ToInt32(dr["RECEIVING_SCAN_PIECES"]),
                        LoadingScanDateTime = Convert.ToString(dr["LOADING_SCAN_DATETIME"]),
                        LoadScanCount = Convert.ToInt32(dr["Load_Scan_Count"]),
                        DeliveryScanDateTime = Convert.ToString(dr["DELIVERY_SCAN_DATETIME"]),
                        DeliveryScanCount = Convert.ToInt32(dr["Delivery_Scan_Count"]),
                        DriverNumber = Convert.ToInt32(dr["DRIVER_NUMBER"]),
                        PhotosExist = Convert.ToString(dr["PHOTOS_EXIST"]),
                        PhotosDeliveryDate = Convert.ToString(dr["PHOTO_DELIVERY_DATE"]),
                        Latitude = Convert.ToString(dr["LATITUDE"]),
                        Longitude = Convert.ToString(dr["LONGITUDE"]),
                    });
                }
            }

            objcarrierKPI.carrierKPISummary = carrierKPISummaryList;
            objcarrierKPI.carrierKPIDetails = carrierKPIDetailsList;
            return PartialView(objcarrierKPI);
            //  return PartialView(carrierKPIDetailsList);

        }
        public List<weeklist> GetWeekNumberList(int CuId)
        {
            List<weeklist> weeklist = new List<weeklist>();

            DataSet ds = new DataSet();
            string constr = configuration.GetConnectionString("DefaultConnection");
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("USP_S_WeekNumberByCuId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CuId", CuId);
                    using (MySqlDataAdapter sda = new MySqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }

            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    weeklist _list = new weeklist();
                    _list.CuId = Convert.ToInt32(dr["Cu_Id"]);
                    _list.WeekNumber = Convert.ToString(dr["Week_Number"]);
                    weeklist.Add(_list);
                }
            }
            return weeklist;
        }

        [HttpPost]
        public async Task<JsonResult> CreateCarrierInDynamicsAndDataTrac(carrierusers carrierusers)
        {

            amazon_equipment_owner_Request objrequestdetails = new amazon_equipment_owner_Request();
            amazon_equipment_owner objrequest = new amazon_equipment_owner();

            objrequest.owner_id = carrierusers.dtuid;
            objrequest.scac_code = Convert.ToString(carrierusers.additionalScac);
            objrequest.carrier_type = carrierusers.carriertype;
            objrequest.owner_name = carrierusers.authorizedPerson;
            objrequest.key_id = carrierusers.cuId;

            if (carrierusers.status == StatusEnum.Inprocess)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            }
            else if (carrierusers.status == StatusEnum.Complete)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            }
            else if (carrierusers.status == StatusEnum.Approved)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.APPD);
            }
            else if (carrierusers.status == StatusEnum.Rejected)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.TERM);
            }
            else if (carrierusers.status == StatusEnum.Onhold)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.HOLD);
            }

            objrequest.ascend_vendor_id = null;
            // objrequest.dx_vendor_id = Convert.ToString(carrierusers.dx_vendor_id);

            // for testing 
            objrequest.dx_vendor_id = Convert.ToString(carrierusers.cuId);

            objrequest.check_name = carrierusers.nametoprintoncheck;


            objrequestdetails.amazon_equipment_owner = objrequest;
            HttpResponseMessage responseMessage = await _datatracService.amazon_equipment_owner_Post(objrequestdetails);

            //  var datatrackresponse = await _datatracService.amazon_equipment_owner_Post(objrequest);
            if (responseMessage.ReasonPhrase == "OK")
            {
                string resp = await responseMessage.Content.ReadAsStringAsync();
                return Json(true);
            }
            return Json(false);

        }


        [HttpPost]
        public async Task<JsonResult> UpdateCarrierInDynamicsAndDataTrac(carrierusers carrierusers)
        {
            // amazon_equipment_owner_Request objrequest = new amazon_equipment_owner_Request();
            amazon_equipment_owner objrequest = new amazon_equipment_owner();

            if (carrierusers.status == StatusEnum.Inprocess)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            }
            else if (carrierusers.status == StatusEnum.Complete)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            }
            else if (carrierusers.status == StatusEnum.Approved)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.APPD);
            }
            else if (carrierusers.status == StatusEnum.Rejected)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.TERM);
            }
            else if (carrierusers.status == StatusEnum.Onhold)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.HOLD);
            }

            string amazon_equipment_ownerrequest = null;
            amazon_equipment_ownerrequest = @"'carrier_status': '" + objrequest.carrier_status + "',";
            amazon_equipment_ownerrequest = amazon_equipment_ownerrequest + @"'dx_vendor_id': '" + carrierusers.dx_vendor_id + "'";
            amazon_equipment_ownerrequest = @"{" + amazon_equipment_ownerrequest + "}";
            string amazon_equipment_owner_RequestObject = @"{'amazon_equipment_owner': " + amazon_equipment_ownerrequest + "}";

            HttpResponseMessage responseMessage = await _datatracService.amazon_equipment_owner_Put(carrierusers.dtuid, amazon_equipment_owner_RequestObject);
            if (responseMessage.ReasonPhrase == "OK")
            {
                string resp = await responseMessage.Content.ReadAsStringAsync();
                return Json(true);
            }
            return Json(false);
        }


        [HttpPost]
        public async Task<JsonResult> UpdateCarrierInDataTrac(carrierusers carrierusers)
        {
            try
            {
                string amazon_equipment_ownerrequest = null;
                amazon_equipment_ownerrequest = @"'dx_vendor_id': '" + carrierusers.dx_vendor_id + "'";
                //   amazon_equipment_ownerrequest = amazon_equipment_ownerrequest + @"'dx_vendor_id': '" + carrierusers.dx_vendor_id + "'";
                amazon_equipment_ownerrequest = @"{" + amazon_equipment_ownerrequest + "}";
                string amazon_equipment_owner_RequestObject = @"{'amazon_equipment_owner': " + amazon_equipment_ownerrequest + "}";

                HttpResponseMessage responseMessage = await _datatracService.amazon_equipment_owner_Put(carrierusers.dtuid, amazon_equipment_owner_RequestObject);
                if (responseMessage.ReasonPhrase == "OK")
                {
                    string resp = await responseMessage.Content.ReadAsStringAsync();
                    return Json(true);
                }
                return Json(false);
            }
            catch (Exception ex)
            {

                _jWLDBContext.errortracelog.Add(new errortracelog()
                {
                    error = ex.ToString(),
                    errorControl = "CarrierController/UpdateCarrierInDataTrac",
                    errorMessage = ex.Message,
                    errorName = ex.Source,
                    errorStack = ex.StackTrace,
                    errorTime = DateTime.Now
                });
                await _jWLDBContext.SaveChangesAsync();
                return Json(false);
            }
        }

        [HttpPost]
        public async Task<JsonResult> DataTracAPI_amazon_equipment_owner_Post(carrierusers carrierusers)
        {
            //amazon_equipment_owner_Request objrequest = new amazon_equipment_owner_Request();

            //if (carrierusers.brokerOptions == "Carrier")
            //{
            //    objrequest.carrier_type = Convert.ToString(CarrierTypeEnum.CARR);
            //}
            //else
            //{
            //    objrequest.carrier_type = Convert.ToString(CarrierTypeEnum.IC);
            //}

            //objrequest.owner_name = carrierusers.cuName;
            //objrequest.key_id = carrierusers.cuId;

            //if (carrierusers.status == StatusEnum.Inprocess)
            //{
            //    objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            //}
            //else if (carrierusers.status == StatusEnum.Complete)
            //{
            //    objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            //}
            //else if (carrierusers.status == StatusEnum.Approved)
            //{
            //    objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.APPD);
            //}
            //else if (carrierusers.status == StatusEnum.Rejected)
            //{
            //    objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.TERM);
            //}
            //else if (carrierusers.status == StatusEnum.Onhold)
            //{
            //    objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.HOLD);
            //}

            //objrequest.ascend_vendor_id = null;
            //objrequest.dx_vendor_id = null;
            //objrequest.check_name = null;

            //var datatrackresponse = await _datatracService.amazon_equipment_owner_Post(objrequest);
            //if (datatrackresponse.success)
            //{
            //    return Json(true);
            //}
            return Json(false);
        }
    }

}

