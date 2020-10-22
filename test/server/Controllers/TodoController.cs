using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    public class TodoController : Controller
    {
        static IList<String> list = new List<String>(){ "buy milk", "do homework" };
     
        [HttpGet]
        public IEnumerable<string> GetAll()
        {
            return list;
        }
        
        [HttpPost]
        public void Add([FromBody]string value)
        {
            list.Add(value);
        }

        public enum State
        {
            Open = 0,
            Closed = 1
        }

        public class HighLowTemps
        {
            public State CurrentState;
        }

        [HttpGet("xyz")]
        public async Task<HighLowTemps> GetThing()
        {
            return new HighLowTemps{ CurrentState = State.Closed };
        }
    }
}
