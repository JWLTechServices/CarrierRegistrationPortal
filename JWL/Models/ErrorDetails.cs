using Newtonsoft.Json;
using System;

namespace Models
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public String Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
