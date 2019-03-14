using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Models.Core;
using NEMS_API.Services;
using NEMS_API.WebApp.Core.Formatters;
using NEMS_API.WebApp.Core.Middlewares;

namespace NEMS_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc(config =>
            {
                // Add FHIR Content Negotiation
                config.RespectBrowserAcceptHeader = true;
                config.ReturnHttpNotAcceptable = false;

                config.InputFormatters.Clear();
                config.OutputFormatters.Clear();

                //Default to fhir+xml
                config.InputFormatters.Insert(0, new FhirXmlInputFormatter());
                config.OutputFormatters.Insert(0, new FhirXmlOutputFormatter());

                //JSON not yet supported in NEMS
                //config.InputFormatters.Insert(1, new FhirJsonInputFormatter());
                //config.OutputFormatters.Insert(1, new FhirJsonOutputFormatter());
            });
            services.AddOptions();
            services.Configure<NemsApiSettings>(options =>
            {
                options.BundleProfileUrl = Configuration.GetSection("NEMSAPI:BundleProfileUrl").Value;
                options.SubscriptionProfileUrl = Configuration.GetSection("NEMSAPI:SubscriptionProfileUrl").Value;
                options.HeaderProfileUrl = Configuration.GetSection("NEMSAPI:HeaderProfileUrl").Value;
                options.SubscriptionCriteriaRules = SetList(Configuration.GetSection("NEMSAPI:SubscriptionCriteriaRules").Value);
                options.ServiceTypeCodes = SetList(Configuration.GetSection("NEMSAPI:ServiceTypeCodes").Value);
            });

            services.AddTransient<IValidationHelper, ValidationHelper>();
            services.AddTransient<IFhirCacheHelper, FhirCacheHelper>();
            services.AddTransient<IFhirValidation, FhirValidation>();
            services.AddTransient<IPublishService, PublishService>();
            services.AddTransient<ISubscribeService, SubscribeService>();
        }

        private List<string> SetList(string configVal)
        {
            if(!string.IsNullOrEmpty(configVal))
            {
                return configVal.Split(",").ToList();
            }

            return new List<string>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<NemsApiSettings> nemsApiSettings)
        {
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = new FhirExceptionMiddleware(env, nemsApiSettings).Invoke
            });

            app.UseCors(builder => builder.WithOrigins(new[] { "*" }).WithMethods(new[] { "GET", "POST", "PUT", "DELETE" }).AllowAnyHeader());

            //handle compression as per spec
            app.UseFhirInputMiddleware();

            //TODO inbound logger
            //TODO outbound logger

            app.UseMvc();
        }
    }
}
