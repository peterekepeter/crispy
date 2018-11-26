using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.JsWeb
{
    [TestClass]
    public class TodoApiTests
    {
        Func<String, Task<String>> Execute;

        [TestInitialize]
        public async Task Setup(){
            var context = await Context.GetInstance();
            Execute = context.Command.Execute;
        }

        [TestMethod]
        public async Task CanCallGetMethodWithNoParameters()
        {
            var result = await Execute("api.todo.getAll()");
            Console.WriteLine(result);
            result.Should().StartWith(@"[""buy milk"",""do homework""");
        }

        [TestMethod]
        public async Task CanCallPostMethod(){
            await Execute(@"api.todo.add(""hello!"")");
            var check = await Execute("api.todo.getAll()");
            check.Should().Contain("hello!");
        }

    }
}