using Killboard.Domain.DTO.Character;
using Killboard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface IUserService
    {
        Task<PostSSOCharacter> SignInWithSSO(string clientId, string secretKey, string authToken, CancellationToken ct = default);
        Task<CallbackModel> RefreshToken(string clientId, string secretKey, string refreshToken);
    }
}
