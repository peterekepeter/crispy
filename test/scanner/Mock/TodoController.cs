using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Test.Scanner.Mock
{
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        static IList<String> list = new List<String>(){ "buy milk", "do homework" };
     
        [HttpGet]
        public IEnumerable<string> GetAll() => list;
        
        [HttpPost]
        public void Add([FromBody]string value) => list.Add(value);

        [HttpPatch]
        public void Update([FromQuery] string which, [FromBody] string value)
            => list[list.IndexOf(which)] = value;
    }
}
