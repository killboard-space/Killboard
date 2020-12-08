using Killboard.Domain.DTO.Character;
using Killboard.Domain.DTO.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface IUserRepository
    {
        IEnumerable<GetUser> GetUsers();
        GetUser GetUser(int id);
        GetUser AddUser(PostUser user);
        GetUser Authenticate(PostAuthenticateUser postAuthenticate);
        Task AddResetRequest(ForgetUser user);
        bool ValidateKey(KeyValidation validation);
        bool ChangePassword(ChangeRequest request);
        string GetEmail(int id);
        List<GetCharacter> GetCharacters(int id);
        Dictionary<int, string> GetCharactersList(int id);
    }
}
