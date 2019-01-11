using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;

namespace Test.Scanner
{
    [TestClass]
    public class BasicEndpointScanFeatures
    {
        [TestMethod]
        public void CanScanController()
        {
            var endpoints = Crispy.EndpointScanner.Scan<Mock.BasicController>();
            endpoints.Should().HaveCount(1);
        }

        internal Crispy.EndpointInfo Endpoint
            => Crispy.EndpointScanner.Scan<Mock.BasicController>().Single();

        [TestMethod]
        public void EnpdointHasController()
            => Endpoint.Controller.Should().NotBeNull();

        [TestMethod]
        public void EnpdointHasControllerName()
            => Endpoint.Controller.Name.Should().Be("Basic");

        [TestMethod]
        public void EnpdointHasControllerNameJs()
            => Endpoint.Controller.NameCamelCase.Should().Be("basic");

        [TestMethod]
        public void HasControllerRoute()
            => Endpoint.Controller.Route.Should().Be("/api/basic");

        [TestMethod]
        public void HttpMethodIsGet()
            => Endpoint.HttpMethod.Should().Be("GET");

        [TestMethod]
        public void RoutShouldBeAsExpected()
            => Endpoint.HttpRoute.Should().Be("/api/basic");

        [TestMethod]
        public void ThereShouldBeNoParameters()
            => Endpoint.Parameters.Should().BeEmpty();
            
        [TestMethod]
        public void EndpointHasPascalCaseName()
            => Endpoint.Name.Should().Be("GetValues");

        [TestMethod]
        public void EndpointHasCamelCaseName()
            => Endpoint.NameCamelCase.Should().Be("getValues");

        [TestMethod]
        public void ReturnTypeIsListOfStrings()
            => Endpoint.ReturnType.Should().Be(typeof(List<string>));
    }
}
