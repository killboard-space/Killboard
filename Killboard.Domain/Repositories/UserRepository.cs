using Killboard.Data.Models;
using Killboard.Domain.DTO.Character;
using Killboard.Domain.DTO.User;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly KillboardContext _ctx;
        private readonly IMailer _mailer;
        private readonly IConfiguration _configuration;

        public UserRepository(KillboardContext ctx, IMailer mailer, IConfiguration configuration)
        {
            _ctx = ctx;
            _configuration = configuration;
            _mailer = mailer;
        }

        public GetUser GetUser(int id) => _ctx.users.Where(u => u.user_id == id).Select(u => new GetUser
        {
            username = u.username,
            user_id = u.user_id
        }).FirstOrDefault();

        public IEnumerable<GetUser> GetUsers() => _ctx.users.Select(u => new GetUser
        {
            username = u.username,
            user_id = u.user_id
        });

        public GetUser Authenticate(PostAuthenticateUser postAuthenticate)
        {
            var user = _ctx.users.FirstOrDefault(u => u.username == postAuthenticate.Username);
            
            if (user == null) return null;
            
            var hashed = HashPassword(postAuthenticate.Password, user.salt);
            if(user.hash == hashed) return new GetUser
            {
                username = user.username,
                user_id = user.user_id
            };
            return null;
        }

        public GetUser AddUser(PostUser user)
        {
            if (_ctx.users.Any(u => u.username == user.Username)) throw new ApplicationException("Username already exists!");

            if (_ctx.users.Any(u => u.email == user.Email)) throw new ApplicationException("Email already exists!");

            if (user.CharacterID.HasValue)
            {
                if (_ctx.characters.Any(c => c.character_id == user.CharacterID && c.user_id.HasValue))
                    throw new ApplicationException("CharacterID supplied is already associated with an account!");
            }

            var (hash, salt) = HashPassword(user.Password);

            var entry = _ctx.Add(new users
            {
                create_ip = "API",
                create_time = DateTime.Now,
                username = user.Username,
                hash = hash,
                salt = salt,
                email = user.Email
            });

            _ctx.SaveChanges();

            if (entry.Entity.user_id == default || !user.CharacterID.HasValue)
                return new GetUser
                {
                    username = entry.Entity.username,
                    user_id = entry.Entity.user_id
                };

            var character = _ctx.characters.FirstOrDefault(c => c.character_id == user.CharacterID);

            if (character == null)
                return new GetUser
                {
                    username = entry.Entity.username,
                    user_id = entry.Entity.user_id
                };

            character.user_id = entry.Entity.user_id;
            _ctx.characters.Update(character);
            _ctx.SaveChanges();

            return new GetUser
            {
                username = entry.Entity.username,
                user_id = entry.Entity.user_id
            };
        }

        public string GetEmail(int id) => _ctx.users.Where(u => u.user_id == id).Select(u => u.email).FirstOrDefault();

        public List<GetCharacter> GetCharacters(int id) => _ctx.characters.Where(c => c.user_id == id).Select(c => new GetCharacter 
        {
            AllianceID = c.alliance_id,
            CharacterID = c.character_id,
            CorporationID = c.corporation_id,
            Description = c.description,
            SecurityStatus = c.security_status,
            Username = c.name
        }).ToList();

        public Dictionary<int, string> GetCharactersList(int id) => _ctx.characters.Where(c => c.user_id == id).ToDictionary(c => c.character_id, c => c.name);

        public async Task AddResetRequest(ForgetUser user)
        {
            if (!string.IsNullOrEmpty(user.Email))
            {
                var userDetails = _ctx.users.Where(u => u.email == user.Email).Select(u => new { u.user_id, u.username }).FirstOrDefault();

                if(userDetails != default)
                {
                    var request = new reset_requests
                    {
                        create_time = DateTime.Now,
                        expire_time = DateTime.Now.AddHours(12),
                        user_id = userDetails.user_id,
                        hash = HashPassword($"{user.Email}", Encoding.UTF8.GetBytes(userDetails.user_id.ToString()))
                    };

                    await _ctx.reset_requests.AddAsync(request);
                    await _ctx.SaveChangesAsync();

                    var body = BuildRequestEmail(userDetails.username, request.hash);

                    await _mailer.SendEmailAsync(new List<string> { user.Email }, "Password Reset", body);
                }
            }
        }

        public bool ValidateKey(KeyValidation validation)
        {
            return !string.IsNullOrEmpty(validation.Key) &&
                   _ctx.reset_requests.Any(r => r.hash == validation.Key && r.expire_time > DateTime.Now);
        }

        public bool ChangePassword(ChangeRequest request)
        {
            if (string.IsNullOrEmpty(request.Key) || string.IsNullOrEmpty(request.Password) ||
                !ValidateKey(new KeyValidation {Key = request.Key})) return false;

            var details = (from r in _ctx.reset_requests
                where r.hash == request.Key
                join u in _ctx.users on r.user_id equals u.user_id
                select new { user = u, reset = r }).FirstOrDefault();

            if (details == null) return false;
            (details.user.hash, details.user.salt) = HashPassword(request.Password);

            _ctx.reset_requests.Remove(details.reset);
            _ctx.users.Update(details.user);
            _ctx.SaveChanges();
            return false;
        }

        private string BuildRequestEmail(string userName, string hash)
        {
            if (string.IsNullOrEmpty(hash)) throw new ApplicationException("Hash Generation failed, empty/null hash string.");

            var mailBuilder = new StringBuilder();

            var url = $"{_configuration.GetSection("WebURL").Value}/forget?key={hash}";

            mailBuilder.AppendLine($"<div>Dear {userName},</div><br />");
            mailBuilder.AppendLine($"<div>Someone has requested a link to reset the password associated to your account.</div>You can click <a href='{url}'>Here</a> or follow the link below to start the process.</div><br />");
            mailBuilder.AppendLine($"<small><a href='{url}'>{url}</a></small><br />");
            mailBuilder.AppendLine($"<b>Note</b>:<small>If this wasn't you, you can disregard this email.</small>");

            return mailBuilder.ToString();
        }

        private static (string, byte[]) HashPassword(string password)
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return (Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA1,
                10000,
                256 / 8)), salt);
        }

        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA1,
                10000,
                256 / 8));
        }
    }
}
