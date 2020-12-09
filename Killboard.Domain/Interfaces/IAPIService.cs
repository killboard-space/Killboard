using Killboard.Domain.DTO.Alliance;
using Killboard.Domain.DTO.Character;
using Killboard.Domain.DTO.Corporation;
using Killboard.Domain.DTO.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface IAPIService
    {
        Task<GetCharacter> PostNewCharacter(PostSSOCharacter character);
        Task<GetUser> PostNewUser(PostUser user);
        Task<GetUser> PostAuthenticate(PostAuthenticateUser user);
        Task<bool> SendForgetPasswordRequest(string email);
        Task<bool> ValidateKey(string key);
        Task<bool> ChangePassword(string key, string password);
        Task<string> GetEmail(int userId);
        Task<List<GetCharacter>> GetCharacters(int userId);
        Task<Dictionary<int, string>> GetCharacterList(int userId);
        Task<GetCharacter> GetCharacter(int characterId);
        Task<GetCharacterDetail> GetCharacterDetail(long characterId);
        Task<GetCorporationDetail> GetCorporationDetail(int corporationId);
        Task<GetAllianceDetail> GetAllianceDetail(int allianceId);
    }
}
