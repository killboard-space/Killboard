using Killboard.Domain.DTO.Character;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Models;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Killboard.Domain.Services
{
    public class UserService : IUserService
    {
        //private const string ESI_URL = "https://esi.evetech.net/latest/";
        private const string HOST_URL = "login.eveonline.com";
        private const string AUTH_ISSUER = "https://login.eveonline.com/";
        private readonly List<string> authIssuers = new List<string>
        {
            "https://login.eveonline.com",
            "login.eveonline.com"
        };

        private HttpClient client;

        public UserService()
        {

        }

        public async Task<PostSSOCharacter> SignInWithSSO(string clientId, string secretKey, string authToken, CancellationToken ct = default)
        {
            client = new HttpClient();
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + secretKey));

            var body = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authToken)
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            client.DefaultRequestHeaders.Host = HOST_URL;
            
            var response = await client.PostAsync("https://login.eveonline.com/v2/oauth/token", body, ct);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var json = JsonConvert.DeserializeObject<CallbackModel>(content);

                var tokenResult = await ValidateToken(json, ct);
                
                if (tokenResult == null) return null;

                var charId = tokenResult.Claims.SingleOrDefault(a => a.Type == "sub");

                return new PostSSOCharacter
                {
                    char_id = int.Parse(charId.Value.Split(":").Skip(2).SingleOrDefault()),
                    access_token = json.access_token,
                    expires_in = json.expires_in,
                    token_type = json.token_type,
                    refresh_token = json.refresh_token
                };
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

        private async Task<JwtSecurityToken> ValidateToken(CallbackModel callback, CancellationToken ct = default)
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                AUTH_ISSUER + ".well-known/oauth-authorization-server",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            if (string.IsNullOrEmpty(callback.access_token)) throw new ArgumentNullException(nameof(callback.access_token));

            var discDoc = await configurationManager.GetConfigurationAsync(ct);

            var signingKeys = discDoc.SigningKeys;

            var validationParams = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateAudience = false,
                ValidIssuers = authIssuers,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ClockSkew = TimeSpan.FromMinutes(2),
            };
            try
            {
                _ = new JwtSecurityTokenHandler()
                    .ValidateToken(callback.access_token, validationParams, out var rawValidToken);
                return (JwtSecurityToken)rawValidToken;
            }
            catch (SecurityTokenValidationException) // Catches tampered tokens (when/if people actually do this)
            {
                return null;
            }
        }

        public async Task<CallbackModel> RefreshToken(string clientId, string secretKey, string refreshToken)
        {
            client = new HttpClient();
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + secretKey));

            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            client.DefaultRequestHeaders.Host = HOST_URL;

            var response = await client.PostAsync("https://login.eveonline.com/v2/oauth/token", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<CallbackModel>(result);
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
