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
        public class MoreTypes
        {
            public DateTime DateTime;
            public DateTimeOffset DateTimeOffset;
            public TimeSpan TimeSpan;
        }

        [HttpGet("xyz")]
        public async Task<MoreTypes> GetThing()
        {
            return new MoreTypes{ DateTime = DateTime.UtcNow, DateTimeOffset = DateTimeOffset.UtcNow, TimeSpan = TimeSpan.FromHours(4) };
        }
    }
}
