using Killboard.Data.Models;
using Killboard.Domain.DTO.Character;
using Killboard.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Killboard.Domain.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly IESIService _esiService;
        private readonly KillboardContext _ctx;

        public CharacterRepository(KillboardContext ctx, IESIService esiService)
        {
            _ctx = ctx;
            _esiService = esiService;
        }

        public GetCharacter GetCharacter(long id) => _ctx.characters.Where(u => u.character_id == id).Select(u => new GetCharacter
        {
            Description = u.description,
            AllianceID = u.alliance_id,
            CharacterID = u.character_id,
            CorporationID = u.corporation_id,
            SecurityStatus = u.security_status,
            Username = u.name
        }).FirstOrDefault();

        public GetCharacterDetail GetCharacterDetail(long id) => (from c in _ctx.characters
                                                                 where c.character_id == id
                                                                 join cp in _ctx.corporations on c.corporation_id equals cp.corporation_id
                                                                 join a in _ctx.alliances on c.alliance_id equals a.alliance_id into all
                                                                 from ali in all.DefaultIfEmpty()
                                                                 select new GetCharacterDetail
                                                                 {
                                                                     Description = c.description,
                                                                     AllianceID = c.alliance_id,
                                                                     CharacterID = c.character_id,
                                                                     CorporationID = c.corporation_id,
                                                                     SecurityStatus = c.security_status,
                                                                     Username = c.name,
                                                                     AllianceName = ali.name,
                                                                     CorporationName = cp.name,
                                                                     Birthday = c.birthday,
                                                                     Gender = c.gender,
                                                                     CorporationTicker = cp.ticker,
                                                                     AllianceTicker = ali.ticker
                                                                 }).FirstOrDefault();

        public IEnumerable<GetCharacter> GetCharacters() => _ctx.characters.Select(u => new GetCharacter
        {
            Description = u.description,
            AllianceID = u.alliance_id,
            CharacterID = u.character_id,
            CorporationID = u.corporation_id,
            SecurityStatus = u.security_status,
            Username = u.name
        });

        public async Task<GetCharacter> AddSSOCharacter(PostSSOCharacter character)
        {
            if (_ctx.characters.Any(a => a.character_id == character.char_id && a.user_id.HasValue)) throw new ApplicationException("Character already associated to a user.");

            if(_ctx.characters.Any(a => a.character_id == character.char_id))
            {
                var charToAssoc = _ctx.characters.FirstOrDefault(a => a.character_id == character.char_id);
                charToAssoc.user_id = character.user_id;
                _ctx.SaveChanges();

                _ctx.access_tokens.Add(new access_tokens
                {
                    char_id = character.char_id,
                    access_token = character.access_token,
                    date_added = DateTime.Now,
                    refresh_token = character.refresh_token,
                    expires_on = DateTime.Now.AddSeconds(character.expires_in)
                });
                _ctx.SaveChanges();

                return new GetCharacter
                {
                    AllianceID = charToAssoc.alliance_id,
                    CharacterID = charToAssoc.character_id,
                    CorporationID = charToAssoc.corporation_id,
                    Description = charToAssoc.description,
                    SecurityStatus = charToAssoc.security_status,
                    Username = charToAssoc.name
                };
            }
            else
            {
                var publicData = await _esiService.GetPublicData(character.char_id);

                if (publicData != null)
                {
                    var newCharacter = _ctx.characters.Add(new characters
                    {
                        character_id = character.char_id,
                        alliance_id = publicData.alliance_id,
                        ancestry_id = publicData.acestry_id,
                        birthday = DateTime.Parse(publicData.birthday),
                        bloodline_id = publicData.bloodline_id,
                        corporation_id = publicData.corporation_id,
                        description = publicData.description,
                        gender = publicData.gender,
                        name = publicData.name,
                        race_id = publicData.race_id,
                        security_status = publicData.security_status,
                        user_id = character.user_id
                    });

                    _ctx.Database.OpenConnection();
                    try
                    {
                        _ctx.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.characters ON");
                        _ctx.SaveChanges();
                        _ctx.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.characters OFF");
                    }
                    finally
                    {
                        _ctx.Database.CloseConnection();
                    }

                    _ctx.access_tokens.Add(new access_tokens
                    {
                        char_id = newCharacter.Entity.character_id,
                        access_token = character.access_token,
                        date_added = DateTime.Now,
                        refresh_token = character.refresh_token,
                        expires_on = DateTime.Now.AddSeconds(character.expires_in)
                    });
                    _ctx.SaveChanges();

                    return new GetCharacter
                    {
                        AllianceID = newCharacter.Entity.alliance_id,
                        CharacterID = newCharacter.Entity.character_id,
                        CorporationID = newCharacter.Entity.corporation_id,
                        Description = newCharacter.Entity.description,
                        SecurityStatus = newCharacter.Entity.security_status,
                        Username = newCharacter.Entity.name
                    };
                }
            }

            return null;
        }
    }
}
