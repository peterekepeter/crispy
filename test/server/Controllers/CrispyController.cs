using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    public class CrispyController : Controller
    {
        private static ConcurrentDictionary<Crispy.ModuleLoaderType, ContentResult>
             Cached = new ConcurrentDictionary<Crispy.ModuleLoaderType, ContentResult>
             ();

        private ContentResult GenerateContent(Crispy.ModuleLoaderType type)
        {
            // create generator instance
            var generator = new Crispy.JsGenerator()
                // and configure it
                .UseModuleType(type)
                .UseVariableName("api")
                .UsePrettyPrint();

            // get assembly of web project
            var assembly = typeof(CrispyController).GetTypeInfo().Assembly;

            // generate some js
            var javascript = generator.GenerateSingleFile(assembly, "ApiTest.Controllers");
            
            // all done
            return Content(javascript, "application/javascript");
        }
        private ContentResult GetCached(Crispy.ModuleLoaderType type) 
            => Cached.GetOrAdd(type, GenerateContent);

        [HttpGet]
        public ContentResult GetApiDefinition()
            => GetCached(Crispy.ModuleLoaderType.GlobalVariable);

        [HttpGet("es6")]
        public ContentResult GetApiModuleDefinition()
            => GetCached(Crispy.ModuleLoaderType.Es6);
    }
}
