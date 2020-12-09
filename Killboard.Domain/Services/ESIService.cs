using Killboard.Domain.Interfaces;
using Killboard.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Killboard.Domain.Services
{
    public class ESIService : IESIService
    {
        private const string ESI_URL = "https://esi.evetech.net/latest/";

        private readonly HttpClient _client;

        public ESIService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(ESI_URL)
            };
        }

        public async Task<PublicDataModel> GetPublicData(long charID)
        {
            var task = await _client.GetAsync(ESI_URL + "characters/" + charID + "/");
            var result = await task.Content.ReadAsStringAsync();
            var jsonResult = JsonConvert.DeserializeObject<PublicDataModel>(result);
            _client.Dispose();
            return jsonResult;
        }

    }
}
