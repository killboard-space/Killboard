using Killboard.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface IESIService
    {
        Task<PublicDataModel> GetPublicData(long charID);
    }
}
