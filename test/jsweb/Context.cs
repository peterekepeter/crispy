using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ApiTest.Services;
using System.Diagnostics;

namespace Test.JsWeb
{
    public class Context
    {
        private static SemaphoreSlim factorySemaphore = new SemaphoreSlim(1,1);
        private static Context _instance;

        public int Port { get; }
        public string Url { get; }
        public CommandService Command { get; private set; }

        public static async Task<Context> GetInstance(){
            if (_instance != null) { return _instance; };
            await factorySemaphore.WaitAsync();
            try{
                WriteLine("Aquired factory lock!");
                if (_instance != null) { return _instance; };
                _instance = new Test.JsWeb.Context();
                
                WriteLine("Creating Server Instance");
                await Task.WhenAll(_instance.StartAsync(), _instance.StartBrowser());
                await _instance.Command.Execute("console.log(42)");
                return _instance;
            }
            finally
            {
                factorySemaphore.Release();
            }
        }

        Context(){
            this.Port = 8080;
            this.Url = $"http://localhost:{this.Port}/";
        }

        Task<string> Execute(string javascript) 
            => _instance.Execute(javascript);

        async Task StartAsync()
        {
            WriteLine("Server thread started!");
            var currentDir = Directory.GetCurrentDirectory();
            WriteLine($"Current dir is: {currentDir}");
            var dir = currentDir;
            var separator = "\\";
            while (!dir.EndsWith("test")) { 
                var lastIndex = dir.LastIndexOf(separator);
                if (lastIndex <= 0) { break; }
                dir = dir.Substring(0, lastIndex);
                WriteLine(dir);
            }
            var serverDir = dir + separator + "server";
            WriteLine($"Using directory: {serverDir}");
            var host = new WebHostBuilder()
                .UseKestrel(option => option.ListenLocalhost(this.Port))
                .UseContentRoot(serverDir)
                .UseStartup<ApiTest.Startup>()
                .Build();
            
            Command = host.Services.GetService<CommandService>();
            await host.StartAsync();
        }

        async Task StartBrowser(bool firefox = true){
            if (await Command.WaitClientAsync(2000)){
                WriteLine("Reconnected to previous tab!");
                return;
            }

            // works well on firefox
            var firefoxPath= "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe";
            // for some reason it doesnt work on chrome
            var chromePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            
            var path = firefox ? firefoxPath : chromePath;

            WriteLine($"Looking for a browser {path}");
            if (File.Exists(path)){
                WriteLine("Found a browser!");
            }
            var info = new ProcessStartInfo(){
                FileName = path,
                Arguments = $"{Url}command.html"
            };
            
            var process = Process.Start(info);
            return;
        }
    }


}