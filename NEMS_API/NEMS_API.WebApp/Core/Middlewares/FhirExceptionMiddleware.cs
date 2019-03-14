using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Models.Core;
using NEMS_API.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Middlewares
{
    public class FhirExceptionMiddleware
    {
        private readonly IHostingEnvironment _env;
        private NemsApiSettings _nemsApiSettings;

        public FhirExceptionMiddleware(IHostingEnvironment env, IOptions<NemsApiSettings> nemsApiSettings)
        {
            _env = env;
            _nemsApiSettings = nemsApiSettings.Value;
        }

        public async SystemTasks.Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            HttpFhirException ooException = null;

            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null) return;

            if (ex.GetType() == typeof(HttpFhirException))
            {
                ooException = ex as HttpFhirException;
                context.Response.StatusCode = (int)ooException.StatusCode;
            }

            var diagnostics = "An unknown error has occured.";

            if (_env.IsDevelopment())
            {
                diagnostics = $"Message: {ex.Message}. StackTrace: {ex.StackTrace}";

                if (ex.InnerException != null)
                {
                    diagnostics = $"Message: {ex.Message}. StackTrace: {ex.StackTrace}. Inner Exception Stack Trace: {ex.InnerException.StackTrace}";
                }
            }

            var outcome = ooException?.OperationOutcome != null ? ooException.OperationOutcome : OperationOutcomeFactory.CreateInternalError(diagnostics);
            string output;

            if (UseJsonOutput(context))
            {
                output = new FhirJsonSerializer().SerializeToString(outcome);
                context.Response.ContentType = ContentType.JSON_CONTENT_HEADER;
            }
            else
            {
                output = new FhirXmlSerializer().SerializeToString(outcome);
                output = Regex.Replace(output, @"((\s){1})(/>)", "/>", RegexOptions.Compiled);
                context.Response.ContentType = ContentType.XML_CONTENT_HEADER;
            }

            await context.Response.WriteAsync(output, Encoding.UTF8);

            return;
        }

        private bool UseJsonOutput(HttpContext context)
        {
            var formatKey = "_format";
            var acceptKey = HeaderNames.Accept;

            var parameters = context.Request.QueryString.Value.GetParameters();

            bool hasFormatParam = parameters?.FirstOrDefault(x => x.Item1 == "_format") != null;
            string formatParam = parameters?.GetParameter(formatKey);

            string acceptHeader = null;
            bool hasAcceptHeader = context.Request.Headers.ContainsKey(acceptKey);
            if (hasAcceptHeader)
            {
                acceptHeader = context.Request.Headers[acceptKey];
            }

            var jsonFormat = hasFormatParam && !string.IsNullOrWhiteSpace(formatParam) && _nemsApiSettings.SupportedContentTypes.Contains(formatParam) && formatParam.ToLowerInvariant().Contains("json");
            var jsonAccept = hasAcceptHeader && !string.IsNullOrWhiteSpace(acceptHeader) && ValidAccept(acceptHeader) && acceptHeader.ToLowerInvariant().Contains("json");

            return jsonFormat || jsonAccept;
        }

        private bool ValidAccept(string accept)
        {
            foreach (var type in _nemsApiSettings.SupportedContentTypes)
            {
                if (accept.Contains(type))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
