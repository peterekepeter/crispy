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
}