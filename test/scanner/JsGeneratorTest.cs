using System;
using System.Reflection;
using Crispy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Scanner.BadPostController;
using Test.Scanner.Controllers;

namespace Test.Scanner
{
    namespace Controllers
    {
        [Route("api/custom-route")] public class SomeValueController : Controller
        {
            [HttpGet] public string GetAllValues() => "";
        }
    }


    [TestClass]
    public class JsGeneratorTest
    {
        Type TargetType;
        Assembly ThisAssembly;
        string GeneratedCode;

        public JsGeneratorTest(){
            TargetType = typeof(SomeValueController);
            Assembly ThisAssembly = TargetType.Assembly;
            GeneratedCode = new JsGenerator()
                .UseModuleType(ModuleLoaderType.GlobalVariable)
                .GenerateSingleFile(ThisAssembly, "Test.Scanner.Controllers");
        }
        
        [TestMethod]
        public void SomeCodeIsGenerated() => GeneratedCode.Should().NotBeNullOrWhiteSpace();

        [TestMethod]
        public void ControllerNameIsInJs() => GeneratedCode.Should().Contain("api.someValue");

        [TestMethod]
        public void ValueNameIsInJs() => GeneratedCode.Should().Contain("getAllValues");

        [TestMethod]
        public void HttpMethodIsInJs() => GeneratedCode.Should().Contain("GET");

        [TestMethod]
        public void RouteInInJs() => GeneratedCode.Should().Contain("api/custom-route");
    }
}