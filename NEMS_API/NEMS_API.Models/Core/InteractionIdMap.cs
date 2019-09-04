using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Models.Core
{
    public class InteractionIdMap
    {
        public string EndPoint { get; set; }

        public string HttpMethod { get; set; }

        public string InteractionId { get; set; }

        public string ResourceType { get; set; }

        public List<string> Scopes { get; set; }

        public string[] ClinicalScopes(string scopeContext)
        {

            if(Scopes == null || new List<string> { scopeContext, ResourceType }.Concat(Scopes).Any(x => string.IsNullOrWhiteSpace(x)))
            {
                return new string[0];
            }

            var scopes = new List<string>();

            Scopes.ForEach((scope) => {
                scopes.Add($"{scopeContext}/{ResourceType}.{scope}");
            });

            return scopes.ToArray();
        }
    }
}
