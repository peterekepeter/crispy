using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestMethod] [JsImplementation.All]
        public void SomeCodeIsGenerated(string js) => js.Should().NotBeNullOrWhiteSpace();

        [TestMethod] [JsImplementation.All]
        public void ControllerNameIsInJs(string js) => js.Should().Contain("someValue");

        [TestMethod] [JsImplementation.All]
        public void Controller_endpoint_syntax(string js) => js.Should().Contain("api = { someValue: { getAllValues: ");

        [TestMethod] [JsImplementation.All]
        public void ValueNameIsInJs(string js) => js.Should().Contain("getAllValues");

        [TestMethod] [JsImplementation.All]
        public void HttpMethodIsInJs(string js) => js.Should().Contain("GET");

        [TestMethod] [JsImplementation.All]
        public void RouteInInJs(string js) => js.Should().Contain("api/custom-route");

        [TestMethod] [JsImplementation.Xhr]
        public void HasDefaultHttpCode(string js) => js.Should().Contain("new XMLHttpRequest()");

        [TestMethod] [JsImplementation.Custom]
        public void HasCustomCode(string js) => js.Should().Contain("console.info(");

        [TestMethod] [JsImplementation.All]
        public void Resolve_reject_called_with_at_most_1_param(string js) 
            => js.Should().NotMatchRegex("(resolve|reject)\\([a-z]+,\\s*[a-z]+\\)");

    }

    internal class JsImplementation : Attribute, ITestDataSource
    {
        internal class All : JsImplementation { internal All() : base(DefaultHttp, CustomHttp) { } }
        internal class Xhr : JsImplementation { internal Xhr() : base(DefaultHttp) { } }
        internal class Custom : JsImplementation { internal Custom() : base(CustomHttp) { } }

        private static Type TargetType = typeof(SomeValueController);
        private static Assembly TargetAssembly = TargetType.Assembly;
        private static string DefaultHttp = new JsGenerator()
                .UseModuleType(ModuleLoaderType.GlobalVariable)
                .GenerateSingleFile(TargetAssembly, "Test.Scanner.Controllers");

        private static string CustomHttp = new JsGenerator()
                .UseModuleType(ModuleLoaderType.Es6)
                .UseHttpImplementation("function http(){ console.info(...arguments); }")
                .GenerateSingleFile(TargetAssembly, "Test.Scanner.Controllers");

        private List<object[]> data;

        private JsImplementation(params string[] list) => data = list.Select(str => new object[] { str }).ToList();

        public IEnumerable<object[]> GetData(MethodInfo methodInfo) => data;

        public string GetDisplayName(MethodInfo methodInfo, object[] data) => data[0].ToString();
    }
}