using System;
using System.Threading;
using System.Threading.Tasks;
using revghost;
using revghost.Shared.Threading.Tasks;
using revghost.Threading;

namespace PataNext.Export.Desktop
{
    public class Program
    {
        public static void Main()
        {
            using var ghost = GhostInit.Launch(
                scope => {},
                scope => new EntryModule(scope)
                
                
                
                
                
                
                
                
                
            );

            Console.WriteLine($"Ver={Environment.Version}");
            var taskScheduler = new ConstrainedTaskScheduler();
            taskScheduler.StartUnwrap(async () =>
            {
                Console.WriteLine($"0 Scheduler={TaskScheduler.Current}");
                await Task.Yield();
                Console.WriteLine($"1 Scheduler={TaskScheduler.Current}");
                await Task.Delay(100);
                Console.WriteLine($"2 Scheduler={TaskScheduler.Current}");
                while (true)
                {
                    Console.WriteLine($"loop before Scheduler={TaskScheduler.Current}");
                    await Task.Delay(100);
                    Console.WriteLine($"loop after Scheduler={TaskScheduler.Current}");
                    await Task.Yield();
                }
            });
            
            while (true)
            {
                taskScheduler.Execute();
            }

            while (ghost.Loop())
            {
                Thread.Sleep(10);
            }
        }
    }
}