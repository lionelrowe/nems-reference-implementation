using System;

namespace NEMS_API.Models.Core
{
    public class JwtRequest
    {
        public string Iis { get; set; }

        public string Sub { get; set; }

        public string Aud { get; set; }

        public int? Exp { get; set; }

        public int? Iat { get; set; }

        public string ReasonForRequest { get; set; }

        public string Scope { get; set; }

        public string RequestingSystem { get; set; }

        public string RequestingOrganization { get; set; }

        public string RequestingUser { get; set; }

        public string ScopeResource { get; set; }

        public string ScopeAction { get; set; }

        public string OdsCode { get; set; }

        public string Asid { get; set; }

        public string UserId { get; set; }

        public JwtRequest Hydrate()
        {
            if (string.IsNullOrWhiteSpace(Iis))
            {
                Iis = "https://data.developer.nhs.uk/nems-ri/";
            }

            if (!string.IsNullOrWhiteSpace(UserId))
            {
                RequestingUser = Sub = $"https://fhir.nhs.uk/Id/sds-role-profile-id|{UserId}";
            }
            else
            {
                Sub = $"https://fhir.nhs.uk/Id/accredited-system|{Asid}";
            }

            if (string.IsNullOrWhiteSpace(Aud))
            {
                Aud = "https://data.developer.nhs.uk/nems-ri/";
            }

            ReasonForRequest = "directcare";

            Scope = "patient/*.*";

            if(!string.IsNullOrEmpty(ScopeResource) && !string.IsNullOrEmpty(ScopeAction))
            {
                Scope = $"patient/{ScopeResource}.{ScopeAction}";
            }

            if (string.IsNullOrWhiteSpace(RequestingSystem) && string.IsNullOrWhiteSpace(Asid))
            {
                throw new NullReferenceException("Missing ASID value");
            }
            else if(string.IsNullOrWhiteSpace(RequestingSystem))
            {
                RequestingSystem = $"https://fhir.nhs.uk/Id/accredited-system|{Asid}";
            }

            if (string.IsNullOrWhiteSpace(RequestingOrganization) && string.IsNullOrWhiteSpace(OdsCode))
            {
                throw new NullReferenceException("Missing OdsCode value");
            }
            else if (string.IsNullOrWhiteSpace(RequestingOrganization))
            {
                RequestingOrganization = $"https://fhir.nhs.uk/Id/ods-organization-code|{OdsCode}";
            }

            return this;

        }
    }
}
