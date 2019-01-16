using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using Crispy;
using System;

namespace Test.Scanner
{
    [TestClass]
    public class BasicEndpointScanFeatures
    {
        internal static IEnumerable<EndpointInfo> ScanResult => EndpointScanner.Scan<Mock.BasicController>();

        internal static Action Scanning = () => ScanResult.Count();
        
        [TestMethod] public void CanScanController() => Scanning.Should()
            .NotThrow("because it is a simple & valid controller");

        internal static Crispy.EndpointInfo Endpoint => ScanResult.Single();

        [TestMethod] public void EnpdointHasController() => Endpoint.Controller.Should()
            .NotBeNull("because each endpoint should belongs to a controller");

        [TestMethod] public void EnpdointHasControllerName() => Endpoint.Controller.Name.Should()
            .Be("Basic", "because it should be as it's named in C#");

        [TestMethod] public void EnpdointHasControllerNameJs() => Endpoint.Controller.NameCamelCase.Should()
            .Be("basic", "becase it should follow JavaScript conventions");

        [TestMethod] public void HasControllerRoute() => Endpoint.Controller.Route.Should()
            .Be("/api/basic", "because that's how it's defined on the controller");

        [TestMethod] public void HttpMethodIsGet() => Endpoint.HttpMethod.Should()
            .Be("GET", "because that's how it's defined by the HttpGetAttribute on the method");

        [TestMethod] public void RoutShouldBeAsExpected() => Endpoint.HttpRoute.Should()
            .Be("/api/basic", "because the endpoint route extends the controller route");

        [TestMethod] public void ThereShouldBeNoParameters() => Endpoint.Parameters.Should()
            .BeEmpty("because the controller method has no parameters");
            
        [TestMethod] public void EndpointHasPascalCaseName() => Endpoint.Name.Should()
            .Be("GetValues", "because it should reflect the C# naming");

        [TestMethod] public void EndpointHasCamelCaseName() => Endpoint.NameCamelCase.Should()
            .Be("getValues", "becase it should follow JavaScript conventions");

        [TestMethod] public void ReturnTypeIsListOfStrings() => Endpoint.ReturnType.Should()
            .Be(typeof(List<string>), "because that's the return type in C# and it should be detected");
    }
}
