using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using SystemTask = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Formatters
{
    public class JsonOutputFormatter : TextOutputFormatter
    {
        public JsonOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Clear();
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(Resource).IsAssignableFrom(type))
            {
                return false;
            }

            return base.CanWriteType(type);
        }

        public override SystemTask.Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;

            var buffer = new StringBuilder();

            if(context.ObjectType != typeof(OperationOutcome) || !typeof(Resource).IsAssignableFrom(context.ObjectType))
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                };

                var resource = JsonConvert.SerializeObject(context.Object, settings);

                buffer.Append(resource);
            }

            response.Headers.Remove(HeaderNames.ContentType);
            response.Headers.Add(HeaderNames.ContentType, $"application/json; charset={Encoding.UTF8.WebName}");

            return response.WriteAsync(buffer.ToString(), Encoding.UTF8);
        }

    }
}
