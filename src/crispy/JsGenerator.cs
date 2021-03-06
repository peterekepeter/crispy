﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace Crispy
{
    /// <summary> Generates the JavaScript code necessary for the API. </summary>
    public partial class JsGenerator
    {
        private static readonly string XhrBoilerplate = @"
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
                            resolve(data);
                        } else {
                            if (request.responseText !== null && request.responseText.length > 0) {
                                data = JSON.parse(request.responseText);
                            }
                            reject(data);
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
";

        private static readonly string DateFormatterBoilerplate = @"
        function DateFormatter(value) { return new Date(value); }
";

        private ModuleLoaderType ModuleType;
        private string VariableName;
        private string JavascriptHttpImplementation = XhrBoilerplate;
        private bool prettyPrint;
        private static Regex uglifyRegex = new Regex(@"(?:\/\/[^\n]*\n\s*|\/\*.*?\*\/\s*|\s+)", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private TsOptions TsOptions;

        /// <summary> Generates JavaScript code. </summary>
        public JsGenerator()
        {
            ModuleType = ModuleLoaderType.Autodetect;
            VariableName = "api"; 
        }
        
        /// <summary> Provide a custom function that makes the http request. </summary>
        /// <param name="javascriptFunction">Example: 
        /// "function http (method, url, requestData, responseFormatter) { }" - should be javascript code
        /// which defines this function. </param>
        public JsGenerator UseHttpImplementation(string javascriptFunction) 
        {
            this.JavascriptHttpImplementation = javascriptFunction;
            return this;
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

        /// <summary> Emit type info for parameters and returned models </summary>
        public JsGenerator UseTypeScript(bool value=true){
            this.TsOptions = value ? new TsOptions() : null;
            return this;
        }

        /// <summary> Emit type info with the given options </summary>
        public JsGenerator UseTypeScript(TsOptions options){
            this.TsOptions = options;
            return this;
        }

        /// <summary> Specify how the JS lib makes the requests </summary>
        public JsGenerator UseHttpImplementation(HttpImplementation imp)
        {
            switch (imp){
                case HttpImplementation.Xhr: JavascriptHttpImplementation = XhrBoilerplate; break;
            }
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
            foreach (var controller in controllerScanner.ScanForControllers())
            {
                sb.AppendLine($"\t{controller.NameCamelCase}: {{");
                GenerateController(sb, controller, this.TsOptions);
                sb.AppendLine("\t},");
            }
            GenerateApiEnd(sb);
            GenerateBoilerplate(sb);
            GenerateFooter(sb);
            var str = sb.ToString();
            if (!prettyPrint)
            {
                str = uglifyRegex.Replace(str, " ");
            }
            return str;
        }

        private void GenerateBoilerplate(StringBuilder sb)
        {
            switch(this.ModuleType){
                case ModuleLoaderType.Amd:
                case ModuleLoaderType.GlobalVariable:
                    sb.AppendLine(JavascriptHttpImplementation.Replace("    ", "\t").Replace("\n\t\t","\n"));
                    break;
                case ModuleLoaderType.Es6:
                case ModuleLoaderType.CommonJs:
                    sb.AppendLine(JavascriptHttpImplementation.Replace("    ", "\t").Replace("\n\t\t\t","\n"));
                    break;
            }
        }

        private void GenerateApiEnd(StringBuilder sb)
        {
            switch (this.ModuleType)
            {
                case ModuleLoaderType.Amd:
                    sb.AppendLine("\t};");
                    break;
                case ModuleLoaderType.CommonJs:
                    sb.AppendLine("\t};");
                    break;
                case ModuleLoaderType.GlobalVariable:
                    sb.AppendLine("\t};");
                    break;
                case ModuleLoaderType.Es6:
                    sb.AppendLine("};");
                    break;
            }
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
                    sb.AppendLine("export default api;");
                    break;
            }
        }

        private void GenerateHeader(StringBuilder sb)
        {
            switch (ModuleType)
            {
                case ModuleLoaderType.Es6:
                    sb.AppendLine("export const " + this.VariableName + " = {");
                    break;
                case ModuleLoaderType.Amd:
                    sb.AppendLine("define(function(){");
                    sb.AppendLine("\tvar " + this.VariableName + " = {");
                    break;
                case ModuleLoaderType.CommonJs:
                    sb.AppendLine("module.exports = (function(){");
                    sb.AppendLine("\tvar " + this.VariableName + " = {");
                    break;
                case ModuleLoaderType.GlobalVariable:
                    sb.AppendLine("(function(){");
                    sb.AppendLine("\twindow."+this.VariableName+" = {");
                    break;
            }
        }

        private static void GenerateController(StringBuilder sb, ControllerInfo controller, TsOptions tsOptions)
        {
            // enumarates endpoits in controller
            var endpoints = new Scanner.EndpointScannerImpl(controller);
            foreach (var endpoint in endpoints.Enumerate())
            {
                try
                {
                    GenerateEndpoint(sb, endpoint, tsOptions);
                }
                catch (Exception exception)
                {
                    throw new CrispyException(
                        $"Generator error at '{endpoint.Controller.Type.Name}.{endpoint.Name}'", 
                        exception);
                }
            }
        }

        private static void GenerateEndpoint(StringBuilder sb, EndpointInfo endpoint, TsOptions tsOptions)
        {
            sb.Append($"\t\t{endpoint.NameCamelCase}: function(");
            // generate function vars
            var separator = "";
            foreach (var parameter in endpoint.Parameters)
            {
                sb.Append(separator);
                sb.Append(parameter.JsName);
                if (tsOptions != null){
                    sb.Append(": ").Append(TsGenerator.GenerateTypescriptType(parameter.Info.ParameterType, tsOptions));
                }
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
                        throw new CrispyException($"It can only have 1 body param but there are at least 2.");
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
            else if (TypeHelpers.IsNumericType(endpoint.ReturnType))
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
            sb.Append(")");
            if (tsOptions != null){
                sb.Append(" as Promise<").Append(TsGenerator.GenerateTypescriptType(endpoint.ReturnType, tsOptions)).Append(">");
            }
            sb.Append("; },\n");
        }



    }
}
