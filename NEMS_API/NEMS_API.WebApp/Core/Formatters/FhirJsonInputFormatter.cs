﻿using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
//using NRLS_API.Core.Exceptions;
//using NRLS_API.Core.Factories;
using System;
using System.Text;
using SystemTask = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Formatters
{
    public class FhirJsonInputFormatter : TextInputFormatter
    {
        public FhirJsonInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/fhir+json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json+fhir"));

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

            using (var streamReader = context.ReaderFactory(request.Body, encoding))
            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
            {
                var type = context.ModelType;

                try
                {
                    //TODO: parse json

                    //TODO: create a simple model to allow passthrough to validation because a missing element will throw wrong error here
                    var resource = new FhirJsonParser().Parse(jsonReader, type);
                    return InputFormatterResult.SuccessAsync(resource);
                }
                catch (Exception ex)
                {
                    //TODO: Remove Fhir Hack
                    if (ex != null)
                    {
                        //Assuming invalid json here, see above
                        return InputFormatterResult.SuccessAsync("TODO: OperationOutcomeFactory.CreateInvalidRequest()");
                    }

                    return InputFormatterResult.FailureAsync();
                }
            }
        }

    }
}
