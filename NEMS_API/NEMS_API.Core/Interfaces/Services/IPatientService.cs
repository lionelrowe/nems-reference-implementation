using Hl7.Fhir.Model;
using System.Collections.Generic;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IPatientService
    {
        IEnumerable<Patient> GetPatients();

        Bundle GetPatientBundle();

        Patient GetPatient(string nhsNumber);
    }
}
