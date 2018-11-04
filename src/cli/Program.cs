using System;
using System.Reflection;
using Cirspy;

namespace Crispy.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            ModuleLoaderType[] supportedTypes = { ModuleLoaderType.Amd, ModuleLoaderType.CommonJs, ModuleLoaderType.Es6, ModuleLoaderType.GlobalVariable };
            foreach(var  type in supportedTypes)
            {
                Console.WriteLine("\n======== " + type.ToString() + "\n");
                var generator = new Cirspy.JsGenerator().UsePrettyPrint(true).UseModuleType(type);
                Generate(generator);
            }

        }

        private static void Generate(Crispy.JsGenerator generator)
        {
            var result = generator.GenerateSingleFile(typeof(ApiTest.Program).GetTypeInfo().Assembly, "ApiTest.Controllers");
            Console.WriteLine(result);
        }
    }
}