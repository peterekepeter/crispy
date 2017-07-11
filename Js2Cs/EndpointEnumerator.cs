using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Js2Cs
{
    internal class EndpointEnumerator
    {
        private ControllerEnumerator.Model controller;

        public EndpointEnumerator(ControllerEnumerator.Model controller)
        {
            this.controller = controller;
        }

        internal class Model
        {
            internal MethodInfo method;
            internal string httpMehod;
            internal string httpRoute;
            internal AuthorizationModel authorization;
            internal string jsname;
            internal List<ParameterModel> parameters = new List<ParameterModel>();
            internal Type returnType;
        }

        internal class AuthorizationModel
        {
            public bool AllowAnonymous, IsSigninRequired;
            public String Roles;
            public String Policy;
        }

        internal class ParameterModel
        {
            internal ParameterInfo info;
            internal bool isRouteParameter;
            internal string httpName;
            internal bool isBodyParameter;
            internal bool isQueryParameter;
            internal string jsname;
        }

        private void DetermineAuthorization(TypeInfo type, out AuthorizationModel authorization)
        {
            var allowAnonymousAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            DetermineAuthorization(allowAnonymousAttribute, authorizeAttribute, out authorization);
        }

        private void DetermineAuthorization(MethodInfo type, out AuthorizationModel authorization)
        {
            var allowAnonymousAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>();
            var authorizeAttribute = type.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            DetermineAuthorization(allowAnonymousAttribute, authorizeAttribute, out authorization);
        }

        private void DetermineAuthorization(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute allowAnonymousAttribute, Microsoft.AspNetCore.Authorization.AuthorizeAttribute authorizeAttribute, out AuthorizationModel authorization)
        {
            authorization = new AuthorizationModel(); ;
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

        public IEnumerable<Model> Enumerate()
        {
            DetermineAuthorization(controller.typeInfo, out var controllerAuthorization);
            foreach (var method in controller.type.GetMethods())
            {
                if (method.DeclaringType != controller.type)
                {
                    // this is inherited method
                    continue;
                }
                DetermineAuthorization(method, out var methodAuthorization);
                DetermineHttpMethodAndRoute(method, controller.route, out var httpMethod, out var httpRoute);
                var model = new Model();
                model.method = method;
                model.jsname = method.Name.Substring(0,1).ToLowerInvariant() + method.Name.Substring(1);
                model.httpMehod = httpMethod;
                model.httpRoute = httpRoute;
                model.authorization = methodAuthorization;
                // and now for the parameters
                foreach (var parameter in method.GetParameters())
                {
                    var parameterModel = new ParameterModel();
                    parameterModel.info = parameter;
                   
                    bool basic = true;
                    var fromRoute = parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromRouteAttribute>();
                    var fromQuery = parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromQueryAttribute>();
                    var fromBody = parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromBodyAttribute>();

                    if (fromRoute != null)
                    {
                        parameterModel.isRouteParameter = true;
                        parameterModel.httpName = fromRoute.Name ?? parameter.Name;
                    }
                    else  if (fromQuery != null)
                    {
                        parameterModel.isQueryParameter = true;
                        parameterModel.httpName = fromQuery.Name ?? parameter.Name;
                    }
                    else if (fromBody != null)
                    {
                        parameterModel.isBodyParameter = true;
                        parameterModel.httpName = null;
                    }
                    else
                    {
                        var t = parameter.ParameterType;
                        if (t.GetTypeInfo().IsPrimitive || t == typeof(Decimal) || t == typeof(String))
                        {
                            if (model.httpRoute.Contains("{" + parameter.Name + "}"))
                            {
                                parameterModel.isRouteParameter = true;
                                parameterModel.httpName = parameter.Name;
                            }
                            else
                            {
                                parameterModel.isQueryParameter = true;
                                parameterModel.httpName = parameter.Name;
                            }
                        } else
                        {
                            parameterModel.isBodyParameter = true;
                            parameterModel.httpName = null;
                        }
                    }
                    parameterModel.jsname = parameter.Name.Substring(0, 1).ToLowerInvariant() + parameter.Name.Substring(1);
                    model.parameters.Add(parameterModel);
                }
                model.returnType = method.ReturnType;
                yield return model;
            }
        }
        


    }
}
