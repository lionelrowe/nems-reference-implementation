using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using NEMS_API.Core.Factories;
//using NRLS_API.Core.Exceptions;
//using NRLS_API.Core.Factories;
using System;
using System.IO;
using System.Text;
using System.Xml;
using SystemTask = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Formatters
{
    public class FhirXmlInputFormatter : TextInputFormatter
    {
        public FhirXmlInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/fhir+xml"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml+fhir"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("*/*"));

            SupportedEncodings.Clear();
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanReadType(Type type)
        {
            if (typeof(Resource).IsAssignableFrom(type))
            {
                return base.CanReadType(type);
            }
            return false;
        }

        public override SystemTask.Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var request = context.HttpContext.Request;

            if(request.ContentType.ToUpperInvariant().Contains("TEXT/PLAIN"))
            {
                return InputFormatterResult.SuccessAsync("");
            }

            using (var streamReader = context.ReaderFactory(request.Body, encoding))
            using (XmlTextReader xmlReader = new XmlTextReader(streamReader))
            {
                var type = context.ModelType;

                try
                {
                    var settings = new ParserSettings
                    {
                        AllowUnrecognizedEnums = true,
                        AcceptUnknownMembers = false
                    };

                    var resource = new FhirXmlParser(settings).Parse(xmlReader, type);
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
