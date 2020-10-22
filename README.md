

# Crispy

This library makes it relatively easy to call backend functions from javascript.
Gone are the days of worrying about http methods, urls and whatnot. 
Just create your controllers with methods and call them as any other function from javascript.

There is a console project that calls it on the values controller. 
For bigger projects it's recommended to use a console project and generate the API wrappers.
Configure it to be used as a build step or something.

There is also an example Web API project where it's used to generate javascript wrappers.
This is most suitable for experimentation, weekend projects, school projects and other small stuff.


## K, how do I use?

### Step 1: Generate JS for your API ###

Crispy library project contains the JsGenerator. That's what you need. 
You can create an API endpoint which serves the javascript to be used in your frontend application.

```csharp
using Crispy;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    public class CrispyController : Controller
    {
        [HttpGet]
        public ContentResult GetApiDefinition()
        {
            // create generator instance
            var generator = new JsGenerator()
                // and configure it
                .UseModuleType(ModuleLoaderType.GlobalVariable)
                .UseVariableName("api")
                // .UseTypescript(new TsOptions()) // uncomment to generate types
                .UsePrettyPrint();

            // get assembly of web project
            var assembly = typeof(CrispyController).GetTypeInfo().Assembly;

            // generate some js
            var javascript = generator.GenerateSingleFile(assembly, "ApiTest.Controllers");

            // all done
            return Content(javascript, "application/javascript");
        }
    }
}
```


### Step 2: Implement API for application logic ###

Here we have an example Http API controller for a simple todo list. 
We'll use it in this example. 
Note that we care about function names. 
Put the attributes so that you have control how parameters are sent. 
(current version only works if you use these attributes)

```csharp
using System;
using System.Collections.Generic;
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
    }
}
```

### Step 3: Use the C# methods in your JS code ###

That's all, we can call the backend code with `api.todo.getAll()` and `api.todo.add(item)`, easy!

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>My Todo List</title>
    <script src="api/crispy"></script>
</head>
<body>
    <h1>My Todo List:</h1>
    <ul id="my-todo"></ul>
    <form id="my-todo-add">
        <input class="item-text" type="text" name="todoItem" value=""/>
        <input type="submit"/>
    </form>
    <script>
        // render todo items from C#
        function reloadTodo() {
            api.todo.getAll().then(function (data) { // get all todo items from C#
                var container = document.querySelector('ul#my-todo');
                container.innerHTML = ''; // clear
                for (var i = 0; i < data.length; i++) {
                    // render each item
                    var item = document.createElement('li');
                    item.textContent = data[i];
                    container.appendChild(item);
                }
            });
        }
        // event handler for creating todo 
        document.querySelector('form#my-todo-add').addEventListener('submit', function (event) {
            event.preventDefault();
            var value = event.target.querySelector('input.item-text').value;
            api.todo.add(value).then(reloadTodo); // call C# method
            return false;
        });
        // initialize
        reloadTodo();
    </script>
</body>
</html>
```

If you're using an API project, you might want to include the static file module so that you can serve the html from above. 
Don't forget to add `app.UseStaticFiles();` into your `Startup.cs`.


## K, but this doesn't do XYZ

This project is still at the very beginning so a lot of features are missing.
Plans are to:

    - framework to expose C# methods via Crispy instead of regular API controllers, 
    this way you don't need to write Http controllers anymore, just regular C# classes.
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
Ofc feel free to fork, if you do something useful, create a pull request.


## License?

The Unlicense, as free as possibly possible.
