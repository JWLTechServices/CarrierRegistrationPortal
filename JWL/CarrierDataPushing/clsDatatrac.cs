using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CarrierDataPushing
{
    class clsDatatrac : clsCommon
    {

        public ReturnResponse DataTrac_amazon_equipment_owner_PutAPI(string UniqueId, string jsonreq)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    string url = objCommon.GetConfigValue("DatatracURL", "DatatracSettings") + "/amazon_equipment_owner/" + UniqueId;
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var Username = objCommon.GetConfigValue("DatatracUserName", "DatatracSettings");
                    var Password = objCommon.GetConfigValue("DatatracPassword", "DatatracSettings");

                    UTF8Encoding utf8 = new UTF8Encoding();

                    byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                    string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);

                    JObject jsonobj = JObject.Parse(jsonreq);
                    string payload = jsonobj.ToString();
                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PutAsync(url, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.ResponseVal = true;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;

                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in DataTrac_amazon_equipment_owner_PutAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            return objresponse;
        }


        public ReturnResponse DataTrac_amazon_equipment_owner_PostAPI(amazon_equipment_owner_Request objRequest)
        {
            ReturnResponse objresponse = new ReturnResponse();

            string json = string.Empty;
            clsCommon objCommon = new clsCommon();
            try
            {
                using (var client = new HttpClient())
                {

                    client.Timeout = TimeSpan.FromMinutes(5);
                    string url = objCommon.GetConfigValue("DatatracURL", "DatatracSettings") + "/amazon_equipment_owner";
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                  //  var Username = _settings.DatatracUserName;
                   // var Password = _settings.DatatracPassword;

                    var Username = objCommon.GetConfigValue("DatatracUserName", "DatatracSettings");
                    var Password = objCommon.GetConfigValue("DatatracPassword", "DatatracSettings");

                    UTF8Encoding utf8 = new UTF8Encoding();

                    byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                    string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);

                    string payload = JsonConvert.SerializeObject(objRequest);


                    using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    {
                        content.Headers.ContentType.CharSet = "UTF-8";
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = client.PostAsync(url, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            objresponse.ResponseVal = true;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;

                        }
                        else
                        {
                            objresponse.ResponseVal = false;
                            objresponse.Reason = response.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "exception in DataTrac_amazon_equipment_owner_PostAPI " + ex;
                objresponse.Reason = strExecutionLogMessage;
                objresponse.ResponseVal = false;
                objCommon.WriteExecutionLog(strExecutionLogMessage);
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            return objresponse;
        }
    }
}
