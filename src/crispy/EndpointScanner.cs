using System.Collections.Generic;
using System.Linq;

namespace Crispy
{
    /// <summary> Scans AspNet.Mvc Controllers for accessible endpoints </summary>
    public static class EndpointScanner
    {
        /// <summary> Scans a single controller </summary>
        public static IEnumerable<EndpointInfo> Scan<TController>()
            => new Scanner.ControllerScannerImpl(typeof(TController))
            .ScanForControllers()
            .SelectMany(y => new Scanner.EndpointScannerImpl(y)
            .ScanEndpoints());
            
    }
}