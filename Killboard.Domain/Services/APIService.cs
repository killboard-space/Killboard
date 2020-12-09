using Killboard.Domain.DTO.Alliance;
using Killboard.Domain.DTO.Character;
using Killboard.Domain.DTO.Corporation;
using Killboard.Domain.DTO.User;
using Killboard.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Services
{
    public class APIService : IAPIService
    {
        private readonly string BASE_URL;
        private readonly string API_KEY;

        private readonly IConfiguration _configuration;
        private readonly HttpClient _client;

        public APIService(IConfiguration configuration)
        {
            _configuration = configuration;

            BASE_URL = _configuration["API_URL"];
            API_KEY = _configuration["Killboard:ApiKey"];

            _client = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL)
            };
            _client.DefaultRequestHeaders.Add("X-API-KEY", API_KEY);
        }

        public async Task<GetUser> PostAuthenticate(PostAuthenticateUser user)
        {
            var response = await _client.PostAsync($"{BASE_URL}/User/Authenticate", new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetUser>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<GetCharacter> PostNewCharacter(PostSSOCharacter character)
        {
            var response = await _client.PostAsync($"{BASE_URL}/Character/SSO", new StringContent(JsonConvert.SerializeObject(character), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GetCharacter>(jsonStr);
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<GetUser> PostNewUser(PostUser user)
        {
            var response = await _client.PostAsync($"{BASE_URL}/User", new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetUser>(jsonStr);
            }

            if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<bool> SendForgetPasswordRequest(string email)
        {
            var response = await _client.PostAsync($"{BASE_URL}/User/Forget", new StringContent(JsonConvert.SerializeObject(new ForgetUser { Email = email }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return false;
        }

        public async Task<bool> ValidateKey(string key)
        {
            var response = await _client.PostAsync($"{BASE_URL}/User/KeyValidation", new StringContent(JsonConvert.SerializeObject(new KeyValidation { Key = key }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return bool.TryParse(content, out var success) && success;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return false;
        }

        public async Task<bool> ChangePassword(string key, string password)
        {
            var response = await _client.PostAsync($"{BASE_URL}/User/Change", new StringContent(JsonConvert.SerializeObject(new ChangeRequest { Key = key, Password = password}), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return bool.TryParse(content, out var success) && success;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return false;
        }

        public async Task<string> GetEmail(int userId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/User/{userId}/Email");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return jsonStr;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<List<GetCharacter>> GetCharacters(int userId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/User/{userId}/Characters");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<GetCharacter>>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<Dictionary<int, string>> GetCharacterList(int userId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/User/{userId}/Characters/short");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Dictionary<int, string>>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<GetCharacter> GetCharacter(int characterId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/Character/{characterId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetCharacter>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<GetCharacterDetail> GetCharacterDetail(long characterId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/Character/{characterId}/detail");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetCharacterDetail>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<GetCorporationDetail> GetCorporationDetail(int corporationId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/Corporation/{corporationId}/detail");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetCorporationDetail>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }

        public async Task<GetAllianceDetail> GetAllianceDetail(int allianceId)
        {
            var response = await _client.GetAsync($"{BASE_URL}/Alliance/{allianceId}/detail");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<GetAllianceDetail>(jsonStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(errStr);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var errStr = await response.Content.ReadAsStringAsync();
                throw new Exception(errStr);
            }
            return null;
        }
    }
}
