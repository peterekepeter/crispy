using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Js2Cs
{
    internal class ControllerEnumerator
    {
        private Assembly assembly;
        private string targetNamespace = null;
        private string controllerSuffix = "Controller";

        internal class Model
        {
            public Model(Type type, String controllerSuffix)
            {
                this.type = type;
                this.name = controllerSuffix == null ? type.Name : type.Name.Substring(0, type.Name.LastIndexOf(controllerSuffix));
                this.jsname = name.Substring(0,1).ToLowerInvariant() + name.Substring(1);
                this.typeInfo = type.GetTypeInfo();
                var route = type.GetTypeInfo().GetCustomAttribute<Microsoft.AspNetCore.Mvc.RouteAttribute>();
                this.route = route == null ? "/" + name.ToLowerInvariant() : "/" + route.Template.Replace("[controller]", this.name.ToLowerInvariant());
            }
            public String name;
            public String jsname;
            public Type type;
            public TypeInfo typeInfo;
            public String route;
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => t.Namespace.StartsWith(nameSpace)).ToArray();
        }

        public ControllerEnumerator(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public ControllerEnumerator UseNamespace(String targetNamespace)
        {
            this.targetNamespace = targetNamespace;
            return this;
        }

        public ControllerEnumerator UseSuffix(String newSuffix)
        {
            this.controllerSuffix = newSuffix;
            return this;
        }

        public IEnumerable<Model> Enumerate()
        {
            IEnumerable<Type> types = assembly.GetTypes();
            if (targetNamespace != null)
            {
                types = types.Where(type => type.Namespace.StartsWith(targetNamespace));
            }
            if (controllerSuffix != null)
            {
                types = types.Where(type => type.Name.EndsWith(controllerSuffix));
            }
            return types.Select(type => new Model(type, controllerSuffix));
        }
    }
}
