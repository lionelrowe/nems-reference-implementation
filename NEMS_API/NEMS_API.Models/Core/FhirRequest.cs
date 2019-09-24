using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;

namespace NEMS_API.Models.Core
{
    public class FhirRequest
    {
        public string Id { get; set; }

        public ResourceType? ResourceType { get; set; }

        public string StrResourceType => ResourceType.ToString();

        public Resource Resource { get; set; }

        //public Uri RequestUrl { get; set; }

        public string RequestingAsid { get; set; }

        public static FhirRequest Create(string id, Resource resource, HttpRequest request, string requestingAsid)
        {
            return new FhirRequest
            {
                Id = id,
                ResourceType = resource?.ResourceType,
                Resource = resource,
                //RequestUrl = CreateUrl(request.Scheme, request.Host.Value, request.Path, request.QueryString.Value),
                RequestingAsid = requestingAsid
            };
        }

        //public static Uri CreateUrl(string scheme, string host, string path, string queryString)
        //{
        //    return new Uri($"{scheme}://{host}{path}{queryString}");
        //}

    }
}
