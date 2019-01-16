
using System.Collections.Generic;
using System.Linq;
using Crispy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Scanner
{
    [TestClass]
    public class RoutelessControllerTest   
    {
        internal static EndpointInfo[] ScanResult = EndpointScanner.Scan<Mock.RoutelessController>().ToArray();

        [TestMethod] public void HasEndpoint() => ScanResult.Should()
            .NotBeEmpty("because the endpoint still exist and can be routed using MVC an template");

        internal static EndpointInfo IndexEndpoint = ScanResult.First();

        [TestMethod] public void EndpointHasNoRoute() => IndexEndpoint.HttpRoute.Should()
            .BeNull("because if there is no routing attribute then there should be no route");

        [TestMethod] public void EndpointHasNoHttpMethod() => IndexEndpoint.HttpMethod.Should()
            .BeNull("because there is no attribute which specifies method");

        internal static ControllerInfo Controller = IndexEndpoint.Controller;

        [TestMethod] public void ControllerHasNoRoute() => Controller.Route.Should()
            .BeNull("because there is no routing attribute on the controller");
    }
}