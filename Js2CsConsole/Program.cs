using System;
using System.Reflection;

namespace Js2CsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new Js2Cs.JsGenerator();
            var result  = generator.Generate(typeof(ApiTest.Program).GetTypeInfo().Assembly, "ApiTest.Controllers");
            Console.WriteLine(result);

        }
    }
}