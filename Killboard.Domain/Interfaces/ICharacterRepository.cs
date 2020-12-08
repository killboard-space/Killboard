using Killboard.Domain.DTO.Character;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface ICharacterRepository
    {
        IEnumerable<GetCharacter> GetCharacters();
        GetCharacter GetCharacter(int id);
        GetCharacterDetail GetCharacterDetail(int id);
        Task<GetCharacter> AddSSOCharacter(PostSSOCharacter character);
    }
}
