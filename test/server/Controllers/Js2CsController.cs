using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    public class CrispyController : Controller
    {
        private static ContentResult Cached = null;
        private static SemaphoreSlim Semaphore = new SemaphoreSlim(1,1);

        [HttpGet]
        public async Task<ContentResult> GetApiDefinition()
        {
            if (Cached != null){
                return Cached;
            }
            await Semaphore.WaitAsync();
            try
            { 
                if (Cached != null){
                    return Cached;
                }
                // create generator instance
                var generator = new Crispy.JsGenerator()
                    // and configure it
                    .UseModuleType(Crispy.ModuleLoaderType.GlobalVariable)
                    .UseVariableName("api")
                    .UsePrettyPrint();

                // get assembly of web project
                var assembly = typeof(CrispyController).GetTypeInfo().Assembly;

                // generate some js
                var javascript = generator.GenerateSingleFile(assembly, "ApiTest.Controllers");
                
                // all done
                Cached = Content(javascript, "application/javascript");
            }
            finally
            {
                Semaphore.Release();
            }
            return Cached;
        }
    }
}
