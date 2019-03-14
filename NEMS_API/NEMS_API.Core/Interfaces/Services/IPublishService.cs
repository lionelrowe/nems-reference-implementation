using Hl7.Fhir.Model;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.Core.Interfaces.Services
{
    public interface IPublishService
    {
        SystemTasks.Task<OperationOutcome> PublishEvent(Bundle bundle);
    }
}
