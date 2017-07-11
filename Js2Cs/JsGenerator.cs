using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Js2Cs
{
    public class JsGenerator
    {
        public JsGenerator()
        {

        }

        public string Generate(Assembly targetAssembly, String targetNamespace)
        {
            var controllerEnumerator = new ControllerEnumerator(targetAssembly).UseNamespace(targetNamespace);
            var sb = new StringBuilder();
            sb.AppendLine("(function(){");
            sb.AppendLine("\twindow.api = {");
            foreach (var controller in controllerEnumerator.Enumerate())
            {
                sb.AppendLine($"\t\t{controller.jsname}: {{");
                // enumarates endpoits in controller
                var endpoints = new EndpointEnumerator(controller);
                foreach (var endpoint in endpoints.Enumerate())
                {
                    sb.Append($"\t\t\t{endpoint.jsname}: function(");
                    // generate function vars
                    var separator = "";
                    foreach (var parameter in endpoint.parameters)
                    {
                        sb.Append(separator);
                        sb.Append(parameter.jsname);
                        separator = ", ";
                    }
                    // continue function
                    sb.Append($"){{ return http(\"{endpoint.httpMehod}\", ");
                    // resolve route params
                    var httpRoute = "\"" + endpoint.httpRoute;
                    foreach (var parameter in endpoint.parameters)
                    {
                        if (parameter.isRouteParameter)
                        {
                            var templateName = "{" + parameter.httpName + "}";
                            httpRoute = httpRoute.Replace(templateName, "\" + encodeURIComponent(" + parameter.jsname + ") + \"");
                        }
                    }
                    // resolve query params
                    separator = "?";
                    foreach (var parameter in endpoint.parameters)
                    {
                        if (parameter.isQueryParameter)
                        {
                            httpRoute += separator + parameter.httpName + "=\" + encodeURIComponent(" + parameter.jsname + ") + \"";
                            separator = "&";
                        }
                    }
                    // write route
                    sb.Append(httpRoute + "\", ");
                    // find body param
                    EndpointEnumerator.ParameterModel bodyParam = null;
                    foreach (var parameter in endpoint.parameters)
                    {
                        if (parameter.isBodyParameter)
                        {
                            if (bodyParam == null)
                            {
                                bodyParam = parameter;
                            }
                            else
                            {
                                throw new ArgumentException("invalid controller, it should only have 1 body param");
                            }
                        }
                    }
                    if (bodyParam == null)
                    {
                        sb.Append("null");
                    } else
                    {
                        sb.Append(bodyParam.jsname);
                    }

                    sb.Append(", ");
                    // return type formatter

                    if (endpoint.returnType == typeof(String))
                    {
                        sb.Append("String");
                    }
                    else if (endpoint.returnType == typeof(Boolean))
                    {
                        sb.Append("Boolean");
                    }
                    else if (endpoint.returnType == typeof(DateTime) || endpoint.returnType == typeof(DateTimeOffset))
                    {
                        sb.Append("DateFormatter");
                    }
                    else if (endpoint.returnType == typeof(Int16) || endpoint.returnType == typeof(Int32) || endpoint.returnType == typeof(Int64)
                        || endpoint.returnType == typeof(UInt16) || endpoint.returnType == typeof(UInt32) || endpoint.returnType == typeof(UInt64)
                        || endpoint.returnType == typeof(Decimal) || endpoint.returnType == typeof(Single) || endpoint.returnType == typeof(Double))
                    {
                        sb.Append("Number");
                    } 
                    else if (endpoint.returnType == typeof(void))
                    {
                        sb.Append("undefined");
                    }
                    else
                    {
                        sb.Append("JSON.parse");
                    }

                    // done method
                    sb.Append("); },\n");
                }
                sb.AppendLine("\t\t},");
            }
            sb.AppendLine("\t}");
            sb.AppendLine("})()");
            return sb.ToString();
        }
    }
}
