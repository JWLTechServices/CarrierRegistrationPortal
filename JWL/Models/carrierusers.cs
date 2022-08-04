using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class carrierusers
    {
        [Key]
        public int cuId { get; set; }
        public string cuName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Please enter valid Email")]
        public string cuEmail { get; set; }
        public DateTime agreementDate { get; set; } = DateTime.Today;
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter MC#")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter valid MC")]
        public string MC { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Authorized Person")]
        public string authorizedPerson { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Title")]
        public string title { get; set; }
        public string legalCompanyName { get; set; }
        public string DBA { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Physical Address")]
        public string physicalAddress { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter City")]
        public string city { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select State")]
        public int state { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Zip code")]
        public string zipcode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Phone")]
        public string telephone { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Fax")]
        public string fax { get; set; }
        public StatusEnum status { get; set; }
        public string factoryCompanyName { get; set; }
        public string factoryContactName { get; set; }
        public string factoryPhysicalAddress { get; set; }
        public string factoryCity { get; set; }
        public int? factoryState { get; set; }
        public string factoryZipCode { get; set; }
        public string factoryTelephone { get; set; }
        public string factoryFax { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Disptach Person Name")]
        public string additionalPersonName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Phone of Dispatch Person Name")]
        public string addtionalPersonTelephone { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Dispatch After-Hours Person Name")]
        public string addtionalAfterHoursPersonName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter After-Hours Dispatch Phone")]
        public string addtionalAfterHoursPersonTelephone { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter DOT")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter valid DOT")]
        public string addtionalDot { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter SCAC Code")]
        [RegularExpression(@"^[^\s]{4}$", ErrorMessage = "Please enter valid SCAC Code")]
        public string additionalScac { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Federal Tax ID")]
        //[RegularExpression("^[0-9]+(-[0-9]+)+$", ErrorMessage = "Please enter valid Federal Tax ID")]    
        //[RegularExpression(@"[0-9]{2}\-[0-9]{7}", ErrorMessage = "Please enter valid Federal Tax ID")]
        //[RegularExpression(@"[0-9]{3}\-[0-9]{2}\-[0-9]{4}", ErrorMessage = "Please enter valid Federal Tax ID")]
        public string additionalFedaralID { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select Hazmat Certified")]
        public bool? additionalHazmatCertified { get; set; }
        [DataType(DataType.Date)]
        public DateTime? additinalHazmatExpiryDate { get; set; } = DateTime.Now;
        public string additionalPreferredLanes { get; set; }
        //[Required(AllowEmptyStrings = false, ErrorMessage = "At least on major market should be selected")]
        public string additionalMajorMakets { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Authorized Signature")]
        public string authorizedSignature { get; set; }
        public string authorizedSignaturePath { get; set; }
        public bool? completedMBCA { get; set; }
        public string reportSortKey { get; set; }
        public bool? ndaReturned { get; set; }
        public bool? onboardingCompleted { get; set; }
        public string onboardingFileLink { get; set; }
        public string insuranceType { get; set; }
        public bool? Ins1MGeneral { get; set; }
        public bool? Ins1MAuto { get; set; }
        public bool? Ins250KCargo { get; set; }
        public bool? Ins100KCargo { get; set; }
        public string paymentTerms { get; set; }
        public string majorMarkets { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? createdBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public bool? twic { get; set; }
        public string ca { get; set; }
        public bool? isFemaleOwned { get; set; }
        public bool? isVeteranOwned { get; set; }
        public int? modifiedBy { get; set; }
        public int? assignee { get; set; }
        public bool? isMinorityBusiness { get; set; }
        public bool? isDeleted { get; set; }
        public string businessYear { get; set; }
        public string ethnicIdentification { get; set; }
        public string milesPerYear { get; set; }
        public string cargoSpecification { get; set; }
        public string paymentMethods { get; set; }
        [ForeignKey("modifiedBy")]
        public users modifiedByUser { get; set; }
        //[ForeignKey("city")]
        //public city Cities { get; set; }
        [ForeignKey("state")]
        public state States { get; set; }
        //[ForeignKey("factoryCity")]
        //public city FactoryCities { get; set; }
        [ForeignKey("factoryState")]
        public state FactoryStates { get; set; }
        [ForeignKey("assignee")]
        public users Users { get; set; }
        [NotMapped]
        public string CarrierwiseVehicle { get; set; }
        [NotMapped]
        public string CarrierwiseTrailer { get; set; }
        [NotMapped]
        public string CarrierDocument { get; set; }
        [NotMapped]
        public string CreatedUserName { get; set; }
        [NotMapped]
        public string Token { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select Service Area")]
        public string serviceArea { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select Any Option")]
        public string brokerOptions { get; set; }
        public string createdByUserName { get; set; }
        public string ndaUrl { get; set; }
        public string mbcaUrl { get; set; }

        [DataType(DataType.Date)]
        public DateTime? COIExpiryDate { get; set; }
        public string dx_vendor_id { get; set; }
        public string nametoprintoncheck { get; set; }
        public string carriertype { get; set; }

        //[RegularExpression(@"^[^\s]{4}$", ErrorMessage = "Please enter valid SCAC Code")]
        //public string scacCode { get; set; }

       [RegularExpression(@"^[^\s]{4}$", ErrorMessage = "Please enter valid dtuid")]
       public string dtuid { get; set; }

    }
    public enum StatusEnum
    {
        [Display(Name = "New")]
        New = 1,
        [Display(Name = "In-process")]
        Inprocess,
        [Display(Name = "Complete")]
        Complete,
        [Display(Name = "Approved")]
        Approved,
        [Display(Name = "Rejected")]
        Rejected,
        [Display(Name = "On-hold")]
        Onhold,
        [Display(Name = "Terminated")]
        Terminated
    }

    public class cuusers
    {
        [Key]
        public int userId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Name")]
        public string name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Please enter valid Email")]
        public string email { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        [RegularExpression(pattern: @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Please enter Strong Password")]
        public string password { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Please enter Confirm Password")]
        [Compare("password", ErrorMessage = "Password and Confirm Password must be same")]
        public string confirmPassword { get; set; }
        public bool? isFirstTime { get; set; }
        public bool? isDeleted { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Select Active Users")]
        public bool isActive { get; set; }
        public int cuId { get; set; }
        public string passwordSalt { get; set; }

    }
    public class OtpLogs
    {
        [Key]
        public int OtpId { get; set; }
        public int UserId { get; set; }
        public int OTP { get; set; }
        public string Token { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDateTime { get; set; }
        public bool IsActive { get; set; }
    }
    public class CarrierUserAuth
    {
        public int OtpLength { get; set; }
        public int OtpexpiryInMin { get; set; }
        public int GeneratedOtp { get; set; }
        public string EmailTwoFactAuthRequired { get; set; }
        public int TokenexpiryInMin { get; set; }
        public string DefaultPassword { get; set; }

    }
    public class Application
    {
        public string LogoUrlInTemplate { get; set; }
        public string DisclaimerReplyEmailId { get; set; }

    }

    public class TwoFactorAuth
    {
        public int UserId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Otp")]
        public string Otp { get; set; }
        public string Token { get; set; }
        public int ActualOtp { get; set; }
        public bool OtpExpired { get; set; }
        public bool OtpResend { get; set; }

    }

    public class ForgotPassword
    {
        public int UserId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Please enter valid Email")]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Otp")]
        public string Otp { get; set; }
        public string Token { get; set; }
        // public int ActualOtp { get; set; }
        public bool OtpExpired { get; set; }
        public bool OtpResend { get; set; }
        public bool ResetPassLinkSent { get; set; }
        public bool OtpSent { get; set; }

    }

    public class carrierKPI
    {
        [Key]
        public int cuId { get; set; }
        public List<carrierKPISummary> carrierKPISummary { get; set; }
        public List<carrierKPIDetails> carrierKPIDetails { get; set; }
    }

    public class carrierKPISummary
    {
        [Key]
        public int cuId { get; set; }
        public string WeekNumber { get; set; }
        public string Carrier { get; set; }
        public int WeeklyCount { get; set; }
        public int ExpectedPieces { get; set; }
        public int LoadScanCount { get; set; }
        public int DeliveryScanCount { get; set; }
        public int PhotoComplianceCount { get; set; }
        public int GPSComplianceCount { get; set; }
        public string DeliveryScanPercentage { get; set; }
        public string LoadScanPercentage { get; set; }
        public string PhotoCompliancePercentage { get; set; }
        public string GPSCompliancePercentage { get; set; }
        public string LoadScanBonusOrDeduction { get; set; }
        public string DeliveryScanBonusOrDeduction { get; set; }
        public string PhotoBonusOrDeduction { get; set; }
        public string GPSComplianceBonusOrDeduction { get; set; }
        public string TotalBonusOrDeduction { get; set; }
        public string TotalBonusOrDeductionToBeApplied { get; set; }

    }

    public class carrierKPIDetails
    {
        [Key]
        public int cuId { get; set; }
        public string WeekNumber { get; set; }
        public string Carrier { get; set; }
        public int CompanyNumber { get; set; }
        public int CustomerNumber { get; set; }
        public string Market { get; set; }
        public string Region { get; set; }
        public int UniqueIdNo { get; set; }
        public string LoadId { get; set; }
        public string BranchId { get; set; }
        public string RouteCode { get; set; }
        public string RouteDate { get; set; }
        public int StopExpectedPieces { get; set; }
        public string ReceivingScanDateTime { get; set; }
        public int ReceivingScanPieces { get; set; }
        public string LoadingScanDateTime { get; set; }
        public int LoadScanCount { get; set; }
        public string DeliveryScanDateTime { get; set; }
        public int DeliveryScanCount { get; set; }
        public int DriverNumber { get; set; }
        public string PhotosExist { get; set; }
        public string PhotosDeliveryDate { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

    }

    public class carrierDashBoard
    {
        [Key]
        public int cuId { get; set; }
        public string WeekNumber { get; set; }
        public string CarrierName { get; set; }
        public string DeliveryScanPercentage { get; set; }
        public string LoadScanPercentage { get; set; }
        public string PhotoCompliancePercentage { get; set; }
        public string GPSCompliancePercentage { get; set; }
    }

    public class weeklist
    {
        [Key]
        public int CuId { get; set; }
        public string WeekNumber { get; set; }
    }
}
