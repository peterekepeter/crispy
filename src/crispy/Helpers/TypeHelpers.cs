

using System;

namespace Crispy
{
    static class TypeHelpers
    {
        public static bool IsNumericType(Type type) 
            => type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)
            || type == typeof(UInt16) || type == typeof(UInt32) || type == typeof(UInt64)
            || type == typeof(Decimal) || type == typeof(Single) || type == typeof(Double);
    }
}
