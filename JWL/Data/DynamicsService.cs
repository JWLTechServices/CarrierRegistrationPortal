using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DynamicsService
    {
        private readonly DynamicsSettings _settings;

        public DynamicsService(IOptions<DynamicsSettings> settings)
        {
            _settings = settings.Value;
        }
    }
}
