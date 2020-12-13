using Killboard.Domain.Interfaces;
using Killboard.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Universe.Types;
using Killboard.Domain.Utils;

namespace Killboard.Domain.Services
{
    public class ESIService : IESIService
    {
        private const int MaxRetryCount = 10;
        private const string EsiUrl = "https://esi.evetech.net/latest/";

        private readonly HttpClient _client;

        public ESIService()
        {
            _client = new HttpClient(new RetryHandler(new HttpClientHandler()), true)
            {
                BaseAddress = new Uri(EsiUrl)
            };
        }

        public async Task<PublicDataModel> GetPublicData(long charId)
        {
            var task = await _client.GetAsync(EsiUrl + "characters/" + charId + "/");
            var result = await task.Content.ReadAsStringAsync();
            var jsonResult = JsonConvert.DeserializeObject<PublicDataModel>(result);
            return jsonResult;
        }

        public async Task<IEnumerable<int>> GetItems()
        {
            // Send initial request for items
            var response = await _client.GetAsync(EsiUrl + "universe/types/");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Request to get all Items failed | {response.StatusCode}\n{error}");
            }

            var items = new List<int>();

            // Multiple pages
            if (response.Headers.TryGetValues("X-Pages", out var vals) && int.TryParse(vals.FirstOrDefault(), out var pages) && pages > 1)
            {
                // 1 is already done, start at 2 until the end.
                for (var i = 2; i <= pages; i++)
                {
                    // Send another paginated request
                    var pageResponse = _client.GetAsync(EsiUrl + $"universe/types?page={i}").Result;

                    if (!pageResponse.IsSuccessStatusCode) continue;

                    var raw = await pageResponse.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<List<int>>(raw);
                    items.AddRange(results);
                }
            }
            else // Single page
            {
                var raw = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<List<int>>(raw);
                items.AddRange(results);
            }

            return items;
        }

        public async Task<Item> GetItemDetail(int itemId)
        {
            var response = await _client.GetAsync($"{EsiUrl}universe/types/{itemId}/");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Request to get Item detail for Item Id: {itemId} failed | {response.StatusCode}\n{error}");
            }

            var jsonStr = await response.Content.ReadAsStringAsync();

            return jsonStr.Length > 0 ? JsonConvert.DeserializeObject<Item>(jsonStr) : null;
        }

        public async Task<Group> GetGroupDetail(int groupId)
        {
            var response = await _client.GetAsync($"{EsiUrl}universe/groups/{groupId}/");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Request to get Group detail for Group Id: {groupId} failed | {response.StatusCode}\n{error}");
            }

            var jsonStr = await response.Content.ReadAsStringAsync();

            return jsonStr.Length > 0 ? JsonConvert.DeserializeObject<Group>(jsonStr) : null;
        }

        public async Task<Category> GetCategoryDetail(int categoryId)
        {
            var response = await _client.GetAsync($"{EsiUrl}universe/categories/{categoryId}/");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Request to get Category detail for Category Id: {categoryId} failed | {response.StatusCode}\n{error}");
            }

            var jsonStr = await response.Content.ReadAsStringAsync();

            return jsonStr.Length > 0 ? JsonConvert.DeserializeObject<Category>(jsonStr) : null;
        }
    }
}
