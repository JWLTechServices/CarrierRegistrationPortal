using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DatatracService
    {
        private readonly DatatracSettings _settings;

        public DatatracService(IOptions<DatatracSettings> settings)
        {
            _settings = settings.Value;
        }
        public virtual async Task<HttpResponseMessage> amazon_equipment_owner_Post(amazon_equipment_owner_Request objRequest)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage responseMessage = null;
           // amazon_equipment_owner_Response datatracResponse = default;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                string url = _settings.DatatracUrl + "/amazon_equipment_owner";
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var Username = _settings.DatatracUserName;
                var Password = _settings.DatatracPassword;

                UTF8Encoding utf8 = new UTF8Encoding();

                byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);

                string payload = JsonConvert.SerializeObject(objRequest);

                using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                {
                    content.Headers.ContentType.CharSet = "UTF-8";
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // responseMessage = await client.PostAsync(url, content);
                    // datatracResponse = JsonConvert.DeserializeObject<amazon_equipment_owner_Response>(response.ToString());

                    //if (response.IsSuccessStatusCode)
                    //{
                    //    datatracResponse = JsonConvert.DeserializeObject<DatatrackAPI_amazon_equipment_owner_Response>(response.ToString());
                    //    //objresponse.ResponseVal = true;
                    //}
                    //else
                    //{
                    //    //  objresponse.ResponseVal = false;
                    //    // objresponse.Reason = response.Content.ReadAsStringAsync().Result;

                    //}

                    try
                    {
                        responseMessage = await client.PostAsync(url, content);

                    }
                    catch (Exception ex)
                    {
                        if (responseMessage == null)
                        {
                            responseMessage = new HttpResponseMessage();
                        }
                        responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                        responseMessage.ReasonPhrase = string.Format("RestHttpClient.SendRequest failed: {0}", ex);
                    }
                }

            }
            return responseMessage;
        }

        public virtual async Task<HttpResponseMessage> amazon_equipment_owner_Put(string UniqueId, string jsonreq)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage responseMessage = null;
           // amazon_equipment_owner_Response datatracResponse = default;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                string url = _settings.DatatracUrl + "/amazon_equipment_owner/" + UniqueId;
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var Username = _settings.DatatracUserName;
                var Password = _settings.DatatracPassword;

                UTF8Encoding utf8 = new UTF8Encoding();

                byte[] encodedBytes = utf8.GetBytes(Username + ":" + Password);
                string userCredentialsEncoding = Convert.ToBase64String(encodedBytes);
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + userCredentialsEncoding);

                JObject jsonobj = JObject.Parse(jsonreq);
                string payload = jsonobj.ToString();
                //  string payload = JsonConvert.SerializeObject(objRequest);

                using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                {
                    content.Headers.ContentType.CharSet = "UTF-8";
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // responseMessage = await client.PostAsync(url, content);
                    // datatracResponse = JsonConvert.DeserializeObject<amazon_equipment_owner_Response>(response.ToString());

                    //if (response.IsSuccessStatusCode)
                    //{
                    //    datatracResponse = JsonConvert.DeserializeObject<DatatrackAPI_amazon_equipment_owner_Response>(response.ToString());
                    //    //objresponse.ResponseVal = true;
                    //}
                    //else
                    //{
                    //    //  objresponse.ResponseVal = false;
                    //    // objresponse.Reason = response.Content.ReadAsStringAsync().Result;

                    //}

                    try
                    {
                        responseMessage = await client.PutAsync(url, content);

                    }
                    catch (Exception ex)
                    {
                        if (responseMessage == null)
                        {
                            responseMessage = new HttpResponseMessage();
                        }
                        responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                        responseMessage.ReasonPhrase = string.Format("RestHttpClient.SendRequest failed: {0}", ex);
                    }
                }

            }
            return responseMessage;
        }
    }
}
