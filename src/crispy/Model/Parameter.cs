using System.Reflection;

namespace Crispy
{
    public class Parameter
    {
        public ParameterInfo info;
        public bool isRouteParameter;
        public string httpName;
        public bool isBodyParameter;
        public bool isQueryParameter;
        public string jsname;
    }
}