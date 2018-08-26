using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http; 
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace WebApi.CustomControllerSelector
{
    public class NamespaceHttpControllerSelector : IHttpControllerSelector
    {
        private readonly HttpConfiguration config;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> controllers;

        public NamespaceHttpControllerSelector(HttpConfiguration config)
        {
            this.config = config;
            controllers = new Lazy<Dictionary<string, HttpControllerDescriptor>>(InitializeControllerDictionary);
        }


        public HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var routeData = request.GetRouteData();
            if (routeData == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var controllerName = GetControllerName(routeData);
            if (controllerName == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var namespaceName = (string[]) routeData.Values["namespaces"];
            var controllerKey = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", namespaceName[0], controllerName);
            if (controllers.Value.TryGetValue(controllerKey, out var controllerDescriptor))
            {
                return controllerDescriptor;
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return controllers.Value;
        }

        private Dictionary<string, HttpControllerDescriptor> InitializeControllerDictionary()
        {
            var dictionary = new Dictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);
            var assembliesResolver = config.Services.GetAssembliesResolver();
            var controllerTypeResolver = config.Services.GetHttpControllerTypeResolver();
            var Types = controllerTypeResolver.GetControllerTypes(assembliesResolver);
            foreach (var type in Types)
            { 
                var controllerName = type.Name.Remove(type.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length); 
                if (dictionary.Keys.Contains(type.Namespace + "." + controllerName)) continue;
                if (type.Namespace != null)
                    dictionary[type.Namespace + "." + controllerName] = new HttpControllerDescriptor(config, type.Name, type);
            }

            return dictionary;
        }

        private T GetRouteVariable<T>(IHttpRouteData routeData, string name)
        {
            if (routeData.Values.TryGetValue(name, out var result))
            {
                return (T) result;
            }

            return default(T);
        }

        private string GetControllerName(IHttpRouteData routeData)
        {
           return routeData.Values["controller"].ToString();
        }

    }
}