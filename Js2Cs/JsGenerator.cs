using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Js2Cs
{
    public class JsGenerator
    {
        public enum ModuleLoaderType
        {
            Autodetect = 0,
            CommonJs = 1,
            Amd = 2,
            Es6 = 3,
            GlobalVariable = 4
        }

        public enum HttpImplementation
        {
            Xhr = 0,
            Promise = 1
        }

        private static string boilerplatePromise = @"
                function http (method, url, data, formatter) {
                // create a promise
                return new Promise(function (resolve, reject) {
                    var request = new XMLHttpRequest();
                    request.open(method, url, true);
                    request.setRequestHeader('Accept', 'application/json');
                    request.setRequestHeader('X-Request-From', 'apicall');
                    var body = null;
                    if (!(method === 'GET' || method === 'DELETE')) {
                        if (typeof data === 'object') {
                            request.setRequestHeader('Content-Type', 'application/json');
                            body = JSON.stringify(data);
                        } else {
                            request.setRequestHeader('Content-Type', 'application/json');
                            body = '\'' + data.replace(/'/g, '\\\'') + '\'';
                        }
                    }
                    request.onload = function () {
                        var data = null;
                        if (request.status >= 200 && request.status < 400) {
                            if (request.responseText !== null && request.responseText.length > 0 && formatter !== undefined) {
                                data = formatter(request.responseText);
                            }
                            resolve(data, request);
                        } else {
                            if (request.responseText !== null && request.responseText.length > 0) {
                                data = JSON.parse(request.responseText);
                            }
                            reject(data, request);
                        }
                    };
                    request.onerror = function () {
                        reject({ message: 'Failed to connect to server.' });
                    };
                    if (body === null) {
                        request.send();
                    } else {
                        request.send(body);
                    }
                });
            }
            function DateFormatter(value) { return new Date(value); }
";


        private ModuleLoaderType ModuleType;
        private string VariableName;
        private HttpImplementation Boilerplate;
        private bool prettyPrint;
        private static Regex uglifyRegex = new Regex(@"(?:\/\/[^\n]*\n\s*|\/\*.*?\*\/\s*|\s+)", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled); 

        public JsGenerator()
        {
            ModuleType = ModuleLoaderType.Autodetect;
            VariableName = "api"; 
        }
        
        public JsGenerator UsePrettyPrint(bool value=true)
        {
            this.prettyPrint = value;
            return this;
        }

        /// <summary>
        /// Specifies how to export the module
        /// </summary>
        public JsGenerator UseModuleType(ModuleLoaderType type)
        {
            this.ModuleType = type;
            return this; 
        }

        /// <summary>
        /// Specify how the JS lib makes the requests
        /// </summary>
        public JsGenerator UseHttpImplementation(HttpImplementation imp)
        {
            this.Boilerplate = imp;
            return this;
        }
        
        /// <summary>
        /// Set value for global variable, used if module type is globalvar
        /// </summary>
        public JsGenerator UseVariableName(string value)
        {
            this.VariableName = value;
            return this;
        }


        /// <summary>
        /// Generate a single blob
        /// </summary>
        public string GenerateSingleFile(Assembly targetAssembly, String targetNamespace)
        {
            var controllerEnumerator = new ControllerEnumerator(targetAssembly).UseNamespace(targetNamespace);
            var sb = new StringBuilder();

            GenerateHeader(sb);
            foreach (var controller in controllerEnumerator.Enumerate())
            {
                sb.AppendLine($"\t{this.VariableName}.{controller.jsname} = {{");
                GenerateController(sb, controller);
                sb.AppendLine("\t};");
            }
            GenerateFooter(sb);
            var str = sb.ToString();
            if (!prettyPrint)
            {
                str = uglifyRegex.Replace(str, " ");
            }
            return str;
        }

        private void GenerateFooter(StringBuilder sb)
        {
            switch (this.ModuleType)
            {
                case ModuleLoaderType.Amd:
                    sb.AppendLine("\treturn " + this.VariableName + ";");
                    sb.AppendLine("});");
                    break;
                case ModuleLoaderType.CommonJs:
                case ModuleLoaderType.GlobalVariable:
                case ModuleLoaderType.Es6:
                    sb.AppendLine("})();");
                    break;
            }
        }

        private void GenerateHeader(StringBuilder sb)
        {
            switch (ModuleType)
            {
                case ModuleLoaderType.Es6:
                    sb.AppendLine("export (function(){");
                    sb.AppendLine("\tvar " + this.VariableName + " = {};");
                    break;
                case ModuleLoaderType.Amd:
                    sb.AppendLine("define(function(){");
                    sb.AppendLine("\tvar " + this.VariableName + " = {};");
                    break;
                case ModuleLoaderType.CommonJs:
                    sb.AppendLine("module.exports = (function(){");
                    sb.AppendLine("\tvar " + this.VariableName + " = {};");
                    break;
                case ModuleLoaderType.GlobalVariable:
                    sb.AppendLine("(function(){");
                    sb.AppendLine("\tif (window." + this.VariableName + " == null) { window."+this.VariableName+" = {}; };");
                    sb.AppendLine("\tvar " + this.VariableName + " = window." + this.VariableName + ";");
                    break;
            }
        }

        private static void GenerateController(StringBuilder sb, ControllerEnumerator.Model controller)
        {
            // enumarates endpoits in controller
            var endpoints = new EndpointEnumerator(controller);
            foreach (var endpoint in endpoints.Enumerate())
            {
                GenerateEndpoint(sb, endpoint);
            }
        }

        private static void GenerateEndpoint(StringBuilder sb, EndpointEnumerator.Model endpoint)
        {
            sb.Append($"\t\t{endpoint.jsname}: function(");
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
            }
            else
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
    }
}
