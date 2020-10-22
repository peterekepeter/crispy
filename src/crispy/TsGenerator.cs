using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crispy
{
    public static class TsGenerator
    {
        public class Options
        {
            public bool Readable = true;
            public bool Writeable = true;
            public bool Properties = true;
            public bool Fields = true;
            public bool Public = true;
            public bool NonPublic = false;
            public bool LowercaseFirstLetter = true;
        }

        private static Options DEFAULT_OPTIONS = new Options();

        public static String GenerateTypescriptType(Type type, Options options = null)
        {
            if (options == null)
            {
                options = DEFAULT_OPTIONS;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)){
                var redirect = type.GetProperty("Result").PropertyType;
                return GenerateTypescriptType(redirect);
            }
            if (type == typeof(String) || type == typeof(char))
            {
                return "string";
            }
            if (type == typeof(bool))
            {
                return "boolean";
            }
            if (TypeHelpers.IsNumericType(type))
            {
                return "number";
            }
            if (type == typeof(void))
            {
                return "void";    
            }
            if (type.IsArray){
                return GenerateTypescriptType(type.GetElementType()) + "[]";
            }
            var iDictionary = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (iDictionary != null)
            {
                var args = iDictionary.GetGenericArguments();
                if (args.Length == 2){
                    return "{ [key:string]: " + GenerateTypescriptType(args[1]) + " }";
                }
                return "{ [key:string]: string }";
            }
            if (typeof(IEnumerable).IsAssignableFrom(type)){
                var generics = type.GetGenericArguments();
                if (generics.Length == 1){
                    return GenerateTypescriptType(generics[0]) + "[]";
                }
                return "any[]";
            }
            var bindingFlags = BindingFlags.Instance;
            if (options.Public){
                bindingFlags |= BindingFlags.Public;
            }
            if (options.NonPublic){
                bindingFlags |= BindingFlags.NonPublic;
            }
            if (options.Readable){
                if (options.Fields){
                    bindingFlags |= BindingFlags.GetField;
                }
                if (options.Properties){
                    bindingFlags |= BindingFlags.GetProperty;
                }
            }
            if (options.Writeable){
                if (options.Fields){
                    bindingFlags |= BindingFlags.SetField;
                }
                if (options.Properties){
                    bindingFlags |= BindingFlags.SetProperty;
                }
            }
            var members = type.GetMembers(bindingFlags);
            if (members.Length > 0)
            {
                var strb = new StringBuilder();
                var separator = "{ ";
                foreach(var member in members)
                {
                    if (member.MemberType == MemberTypes.Property && options.Properties)
                    {
                        var property = type.GetProperty(member.Name);
                        if (!(options.Readable && property.CanRead && (options.NonPublic || property.GetGetMethod(nonPublic: true).IsPublic)
                        || options.Writeable && property.CanWrite && (options.NonPublic || property.GetSetMethod(nonPublic: true).IsPublic)))
                        {
                            continue; 
                        }
                        var name = options.LowercaseFirstLetter ? property.Name.LowerFirstLetter() : property.Name;
                        strb.Append(separator)
                            .Append(name)
                            .Append(": ")
                            .Append(GenerateTypescriptType(property.PropertyType));
                        separator = ", ";
                    }
                    else if (member.MemberType == MemberTypes.Field && options.Fields)
                    {
                        var field = type.GetField(member.Name, bindingFlags);
                        if (field.Name.Contains("<") || field.Name.Contains(">")){
                            continue; // skip auto generated backing fields
                        }
                        if (options.Readable || !field.IsInitOnly && options.Writeable)
                        {
                            var name = options.LowercaseFirstLetter ? field.Name.LowerFirstLetter() : field.Name;
                            strb.Append(separator)
                                .Append(name)
                                .Append(": ")
                                .Append(GenerateTypescriptType(field.FieldType));
                            separator = ", ";
                        }
                    }
                }
                strb.Append(" }");
                return strb.ToString();
            }
            return "any";
        }


    }

}