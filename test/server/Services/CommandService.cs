using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace ApiTest.Services
{
    public class CommandObject
    {
        public string Js;
        public int Id;
    }

    public class CommandService
    {
        public CommandService(){
            Task.Run(async () => {
                while(true){
                    await Task.Delay(1000);
                    Console.WriteLine(" !!!! trying to exec");
                    await Execute($"console.log('hello from backend {IdGenerator}');");
                }
            });
        }

        internal int IdGenerator = 0;

        private class Command
        {
            internal CommandObject Obj;
            internal TaskCompletionSource<string> Completion;
            

            internal Command(string js, int id){
                Obj = new CommandObject{
                    Js = js,
                    Id = id
                };
                Completion = new TaskCompletionSource<string>();
            }
        }

        public class CommandException : Exception
        {
            public CommandException(string message) : base(message) { }
        }

        private ConcurrentDictionary<int, Command> CommandRegistry
            = new ConcurrentDictionary<int, Command>();

        private BufferBlock<Command> CommandQueue
            = new BufferBlock<Command>();

        public async Task<CommandObject> GetExecutionCommand()
        {
            var item = await CommandQueue.ReceiveAsync();
            
            Task.Delay(1000).ContinueWith(async (task)=>{
                if (item.Completion.Task.IsCompleted) { return; }
                await Console.Out.WriteAsync($"Re-issuing command {item.Obj.Id}");
                await CommandQueue.SendAsync(item);
            });
            return item.Obj;
        }

        public void SubmitExecutionResult(int id, bool success, string result)
        {
            if (!CommandRegistry.TryGetValue(id, out var item)){
                throw new ArgumentException("Command not found!");
            }
            bool transitioned;
            if (success){
                transitioned = item.Completion.TrySetResult(result);
            }
            else {
                var exception = new CommandException(result);
                transitioned = item.Completion.TrySetException(exception);
            }
            if (!transitioned){
                throw new InvalidOperationException("Failed to update Command!");
            }
        }

        public async Task<string> Execute(string js)
        {
            var id = Interlocked.Increment(ref IdGenerator);
            var command = CommandRegistry.GetOrAdd(id, new Command(js, id));
            if (await CommandQueue.SendAsync(command) == false){
                throw new InvalidOperationException("Command was rejected.");
            }
            return await command.Completion.Task;
        }

    }
}