using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NEMS_API.Models.Core
{
    public class SubscriptionCriteria
    {
        public SubscriptionCriteria(string criteria)
        {
            Parameters = new List<(string Key, string Value)>();

            if(string.IsNullOrWhiteSpace(criteria) || !criteria.Contains("?") || !criteria.StartsWith("/"))
            {
                return;
            }

            //I'm assuming there is always a query and path is relative
            var criteriaSplit = criteria.IndexOf("?");
            var criteriaUri = new UriBuilder("http", "fakedomain.com", 80, criteria.Substring(0, criteriaSplit), criteria.Substring(criteriaSplit)).Uri;
            var criteriaQuery = !string.IsNullOrWhiteSpace(criteriaUri.Query) ? criteriaUri.Query.Substring(1).Split("&").ToList() : new List<string>();

            if(criteriaQuery.Count > 0)
            {
                Parameters.Add(("prefix", $"{criteriaUri.AbsolutePath}?{criteriaQuery.First()}"));

                criteriaQuery.ForEach(item => 
                {

                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        var listItem = item.Split("=").ToList();
                        if (listItem.Count == 2)
                        {
                            Parameters.Add((listItem.ElementAt(0).ToLowerInvariant(), WebUtility.UrlDecode(listItem.ElementAt(1))));
                        }
                    }
                });
            }
        }

        public List<(string Key, string Value)> Parameters { get; set; }
      
    }
}
