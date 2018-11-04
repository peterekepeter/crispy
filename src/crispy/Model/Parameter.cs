using System.Reflection;

namespace Crispy
{
    public class Parameter
    {
        internal ParameterInfo info;
        internal bool isRouteParameter;
        internal string httpName;
        internal bool isBodyParameter;
        internal bool isQueryParameter;
        internal string jsname;
    }
}