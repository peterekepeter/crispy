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
        public void ThereShouldBeTwoEndpoints()
            => Endpoints.Should().HaveCount(2);

        [TestMethod]
        public void ShouldDetectParameter()
            => AddEndpoint.Parameters.Should().HaveCount(1);
        
    }
}
