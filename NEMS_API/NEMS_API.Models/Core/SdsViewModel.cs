using NEMS_API.Models.Interfaces;
using System;
using System.Collections.Generic;

namespace NEMS_API.Models.Core
{
    public class SdsViewModel : IDataItem
    {
        public SdsViewModel()
        {
            WorkflowIds = new List<string>();
        }

        public string Id { get; set; }

        public Guid PartyKey { get; set; }

        public string Fqdn { get; set; }

        public IEnumerable<Uri> EndPoints { get; set; }

        public string OdsCode { get; set; }

        public IEnumerable<string> Interactions { get; set; }

        public string Asid { get; set; }

        public string Thumbprint { get; set; }

        public string MeshMailboxId { get; set; }

        public IEnumerable<string> WorkflowIds { get; set; }

        //public bool Active { get; set; }

        public string CacheKey
        {
            get
            {
                return CacheKeys.SdsEntries;
            }
        }

    }
}
