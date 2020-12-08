using Killboard.Domain.DTO.Universe.System;
using Killboard.Tools.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Killboard.Tools.Domain.Services
{
    public class KillboardAPIService : IKillboardAPIService
    {
        public HttpClient Client { get; set; }

        private readonly IConfiguration _configuration;

        public KillboardAPIService(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            client.BaseAddress = new Uri(_configuration.GetSection("API_URL").Value);
            Client = client;
        }

        public async Task<int[]> GetSystemsWithinRange(int systemId, int jumps)
        {
            var systemsInRange = Array.Empty<int>();
            await Client.GetAsync($"/api/Route/range/{systemId}/{jumps}")
                .ContinueWith(async (routeSearch) =>
                {
                    var response = await routeSearch;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        systemsInRange = JsonConvert.DeserializeObject<int[]>(jsonString);
                    }
                });
            return systemsInRange;
        }

        public async Task<List<GetSystem>> GetAllSystems()
        {
            var systems = new List<GetSystem>();
            var currentPage = 1;
            var totalPages = 0;

            const string nextUrl = "/api/system?pagesize=1500&pagenumber={0}";

            do
            {
                await Client.GetAsync(string.Format(nextUrl, currentPage))
                    .ContinueWith(async (systemSearch) =>
                    {
                        var response = await systemSearch;
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<IEnumerable<GetSystem>>(jsonString);
                            if (result != null)
                            {
                                if (totalPages == default)
                                    totalPages = int.Parse(response.Headers.GetValues("X-Pages").FirstOrDefault() ?? string.Empty);
                                if (result.Any())
                                    systems.AddRange(result.ToList());

                                currentPage++;
                            }
                        }
                    });
            } while (currentPage <= totalPages);
            return systems;
        }
    }
}
