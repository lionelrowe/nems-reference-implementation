using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NEMS_API.Core.Helpers;
using NEMS_API.WebApp.Core.Filters.Attributes;
using NEMS_API.WebApp.Core.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;

namespace NEMS_API.WebApp.Core.Filters
{
    public class ParameterContentTypeOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (!context.ApiDescription.TryGetMethodInfo(out var methodInfo))
            {
                return;
            }

            var requestAttributes = methodInfo.GetCustomAttributes(true).OfType<SwaggerParameterContentTypeAttribute>().FirstOrDefault();

            if (requestAttributes != null)
            {
                //operation.Consumes.Remove("*/*");

                operation.Consumes.Add(requestAttributes.ContentType);

                var resource = operation.Parameters.FirstOrDefault(x => x.Name == requestAttributes.Name);
                var resourceIndex = operation.Parameters.IndexOf(resource);
                operation.Parameters.RemoveAt(resourceIndex);

                var basePath = DirectoryHelper.GetBaseDirectory();

                var examplePath = Path.Combine(basePath, "Data", requestAttributes.Exampleurl);

                var exampleString = File.Exists(examplePath) ? File.ReadAllText(examplePath) : "";
                //exampleString = exampleString.Replace('"', '\"');

                //var schema = new FhirJsonParser().Parse<Bundle>(exampleString);


                //var jsonExample = JsonConvert.DeserializeObject<Schema>(exampleString, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

                //(operation.Parameters.ElementAt(resourceIndex) as SwaggerExampleBodyParameter).Example = exampleString;

                operation.Parameters.Add(new SwaggerExampleBodyParameter
                {
                    Name = resource.Name,
                    In = resource.In,
                    Description = resource.Description,
                    Required = requestAttributes.Required,
                    Example = exampleString
                    //Schema = jsonExample
                });
            }
        }
    }
}
