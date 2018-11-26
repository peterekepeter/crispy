using static System.Console;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FluentAssertions;
using System.Net.Http;
using System.Net;

namespace Test.JsWeb
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task WeCanActuallyStartTheServer()
        {
            var context = await Context.GetInstance();
            var httpClient = new HttpClient();
            var url = $"{context.Url}todo.html";
            var result = await httpClient.GetAsync(url);
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task WeCanActuallyRunJavascript()
        {
            var context = await Context.GetInstance();
            var result = await context.Command.Execute("3+4");
            result.Should().Be("7");
        }
    }
}
