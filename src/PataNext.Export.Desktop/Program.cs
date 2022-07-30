using System.Threading;
using revghost;

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

            while (ghost.Loop())
            {
                Thread.Sleep(10);
            }
        }
    }
}