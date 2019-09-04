using NEMS_API.Models.Core;
using System;

namespace NEMS_API.Core.Interfaces.Helpers
{
    public interface IJwtHelper
    {
        Response IsValidUser(string jwt);

        Response IsValid(string jwt, string[] validScopes, DateTime? tokenIssued);
    }
}
