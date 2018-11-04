using System.Collections.Generic;
using System.Linq;

namespace Crispy
{
    public static class EndpointScanner
    {
        public static IEnumerable<Endpoint> Scan<TController>()
            => new Scanner.ControllerScannerImpl(typeof(TController))
            .ScanForControllers()
            .SelectMany(y => new Scanner.EndpointScannerImpl(y)
            .ScanEndpoints());
    }
}