using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Crispy.Scanner
{
    internal class EndpointScannerImpl
    {
        private ControllerInfo controller;

        public EndpointScannerImpl(ControllerInfo controller)
        {
            this.controller = controller;
        }

        private void DetermineAuthorization(TypeInfo type, out AuthorizationInfo authorization)
        {
            var allowAnonymousAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            DetermineAuthorization(allowAnonymousAttribute, authorizeAttribute, out authorization);
        }

        private void DetermineAuthorization(MethodInfo type, out AuthorizationInfo authorization)
        {
            var allowAnonymousAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            DetermineAuthorization(allowAnonymousAttribute, authorizeAttribute, out authorization);
        }

        private void DetermineAuthorization(
            Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute allowAnonymousAttribute, 
            Microsoft.AspNetCore.Authorization.AuthorizeAttribute authorizeAttribute, 
            out AuthorizationInfo authorization)
        {
            authorization = new AuthorizationInfo(); ;
            if (allowAnonymousAttribute != null)
            {
                authorization.AllowAnonymous = true;
                authorization.IsSigninRequired = false;
            }
            else if (authorizeAttribute != null)
            {
                authorization.AllowAnonymous = false;
                authorization.IsSigninRequired = true;
                if (!String.IsNullOrWhiteSpace(authorizeAttribute.Roles))
                {
                    authorization.Roles = authorizeAttribute.Roles;
                }
                if (!String.IsNullOrWhiteSpace(authorizeAttribute.Policy))
                {
                    authorization.Policy = authorizeAttribute.Policy;
                }
            }
        }

        private void DetermineHttpMethodAndRoute(MethodInfo method, String httpRouteRoot, out String httpMethod,
            out String httpRoute)
        {
            httpRoute = httpRouteRoot;
            httpMethod = null;
            String routeTemplate = null;

            var httpMethods = method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>();
            foreach (var attribute in httpMethods){
                // TODO add a warning over here
                httpMethod = attribute.HttpMethods.First();
                routeTemplate = attribute.Template;
                break;
            }

            if (!String.IsNullOrWhiteSpace(routeTemplate))
            {
                httpRoute += "/" + routeTemplate;
            }
        }

        public IEnumerable<EndpointInfo> Enumerate()
        {
            DetermineAuthorization(controller.TypeInfo, out var controllerAuthorization);
            foreach (var method in controller.Type.GetMethods())
            {
                if (method.DeclaringType != controller.Type)
                {
                    // this is inherited method
                    continue;
                }
                if (method.IsStatic){
                    // filter out static methods
                    continue;
                }
                DetermineAuthorization(method, out var methodAuthorization);
                DetermineHttpMethodAndRoute(method, controller.Route, out var httpMethod, out var httpRoute);
                var endpoint = new EndpointInfo() {
                    Controller = controller,
                    MethodInfo = method,
                    Name = method.Name,
                    NameCamelCase = method.Name.LowerFirstLetter(),
                    HttpMethod = httpMethod,
                    HttpRoute = httpRoute,
                    Authorization = methodAuthorization
                };

                // and now for the parameters
                foreach (var methodParam in method.GetParameters())
                {
                    var fromSerivces = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromServicesAttribute>();

                    if (fromSerivces != null){
                        continue; // skip this param
                    }

                    var parameter = new ParameterInfo();
                    parameter.Info = methodParam;

                    var fromRoute = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromRouteAttribute>();
                    var fromQuery = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromQueryAttribute>();
                    var fromBody = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromBodyAttribute>();
                    
                    if (fromRoute != null)
                    {
                        parameter.IsRoute = true;
                        parameter.HttpName = fromRoute.Name ?? methodParam.Name;
                    }
                    else  if (fromQuery != null)
                    {
                        parameter.IsQuery = true;
                        parameter.HttpName = fromQuery.Name ?? methodParam.Name;
                    }
                    else if (fromBody != null)
                    {
                        parameter.IsBody = true;
                        parameter.HttpName = null;
                    }
                    else
                    {
                        var t = methodParam.ParameterType;
                        if (t.GetTypeInfo().IsPrimitive || t == typeof(Decimal) || t == typeof(String))
                        {
                            if (endpoint.HttpRoute.Contains("{" + methodParam.Name + "}"))
                            {
                                parameter.IsRoute = true;
                                parameter.HttpName = methodParam.Name;
                            }
                            else
                            {
                                parameter.IsQuery = true;
                                parameter.HttpName = methodParam.Name;
                            }
                        } else
                        {
                            parameter.IsBody = true;
                            parameter.HttpName = null;
                        }
                    }
                    parameter.JsName = methodParam.Name.LowerFirstLetter();
                    endpoint.Parameters.Add(parameter);
                }
                endpoint.ReturnType = method.ReturnType;
                yield return endpoint;
            }
        }

        public IEnumerable<EndpointInfo> ScanEndpoints() => Enumerate();
        


    }
}
