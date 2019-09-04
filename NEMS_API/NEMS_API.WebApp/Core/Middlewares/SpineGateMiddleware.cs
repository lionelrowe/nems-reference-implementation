using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NEMS_API.Core.Exceptions;
using NEMS_API.Core.Factories;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Core.Resources;
using NEMS_API.Models.Core;
using System;
using System.Linq;
using System.Net;
using SystemTasks = System.Threading.Tasks;

namespace NEMS_API.WebApp.Core.Middlewares
{
    public class SpineGateMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly NemsApiSettings _nemsApiSettings;
        private readonly ISdsService _sdsService;
        private readonly IJwtHelper _jwtHelper;

        public SpineGateMiddleware(RequestDelegate next, IOptions<NemsApiSettings> nemsApiSettings, ISdsService sdsService, IJwtHelper jwtHelper)
        {
            _next = next;
            _nemsApiSettings = nemsApiSettings.Value;
            _sdsService = sdsService;
            _jwtHelper = jwtHelper;
        }

        public async SystemTasks.Task Invoke(HttpContext context)
        {
            //Order of validation is Important
            var request = context.Request;
            var headers = request.Headers;
            var method = request.Method;
            var endpointPath = request.Path;


            //Accept is optional but must be valid if supplied
            //Check is delegated to FhirInputMiddleware

            var interactionIdMap = _nemsApiSettings.InteractionIdMap.FirstOrDefault(x => endpointPath.ToString().EndsWith(x.EndPoint) && x.HttpMethod.ToUpperInvariant().Equals(method.ToUpperInvariant()));
            if (interactionIdMap == null)
            {
                SetJwtError(HeaderNames.Authorization, "The request does not match the allowed interactions.");
            }

            var clincialScopes = interactionIdMap.ClinicalScopes("patient");

            var authorization = GetHeaderValue(headers, HeaderNames.Authorization);
            var jwtResponse = _jwtHelper.IsValid(authorization, clincialScopes, null);
            if (!jwtResponse.Success)
            {
                SetJwtError(HeaderNames.Authorization, jwtResponse.Message);
            }

            var fromASID = GetHeaderValue(headers, FhirConstants.HeaderFromAsid);
            var clientCache = _sdsService.GetFor(fromASID);

            if (clientCache == null)
            {
                SetError(FhirConstants.HeaderFromAsid, null);
            }

            var toASID = GetHeaderValue(headers, FhirConstants.HeaderToAsid);
            if (string.IsNullOrEmpty(toASID) || toASID != _nemsApiSettings.SpineASID)
            {
                SetError(FhirConstants.HeaderToAsid, null);
            }

            //Interaction 
            var interactionID = GetHeaderValue(headers, FhirConstants.HeaderInteractionID);
            if (string.IsNullOrEmpty(interactionID) || !interactionID.Equals(interactionIdMap.InteractionId))
            {
                SetError(FhirConstants.HeaderInteractionID, null);
            }

            //We've Passed! Continue to App...
            await _next.Invoke(context);
            return;

        }

        private string GetHeaderValue(IHeaderDictionary headers, string header)
        {
            string headerValue = null;

            if (headers.ContainsKey(header))
            {
                var check = headers[header];

                if (!string.IsNullOrWhiteSpace(check))
                {
                    headerValue = check;
                }
            }

            return headerValue;
        }

        private void SetError(string header, string diagnostics)
        {
            //throw new HttpFhirException("Invalid/Missing Header", OperationOutcomeFactory.CreateInvalidHeader(header, diagnostics), HttpStatusCode.BadRequest);
        }

        private void SetJwtError(string header, string diagnostics)
        {
            //throw new HttpFhirException("Invalid/Missing Header", OperationOutcomeFactory.CreateInvalidJwtHeader(header, diagnostics), HttpStatusCode.BadRequest);
        }
    }

    public static class SpineGateMiddlewareExtension
    {
        public static IApplicationBuilder UseSpineGateMiddleware(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<SpineGateMiddleware>();
        }
    }
}
