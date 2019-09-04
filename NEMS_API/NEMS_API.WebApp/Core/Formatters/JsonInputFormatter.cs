using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Text;
using SystemTask = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Formatters
{
    public class JsonInputFormatter : TextInputFormatter
    {
        public JsonInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Clear();
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanReadType(Type type)
        {
            if (typeof(Resource).IsAssignableFrom(type))
            {
                return false; 
            }

            return base.CanReadType(type);
        }

        public override SystemTask.Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {

            var request = context.HttpContext.Request;

            using (var streamReader = context.ReaderFactory(request.Body, encoding))
            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
            {
                var type = context.ModelType;

                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var resource = serializer.Deserialize(jsonReader);

                    return InputFormatterResult.SuccessAsync(resource);
                }
                catch (Exception ex)
                {
                    context.ModelState.AddModelError("InputFormatter", ex.Message);

                    return InputFormatterResult.FailureAsync();
                }
            }
        }

    }
}
