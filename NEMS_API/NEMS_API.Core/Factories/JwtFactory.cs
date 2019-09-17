using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using NEMS_API.Models.Core;
using NEMS_API.Core.Resources;

namespace NEMS_API.Core.Factories
{
    public class JwtFactory
    {
        public static string Generate(JwtRequest jwtRequest, DateTime? tokenStart = null)
        {
            return $"{Header}.{Payload(jwtRequest, tokenStart)}.";
        }

        private static string Header => new JwtHeader().Base64UrlEncode();

        private static string Payload(JwtRequest jwtRequest, DateTime? tokenStart)
        {
            var start = tokenStart.HasValue ? tokenStart.Value : DateTime.UtcNow;
            var claims = new List<Claim>();
            var exp = jwtRequest.Exp.HasValue ? jwtRequest.Exp.Value : EpochTime.GetIntDate(start.AddMinutes(5));
            var iat = jwtRequest.Iat.HasValue ? jwtRequest.Iat.Value : EpochTime.GetIntDate(start);

            claims.Add(new Claim(FhirConstants.JwtClientSysIssuer, jwtRequest.Iis, ClaimValueTypes.String));
            claims.Add(new Claim(FhirConstants.JwtIndOrSysIdentifier, jwtRequest.Sub, ClaimValueTypes.String));
            claims.Add(new Claim(FhirConstants.JwtEndpointUrl, jwtRequest.Aud, ClaimValueTypes.String));
            claims.Add(new Claim(FhirConstants.JwtExpiration, exp.ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(FhirConstants.JwtIssued, iat.ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim(FhirConstants.JwtReasonForRequest, jwtRequest.ReasonForRequest, ClaimValueTypes.String));
            claims.Add(new Claim(FhirConstants.JwtScope, jwtRequest.Scope, ClaimValueTypes.String));
            claims.Add(new Claim(FhirConstants.JwtRequestingSystem, jwtRequest.RequestingSystem, ClaimValueTypes.String)); 
            claims.Add(new Claim(FhirConstants.JwtRequestingOrganization, jwtRequest.RequestingOrganization, ClaimValueTypes.String)); 
            

            if(!string.IsNullOrWhiteSpace(jwtRequest.RequestingUser))
            {
                claims.Add(new Claim(FhirConstants.JwtRequestingUser, jwtRequest.RequestingUser, ClaimValueTypes.String));
            }

            return new JwtPayload(claims).Base64UrlEncode();
        }

    }
}
