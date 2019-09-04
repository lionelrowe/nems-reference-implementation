using NEMS_API.Models.Core;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface ISdsService
    {
        SdsViewModel GetFor(string asid);

        SdsViewModel GetFor(string odsCode, string interactionId);

        List<SdsViewModel> GetAll();
    }
}
