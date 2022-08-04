using System;

namespace Models
{
    public class RecaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
    }
    public class GoogleRequest
    {
        public string response { get; set; }//token
        public string secret { get; set; }
    }
    public class GoogleResponse
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
