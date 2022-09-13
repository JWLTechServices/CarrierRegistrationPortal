using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Data;
using Interfaces;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarrierDataPushing
{
    class Program
    {
        static void Main(string[] args)
        {
            StartProcessing();
            clsCommon objCommon = new clsCommon();
            string AppName = objCommon.GetConfigValue("ApplicationName");
            if (clsCommon.IsException)
            {
                string strEmailSubject = "Got Exception while running " + AppName + " on " + DateTime.Now.ToString("yyyyMMdd");
                string strEmailBody = strEmailSubject + System.Environment.NewLine + "Requesting you to please go and check error log file for :" + DateTime.Now.ToString("yyyyMMdd");
                objCommon.SendExceptionMail(strEmailSubject, strEmailBody);
            }
        }
        private static void StartProcessing()
        {
            clsCommon objCommon = new clsCommon();
            string AppName = objCommon.GetConfigValue("ApplicationName");
            string strInputFilePath = objCommon.GetConfigValue("FilePath") + @"\InputFileFolder";
            DirectoryInfo dir;
            FileInfo[] XLSfiles;
            string strFileName;
            string strDatetime;
            try
            {

                string strExecutionLogMessage;
                string strBillingHistoryFileLocation = strInputFilePath + @"\HistoricalFiles";
                strExecutionLogMessage = "Beginning the new instance for " + AppName + " processing ";
                objCommon.WriteExecutionLog(strExecutionLogMessage);

                dir = new DirectoryInfo(strInputFilePath);
                XLSfiles = dir.GetFiles("*.xlsx");

                strExecutionLogMessage = "Found # of Excel Files: " + XLSfiles.Count();
                objCommon.WriteExecutionLog(strExecutionLogMessage);

                foreach (var file in XLSfiles)
                {
                    strFileName = file.Name.ToString();
                    try
                    {
                        DataSet dsExcel = new DataSet();
                        dsExcel = clsExcelHelper.ImportExcelXLSX(strInputFilePath + @"\" + strFileName, false);
                        if (dsExcel.Tables.Count > 0)
                        {
                            objCommon.MoveTheFileToHistoryFolder(strBillingHistoryFileLocation, file);
                            strDatetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            int j = 0;


                            clsCarrierService objcarrierUser = new clsCarrierService();
                            List<state> objStateList = new List<state>();
                            objStateList = objcarrierUser.GetActiveStates();

                            string strDefaultPassword = objCommon.GetConfigValue("DefaultPassword", "CarrierUserAuth");

                            DataTable exceptiondataTable = dsExcel.Tables[0].Clone();
                            foreach (DataTable table in dsExcel.Tables)
                            {
                                if (j == 1)
                                {
                                    break;
                                }
                                foreach (DataRow dr in table.Rows)
                                {

                                    object cuEmail = dr["Email"];
                                    if (cuEmail == DBNull.Value)
                                        break;

                                    Boolean AlreadyExistAtJWL = false;
                                    Boolean AlreadyExistdtuid = false;

                                    Boolean AlreadyExistEmail = false;
                                    Boolean AlreadyExistDot = false;
                                    Boolean IsDT_UIDNew = false;

                                    int AlreadyExistEmailCuId=0;
                                    int AlreadyExistDotCuId=0;
                                    int AlreadyExistdtuidCuId=0;

                                    try
                                    {
                                        carrierusers carrierUser = new carrierusers();
                                        carrierUser.cuEmail = Convert.ToString(dr["Email"]);
                                        DateTime dtagreementDate = Convert.ToDateTime(dr["Agreement Date"]);
                                        carrierUser.agreementDate = dtagreementDate;

                                        carrierUser.authorizedPerson = Convert.ToString(dr["Authorized Person"]);
                                        carrierUser.title = Convert.ToString(dr["Title"]);
                                        carrierUser.physicalAddress = Convert.ToString(dr["Physical Address"]);

                                        var state = Convert.ToString(dr["State"]);
                                        int stateId = 0;
                                        //   var res = objStateList.Where(x => x.stateName == state);
                                        var res = objStateList.Where(x => x.stateName.Trim().Substring(0, 2) == state.Trim().ToUpper());
                                        if (res.Any())
                                        {
                                            var firstItem = res.ElementAt(0);
                                            stateId = firstItem.stateId;
                                        }
                                        else
                                        {
                                            exceptiondataTable.Rows.Add(dr.ItemArray);
                                            strExecutionLogMessage = "Carrier Registration Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Cannot process this record as state not found in data base  " + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                            strExecutionLogMessage += "State -" + state + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);

                                            clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                            objErrorResponse.error = "Cannot process this record as state not found in data base";
                                            objErrorResponse.code = "Carrier Registration Failed ";
                                            objErrorResponse.reference = carrierUser.cuEmail;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, strFileName, strDatetime, "Error");
                                            continue;
                                        }

                                        carrierUser.state = Convert.ToInt16(stateId);
                                        carrierUser.city = Convert.ToString(dr["City"]);
                                        carrierUser.zipcode = Convert.ToString(dr["Zip Code"]);
                                        carrierUser.telephone = Convert.ToString(dr["Phone"]);


                                        carrierUser.additionalPersonName = Convert.ToString(dr["Disptach Person Name"]);
                                        carrierUser.addtionalPersonTelephone = Convert.ToString(dr["Dispatch Phone"]);
                                        carrierUser.addtionalAfterHoursPersonName = Convert.ToString(dr["After-Hours Dispatch Person Name"]);
                                        carrierUser.addtionalAfterHoursPersonTelephone = Convert.ToString(dr["After-Hours Dispatch Phone"]);

                                        //   is exit 
                                        carrierUser.addtionalDot = Convert.ToString(dr["DOT#"]);

                                        //  0,1
                                        var additionalHazmatCertified = false;
                                        if (Convert.ToString(dr["Hazmat Certified?"]).ToLower() == "yes")
                                        {
                                            additionalHazmatCertified = true;
                                        }

                                        carrierUser.additionalHazmatCertified = additionalHazmatCertified;

                                        carrierUser.authorizedSignature = Convert.ToString(dr["Authorized Signature"]);

                                        carrierUser.additionalFedaralID = Convert.ToString(dr["Federal Tax ID"]);

                                        //Local
                                        carrierUser.serviceArea = Convert.ToString(dr["Service Area"]);

                                        // carrierUser.status = StatusEnum(Convert.ToString(dr["Service Area"]));
              
                                        //  Carrier
                                        // Broker

                                        carrierUser.brokerOptions = Convert.ToString(dr["Are you a broker or carrier?"]);

                                        // if status is approved below column required
                                        DateTime dtCOIExpiryDate = Convert.ToDateTime(dr["COI Expiry Date"]);
                                        carrierUser.COIExpiryDate = dtCOIExpiryDate;
                                        if (dr.Table.Columns.Contains("DX Vendor Id"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["DX Vendor Id"])))
                                            {
                                                carrierUser.dx_vendor_id = Convert.ToString(dr["DX Vendor Id"]);
                                            }
                                        }

                                        carrierUser.nametoprintoncheck = Convert.ToString(dr["Name to print on check"]);
                                        carrierUser.carriertype = Convert.ToString(dr["Carrier type"]);

                                        if (dr.Table.Columns.Contains("Legal Company Name"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Legal Company Name"])))
                                            {
                                                carrierUser.legalCompanyName = Convert.ToString(dr["Legal Company Name"]);
                                            }
                                        }

                                        if (dr.Table.Columns.Contains("DBA"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["DBA"])))
                                            {
                                                carrierUser.DBA = Convert.ToString(dr["DBA"]);
                                            }
                                        }


                                        if (dr.Table.Columns.Contains("Datatrac UID"))
                                        {
                                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Datatrac UID"])))
                                            {
                                                carrierUser.dtuid = Convert.ToString(dr["Datatrac UID"]);
                                            }
                                        }

                                        var CheckEmail = objcarrierUser.CheckEmail(carrierUser.cuEmail);
                                        if (CheckEmail.Split("$")[0] == "True")
                                        {
                                            AlreadyExistAtJWL = true;
                                            AlreadyExistEmail = true;

                                            AlreadyExistEmailCuId = Convert.ToInt16(CheckEmail.Split("$")[1]);

                                            //exceptiondataTable.Rows.Add(dr.ItemArray);
                                            strExecutionLogMessage = "Carrier Registration Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Cannot process this record as email id already exist " + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                            strExecutionLogMessage += "DOT -" + carrierUser.addtionalDot + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);

                                   //         clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                   //         objErrorResponse.error = "Cannot process this record as email id already exist";
                                   //         objErrorResponse.code = "Carrier Registration Failed ";
                                   //         objErrorResponse.reference = carrierUser.cuEmail;
                                   //         string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                   //         DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                   //         dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                   //         objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   //strInputFilePath, strFileName, strDatetime, "Error");
                                            // continue;
                                        }

                                        var CheckDot = objcarrierUser.CheckDOT(carrierUser.addtionalDot);
                                        if (CheckDot.Split("$")[0] == "True")
                                        {
                                            AlreadyExistAtJWL = true;
                                            AlreadyExistDot = true;
                                            AlreadyExistDotCuId = Convert.ToInt16(CheckDot.Split("$")[1]);
                                            
                                            //exceptiondataTable.Rows.Add(dr.ItemArray);
                                            strExecutionLogMessage = "Carrier Registration Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Cannot process this record as DOT already exist " + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                            strExecutionLogMessage += "DOT -" + carrierUser.addtionalDot + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);


                                   //         clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                   //         objErrorResponse.error = "Cannot process this record as DOT already exist";
                                   //         objErrorResponse.code = "Carrier Registration Failed ";
                                   //         objErrorResponse.reference = carrierUser.cuEmail;
                                   //         string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                   //         DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                   //         dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                   //         objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   //strInputFilePath, strFileName, strDatetime, "Error");

                                            //continue;
                                        }
                                        if (carrierUser.dtuid != null)
                                        {
                                            IsDT_UIDNew = false;

                                            var CheckDTUID = objcarrierUser.CheckDataTrac_UID(carrierUser.dtuid);
                                            if (CheckDTUID.Split("$")[0] == "True")
                                            {
                                                AlreadyExistAtJWL = true;
                                                AlreadyExistdtuid = true;
                                                AlreadyExistdtuidCuId = Convert.ToInt16(CheckDTUID.Split("$")[1]);
                                               // exceptiondataTable.Rows.Add(dr.ItemArray);
                                                strExecutionLogMessage = "Carrier Registration Failed " + System.Environment.NewLine;
                                                strExecutionLogMessage += "Cannot process this record as DataTrac_UID already exist " + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                                strExecutionLogMessage += "DataTrac_UID -" + carrierUser.dtuid + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);

                                       //         clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                       //         objErrorResponse.error = "Cannot process this record as DataTrac_UID already exist ";
                                       //         objErrorResponse.code = "Carrier Registration Failed ";
                                       //         objErrorResponse.reference = carrierUser.cuEmail;
                                       //         string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                       //         DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                       //         dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                       //         objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                       //strInputFilePath, strFileName, strDatetime, "Error");
                                                // continue;
                                            }
                                        }
                                        else
                                        {
                                            // genarate dtuid and set 
                                            AlreadyExistdtuid = false;
                                            IsDT_UIDNew = true;
                                            carrierUser.dtuid = objcarrierUser.GenerateDatatrac_UID();
                                        }

                                        var status = Convert.ToString(dr["Status"]);
                                        StatusEnum objStatusEnum;
                                        if (Enum.TryParse<StatusEnum>(status, out objStatusEnum))
                                        {
                                            carrierUser.status = (StatusEnum)objStatusEnum;
                                        }
                                        else
                                        {
                                            exceptiondataTable.Rows.Add(dr.ItemArray);
                                            strExecutionLogMessage = "Carrier Registration Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "Invalid Status, Unable to Parse the status, Please enter Proper status. " + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                            strExecutionLogMessage += "DOT -" + carrierUser.addtionalDot + System.Environment.NewLine;
                                            strExecutionLogMessage += "Status -" + status + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);

                                            clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                            objErrorResponse.error = "Invalid Status, Unable to Parse the status, Please enter Proper status.";
                                            objErrorResponse.code = "Carrier Registration Failed ";
                                            objErrorResponse.reference = carrierUser.cuEmail;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, strFileName, strDatetime, "Error");

                                            continue;
                                        }

                                        carrierUser.createdByUserName = carrierUser.authorizedPerson;

                                        if (AlreadyExistAtJWL)
                                        {
                                           // int AlreadyExistEmailCuId;
                                           // int AlreadyExistDotCuId;
                                            //int AlreadyExistdtuidCuId;

                                            if (AlreadyExistEmailCuId == AlreadyExistDotCuId )
                                            {
                                                carrierUser.cuId = AlreadyExistEmailCuId;
                                                UpdateCarrierdataInDataTrac(carrierUser);
                                            }
                                            else
                                            {
                                                exceptiondataTable.Rows.Add(dr.ItemArray);
                                                strExecutionLogMessage = "Carrier Details Updation Failed " + System.Environment.NewLine;
                                                strExecutionLogMessage += "We cannot update this record as data already exist for multiple carriers. " + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                                strExecutionLogMessage += "DOT -" + carrierUser.addtionalDot + System.Environment.NewLine;
                                                strExecutionLogMessage += "Status -" + status + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);

                                                clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                                objErrorResponse.error = "We cannot update this record as data alredy exist for various carriers.";
                                                objErrorResponse.code = "Carrier Details Updation Failed ";
                                                objErrorResponse.reference = carrierUser.cuEmail;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "CarierDetailsUpdationFailure";
                                                objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                       strInputFilePath, strFileName, strDatetime, "Error");
                                                continue;
                                            }

                                            clsCommon.ReturnResponse objresponse1 = new clsCommon.ReturnResponse();
                                            objresponse1 = objcarrierUser.UpdateCarrier(carrierUser, "");
                                            if (objresponse1.ResponseVal)
                                            {
                                                strExecutionLogMessage = "Carrier Details Updation Completed" + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);

                                                clsCommon.SuccessResponse objSuccessResponse = new clsCommon.SuccessResponse();
                                                objSuccessResponse.reference = cuEmail.ToString();
                                                string strSuccessResponse = JsonConvert.SerializeObject(objSuccessResponse);
                                                DataSet dsSuccessResponse = objCommon.jsonToDataSet(strSuccessResponse);
                                                dsSuccessResponse.Tables[0].TableName = "CarierDetailsUpdationSuccess";
                                                objCommon.WriteDataToCsvFile(dsSuccessResponse.Tables[0],
                                       strInputFilePath, strFileName, strDatetime, "Success");

                                            }
                                            else
                                            {
                                                exceptiondataTable.Rows.Add(dr.ItemArray);
                                                strExecutionLogMessage = "Carrier Details Updation Failed" + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);

                                                clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                                objErrorResponse.error = "Carrier Details Updation Failed";
                                                objErrorResponse.code = "Carrier Details Updation Failed";
                                                objErrorResponse.reference = carrierUser.cuEmail;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "CarierDetailsUpdationFailure";
                                                objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                       strInputFilePath, strFileName, strDatetime, "Error");
                                                continue;
                                            }
                                            continue;

                                        }


                                        byte[] salt = CreateSalt();
                                        byte[] hash = HashPassword(strDefaultPassword, salt);
                                        string bas64Passwordhash = Convert.ToBase64String(hash);
                                        string bas64PasswordSalt = Convert.ToBase64String(salt);

                                        clsCommon.ReturnResponse objresponse = new clsCommon.ReturnResponse();
                                        objresponse = objcarrierUser.AddCarrierUser(carrierUser, "", bas64Passwordhash, bas64PasswordSalt);
                                        if (objresponse.ResponseVal)
                                        {
                                            strExecutionLogMessage = "Carrier Registration Success " + System.Environment.NewLine;
                                            strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                            strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;

                                            objCommon.WriteExecutionLog(strExecutionLogMessage);

                                            //strExecutionLogMessage = "Update Carrier data In DataTrac Started " + System.Environment.NewLine;
                                            //objCommon.WriteExecutionLog(strExecutionLogMessage);

                                            //amazon_equipment_owner objrequest = new amazon_equipment_owner();
                                            //string amazon_equipment_ownerrequest = null;
                                            //amazon_equipment_ownerrequest = @"'cuId': '" + carrierUser.cuId + "'";
                                            ////  amazon_equipment_ownerrequest = amazon_equipment_ownerrequest + @"'cuId': '" + carrierusers.cuId + "'";
                                            //amazon_equipment_ownerrequest = @"{" + amazon_equipment_ownerrequest + "}";
                                            //string amazon_equipment_owner_RequestObject = @"{'amazon_equipment_owner': " + amazon_equipment_ownerrequest + "}";
                                            //clsCommon.ReturnResponse objdatatracresponse = new clsCommon.ReturnResponse();
                                            //clsDatatrac objclsDatatrac = new clsDatatrac();
                                            //JObject jsonobj = JObject.Parse(amazon_equipment_owner_RequestObject);
                                            //string request = jsonobj.ToString();
                                            //objdatatracresponse = objclsDatatrac.DataTrac_amazon_equipment_owner_PutAPI(carrierUser.dtuid, amazon_equipment_owner_RequestObject);
                                            //if (objdatatracresponse.ResponseVal)
                                            //{
                                            //    strExecutionLogMessage = "DataTrac_amazon_equipment_owner_PutAPI API Success " + System.Environment.NewLine;
                                            //    strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            //    strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            //    objCommon.WriteExecutionLog(strExecutionLogMessage);
                                            //}
                                            //else
                                            //{
                                            //    strExecutionLogMessage = "DataTrac_amazon_equipment_owner_PutAPI Failed " + System.Environment.NewLine;
                                            //    strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                                            //    strExecutionLogMessage += "Response -" + objresponse.Reason + System.Environment.NewLine;
                                            //    objCommon.WriteExecutionLog(strExecutionLogMessage);
                                            //}

                                            //strExecutionLogMessage = "Update Carrier data In DataTrac Completed " + System.Environment.NewLine;
                                            //objCommon.WriteExecutionLog(strExecutionLogMessage);


                                            //if (AlreadyExistAtJWL)
                                            //{
                                            //    UpdateCarrierdataInDataTrac(carrierUser);
                                            //}
                                            //else
                                            //{
                                            //    CreateCarrierdataInDataTrac(carrierUser);
                                            //}

                                            CreateCarrierdataInDataTrac(carrierUser);

                                            if (Convert.ToString(objCommon.GetConfigValue("EnableSendEmailToCarrier")).ToUpper() == "Y")
                                            {
                                                strExecutionLogMessage = "Sending Email To Carrier Started " + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);
                                                objcarrierUser.SendEmailToCarrier(carrierUser.authorizedPerson, carrierUser.cuEmail, carrierUser.cuId);
                                                strExecutionLogMessage = "Sending Email To Carrier Completed " + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);
                                            }
                                            strExecutionLogMessage = "Generating the PDF's Started " + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);


                                            MemoryStream ms = new MemoryStream();
                                            string FileLocation = objCommon.GetConfigValue("FilePath") + @"\PDFTemplate\JWLNDA.html";
                                            var htmlString = string.Join("", System.IO.File.ReadAllLines(FileLocation));
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

                                            string strfileNameJWLNDA = carrierUser.additionalPersonName;
                                            if (carrierUser.legalCompanyName != null)
                                            {
                                                strfileNameJWLNDA = strfileNameJWLNDA + "_" + carrierUser.legalCompanyName;
                                            }
                                            string fileNameJWLNDA = $@"/\PDFAttachment/\_{strfileNameJWLNDA}_{carrierUser.cuId}_{DateTime.Now.ToString("MMddyyyy")}_{"JWLNDA.pdf"}";

                                            string FilePath2 = objCommon.GetConfigValue("FilePath") + fileNameJWLNDA;

                                            using (FileStream file1 = new FileStream(FilePath2, FileMode.Create, FileAccess.Write))
                                            {
                                                ms.WriteTo(file1);
                                                file1.Close();
                                                ms.Close();

                                            }

                                            string FileLocation2 = objCommon.GetConfigValue("FilePath") + @"\PDFTemplate\MasterBrokerCarrier.html";

                                            var MasterBrokerCarrierHTML = string.Join("", System.IO.File.ReadAllLines(FileLocation2));
                                            string space = new string(' ', 3);
                                            var MasterBrokerCarrierPDF2 = MasterBrokerCarrierHTML.Replace("##DATE##", DateTime.Now.ToString("MM/dd/yyyy")).Replace("##NAME##", carrierUser.authorizedPerson).Replace("##MC##", carrierUser != null ? carrierUser.MC : space).Replace("##DOT##", carrierUser.addtionalDot).Replace("##FEDERALTAXID##", carrierUser.additionalFedaralID)
                                                .Replace("##AUTHORIZEDSIGNATURE##", carrierUser.authorizedSignature).Replace("##TITLE##", carrierUser.title).Replace("##CUEMAIL##", carrierUser.cuEmail).Replace("##PHONE##", carrierUser.telephone);

                                            var MasterBrokerCarrierPDF = GetPDF(MasterBrokerCarrierPDF2);
                                            ms = new MemoryStream(MasterBrokerCarrierPDF);


                                            string strfileMasterBrokerCarrier = carrierUser.additionalPersonName;
                                            if (carrierUser.legalCompanyName != null)
                                            {
                                                strfileMasterBrokerCarrier = strfileMasterBrokerCarrier + "_" + carrierUser.legalCompanyName;
                                            }

                                            string fileMasterBrokerCarrier = $@"/\PDFAttachment/\_{strfileMasterBrokerCarrier}_{carrierUser.cuId}_{DateTime.Now.ToString("MMddyyyy")}_{"MasterBrokerCarrier.pdf"}";
                                            string FilePath3 = objCommon.GetConfigValue("FilePath") + fileMasterBrokerCarrier;

                                            using (FileStream file1 = new FileStream(FilePath3, FileMode.Create, FileAccess.Write))
                                            {
                                                ms.WriteTo(file1);
                                                file1.Close();
                                                ms.Close();
                                            }
                                            carrierUser.ndaUrl = objCommon.GetConfigValue("ApplicationUrl") + fileNameJWLNDA;
                                            carrierUser.mbcaUrl = objCommon.GetConfigValue("ApplicationUrl") + fileMasterBrokerCarrier;

                                            strExecutionLogMessage = "Generating the PDF's Complated and saved in the folder " + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);

                                            clsCommon.ReturnResponse objresponse1 = new clsCommon.ReturnResponse();
                                            objresponse1 = objcarrierUser.UpdateCarrier(carrierUser, "");
                                            if (objresponse1.ResponseVal)
                                            {
                                                strExecutionLogMessage = "Carrier Registration Completed and the URL also updated" + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);

                                                clsCommon.SuccessResponse objSuccessResponse = new clsCommon.SuccessResponse();
                                                objSuccessResponse.reference = cuEmail.ToString();
                                                string strSuccessResponse = JsonConvert.SerializeObject(objSuccessResponse);
                                                DataSet dsSuccessResponse = objCommon.jsonToDataSet(strSuccessResponse);
                                                dsSuccessResponse.Tables[0].TableName = "CarierRegistrationSuccess";
                                                objCommon.WriteDataToCsvFile(dsSuccessResponse.Tables[0],
                                       strInputFilePath, strFileName, strDatetime, "Success");

                                            }
                                            else
                                            {
                                                exceptiondataTable.Rows.Add(dr.ItemArray);
                                                strExecutionLogMessage = "Carrier Registration Success but Update URL Failed" + System.Environment.NewLine;
                                                strExecutionLogMessage += "File Name -" + strFileName + System.Environment.NewLine;
                                                strExecutionLogMessage += "Email Id -" + carrierUser.cuEmail + System.Environment.NewLine;
                                                objCommon.WriteExecutionLog(strExecutionLogMessage);

                                                clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                                objErrorResponse.error = "Carrier Registration Success but Update URL Failed";
                                                objErrorResponse.code = "Carrier Registration Failed ";
                                                objErrorResponse.reference = carrierUser.cuEmail;
                                                string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                                DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                                dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                                objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                       strInputFilePath, strFileName, strDatetime, "Error");
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            exceptiondataTable.Rows.Add(dr.ItemArray);
                                            strExecutionLogMessage = "Carrier Registration Failed " + System.Environment.NewLine;
                                            strExecutionLogMessage += "For Email -" + carrierUser.cuEmail + System.Environment.NewLine;
                                            objCommon.WriteExecutionLog(strExecutionLogMessage);

                                            clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                            objErrorResponse.error = "Found exception while Add Carrier User Details";
                                            objErrorResponse.code = "Carrier Registration Failed ";
                                            objErrorResponse.reference = carrierUser.cuEmail;
                                            string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                            DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                            dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                            objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                                   strInputFilePath, strFileName, strDatetime, "Error");
                                            continue;

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        strExecutionLogMessage = "StartProcessing Exception -" + ex.Message + System.Environment.NewLine;
                                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);

                                        clsCommon.ErrorResponse objErrorResponse = new clsCommon.ErrorResponse();
                                        objErrorResponse.error = ex.Message;
                                        objErrorResponse.code = "Carrier Registration Failed ";
                                        objErrorResponse.reference = cuEmail.ToString();
                                        string strErrorResponse = JsonConvert.SerializeObject(objErrorResponse);
                                        DataSet dsFailureResponse = objCommon.jsonToDataSet(strErrorResponse);
                                        dsFailureResponse.Tables[0].TableName = "CarierRegistrationFailure";
                                        objCommon.WriteDataToCsvFile(dsFailureResponse.Tables[0],
                               strInputFilePath, strFileName, strDatetime, "Error");
                                        continue;
                                    }
                                }
                                j++;
                            }
                            if (exceptiondataTable.Rows.Count > 0)
                            {
                                DataSet dsResult = new DataSet();
                                dsResult.Tables.Add(exceptiondataTable);
                                dsResult.Tables[0].TableName = "Template";
                                clsExcelHelper.ExportOutputtoXLSXFile(dsResult, strInputFilePath, strFileName, strDatetime);
                            }
                        }
                        else
                        {
                            strExecutionLogMessage = "Template sheet data not found for the file " + strInputFilePath + @"\" + strFileName;
                            objCommon.WriteExecutionLog(strExecutionLogMessage);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        strExecutionLogMessage = "StartProcessing Exception -" + ex.Message + System.Environment.NewLine;
                        strExecutionLogMessage += "File Path is  -" + strInputFilePath + System.Environment.NewLine;
                        strExecutionLogMessage += "Found exception while processing the file, filename  -" + strFileName + System.Environment.NewLine;
                        objCommon.WriteErrorLog(ex, strExecutionLogMessage);
                    }
                }

                strExecutionLogMessage = "Process Completed";
                objCommon.WriteExecutionLog(strExecutionLogMessage);
            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex, "StartProcessing - Exception occurred while Processing");
            }
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

        public static byte[] GetPDF(string pHTML)
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

        public static void UpdateCarrierdataInDataTrac(carrierusers carrierUser)
        {
            string strExecutionLogMessage = string.Empty;
            clsCommon objCommon = new clsCommon();
            strExecutionLogMessage = "Update Carrier data In DataTrac Started " + System.Environment.NewLine;
            objCommon.WriteExecutionLog(strExecutionLogMessage);

            amazon_equipment_owner objrequest = new amazon_equipment_owner();
            string amazon_equipment_ownerrequest = null;
            amazon_equipment_ownerrequest = @"'cuId': '" + carrierUser.cuId + "',";
            amazon_equipment_ownerrequest = amazon_equipment_ownerrequest + @"'carrier_type': '" + carrierUser.carriertype + "',";
            amazon_equipment_ownerrequest = amazon_equipment_ownerrequest + @"'nametoprintoncheck': '" + carrierUser.nametoprintoncheck + "',";
            amazon_equipment_ownerrequest = @"{" + amazon_equipment_ownerrequest + "}";


            string amazon_equipment_owner_RequestObject = @"{'amazon_equipment_owner': " + amazon_equipment_ownerrequest + "}";
            clsCommon.ReturnResponse objdatatracresponse = new clsCommon.ReturnResponse();
            clsDatatrac objclsDatatrac = new clsDatatrac();
            JObject jsonobj = JObject.Parse(amazon_equipment_owner_RequestObject);
            string request = jsonobj.ToString();
            objdatatracresponse = objclsDatatrac.DataTrac_amazon_equipment_owner_PutAPI(carrierUser.dtuid, amazon_equipment_owner_RequestObject);
            if (objdatatracresponse.ResponseVal)
            {
                strExecutionLogMessage = "DataTrac_amazon_equipment_owner_PutAPI API Success " + System.Environment.NewLine;
                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                strExecutionLogMessage += "Response -" + objdatatracresponse.Reason + System.Environment.NewLine;
                objCommon.WriteExecutionLog(strExecutionLogMessage);
            }
            else
            {
                strExecutionLogMessage = "DataTrac_amazon_equipment_owner_PutAPI Failed " + System.Environment.NewLine;
                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                strExecutionLogMessage += "Response -" + objdatatracresponse.Reason + System.Environment.NewLine;
                objCommon.WriteExecutionLog(strExecutionLogMessage);
            }
            strExecutionLogMessage = "Update Carrier data In DataTrac Completed " + System.Environment.NewLine;
            objCommon.WriteExecutionLog(strExecutionLogMessage);

        }

        public static void CreateCarrierdataInDataTrac(carrierusers objcarrierusers)
        {
            string strExecutionLogMessage = string.Empty;
            clsCommon objCommon = new clsCommon();
            strExecutionLogMessage = "Create Carrier data In DataTrac Started " + System.Environment.NewLine;
            objCommon.WriteExecutionLog(strExecutionLogMessage);

            amazon_equipment_owner_Request objrequestdetails = new amazon_equipment_owner_Request();
            amazon_equipment_owner objrequest = new amazon_equipment_owner();

            objrequest.owner_id = objcarrierusers.dtuid;
            objrequest.scac_code = Convert.ToString(objcarrierusers.additionalScac);
            objrequest.carrier_type = objcarrierusers.carriertype;
            objrequest.owner_name = objcarrierusers.authorizedPerson;
            objrequest.key_id = objcarrierusers.cuId;

            if (objcarrierusers.status == StatusEnum.Inprocess)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            }
            else if (objcarrierusers.status == StatusEnum.Complete)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.PEND);
            }
            else if (objcarrierusers.status == StatusEnum.Approved)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.APPD);
            }
            else if (objcarrierusers.status == StatusEnum.Rejected)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.TERM);
            }
            else if (objcarrierusers.status == StatusEnum.Onhold)
            {
                objrequest.carrier_status = Convert.ToString(CarrierStatusEnum.HOLD);
            }

            objrequest.ascend_vendor_id = null;
            objrequest.dx_vendor_id = Convert.ToString(objcarrierusers.dx_vendor_id);
            objrequest.check_name = objcarrierusers.nametoprintoncheck;
            objrequestdetails.amazon_equipment_owner = objrequest;

            clsCommon.ReturnResponse objdatatracresponse = new clsCommon.ReturnResponse();
            clsDatatrac objclsDatatrac = new clsDatatrac();
           
            string request = JsonConvert.SerializeObject(objrequestdetails);
            objdatatracresponse = objclsDatatrac.DataTrac_amazon_equipment_owner_PostAPI(objrequestdetails);
            if (objdatatracresponse.ResponseVal)
            {
                strExecutionLogMessage = "DataTrac_amazon_equipment_owner_PostAPI API Success " + System.Environment.NewLine;
                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                strExecutionLogMessage += "Response -" + objdatatracresponse.Reason + System.Environment.NewLine;
                objCommon.WriteExecutionLog(strExecutionLogMessage);
            }
            else
            {
                strExecutionLogMessage = "DataTrac_amazon_equipment_owner_PostAPI Failed " + System.Environment.NewLine;
                strExecutionLogMessage += "Request -" + request + System.Environment.NewLine;
                strExecutionLogMessage += "Response -" + objdatatracresponse.Reason + System.Environment.NewLine;
                objCommon.WriteExecutionLog(strExecutionLogMessage);
            }
            strExecutionLogMessage = "Create Carrier data In DataTrac Completed " + System.Environment.NewLine;
            objCommon.WriteExecutionLog(strExecutionLogMessage);
        }

    }
}
