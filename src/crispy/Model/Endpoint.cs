using System;
using System.Collections.Generic;
using System.Reflection;

namespace Crispy
{
    public class Endpoint
    {
        public ControllerInfo Controller;
        public MethodInfo MethodInfo;
        public string HttpMethod;
        public string HttpRoute;
        public Authorization Authorization;
        public string Name;
        public string NameCamelCase;
        public List<Parameter> Parameters = new List<Parameter>();
        public Type ReturnType;
    }

}