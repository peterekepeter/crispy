
using Microsoft.AspNetCore.Mvc;

namespace Test.Scanner.Mock
{
    public class RoutelessController : Controller
    {
        public string Index() => "42";
    }
}