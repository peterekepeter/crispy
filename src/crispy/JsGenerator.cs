using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Crispy
{
    /// <summary> Generates the JavaScript code necessary for the API. </summary>
    public partial class JsGenerator
    {
        private static string boilerplatePromise = @"
        function http (method, url, data, formatter) {
            return new Promise(function (resolve, reject) {
                var request = new XMLHttpRequest();
                request.open(method, url, true);
                request.setRequestHeader('Accept', 'application/json');
                request.setRequestHeader('X-Request-From', 'crispy');
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

        /// <summary> Generates JavaScript code. </summary>
        public JsGenerator()
        {
            ModuleType = ModuleLoaderType.Autodetect;
            VariableName = "api"; 
        }
        
        /// <summary> Will pretty print the javascript, useful for debugging. </summary>
        public JsGenerator UsePrettyPrint(bool value=true)
        {
            this.prettyPrint = value;
            return this;
        }

        /// <summary> Specifies how to export the module </summary>
        public JsGenerator UseModuleType(ModuleLoaderType type)
        {
            this.ModuleType = type;
            return this; 
        }

        /// <summary> Specify how the JS lib makes the requests </summary>
        public JsGenerator UseHttpImplementation(HttpImplementation imp)
        {
            this.Boilerplate = imp;
            return this;
        }
        
        /// <summary> Set value for global variable, used if module type is globalvar </summary>
        public JsGenerator UseVariableName(string value)
        {
            this.VariableName = value;
            return this;
        }

        /// <summary> Generate a single blob </summary>
        public string GenerateSingleFile(Assembly targetAssembly, String targetNamespace)
        {
            var controllerScanner = 
                new Scanner.ControllerScannerImpl(targetAssembly)
                .FromNamespace(targetNamespace);

            var sb = new StringBuilder();

            GenerateHeader(sb);
            sb.AppendLine(boilerplatePromise);
            foreach (var controller in controllerScanner.ScanForControllers())
            {
                sb.AppendLine($"\t{this.VariableName}.{controller.NameCamelCase} = {{");
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
                    break;
                case ModuleLoaderType.GlobalVariable:
                    sb.AppendLine("})();");
                    break;
                case ModuleLoaderType.Es6:
                    sb.AppendLine("\texport default api;");
                    break;
            }
        }

        private void GenerateHeader(StringBuilder sb)
        {
            switch (ModuleType)
            {
                case ModuleLoaderType.Es6:
                    sb.AppendLine("\tconst " + this.VariableName + " = {};");
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

        private static void GenerateController(StringBuilder sb, ControllerInfo controller)
        {
            // enumarates endpoits in controller
            var endpoints = new Scanner.EndpointScannerImpl(controller);
            foreach (var endpoint in endpoints.Enumerate())
            {
                GenerateEndpoint(sb, endpoint);
            }
        }

        private static void GenerateEndpoint(StringBuilder sb, EndpointInfo endpoint)
        {
            sb.Append($"\t\t{endpoint.NameCamelCase}: function(");
            // generate function vars
            var separator = "";
            foreach (var parameter in endpoint.Parameters)
            {
                sb.Append(separator);
                sb.Append(parameter.JsName);
                separator = ", ";
            }
            // continue function
            sb.Append($"){{ return http(\"{endpoint.HttpMethod}\", ");
            // resolve route params
            var httpRoute = "\"" + endpoint.HttpRoute;
            foreach (var parameter in endpoint.Parameters)
            {
                if (parameter.IsRoute)
                {
                    var templateName = "{" + parameter.HttpName + "}";
                    httpRoute = httpRoute.Replace(templateName, "\" + encodeURIComponent(" + parameter.JsName + ") + \"");
                }
            }
            // resolve query params
            separator = "?";
            foreach (var parameter in endpoint.Parameters)
            {
                if (parameter.IsQuery)
                {
                    httpRoute += separator + parameter.HttpName + "=\" + encodeURIComponent(" + parameter.JsName + ") + \"";
                    separator = "&";
                }
            }
            // write route
            sb.Append(httpRoute + "\", ");
            // find body param
            ParameterInfo bodyParam = null;
            foreach (var parameter in endpoint.Parameters)
            {
                if (parameter.IsBody)
                {
                    if (bodyParam == null)
                    {
                        bodyParam = parameter;
                    }
                    else
                    {
                        throw new CrispyException($"Invalid controller method {endpoint.Controller.Type.Name}.{endpoint.Name}, it can only have 1 body param but there are at least 2.");
                    }
                }
            }
            if (bodyParam == null)
            {
                sb.Append("null");
            }
            else
            {
                sb.Append(bodyParam.JsName);
            }

            sb.Append(", ");
            // return type formatter

            if (endpoint.ReturnType == typeof(String))
            {
                sb.Append("String");
            }
            else if (endpoint.ReturnType == typeof(Boolean))
            {
                sb.Append("Boolean");
            }
            else if (endpoint.ReturnType == typeof(DateTime) || endpoint.ReturnType == typeof(DateTimeOffset))
            {
                sb.Append("DateFormatter");
            }
            else if (endpoint.ReturnType == typeof(Int16) || endpoint.ReturnType == typeof(Int32) || endpoint.ReturnType == typeof(Int64)
                || endpoint.ReturnType == typeof(UInt16) || endpoint.ReturnType == typeof(UInt32) || endpoint.ReturnType == typeof(UInt64)
                || endpoint.ReturnType == typeof(Decimal) || endpoint.ReturnType == typeof(Single) || endpoint.ReturnType == typeof(Double))
            {
                sb.Append("Number");
            }
            else if (endpoint.ReturnType == typeof(void))
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
