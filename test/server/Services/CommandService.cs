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

        private int connectionStat = 0;

        private TaskCompletionSource<object> firstClient 
            = new TaskCompletionSource<object>();

        public async Task<Boolean> WaitClientAsync(int timeout)
        {
            if (connectionStat >= 1) { return true; }
            var waitTask = firstClient.Task;
            var delayTask = Task.Delay(timeout);
            var firstComplete = await Task.WhenAny(waitTask, delayTask);
            if (firstComplete == delayTask){ return false; }
            return true;
        }

        public async Task<CommandObject> GetExecutionCommand()
        {
            if (Interlocked.Increment(ref connectionStat) == 1){
                firstClient.SetResult(null);
            }
            var item = await CommandQueue.ReceiveAsync();
            
            Task.Delay(2000).ContinueWith(async (task)=>{
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