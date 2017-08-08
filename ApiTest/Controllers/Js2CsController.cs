using Js2Cs;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    public class Js2CsController : Controller
    {
        [HttpGet]
        public ContentResult GetApiDefinition()
        {
            // create generator instance
            var generator = new JsGenerator()
                // and configure it
                .UseModuleType(ModuleLoaderType.GlobalVariable)
                .UseVariableName("api")
                .UsePrettyPrint();

            // get assembly of web project
            var assembly = typeof(Js2CsController).GetTypeInfo().Assembly;

            // generate some js
            var javascript = generator.GenerateSingleFile(assembly, "ApiTest.Controllers");
            
            // all done
            return Content(javascript, "application/javascript");
        }
    }
}
