﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Crispy.Scanner
{
    internal class EndpointScannerImpl
    {
        private Controller controller;

        public EndpointScannerImpl(Controller controller)
        {
            this.controller = controller;
        }

        private void DetermineAuthorization(TypeInfo type, out Authorization authorization)
        {
            var allowAnonymousAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            DetermineAuthorization(allowAnonymousAttribute, authorizeAttribute, out authorization);
        }

        private void DetermineAuthorization(MethodInfo type, out Authorization authorization)
        {
            var allowAnonymousAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            DetermineAuthorization(allowAnonymousAttribute, authorizeAttribute, out authorization);
        }

        private void DetermineAuthorization(
            Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute allowAnonymousAttribute, 
            Microsoft.AspNetCore.Authorization.AuthorizeAttribute authorizeAttribute, 
            out Authorization authorization)
        {
            authorization = new Authorization(); ;
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
            httpMethod = "?";

            var httpGet = method.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpGetAttribute>();
            if (httpGet != null)
            {
                httpMethod = "GET";
                var template = httpGet.Template;
                if (!String.IsNullOrWhiteSpace(template))
                {
                    httpRoute += "/" + template;
                }
            }

            var httpPost = method.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPostAttribute>();
            if (httpPost != null)
            {
                httpMethod = "POST";
                var template = httpPost.Template;
                if (!String.IsNullOrWhiteSpace(template))
                {
                    httpRoute += "/" + template;
                }
            }

            var httpPut = method.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPutAttribute>();
            if (httpPut != null)
            {
                httpMethod = "PUT";
                var template = httpPut.Template;
                if (!String.IsNullOrWhiteSpace(template))
                {
                    httpRoute += "/" + template;
                }
            }

            var httpPatch = method.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPatchAttribute>();
            if (httpPatch != null)
            {
                httpMethod = "PATCH";
                var template = httpPatch.Template;
                if (!String.IsNullOrWhiteSpace(template))
                {
                    httpRoute += "/" + template;
                }
            }

            var httpDelete = method.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpDeleteAttribute>();
            if (httpDelete != null)
            {
                httpMethod = "DELETE";
                var template = httpDelete.Template;
                if (!String.IsNullOrWhiteSpace(template))
                {
                    httpRoute += "/" + template;
                }
            }

        }

        public IEnumerable<Endpoint> Enumerate()
        {
            DetermineAuthorization(controller.TypeInfo, out var controllerAuthorization);
            foreach (var method in controller.Type.GetMethods())
            {
                if (method.DeclaringType != controller.Type)
                {
                    // this is inherited method
                    continue;
                }
                DetermineAuthorization(method, out var methodAuthorization);
                DetermineHttpMethodAndRoute(method, controller.Route, out var httpMethod, out var httpRoute);
                var endpoint = new Endpoint() {
                    Controller = controller,
                    MethodInfo = method,
                    Name = method.Name,
                    NameCamelCase = method.Name.LowerFirstLetter(),
                    HttpMethod = httpMethod,
                    HttpRoute = httpRoute,
                    Authorization = methodAuthorization
                };
                Console.WriteLine($"Endpoint {endpoint.Controller.Name}");
                // and now for the parameters
                foreach (var methodParam in method.GetParameters())
                {
                    var parameter = new Parameter();
                    parameter.info = methodParam;
                   
                    var fromRoute = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromRouteAttribute>();
                    var fromQuery = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromQueryAttribute>();
                    var fromBody = methodParam.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromBodyAttribute>();

                    if (fromRoute != null)
                    {
                        parameter.IsRoute = true;
                        parameter.httpName = fromRoute.Name ?? methodParam.Name;
                    }
                    else  if (fromQuery != null)
                    {
                        parameter.IsQuery = true;
                        parameter.httpName = fromQuery.Name ?? methodParam.Name;
                    }
                    else if (fromBody != null)
                    {
                        parameter.IsBody = true;
                        parameter.httpName = null;
                    }
                    else
                    {
                        var t = methodParam.ParameterType;
                        if (t.GetTypeInfo().IsPrimitive || t == typeof(Decimal) || t == typeof(String))
                        {
                            if (endpoint.HttpRoute.Contains("{" + methodParam.Name + "}"))
                            {
                                parameter.IsRoute = true;
                                parameter.httpName = methodParam.Name;
                            }
                            else
                            {
                                parameter.IsQuery = true;
                                parameter.httpName = methodParam.Name;
                            }
                        } else
                        {
                            parameter.IsBody = true;
                            parameter.httpName = null;
                        }
                    }
                    parameter.jsname = methodParam.Name.LowerFirstLetter();
                    endpoint.Parameters.Add(parameter);
                }
                endpoint.ReturnType = method.ReturnType;
                yield return endpoint;
            }
        }

        public IEnumerable<Endpoint> ScanEndpoints() => Enumerate();
        


    }
}
