
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Test.Scanner.Mock
{
    [Route("api/basic")]
    public class BasicController : Controller
    {
        [HttpGet]
        public List<string> GetValues() 
            => new List<string> { "test", "develop" };
    }
}