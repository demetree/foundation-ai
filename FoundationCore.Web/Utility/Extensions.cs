using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.Extensions
{
    /*
     * 
     * Source: https://gist.github.com/damianh/5d69be0e3004024f03b6cc876d7b0bd3

    Courtesy of Damian Hickey.
     * 
     * */

    public static partial class MvcExtensions
    {
        /// <summary>
        /// Finds the appropriate controllers
        /// </summary>
        /// <param name="partManager">The manager for the parts</param>
        /// <param name="controllerTypes">The controller types that are allowed. </param>
        public static void UseSpecificControllers(this ApplicationPartManager partManager, params Type[] controllerTypes)
        {
            partManager.FeatureProviders.Add(new InternalControllerFeatureProvider());
            partManager.ApplicationParts.Clear();
            partManager.ApplicationParts.Add(new SelectedControllersApplicationParts(controllerTypes));
        }

        /// <summary>
        /// Only allow selected controllers
        /// </summary>
        /// <param name="mvcCoreBuilder">The builder that configures mvc core</param>
        /// <param name="controllerTypes">The controller types that are allowed. </param>
        public static IMvcCoreBuilder UseSpecificControllers(this IMvcCoreBuilder mvcCoreBuilder, params Type[] controllerTypes) => mvcCoreBuilder.ConfigureApplicationPartManager(partManager => partManager.UseSpecificControllers(controllerTypes));

        /// <summary>
        /// Only instantiates selected controllers, not all of them. Prevents application scanning for controllers. 
        /// </summary>
        private class SelectedControllersApplicationParts : ApplicationPart, IApplicationPartTypeProvider
        {
            public SelectedControllersApplicationParts()
            {
                Name = "Only allow selected controllers";
            }

            public SelectedControllersApplicationParts(Type[] types)
            {
                Types = types.Select(x => x.GetTypeInfo()).ToArray();
            }

            public override string Name { get; }

            public IEnumerable<TypeInfo> Types { get; }
        }

        /// <summary>
        /// Ensure that internal controllers are also allowed. The default ControllerFeatureProvider hides internal controllers, but this one allows it. 
        /// </summary>
        private class InternalControllerFeatureProvider : ControllerFeatureProvider
        {
            private const string ControllerTypeNameSuffix = "Controller";

            /// <summary>
            /// Determines if a given <paramref name="typeInfo"/> is a controller. The default ControllerFeatureProvider hides internal controllers, but this one allows it. 
            /// </summary>
            /// <param name="typeInfo">The <see cref="TypeInfo"/> candidate.</param>
            /// <returns><code>true</code> if the type is a controller; otherwise <code>false</code>.</returns>
            protected override bool IsController(TypeInfo typeInfo)
            {
                if (!typeInfo.IsClass)
                {
                    return false;
                }

                if (typeInfo.IsAbstract)
                {
                    return false;
                }

                if (typeInfo.ContainsGenericParameters)
                {
                    return false;
                }

                if (typeInfo.IsDefined(typeof(Microsoft.AspNetCore.Mvc.NonControllerAttribute)))
                {
                    return false;
                }

                if (!typeInfo.Name.EndsWith(ControllerTypeNameSuffix, StringComparison.OrdinalIgnoreCase) &&
                           !typeInfo.IsDefined(typeof(Microsoft.AspNetCore.Mvc.ControllerAttribute)))
                {
                    return false;
                }

                return true;
            }
        }
    }


    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 
        /// This builds a string representing the HTTP Request.  There is no method on the .Net 8 Request class that does this.
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetRawRequest(this HttpRequest request)
        {
            StringBuilder resource = new StringBuilder();

            resource.Append(request.Path);
            if (request.QueryString.HasValue == true)
            {
                //resource.Append("?");  // request.QueryString starts with a ? in my testing.  However, complain if it it doesn't just in case..

                if (request.QueryString.Value.StartsWith("?") == false)
                {
                    System.Diagnostics.Debug.Assert(false);     // why does the query string not start with a question mark?
                }

                resource.Append(request.QueryString.Value);
            }


            StringBuilder headers = new StringBuilder();

            foreach (var header in request.Headers)
            {
                headers.Append($"{header.Key}: {header.Value}\r\n");
            }

            string body = "";
            using (StreamReader sr = new StreamReader(request.Body))
            {
                body = sr.ReadToEnd();
            }

            return $"{request.Method} {resource} {request.Protocol}\r\n{headers}\r\n{body}";
        }

        public static async Task<string> GetRawRequestAsync(this HttpRequest request)
        {
            StringBuilder resource = new StringBuilder();

            resource.Append(request.Path);
            if (request.QueryString.HasValue == true)
            {
                //resource.Append("?");  // request.QueryString starts with a ? in my testing.  However, complain if it it doesn't just in case..

                if (request.QueryString.Value.StartsWith("?") == false)
                {
                    System.Diagnostics.Debug.Assert(false);     // why does the query string not start with a question mark?
                }

                resource.Append(request.QueryString.Value);
            }


            StringBuilder headers = new StringBuilder();

            foreach (var header in request.Headers)
            {
                headers.Append($"{header.Key}: {header.Value}\r\n");
            }

            string body = "";
            using (StreamReader sr = new StreamReader(request.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            return $"{request.Method} {resource} {request.Protocol}\r\n{headers}\r\n{body}";
        }
    }
}