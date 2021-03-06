﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Crispy.Scanner
{
    internal class ControllerScannerImpl
    {
        private Type onlyController = null;
        private Assembly assembly;
        private string namespaceFilter = null;
        private string controllerSuffix = "Controller";


        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
            => assembly.GetTypes()
                .Where(t => t.Namespace.StartsWith(nameSpace))
                .ToArray();
        

        public ControllerScannerImpl(Type controller) => onlyController = controller;

        public ControllerScannerImpl(Assembly assembly) => this.assembly = assembly;

        public ControllerScannerImpl FromNamespace(String targetNamespace)
        {
            this.namespaceFilter = targetNamespace;
            return this;
        }

        public ControllerScannerImpl WithSuffix(String newSuffix)
        {
            this.controllerSuffix = newSuffix;
            return this;
        }

        internal IEnumerable<ControllerInfo> ScanForControllers()
        {
            // get some controllers
            IEnumerable<Type> types = onlyController == null 
                ? assembly.GetTypes()
                : new[]{ onlyController };
            
            // only those from desired namespace
            if (namespaceFilter != null)
            {
                types = types.Where(type => type.Namespace != null && 
                    type.Namespace.StartsWith(namespaceFilter));
            }

            // only those that end with "Controller"
            if (controllerSuffix != null)
            {
                types = types.Where(type => type.Name
                    .EndsWith(controllerSuffix));
            }

            return types.Select(CreateController);
        }

        internal ControllerInfo CreateController(Type type)
        {
            var name = type.Name;
            var info = type.GetTypeInfo();
            var route = info
                .GetCustomAttribute<Microsoft.AspNetCore.Mvc.RouteAttribute>();
            var namePascal = controllerSuffix == null
                ? name
                : name.Substring(0, name.LastIndexOf(controllerSuffix));
            var nameCamel = namePascal.LowerFirstLetter();
            var routeString = route == null ? null : "/" + route.Template.Replace("[controller]", nameCamel);
            return new ControllerInfo() {
                Type = type,
                Name = namePascal,
                NameCamelCase = nameCamel,
                TypeInfo = info,
                Route = routeString
            };
        }
    }
}
