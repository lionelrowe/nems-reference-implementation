using System;

namespace NEMS_API.WebApp.Core.Filters.Attributes
{
    /// <summary>
    /// SwaggerParameterContentTypeAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerParameterContentTypeAttribute : Attribute
    {
        /// <summary>
        /// SwaggerParameterContentTypeAttribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contentType"></param>
        /// <param name="description"></param>
        /// <param name="exampleUrl"></param>
        /// <param name="required"></param>
        public SwaggerParameterContentTypeAttribute(string name, string contentType,  string description, string exampleUrl, bool required = false)
        {
            Name = name;
            ContentType = contentType;
            Description = description;
            Exampleurl = exampleUrl;
            Required = required;
        }

        /// <summary>
        /// Name of Parameter
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of Parameter
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Is the Parameter required?
        /// </summary>
        public bool Required { get; private set; }

        /// <summary>
        /// ContentType - Is added to consumes list
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Location of local example file
        /// </summary>
        public string Exampleurl { get; private set; }

    }
}
