using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    public class CrispyController : Controller
    {
        private static ConcurrentDictionary<System.Tuple<Crispy.ModuleLoaderType, bool>, ContentResult>
             Cached = new ConcurrentDictionary<System.Tuple<Crispy.ModuleLoaderType, bool>, ContentResult>();

        private ContentResult GenerateContent(System.Tuple<Crispy.ModuleLoaderType,bool> cacheKey)
        {
            var moduleType = cacheKey.Item1;
            var typescript = cacheKey.Item2;

            // create generator instance
            var generator = new Crispy.JsGenerator()
                // and configure it
                .UseModuleType(moduleType)
                .UseTypeScript(typescript)
                .UseVariableName("api")
                .UsePrettyPrint();

            // get assembly of web project
            var assembly = typeof(CrispyController).GetTypeInfo().Assembly;

            // generate some js
            var javascript = generator.GenerateSingleFile(assembly, "ApiTest.Controllers");
            
            // all done
            return Content(javascript, "application/javascript");
        }
        private ContentResult GetCached(Crispy.ModuleLoaderType moduleType, bool typescript) 
            => Cached.GetOrAdd(System.Tuple.Create(moduleType, typescript), GenerateContent);

        [HttpGet]
        public ContentResult GetApiDefinition()
            => GetCached(Crispy.ModuleLoaderType.GlobalVariable, false);

        [HttpGet("es6")]
        public ContentResult GetApiModuleDefinition()
            => GetCached(Crispy.ModuleLoaderType.Es6, false);

        [HttpGet("ts")]
        public ContentResult GetApiTypescriptModuleDefinition()
            => GetCached(Crispy.ModuleLoaderType.Es6, true);
    }
}
