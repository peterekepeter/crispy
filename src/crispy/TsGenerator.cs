using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crispy
{
    /// <summary> A container for Typescript generators </summary>
    public static class TsGenerator
    {
        private static TsOptions DEFAULT_OPTIONS = new TsOptions();

        /// <summary> For a given C# type, generates a typescript type in string form </summary>
        public static String GenerateTypescriptType(Type type, TsOptions options = null)
        {
            if (options == null)
            {
                options = DEFAULT_OPTIONS;
            }
            return TypeGen(type, options);
        }

        private static String TypeGen(Type type, TsOptions options)
        {
            if (options == null)
            {
                options = DEFAULT_OPTIONS;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)){
                var redirect = type.GetProperty("Result").PropertyType;
                return TypeGen(redirect, options);
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
                return TypeGen(type.GetElementType(), options) + "[]";
            }
            if (type.IsEnum){
                if (options.EnumNumberValues) {
                    return String.Join(" | ", type.GetEnumValues() as IEnumerable<int>);
                } else {
                    return String.Join(" | ", type.GetEnumNames().Select(name => "'" + name + "'"));
                }
            }
            var iDictionary = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (iDictionary != null)
            {
                var args = iDictionary.GetGenericArguments();
                if (args.Length == 2){
                    return "{ [key:string]: " + TypeGen(args[1], options) + " }";
                }
                return "{ [key:string]: string }";
            }
            if (typeof(IEnumerable).IsAssignableFrom(type)){
                var generics = type.GetGenericArguments();
                if (generics.Length == 1){
                    return TypeGen(generics[0], options) + "[]";
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
                            .Append(TypeGen(property.PropertyType, options));
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
                                .Append(TypeGen(field.FieldType, options));
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