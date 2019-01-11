using System.Threading.Tasks;
using ApiTest.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{
    /// <summary> Submits commands to frontend. </summary>
    [Route("api/[controller]")]
    public class CommandController : Controller
    {
        CommandService Service;

        public CommandController(CommandService service) 
            => Service = service;

        [HttpGet]
        public Task<CommandObject> GetNextCommand() 
            => Service.GetExecutionCommand();

        public class ExecutionResult
        {
            public bool Success;
            public string Result;
        }

        [HttpPost("{id}")]
        public string SubmitResult(
                [FromRoute] int id, 
                [FromQuery] bool success, 
                [FromBody] string result){
            Service.SubmitExecutionResult(id, success, result);
            return "ok!";
        }
            
    }
}