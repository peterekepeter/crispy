using System;
using System.Collections.Generic;
using System.Reflection;

namespace Crispy
{
    /// <summary> Information reflected from a controller method. </summary>
    public class EndpointInfo
    {
        /// <summary> Controller to which this endpoint belongs to </summary>
        public ControllerInfo Controller;

        /// <summary> Reflected method information </summary>
        public MethodInfo MethodInfo;

        /// <summary> The HTTP method (GET, POST, ect.) </summary>
        public string HttpMethod;

        /// <summary> The full relative URI to the endpoint </summary>
        public string HttpRoute;

        /// <summary> Contains authorization info </summary>
        public AuthorizationInfo Authorization;

        /// <summary> The method name as found in C# code </summary>
        public string Name;

        /// <summary> camelCase name suitable for JS </summary>
        public string NameCamelCase;

        /// <summary> List of parameters the endpoint can receive </summary>
        public List<ParameterInfo> Parameters = new List<ParameterInfo>();

        /// <summary> The reflected return type of the method </summary>
        public Type ReturnType;
    }

}