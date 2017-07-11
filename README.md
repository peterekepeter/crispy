
# Js2Cs

This library makes it relatively easy to call backend functions from javascript.
Gone are the days of worrying about http methods, urls and whatnot. 
Just create your controllers with methods and call them as any other function from javascript.


## Example

C# backend part:

	[Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static IList<String> list = new List<String>();
        // GET api/values
        [HttpGet]
        public IEnumerable<string> GetAll()
        {
            return list;
        }
	}

JS awesomeness:

	api.values.getAll().then(data => console.log(data));


## K, how do I use?

Js2Cs library project contains the JsGenerator. That's what you need. There is a console project that calls it on the values controller.
Well I still have to figure it out.

In the future this will be a nuget package you just drop in into your web project and hook up to the generator to a controller and you include that controller as a script.


## K, but this doesn't do XYZ

This project is still at the very beginning so a lot of features are missing.
Plans are to:

	- support both .net Core and classic .net
	- role based api generation, only show functions that are accessible from users role
	- convert DateTime to proper JS dates inside objects
	- example codes, improve usability, offer quick ways to integrate into existing project
	- future generator options:
		- choice between promises or callbacks
		- support AMD, CommonoJS or custom global mount point
		- support for newer / older JS syntax


## How can I help

Well please open an issue first so we can discuss what improvements you need. 
Ofc feel free to fork 


## License?

The Unlicense, as free as possibly possible.
