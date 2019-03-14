using System;
using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Models.Core
{
    public class SubscriptionCriteria
    {
        public SubscriptionCriteria(string criteria)
        {
            var criteriaUri = new Uri(criteria);
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
