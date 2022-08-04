using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;


namespace CarrierDataPushing
{
   public class clsCommon
    {
     

        public static bool IsException = false;
        public string GetConfigValue(string Key, string Section="")
        {
            IConfiguration Config = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json")
            .Build();
             string retVal = "";
            //retVal = ConfigurationManager.AppSettings[Key];
            if(string.IsNullOrEmpty(Section))
            {
                Section = "Application";
            }
            retVal = Config.GetSection(Section)[Key];
            return retVal;
        }

        public struct ReturnResponse
        {
            public bool ResponseVal;
            public string Reason;

            public ReturnResponse(bool boolResponse = false)
            {
                this.ResponseVal = boolResponse;
                this.Reason = "Some Error";
            }
        }

        public struct DSResponse
        {
            public ReturnResponse dsResp;
            public DataSet DS;
        }

        public class ErrorResponse
        {
            public string error { get; set; }
            public string status { get; set; } = "Error";
            public string code { get; set; }
            public string reference { get; set; }

        }

        public class SuccessResponse
        {
            public string status { get; set; } = "Success";
            public string reference { get; set; }

        }

        public bool SendExceptionMail(string Subject, string Body)
        {
            try
            {
                string fromMail = GetConfigValue("FromMailID");
                string fromPassword = GetConfigValue("FromMailPasssword");
                string Disclaimer = GetConfigValue("MailDisclaimer");
                string toMail = GetConfigValue("ToMailID");
                return SendMail(fromMail, fromPassword, Disclaimer, toMail, "", Subject, Body, "");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "SendExceptionMail");
                return false;
            }
        }

        public bool SendMail(string fromMail, string fromPassword, string Disclaimer, string toMail, string ccMail, string Subject, string Body, string AttachmentPath)
        {
            try
            {
                string AppName = GetConfigValue("ApplicationName");
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

                if (ccMail != "")
                {
                    string[] ccAddress;
                    ccAddress = ccMail.Split(',');
                    foreach (string strCc in ccAddress)
                    {
                        mailMsg.CC.Add(strCc);
                    }
                }

                mailMsg.Subject = Subject;

                Body = Body + "<br/><br/>Regards,<br/>" + AppName + " <br/>Support Team<br/><br/>";

                if (Disclaimer.Trim() != "")
                {
                    Body = Body + "<br/><br/>" + Disclaimer;
                }

                Body = Body.Replace(System.Environment.NewLine, "<br/>");

                mailMsg.Body = Body;
                mailMsg.IsBodyHtml = true;

                if (AttachmentPath.Trim() != "")
                {
                    Attachment att = new Attachment(AttachmentPath);
                    mailMsg.Attachments.Add(att);
                }

                smtpClient.Send(mailMsg);
                return true;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "SendMail");
                return false;
            }
        }

        public void WriteExecutionLog(string strExecutionLogMessage)
        {
            try
            {
                string AppName = GetConfigValue("ApplicationName");
                string strExecutionLogFilePath = GetConfigValue("ExecutionLogFileLocation"); ;

                if (!System.IO.Directory.Exists(strExecutionLogFilePath + @"\"))
                    System.IO.Directory.CreateDirectory(strExecutionLogFilePath + @"\");

                string filepath = strExecutionLogFilePath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                string Message = "Date/Time: " + DateTime.Now.ToString() + " " + strExecutionLogMessage + System.Environment.NewLine;


                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(Message);
                        sw.Flush();
                        sw.Close();
                    }

                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(Message);
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "WriteExecutionLog -" + strExecutionLogMessage);
                throw new Exception("Error in WriteExecutionLog -->" + ex.Message + ex.StackTrace);
            }
            finally
            {

            }
        }

        public void WriteErrorLog(Exception ex, string strExceptionMethod)
        {

            IsException = true;
            string strErrorLogPath;

            strErrorLogPath = GetConfigValue("ErrorLogFileLocation");

            if (!System.IO.Directory.Exists(strErrorLogPath))
                System.IO.Directory.CreateDirectory(strErrorLogPath);

            string filepath = strErrorLogPath + @"\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string ExeMessage = "================================================" + System.Environment.NewLine;
            ExeMessage += "Date/Time: " + DateTime.Now.ToString() + System.Environment.NewLine;
            ExeMessage += "Message: " + ex.Message + System.Environment.NewLine;
            ExeMessage += "Message: " + ex.StackTrace + System.Environment.NewLine;
            if (strExceptionMethod != null)
            {
                ExeMessage += "Exception Occured in the method: " + strExceptionMethod + System.Environment.NewLine;
            }
            ExeMessage += "================================================" + System.Environment.NewLine;
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {

                    sw.WriteLine(ExeMessage);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(ExeMessage);
                }
            }

        }
        public void MoveTheFileToHistoryFolder(string strFolderToCopyTheFilesTo, FileInfo workFile)
        {
            try
            {
                if (Directory.Exists(strFolderToCopyTheFilesTo + @"\"))
                {
                    if (File.Exists(strFolderToCopyTheFilesTo + @"\" + workFile.Name))
                    {
                        string fileName = workFile.Name;
                        int fileExtPos = fileName.LastIndexOf(".");
                        if (fileExtPos >= 0)
                            fileName = fileName.Substring(0, fileExtPos);
                        fileName = strFolderToCopyTheFilesTo + @"\" + fileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                        workFile.MoveTo(fileName); ;
                    }
                    else
                    {
                        workFile.MoveTo(strFolderToCopyTheFilesTo + @"\" + workFile.Name);
                    }
                }
                else
                {
                    Directory.CreateDirectory(strFolderToCopyTheFilesTo + @"\");
                    workFile.MoveTo(strFolderToCopyTheFilesTo + @"\" + workFile.Name);
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in MoveTheFileToHistoryFolder" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);
            }
        }


        public DataSet jsonToDataSet(string jsonString, string type = null)
        {
            DataSet ds = new DataSet();
            try
            {
                XmlDocument xd = new XmlDocument();
                jsonString = "{ \"rootNode\": {" + jsonString.Trim().TrimStart('{').TrimEnd('}') + "} }";
                xd = (XmlDocument)JsonConvert.DeserializeXmlNode(jsonString);

                ds.ReadXml(new XmlNodeReader(xd));

            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, "jsonToDataSet");

            }
            return ds;
        }
        public void WriteDataToCsvFile(System.Data.DataTable dataTable, string strInputFilePath, string fileName, string Datetime,string type)
        {
            try
            {

                string strOutputFileLocation;
                string strOutputFile;

                strOutputFileLocation = strInputFilePath + @"\Outputs" + @"\" + type;

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);


                strOutputFile = fileName + "-" + dataTable.TableName + "-" + Datetime;// + ".xlsx";
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".csv"; // ".csv";

                StringBuilder fileContent = new StringBuilder();
                StringBuilder HeaderContent = new StringBuilder();

                if (!File.Exists(strOutputFile))
                {
                    foreach (var col in dataTable.Columns)
                    {
                        HeaderContent.Append(col.ToString() + ",");
                    }
                    HeaderContent.Replace(",", System.Environment.NewLine, HeaderContent.Length - 1, 1);
                    File.WriteAllText(strOutputFile, HeaderContent.ToString());
                }

                foreach (DataRow dr in dataTable.Rows)
                {
                    foreach (var column in dr.ItemArray)
                    {
                        fileContent.Append("\"" + column.ToString() + "\",");
                    }

                    fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

                }
                File.AppendAllText(strOutputFile, fileContent.ToString());
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in WriteDataToCsvFile" + System.Environment.NewLine;
                WriteErrorLog(ex, strExecutionLogMessage);

            }
        }



    }

}
