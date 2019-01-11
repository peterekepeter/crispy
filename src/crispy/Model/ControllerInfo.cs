using System;
using System.Reflection;

namespace Crispy
{
    /// <summary> Reflected information about an AspNetCore.Mvc controller </summary>
    public class ControllerInfo
    {
        /// <summary> Name of the controller in PascalCase </summary>
        public string Name;

        /// <summary> Reflected type of the controller </summary>
        public Type Type;

        /// <summary> Name of the controller converted to camelCase </summary>
        public string NameCamelCase;

        /// <summary> Relaitve URI string to this controller. </summary>
        public string Route;

        /// <summary> Reflected type information of the controller. </summary>
        public TypeInfo TypeInfo;
    }
}