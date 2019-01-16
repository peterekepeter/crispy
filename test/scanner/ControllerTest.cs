using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Crispy;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Test.Scanner
{
    
    namespace BadPostController
    {
        [Route("api/things")] public class ThingInsertController : Controller
        {
            [HttpPost("api/insert")] public string PostMethod([FromBody] string a, [FromBody] string b) => "";
        }
    }

    [TestClass] public class BadPostControllerTest {
        public Action Generating = () => new JsGenerator()
                .UseModuleType(ModuleLoaderType.GlobalVariable)
                .GenerateSingleFile(Assembly.GetExecutingAssembly(), 
                    "Test.Scanner.BadPostController");
        
        [TestMethod]
        public void BadPostThrows() => Generating.Should().Throw<CrispyException>()
            .WithMessage("Invalid controller method*");

        [TestMethod]
        public void MessageContainsControllerAndMethodName() => Generating.Should()
            .Throw<CrispyException>().WithMessage("*ThingInsertController*PostMethod*");
    }

    namespace ControllerWithServiceInjection
    {
        [Authorize(Roles = "Admin")] 
        public class SomethingController : Controller
        {
            public class Entity { }
            public class ServiceA { }
            public class ServiceB {} 
            public Task<IEnumerable<Entity>> GetAllInfo(
                [FromServices] ServiceA serviceA, 
                [FromServices] ServiceB serviceB, 
                [FromQuery] String filter1, 
                [FromQuery] String filter2, 
                [FromQuery] String filter3) { return null; }
        }
    }

    [TestClass] public class ControllerWithServiceInjectionTest {

        public Action Generating = () => new JsGenerator()
                .UseModuleType(ModuleLoaderType.GlobalVariable)
                .GenerateSingleFile(Assembly.GetExecutingAssembly(), 
                    "Test.Scanner.ControllerWithServiceInjection");

        [TestMethod]
        public void BadPostThrows() => Generating.Should().NotThrow();
        
        internal Crispy.EndpointInfo ScannedEndpoint
            => Crispy.EndpointScanner.Scan<ControllerWithServiceInjection.SomethingController>().Single();

        [TestMethod]
        public void ShouldHave3Parameters() => ScannedEndpoint.Parameters.Count().Should().Be(3);

        [TestMethod]
        public void ShouldNotDetectInjectedServicesAsParameters() => ScannedEndpoint.Parameters
            .Find(param => param.JsName == "serviceA").Should()
            .BeNull("because parameters with FromServiceAttribute should not be detected as parameters.");
    }

    namespace ControllerWithStaticMethod
    {
        [Authorize(Roles = "Admin")] 
        public class SomethingController : Controller
        {
            public String[] GetAll([FromQuery] String filter) => StaticGetAll(filter);

            public static String[] StaticGetAll(String filter) => null;
        }
    }
    
    [TestClass] public class ControllerWithStaticMethodnTest {
        
        internal IEnumerable<EndpointInfo> ScannedController
            => Crispy.EndpointScanner.Scan<ControllerWithStaticMethod.SomethingController>();

        [TestMethod]
        public void ShouldHaveOneMethod() => ScannedController.Count().Should().Be(1);

        [TestMethod]
        public void NonStaticMethodFound() => ScannedController
            .FirstOrDefault(endpoint => endpoint.Name == "GetAll")
            .Should().NotBeNull("because non static method must be detected");

        [TestMethod]
        public void StaticMethodNotFound() => ScannedController
            .FirstOrDefault(endpoint => endpoint.Name == "StaticGetAll")
            .Should().BeNull("because static method must NOT be detected");
    }

    [TestClass] public class ControllerWithMultipleGets {
    
        public class SomethingWithMultipleAttributesController : Controller
        {
            [HttpGet("test/something")]
            [HttpGet("api/something")]
            public String[] GetAllThings([FromQuery] String filter) => null;
        }

        internal static IEnumerable<EndpointInfo> ScannedController 
            => Crispy.EndpointScanner.Scan<SomethingWithMultipleAttributesController>();
        internal Action Scanning = () => ScannedController.Count();

        [TestMethod]
        public void ScanningShouldThrow() => Scanning.Should().NotThrow();

        [TestMethod]
        public void ShouldHaveOneMedthod() => ScannedController.Count().Should().Be(1);
    }
}