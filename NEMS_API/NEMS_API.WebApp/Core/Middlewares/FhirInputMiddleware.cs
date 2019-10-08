using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Models.Core;
using NEMS_API.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Middlewares
{
    public class FhirInputMiddleware
    {
        private readonly RequestDelegate _next;
        private NemsApiSettings _nemsApiSettings;

        public FhirInputMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async SystemTasks.Task Invoke(HttpContext context, IOptions<NemsApiSettings> nemsApiSettings)
        {
            CheckRequestRequirements(context);

            _nemsApiSettings = nemsApiSettings.Value;

            //Default the content-type
            //if(!context.Request.Headers.ContainsKey(HeaderNames.ContentType) || string.IsNullOrWhiteSpace(context.Request.Headers[HeaderNames.ContentType]))
            //{
            //    var contentTypeHeader = new MediaTypeHeaderValue(ContentType.XML_CONTENT_HEADER);

            //    context.Request.Headers.Remove(HeaderNames.ContentType);
            //    context.Request.Headers.Add(HeaderNames.ContentType, new StringValues(contentTypeHeader.ToString()));
            //}

            var formatKey = "_format";
            var acceptKey = HeaderNames.Accept;

            var parameters = context.Request.QueryString.Value.GetParameters();

            bool hasFormatParam = parameters?.FirstOrDefault(x => x.Item1 == formatKey) != null;
            string formatParam = parameters?.GetParameter(formatKey);

            string acceptHeader = null;
            bool hasAcceptHeader = context.Request.Headers.ContainsKey(acceptKey);
            if (hasAcceptHeader)
            {
                acceptHeader = context.Request.Headers[acceptKey];
            }

            var validFormatParam = !hasFormatParam || (!string.IsNullOrWhiteSpace(formatParam) && ValidContentType(formatParam));
            var validAcceptHeader = !hasAcceptHeader || (!string.IsNullOrWhiteSpace(acceptHeader) && ValidContentType(acceptHeader));

            if (!validFormatParam && (hasFormatParam || !validAcceptHeader))
            {
                throw new HttpFhirException("Unsupported Media Type", OperationOutcomeFactory.CreateInvalidMediaType(), HttpStatusCode.UnsupportedMediaType);
            }

            if (validFormatParam && hasFormatParam)
            {
                var accepted = ContentType.GetResourceFormatFromFormatParam(formatParam);
                if (accepted != ResourceFormat.Unknown)
                {
                    var newAcceptHeader = ContentType.XML_CONTENT_HEADER;

                    if (accepted == ResourceFormat.Json)
                    {
                        newAcceptHeader = ContentType.JSON_CONTENT_HEADER;
                    }

                    var header = new MediaTypeHeaderValue(newAcceptHeader);

                    context.Request.Headers.Remove(acceptKey);
                    context.Request.Headers.Add(acceptKey, new StringValues(header.ToString()));
                }
            }

            await this._next(context);
        }

        private void CheckRequestRequirements(HttpContext context)
        {
            var contentLength = context?.Request?.ContentLength;
            var type = context?.Request?.Method;
            if (new string[] { HttpMethods.Post, HttpMethods.Put }.Contains(type) && (!context.Request.Path.Value.Contains("messageexchange") && (!contentLength.HasValue || contentLength.Value == 0)))
            {
                throw new HttpFhirException("Invalid Request", OperationOutcomeFactory.CreateInvalidRequest(), HttpStatusCode.BadRequest);
            }
        }

        private bool ValidContentType(string type)
        {
            return _nemsApiSettings.SupportedContentTypes.Select(x => x.Value).Contains(type);
        }
    }

    public static class FhirInputMiddlewareExtensions
    {
        public static IApplicationBuilder UseFhirInputMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FhirInputMiddleware>();
        }
    }
}
