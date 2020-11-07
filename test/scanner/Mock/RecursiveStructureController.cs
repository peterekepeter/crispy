using Microsoft.AspNetCore.Mvc;

namespace Test.Scanner.Mock.RecursiveStructureController
{
    public class DataResult
    {
        public int Value;
        public DataResult Next;
    }

    [Route("api/basic")]
    public class RecursiveStructureController : Controller
    {

        [HttpGet]
        public DataResult GetValues() 
            => new DataResult { Value = 1, Next = new DataResult { Value = 4, Next = null } };
    }
}