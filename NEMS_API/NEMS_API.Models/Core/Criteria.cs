using System.Collections.Generic;
using System.Linq;

namespace NEMS_API.Models.Core
{
    public class Criteria
    {

        public Criteria(string criteriaQuery)
        {
            MessageHeaderEvents = new List<string>();

            PatientAges = new List<string>();

            var queryItems = criteriaQuery.Substring(8).ToLowerInvariant().Split("&");

            foreach(var item in queryItems)
            {
                if (item.StartsWith("type"))
                {
                    Type = item.Split("=")[1].ToUpperInvariant();
                }

                if (item.StartsWith("servicetype"))
                {
                    ServiceType = item.Split("=")[1].ToUpperInvariant();
                }

                if (item.StartsWith("patient.identifier"))
                {
                    PatientIdentifier = item.Split("=")[1].ToUpperInvariant();
                }

                if (item.StartsWith("messageheader.event"))
                {
                    MessageHeaderEvents.Add(item.Split("=")[1].ToUpperInvariant());
                }

                if (item.StartsWith("patient.age"))
                {
                    PatientAges.Add(item.Split("=")[1]);
                }
            }
        }

        public string Type { get; set; }

        public string ServiceType { get; set; }

        public string PatientIdentifier { get; set; }

        public List<string> MessageHeaderEvents { get; set; } 

        public List<string> PatientAges { get; set; }

        public bool ValidAge(int age) => CheckAge(PatientAges, age);


        public bool CheckAge(List<string> patientAges, int age)
        {
            var younger = false;
            var older = false;

            if (!patientAges.Any())
            {
                return true;
            }

            var lessThan = patientAges.FirstOrDefault(x => x.Contains("lt"));
            var moreThan = patientAges.FirstOrDefault(x => x.Contains("gt"));

            if (lessThan == null && moreThan == null)
            {
                if(int.TryParse(lessThan, out int expectedAge))
                {
                    return expectedAge == age;
                }

                return false;
            }

            if(lessThan != null && int.TryParse(lessThan.Replace("lt", ""), out int younderThan))
            {
                if(age < younderThan)
                {
                    younger = true;
                }
            }

            if (moreThan != null && int.TryParse(moreThan.Replace("gt", ""), out int olderThan))
            {
                if (age > olderThan)
                {
                    older = true;
                }
            }

            return older && younger;
        }

    }
}
