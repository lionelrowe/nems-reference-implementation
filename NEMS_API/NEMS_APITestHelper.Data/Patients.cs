using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace NEMS_APITestHelper.Data
{
    public class Patients
    {
        public static Patient Patient_2686033207
        {
            get
            {
                return new Patient
                {
                    Meta = new Meta
                    {
                        Profile = new string[]
                        {
                          "https://fhir.nhs.uk/STU3/StructureDefinition/CareConnect-EMS-Patient-1"
                        }
                    },
                    Id = "d29154ba-f000-4f0c-a567-124a349fe466",
                    Identifier = new List<Identifier>
                    {
                        new Identifier {
                          System =  "https://fhir.nhs.uk/Id/nhs-number",
                          Value =  "2686033207",
                        }
                    },
                    Name = new List<HumanName>
                    {
                        new HumanName
                        {
                            Use = HumanName.NameUse.Official,
                            Family = "Chalmers",
                            Given =  new string [] { "Peter", "James" }
                        }
                    },
		            Gender = AdministrativeGender.Male,
		            BirthDate = "1974-12-25"
                };
            }

        }
    }
}
