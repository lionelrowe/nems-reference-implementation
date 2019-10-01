using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NEMS_API.Core.Helpers;
using NEMS_API.Core.Interfaces.Data;
using NEMS_API.Core.Interfaces.Helpers;
using NEMS_API.Core.Interfaces.Services;
using NEMS_API.Data;
using NEMS_API.Models.Core;
using NEMS_API.Services;
using NEMS_API.WebApp.Core.Filters;
using NEMS_API.WebApp.Core.Formatters;
using NEMS_API.WebApp.Core.Middlewares;
using Swashbuckle.AspNetCore.Swagger;

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

                //JSON not yet supported in NEMS
                config.InputFormatters.Insert(0, new FhirJsonInputFormatter());
                config.OutputFormatters.Insert(0, new FhirJsonOutputFormatter());

                //Default to fhir+xml
                config.InputFormatters.Insert(1, new FhirXmlInputFormatter());
                config.OutputFormatters.Insert(1, new FhirXmlOutputFormatter());

                config.InputFormatters.Insert(0, new WebApp.Core.Formatters.JsonInputFormatter());
                config.OutputFormatters.Insert(0, new WebApp.Core.Formatters.JsonOutputFormatter());
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "NEMS API Reference Implementation",
                    Description = "A reference implementation of the NEMS API which conforms to the NEMS Technical Specification.",
                    Contact = new Contact()
                    {
                        Name = "NEMS Team",
                        Email = "nems@nhs.net"
                    }
                });
             
                //https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.0&tabs=visual-studio
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<ParameterContentTypeOperationFilter>();
            });
            services.AddOptions();
            services.Configure<NemsApiSettings>(Configuration.GetSection("NEMSAPI"));

            services.AddTransient<IValidationHelper, ValidationHelper>();
            services.AddTransient<IFhirResourceHelper, FhirResourceHelper>();
            services.AddTransient<IFhirValidation, FhirValidation>();
            services.AddTransient<IPublishService, PublishService>();
            services.AddTransient<ISubscribeService, SubscribeService>();
            services.AddTransient<IStaticCacheHelper, StaticCacheHelper>();
            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IFhirUtilities, FhirUtilities>();
            services.AddTransient<IJwtHelper, JwtHelper>();
            services.AddTransient<ISdsService, SdsService>();
            services.AddTransient<IFileHelper, FileHelper>();
            services.AddTransient<IDataWriter, CacheDataWriter>();
            services.AddTransient<IDataReader, CacheDataReader>();
            services.AddTransient<ISchemaValidationHelper, SchemaValidationHelper>();
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

            app.UseWhen(context => context.Request.Path.StartsWithSegments(new PathString("/nems-ri/STU3")) && !nemsApiSettings.Value.SkipSpineGateWay,
                a => a.UseSpineGateMiddleware()
            );

            //TODO inbound logger
            //TODO outbound logger

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/nems-ri/swagger/v1/swagger.json", "NEMS Reference Implementation");
                //c.InjectStylesheet("/static/swagger/ui/custom.css");
                c.DefaultModelsExpandDepth(-1);
                c.EnableDeepLinking();
                //c.IndexStream = () => File.OpenText(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "static", "swagger", "ui", "index.html")).BaseStream;
                c.DocumentTitle = "NEMS API Reference Implementation - Explore with Swagger";
            });

            app.UseMvc();
        }
    }
}
