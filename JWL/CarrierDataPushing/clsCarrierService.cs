using Data;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CarrierDataPushing
{
    public class clsCarrierService : clsCommon
    {
        //  private readonly ClsJWLDBContext _jWLDBContext;
        //public clsCarrierService(JWLDBContext jWLDBContext)
        //{
        //    _jWLDBContext = jWLDBContext;
        //}
        //public JWLDBContext1(DbContextOptions<JWLDBContext> options)
        //{

        //}
        public ReturnResponse AddCarrierUser(carrierusers carrierUser, string UserID, string defaultPassword = "", string passwordSalt = "")
        {
            ReturnResponse objresponse = new ReturnResponse();
            try
            {
                using (var _jWLDBContext = new clsJWLDBContext())
                {
                    _jWLDBContext.Database.EnsureCreated();

                    UserID = UserID == null ? "0" : UserID;
                    carrierUser.CreatedDate = carrierUser.ModifiedDate = DateTime.Now;
                    var Added = _jWLDBContext.carrierusers.AddAsync(carrierUser);
                    _jWLDBContext.SaveChanges();
                    if (carrierUser.assignee == null)
                    {
                        carrierUser.modifiedBy = carrierUser.createdBy = carrierUser.cuId;
                        if (UserID == null || UserID == "0")
                        {
                            _jWLDBContext.carrierusers.Update(carrierUser);
                            _jWLDBContext.SaveChangesAsync();
                        }
                        else
                        {
                            _jWLDBContext.SaveChanges();
                        }
                    }


                    // To insert cuuser details 
                    if (carrierUser.createdBy != null && defaultPassword != "" && passwordSalt != "")
                    {
                        cuusers objcuusers = new cuusers();
                        objcuusers.cuId = carrierUser.cuId;
                        objcuusers.name = carrierUser.authorizedPerson;
                        objcuusers.email = carrierUser.cuEmail;
                        objcuusers.password = defaultPassword;
                        objcuusers.passwordSalt = passwordSalt;
                        objcuusers.isFirstTime = true;
                        objcuusers.isDeleted = false;
                        objcuusers.isActive = true;

                        _jWLDBContext.cuusers.AddAsync(objcuusers);
                        if (UserID == null || UserID == "0")
                        {
                            _jWLDBContext.SaveChangesAsync();
                        }
                        else
                        {
                            _jWLDBContext.SaveChanges();
                        }
                    }
                }
                objresponse.ResponseVal = true;
            }
            catch (Exception ex)
            {
                //using (var _jWLDBContext = new clsJWLDBContext())
                //{
                //    _jWLDBContext.Database.EnsureCreated();
                //    _jWLDBContext.errortracelog.Add(new errortracelog()
                //    {
                //        error = ex.ToString(),
                //        errorControl = "clsCarrierService/AddCarrierUser",
                //        errorMessage = ex.Message,
                //        errorName = ex.Source,
                //        errorStack = ex.StackTrace,
                //        errorTime = DateTime.Now
                //    });
                //    _jWLDBContext.SaveChangesAsync();
                //}
                WriteErrorLog(ex, "clsCarrierService/AddCarrierUser");
                objresponse.ResponseVal = false;
            }
            return objresponse;
        }

        public ReturnResponse UpdateCarrier(carrierusers carrierUser, string UserID)
        {
            ReturnResponse objresponse = new ReturnResponse();
            try
            {
                using (var _jWLDBContext = new clsJWLDBContext())
                {
                    carrierusers carrierusers = _jWLDBContext.carrierusers.Where(t => t.cuId == carrierUser.cuId).FirstOrDefault();
                    carrierusers.cuName = carrierUser.cuName;
                    carrierusers.cuEmail = carrierUser.cuEmail;
                    carrierusers.agreementDate = carrierUser.agreementDate;
                    // carrierusers.agreementDate = carrierUser.agreementDate;
                    carrierusers.MC = carrierUser.MC;
                    // carrierusers.MC = carrierUser.MC;
                    carrierusers.authorizedPerson = carrierUser.authorizedPerson;
                    carrierusers.title = carrierUser.title;
                    carrierusers.legalCompanyName = carrierUser.legalCompanyName;
                    carrierusers.DBA = carrierUser.DBA;
                    carrierusers.physicalAddress = carrierUser.physicalAddress;
                    carrierusers.city = carrierUser.city;
                    carrierusers.state = carrierUser.state;
                    carrierusers.zipcode = carrierUser.zipcode;
                    carrierusers.telephone = carrierUser.telephone;
                    carrierusers.fax = carrierUser.fax;
                    carrierusers.status = carrierUser.status;
                    carrierusers.factoryCompanyName = carrierUser.factoryCompanyName;
                    carrierusers.factoryContactName = carrierUser.factoryContactName;
                    carrierusers.factoryPhysicalAddress = carrierUser.factoryPhysicalAddress;
                    carrierusers.factoryCity = carrierUser.factoryCity;
                    carrierusers.factoryState = carrierUser.factoryState;
                    carrierusers.factoryZipCode = carrierUser.factoryZipCode;
                    carrierusers.factoryTelephone = carrierUser.factoryTelephone;
                    carrierusers.factoryFax = carrierUser.factoryFax;
                    carrierusers.additionalPersonName = carrierUser.additionalPersonName;
                    carrierusers.addtionalPersonTelephone = carrierUser.addtionalPersonTelephone;
                    carrierusers.addtionalAfterHoursPersonName = carrierUser.addtionalAfterHoursPersonName;
                    carrierusers.addtionalAfterHoursPersonTelephone = carrierUser.addtionalAfterHoursPersonTelephone;
                    carrierusers.addtionalDot = carrierUser.addtionalDot;
                    carrierusers.additionalScac = carrierUser.additionalScac;
                    carrierusers.additionalFedaralID = carrierUser.additionalFedaralID;
                    carrierusers.additionalHazmatCertified = carrierUser.additionalHazmatCertified;
                    carrierusers.additinalHazmatExpiryDate = carrierUser.additinalHazmatExpiryDate;
                    carrierusers.additionalPreferredLanes = carrierUser.additionalPreferredLanes;
                    carrierusers.additionalMajorMakets = carrierUser.additionalMajorMakets;
                    carrierusers.authorizedSignature = carrierUser.authorizedSignature;
                    carrierusers.authorizedSignaturePath = carrierUser.authorizedSignaturePath;
                    carrierusers.completedMBCA = carrierUser.completedMBCA;
                    carrierusers.reportSortKey = carrierUser.reportSortKey;
                    carrierusers.ndaReturned = carrierUser.ndaReturned;
                    carrierusers.onboardingCompleted = carrierUser.onboardingCompleted;
                    carrierusers.onboardingFileLink = carrierUser.onboardingFileLink;
                    carrierusers.insuranceType = carrierUser.insuranceType;
                    carrierusers.paymentTerms = carrierUser.paymentTerms;
                    carrierusers.majorMarkets = carrierUser.majorMarkets;
                    carrierusers.CreatedDate = carrierUser.CreatedDate;
                    carrierusers.createdBy = carrierUser.createdBy;
                    carrierusers.ModifiedDate = carrierUser.ModifiedDate;
                    carrierusers.twic = carrierUser.twic;
                    carrierusers.ca = carrierUser.ca;
                    carrierusers.isFemaleOwned = carrierUser.isFemaleOwned;
                    carrierusers.isVeteranOwned = carrierUser.isVeteranOwned;
                    carrierusers.modifiedBy = carrierUser.modifiedBy;
                    carrierusers.assignee = carrierUser.assignee;
                    carrierusers.isMinorityBusiness = carrierUser.isMinorityBusiness;
                    carrierusers.isDeleted = carrierUser.isDeleted;
                    carrierusers.businessYear = carrierUser.businessYear;
                    carrierusers.ethnicIdentification = carrierUser.ethnicIdentification;
                    carrierusers.milesPerYear = carrierUser.milesPerYear;
                    carrierusers.paymentMethods = carrierUser.paymentMethods;
                    carrierusers.CarrierwiseVehicle = carrierUser.CarrierwiseVehicle;
                    carrierusers.CarrierwiseTrailer = carrierUser.CarrierwiseTrailer;
                    carrierusers.CarrierDocument = carrierUser.CarrierDocument;
                    carrierusers.CreatedUserName = carrierUser.CreatedUserName;
                    carrierusers.serviceArea = carrierUser.serviceArea;
                    carrierusers.brokerOptions = carrierUser.brokerOptions;
                    carrierusers.createdByUserName = carrierUser.createdByUserName;
                    carrierusers.cargoSpecification = carrierUser.cargoSpecification;
                    carrierusers.Ins1MGeneral = carrierUser.Ins1MGeneral;
                    carrierusers.Ins1MAuto = carrierUser.Ins1MAuto;
                    carrierusers.Ins250KCargo = carrierUser.Ins250KCargo;
                    carrierusers.Ins100KCargo = carrierUser.Ins100KCargo;
                    carrierusers.ndaUrl = carrierUser.ndaUrl;
                    carrierusers.mbcaUrl = carrierUser.mbcaUrl;
                    carrierusers.COIExpiryDate = carrierUser.COIExpiryDate;
                    carrierusers.dx_vendor_id = carrierUser.dx_vendor_id;
                    carrierusers.carriertype = carrierUser.carriertype;
                    carrierusers.nametoprintoncheck = carrierUser.nametoprintoncheck;
                    carrierusers.dtuid = carrierUser.dtuid;
                    // carrierusers.scacCode = carrierUser.scacCode;

                    //_jWLDBContext.carrierusers.Update(carrierusers);
                    if (UserID == null || UserID == "" || UserID == "0")
                    {
                        _jWLDBContext.carrierusers.Update(carrierusers);
                        _jWLDBContext.SaveChangesAsync();
                    }
                    else
                    {
                        _jWLDBContext.SaveChanges();
                    }
                    objresponse.ResponseVal = true;

                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "clsCarrierService/UpdateURL");
                objresponse.ResponseVal = false;
            }
            return objresponse;
        }
        public List<state> GetActiveStates()
        {
            using (var _jWLDBContext = new clsJWLDBContext())
            {
                return _jWLDBContext.state.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToList();
            }
        }
        public string CheckDOT(string DOT)
        {
            string strReturn = string.Empty;
            try
            {
                using (var _jWLDBContext = new clsJWLDBContext())
                {
                    var Carriers = _jWLDBContext.carrierusers.AsNoTracking().Where(x => x.addtionalDot != null);
                    bool IsDOT = Carriers.Any(t => t.addtionalDot.ToLower() == DOT.ToLower());

                    if (IsDOT)
                    {
                        carrierusers carrierusers = _jWLDBContext.carrierusers.Where(t => t.addtionalDot.ToLower() == DOT.ToLower()).FirstOrDefault();
                        strReturn = carrierusers.cuId.ToString();
                    }
                    return IsDOT.ToString() + "$" + strReturn;

                    //return IsDOT.ToString();
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "clsCarrierService/CheckDOT");
                return false.ToString();
            }

        }

        public string CheckEmail(string Email)
        {
            string strReturn = string.Empty;
            try
            {
                using (var _jWLDBContext = new clsJWLDBContext())
                {
                    var Carriers = _jWLDBContext.carrierusers.AsNoTracking().Where(x => x.cuEmail != null);
                    bool IsEmail = Carriers.Any(t => t.cuEmail.ToLower() == Email.ToLower());
                    if (IsEmail)
                    {
                        carrierusers carrierusers = _jWLDBContext.carrierusers.Where(t => t.cuEmail.ToLower() == Email.ToLower()).FirstOrDefault();
                        strReturn = carrierusers.cuId.ToString();
                    }
                    return IsEmail.ToString() + "$" + strReturn;
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "CheckEmail");
                return false.ToString();
            }
        }



        public string CheckDataTrac_UID(string dtuid)
        {
            string strReturn = string.Empty;
            try
            {
                using (var _jWLDBContext = new clsJWLDBContext())
                {
                    var Carriers = _jWLDBContext.carrierusers.AsNoTracking().Where(x => x.dtuid != null);
                    bool Isdtuid = Carriers.Any(t => t.dtuid.ToLower() == dtuid.ToLower());

                    if (Isdtuid)
                    {
                        carrierusers carrierusers = _jWLDBContext.carrierusers.Where(t => t.dtuid.ToLower() == dtuid.ToLower()).FirstOrDefault();
                        strReturn = carrierusers.cuId.ToString();
                    }
                    return Isdtuid.ToString() + "$" + strReturn;
                   // return Isdtuid.ToString();
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "clsCarrierService/CheckDataTrac_UID");
                return false.ToString();
            }
        }
        public string GenerateDatatrac_UID()
        {
            string dtuid;
            int i = 0;
            int j = 1000;
            do
            {
                dtuid = null;
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                dtuid = new string(Enumerable.Repeat(chars, 4)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                clsCarrierService objcarrierUser = new clsCarrierService();
                var Checkdtuid = objcarrierUser.CheckDataTrac_UID(dtuid);
                if (Checkdtuid.Split("$")[0] == "False")
                {
                    break;
                }
                i++;
            }
            while (i < j);
            return dtuid;
        }




        public string GenerateNumericOTP(int OtpLength)
        {
            try
            {
                int digits = Convert.ToInt32(OtpLength);
                if (digits < 3)
                    return Convert.ToString(new Random().Next(10, 99));
                else
                    return Convert.ToString(new Random().Next(MultiplyNTimes(digits), MultiplyNTimes(digits + 1) - 1));
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "clsCarrierService/GenerateNumericOTP");
                return false.ToString();
            }

        }
        private int MultiplyNTimes(int n)
        {
            if (n == 1)
                return 1;
            else
                return 10 * MultiplyNTimes(n - 1);
        }

        public ReturnResponse InsertOtp(OtpLogs objOtpLogs)
        {
            ReturnResponse objReturnResponse = new ReturnResponse();
            try
            {
                using (var _jWLDBContext = new clsJWLDBContext())
                {
                    var Added = _jWLDBContext.otplogs.AddAsync(objOtpLogs);
                    _jWLDBContext.SaveChangesAsync();
                }
                objReturnResponse.ResponseVal = true;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "clsCarrierService/InsertOtp");
                objReturnResponse.ResponseVal = false;

            }
            return objReturnResponse;
        }

        public ReturnResponse SendEmailToCarrier(string Name, string strEmail, int cuId)
        {
            ReturnResponse objReturnResponse = new ReturnResponse();
            try
            {
                string fromMail = GetConfigValue("FromMailID");
                string fromPassword = GetConfigValue("FromMailPasssword");
                string Subject = "Registration Approved";
                string toMail = strEmail;
                SmtpClient smtpClient = new SmtpClient(GetConfigValue("MailSMTPHost"), Convert.ToInt32(GetConfigValue("MailSMTPPort")));
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(fromMail, fromPassword);
                smtpClient.EnableSsl = true;

                MailAddress fromAddress = new MailAddress(fromMail);

                MailMessage mailMsg = new MailMessage();
                mailMsg.From = fromAddress;

                string[] toAddress;
                toAddress = toMail.Split(',');
                foreach (string strTo in toAddress)
                {
                    mailMsg.To.Add(strTo);
                }

                mailMsg.Subject = Subject;

                string strPDFTemplatePath = GetConfigValue("FilePath");
                string OnboardingFile = $@"{strPDFTemplatePath}/\MailTemplate\/Approved.html";
                var UserHTML = string.Join("", System.IO.File.ReadAllLines(OnboardingFile));
                UserHTML = UserHTML.Replace("##Date##", DateTime.Now.ToString("MMM dd, yyyy")).Replace("##Name##", Name).Replace("##LogoUrlInTemplate##", GetConfigValue("LogoUrlInTemplate")).Replace("##DisclaimerReplyEmailId##", GetConfigValue("DisclaimerReplyEmailId"));

                //Start -- To send set password link to carrier for the first time
                Guid AuthToken = Guid.NewGuid();
                string SetPasswordlink = GetConfigValue("ApplicationUrl") + @"/Carrier/SetPassword?AuthToken=" + AuthToken.ToString();
                UserHTML = UserHTML.Replace("##SetPasswordlink##", SetPasswordlink);

                int otp = Convert.ToInt32(GenerateNumericOTP(Convert.ToInt32(GetConfigValue("OtpLength", "CarrierUserAuth"))));
                OtpLogs objOtpLogs = new OtpLogs();
                objOtpLogs.UserId = cuId;
                objOtpLogs.OTP = otp;
                objOtpLogs.Token = AuthToken.ToString();
                objOtpLogs.EmailId = strEmail;
                objOtpLogs.CreatedDate = DateTime.Now;
                objOtpLogs.IsActive = true;
                InsertOtp(objOtpLogs);
                mailMsg.Body = UserHTML;
                mailMsg.IsBodyHtml = true;

                smtpClient.Send(mailMsg);
                objReturnResponse.ResponseVal = true;

            }
            catch (Exception ex)
            {
                objReturnResponse.ResponseVal = false;
                WriteErrorLog(ex, "clsCarrierService/SendEmailToCarrier");
            }
            return objReturnResponse;
        }
    }
}
