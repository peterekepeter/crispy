using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;

namespace Test.Scanner
{
    [TestClass]
    public class TodoControllerScanTest
    {
        internal IEnumerable<Crispy.EndpointInfo> Endpoints
            => Crispy.EndpointScanner.Scan<Mock.TodoController>();

        internal Crispy.EndpointInfo AddEndpoint 
            => Endpoints.First(y => y.Name == "Add");

        [TestMethod]
        public void ControllerPascalNameIsCorrect()
            => AddEndpoint.Controller.Name.Should().Be("Todo");

        [TestMethod]
        public void ControllerCameNameIsCorrect()
            => AddEndpoint.Controller.NameCamelCase.Should().Be("todo");
            
        [TestMethod]
        public void ControllerRouteIsCorrect()
            => AddEndpoint.Controller.Route.Should().Be("/api/todo");

        [TestMethod]
        public void EndpointRouteIsCorrect()
            => AddEndpoint.HttpRoute.Should().Be("/api/todo");
        

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

        internal Crispy.ParameterInfo AddParameter 
            => AddEndpoint.Parameters.Single();

        [TestMethod]
        public void AddEndpointParameterIsInBody()
            => AddParameter.IsBody.Should().Be(true);

        internal Crispy.EndpointInfo UpdateEndpoint 
            => Endpoints.First(y => y.Name == "Update");

        [TestMethod]
        public void UpdateEndpointHasPatchMethod()
            => UpdateEndpoint.HttpMethod.Should().Be("PATCH");

        [TestMethod]
        public void UpdateEndpointHasTwoParameters()
            => UpdateEndpoint.Parameters.Should().HaveCount(2);

        internal Crispy.ParameterInfo UpdateFirstParam
            => UpdateEndpoint.Parameters[0];

        internal Crispy.ParameterInfo UpdateSecondParam
            => UpdateEndpoint.Parameters[1];

        [TestMethod]
        public void UpdateFirstParamIsQuery()
            => UpdateFirstParam.IsQuery.Should().Be(true);

        [TestMethod]
        public void UpdateSecondParamIsBody()
            => UpdateSecondParam.IsBody.Should().Be(true);
        
    }
}
