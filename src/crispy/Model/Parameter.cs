using System.Reflection;

namespace Crispy
{
    /// <summary> Represents and API endpoint parameter. </summary>
    public class Parameter
    {
        /// <summary> Reflection, internal use. </summary>
        internal ParameterInfo info;

        /// <summary> Should be sent thougt URL Route (path). </summary>
        public bool IsRoute;

        /// <summary> HTTP safe name, which the controller will match. </summary>
        public string httpName;

        /// <summary> Should be sent in the HTTP body. </summary>
        public bool IsBody;

        /// <summary> Sent as a query string parameter in the URL. </summary>
        public bool IsQuery;

        /// <summary> lowerCamelCase name which shouldbe used in javascript. </summary>
        public string jsname;
    }
}