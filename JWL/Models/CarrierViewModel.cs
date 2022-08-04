using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    [Serializable]
    public class CarrierViewModel
    {
        [DisplayName("ID")]
        public string cuId { get; set; }
        [DisplayName("Agreement Date")]
        public string agreementDate { get; set; }
        [DisplayName("DOT#")]
        public string addtionalDot { get; set; }
        [DisplayName("MC#")]
        public string MC { get; set; }
        [DisplayName("CA#")]
        public string ca { get; set; }
        [DisplayName("Are you a broker or carrier?")]
        public string brokerOptions { get; set; }
        [DisplayName("Authorized Person")]
        public string authorizedPerson { get; set; }
        [DisplayName("Title")]
        public string title { get; set; }
        [DisplayName("Legal Company Name")]
        public string legalCompanyName { get; set; }
        [DisplayName("DBA")]
        public string DBA { get; set; }
        [DisplayName("Email")]
        public string cuEmail { get; set; }
        [DisplayName("Physical Address")]
        public string physicalAddress { get; set; }
        [DisplayName("City")]
        public string city { get; set; }
        [DisplayName("State")]
        public string state { get; set; }
        [DisplayName("Zip Code")]
        public string zipcode { get; set; }
        [DisplayName("Phone")]
        public string telephone { get; set; }
        [DisplayName("Fax")]
        public string fax { get; set; }
        [DisplayName("Years in business")]
        public string businessYear { get; set; }
        [DisplayName("Miles per year?")]
        public string milesPerYear { get; set; }
        [DisplayName("Cargo Specialties")]
        public string cargoSpecification { get; set; }
        [DisplayName("Payment Methods")]
        public string paymentMethods { get; set; }
        [DisplayName("Racial or ethnic identification")]
        public string ethnicIdentification { get; set; }
        [DisplayName("Minority-owned business?")]
        public string isMinorityBusiness { get; set; }
        [DisplayName("Female-owned business?")]
        public string isFemaleOwned { get; set; }
        [DisplayName("Veteran-owned business?")]
        public string isVeteranOwned { get; set; }
        [DisplayName("Factoring Company Name")]
        public string factoryCompanyName { get; set; }
        [DisplayName("Factoring Company Contact")]
        public string factoryContactName { get; set; }
        [DisplayName("Factoring Company Address")]
        public string factoryPhysicalAddress { get; set; }
        [DisplayName("Factoring Company City")]
        public string factoryCity { get; set; }
        [DisplayName("Factoring Company State")]
        public string factoryState { get; set; }
        [DisplayName("Factoring Company Zip Code")]
        public string factoryZipCode { get; set; }
        [DisplayName("Factoring Company Phone")]
        public string factoryTelephone { get; set; }
        [DisplayName("Factoring Company Fax")]
        public string factoryFax { get; set; }
        [DisplayName("Dispatch Person")]
        public string additionalPersonName { get; set; }
        [DisplayName("Dispatch Phone")]
        public string addtionalPersonTelephone { get; set; }
        [DisplayName("After-Hours Dispatch Person")]
        public string addtionalAfterHoursPersonName { get; set; }
        [DisplayName("After-Hours Dispatch Phone")]
        public string addtionalAfterHoursPersonTelephone { get; set; }
        [DisplayName("SCAC Code")]
        public string additionalScac { get; set; }
        [DisplayName("Federal Tax ID")]
        public string additionalFedaralID { get; set; }
        [DisplayName("TWIC Certified?")]
        public string twic { get; set; }
        [DisplayName("Hazmat Certified?")]
        public string additionalHazmatCertified { get; set; }
        [DisplayName("Hazmat Expiration Date")]
        public string additinalHazmatExpiryDate { get; set; }
        [DisplayName("Preferred Lanes")]
        public string additionalPreferredLanes { get; set; }
        [DisplayName("Service Area")]
        public string serviceArea { get; set; }
        [DisplayName("Operating States")]
        public string additionalMajorMakets { get; set; }
        [DisplayName("Vehicle Type")]
        public string ExportVehicle { get; set; }
        [DisplayName("Fleet Size")]
        public string FleetVehicle { get; set; }
        [DisplayName("Trailer Type")]
        public string ExportTrailer { get; set; }
        [DisplayName("Trailer Count")]
        public string ExportTrailerCount { get; set; }
        [DisplayName("Document Type")]
        public string CarrierDocument { get; set; }
        [DisplayName("Document URL")]
        public string CarrierDocumentUrl { get; set; }
        [DisplayName("Authorized Signature")]
        public string authorizedSignature { get; set; }
        [DisplayName("Completed MBCA")]
        public string completedMBCA { get; set; }
        [DisplayName("Report Sort key")]
        public string reportSortKey { get; set; }
        [DisplayName("NDA returned?")]
        public string ndaReturned { get; set; }
        [DisplayName("Onboarding Completed?")]
        public string onboardingCompleted { get; set; }
        [DisplayName("1M General")]
        public string Ins1MGeneral { get; set; }
        [DisplayName("1M Auto")]
        public string Ins1MAuto { get; set; }
        [DisplayName("100K Cargo")]
        public string Ins100KCargo { get; set; }
        [DisplayName("250K Cargo")]
        public string Ins250KCargo { get; set; }
        [DisplayName("Payment Terms")]
        public string paymentTerms { get; set; }
        [DisplayName("Assignee")]
        public string assignee { get; set; }
        [DisplayName("Status")]
        public string status { get; set; }
        [DisplayName("Created By")]
        public string createdBy { get; set; }
        [DisplayName("Created Date")]
        public string CreatedDate { get; set; }
        [DisplayName("Modified By")]
        public string modifiedBy { get; set; }
        [DisplayName("Modified Date")]
        public string ModifiedDate { get; set; }
        
    }
}
