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

        Task StartAsync()
        {
            WriteLine("Server thread started!");
            var currentDir = Directory.GetCurrentDirectory();
            WriteLine($"Current dir is: {currentDir}");
            var serverDir = currentDir + "/../../../../server";
            var host = new WebHostBuilder()
                .UseKestrel(option => option.ListenLocalhost(this.Port))
                .UseContentRoot(serverDir)
                .UseStartup<ApiTest.Startup>()
                .Build();
            
            Command = host.Services.GetService<CommandService>();
            host.RunAsync();
            return Task.Delay(1);
        }

        Task StartBrowser(){
            
            var path = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            WriteLine($"Looking for chrome {path}");
            if (File.Exists(path)){
                WriteLine("Found!");
            }
            var info = new ProcessStartInfo(){
                FileName = path,
                Arguments = $"{Url}command.html"
            };
            
            var process = Process.Start(info);
            return Task.CompletedTask;
        }
    }


}