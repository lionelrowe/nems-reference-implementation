using Hl7.Fhir.Model;
using NEMS_API.Models.Core;
using NEMS_API.Models.Interfaces;
using System;
using System.Web;

namespace NEMS_API.Models.FhirResources
{
    public class NemsSubscription : Subscription, IDataItem
    {
        public NemsSubscription()
        {

        }

        public string RequesterOdsCode { get; set; }

        public string RequesterAsid { get; set; }

        public NemsSubscription(Subscription subscription)
        {
            Status = subscription.Status;
            Contact = subscription.Contact;
            End = subscription.End;
            Reason = subscription.Reason;
            Criteria = subscription.Criteria;
            Channel = subscription.Channel;
            Meta = subscription.Meta;          
        }

        public string CriteriaDecoded
        {
            get
            {
                return this.Criteria;
            }

            set
            {
                this.Criteria = HttpUtility.HtmlDecode(value);
            }
        }

        public string CacheKey
        {
            get
            {
                return CacheKeys.SubscriptionEntries;
            }
        }

        public void SetMeta()
        {
            if(string.IsNullOrEmpty(Id))
            {
                Id = Guid.NewGuid().ToString();
            }

            if(Meta == null)
            {
                Meta = new Meta();
            }

            Meta.LastUpdated = DateTime.UtcNow;
            Meta.VersionId = Guid.NewGuid().ToString();

        }

        public static Subscription ToSubscription(NemsSubscription nemsSubscription)
        {
            var subscription = new Subscription();
            subscription.Meta = nemsSubscription.Meta;
            subscription.Status = nemsSubscription.Status;
            subscription.Contact = nemsSubscription.Contact;
            subscription.End = nemsSubscription.End;
            subscription.Reason = nemsSubscription.Reason;
            subscription.Criteria = nemsSubscription.Criteria;
            subscription.Channel = nemsSubscription.Channel;

            return subscription;
        }
    }
}
