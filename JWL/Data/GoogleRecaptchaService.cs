using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Data
{
    public class GoogleRecaptchaService
    {
        private readonly RecaptchaSettings _settings;

        public GoogleRecaptchaService(IOptions<RecaptchaSettings> settings)
        {
            _settings = settings.Value;
        }
        public virtual async Task<GoogleResponse> Verification(string token)
        {
            GoogleRequest googleRequest = new GoogleRequest()
            {
                response = token,
                secret = _settings.SecretKey
            };
            HttpClient httpClient = new HttpClient();

            var res = await httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={googleRequest.secret}&response={googleRequest.response}");
            GoogleResponse googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(res);
            return googleResponse;

        }
    }
}
