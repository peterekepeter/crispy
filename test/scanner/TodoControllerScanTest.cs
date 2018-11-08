using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;

namespace Test.Scanner
{
    [TestClass]
    public class TodoControllerScanTest
    {
        internal IEnumerable<Crispy.Endpoint> Endpoints
            => Crispy.EndpointScanner.Scan<Mock.TodoController>();

        internal Crispy.Endpoint AddEndpoint 
            => Endpoints.First(y => y.Name == "Add");

        [TestMethod]
        public void ThereShouldBeFiveEndpoints()
            => Endpoints.Should().HaveCount(5);

        [TestMethod]
        public void ShouldDetectParameter()
            => AddEndpoint.Parameters.Should().HaveCount(1);

        [TestMethod]
        public void AddEnpointHasPostMethod()
            => AddEndpoint.HttpMethod.Should().Be("POST");

        [TestMethod]
        public void AddEndpointHasParameter()
            => AddEndpoint.Parameters.Should().NotBeEmpty();

        [TestMethod]
        public void AddEndpointHasOneParameter()
            => AddEndpoint.Parameters.Should().HaveCount(1);

        internal Crispy.Parameter AddParameter 
            => AddEndpoint.Parameters.Single();

        [TestMethod]
        public void AddEndpointParameterIsInBody()
            => AddParameter.IsBody.Should().Be(true);

        internal Crispy.Endpoint UpdateEndpoint 
            => Endpoints.First(y => y.Name == "Update");

        [TestMethod]
        public void UpdateEndpointHasPatchMethod()
            => UpdateEndpoint.HttpMethod.Should().Be("PATCH");

        [TestMethod]
        public void UpdateEndpointHasTwoParameters()
            => UpdateEndpoint.Parameters.Should().HaveCount(2);

        internal Crispy.Parameter UpdateFirstParam
            => UpdateEndpoint.Parameters[0];

        internal Crispy.Parameter UpdateSecondParam
            => UpdateEndpoint.Parameters[1];

        [TestMethod]
        public void UpdateFirstParamIsQuery()
            => UpdateFirstParam.IsQuery.Should().Be(true);

        [TestMethod]
        public void UpdateSecondParamIsBody()
            => UpdateSecondParam.IsBody.Should().Be(true);
        
    }
}
