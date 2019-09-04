using Swashbuckle.AspNetCore.Swagger;
using System;

namespace NEMS_API.WebApp.Core.Models
{
    public class SwaggerExampleBodyParameter :  BodyParameter
    {
        /// <summary>
        /// Example string
        /// </summary>
        public string Example { get; set; }

        public Type Type { get; set; }
    }
}
