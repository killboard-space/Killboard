using Killboard.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Universe.Types;

namespace Killboard.Domain.Interfaces
{
    public interface IESIService
    {
        Task<PublicDataModel> GetPublicData(long charID);
        Task<IEnumerable<int>> GetItems();
        Task<Item> GetItemDetail(int itemId);
        Task<Group> GetGroupDetail(int groupId);
        Task<Category> GetCategoryDetail(int categoryId);
    }
}
