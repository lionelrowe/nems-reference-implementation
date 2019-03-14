using System;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Models.Core
{
    public class SubscriptionCriteria
    {
        public SubscriptionCriteria(string criteria)
        {
            //I'm assuming there is always a query and path is relative
            var criteriaSplit = criteria.IndexOf("?");
            var criteriaUri = new UriBuilder("http", "fakedomain.com", 80, criteria.Substring(0, criteriaSplit), criteria.Substring(criteriaSplit)).Uri;
            var criteriaQuery = !string.IsNullOrWhiteSpace(criteriaUri.Query) ? criteriaUri.Query.Substring(1).Split("&").ToList() : null;

            Base = criteriaUri.AbsolutePath;

            Parameters = new List<(string Key, string Value)>();

            if(criteriaQuery != null && criteriaQuery.Count > 0)
            {
                criteriaQuery.ForEach(item => {

                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        var listItem = item.Split("=").ToList();
                        if (listItem.Count == 2)
                        {
                            Parameters.Add((listItem.ElementAt(0), listItem.ElementAt(1)));
                        }
                    }
                });
            }
        }

        public string Base { get; set; }

        public List<(string Key, string Value)> Parameters { get; set; }
      
    }
}
