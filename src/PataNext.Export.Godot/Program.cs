using System.Runtime.Loader;
using GodotCLR;
using GodotCLR.HighLevel;
using revghost;
using revghost.Domains;
using revghost.Injection;
using revghost.IO.Storage;
using revghost.Module.Storage;

namespace PataNext.Export.Godot;

public class Program
{
    public static unsafe int Load(IntPtr ptr, int args)
    {
        Console.WriteLine("Program - Load");
        
        GodotHL.Load(ptr, OnUpdate, OnClean, OnExchange);

        var directory = Native.GetDirectory();
        for (var i = 0; i < directory.Length; i++)
        {
            if (directory[i] == '\0')
            {
                directory = directory[..i];
                break;
            }
        }

        Environment.CurrentDirectory = directory;

        AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
        {
            Console.WriteLine($"trying to resolve: {eventArgs.Name} (requested by {eventArgs.RequestingAssembly})");
                
            // check for assemblies already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == eventArgs.Name);
            if (assembly != null)
                return assembly;
                
            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string filename = eventArgs.Name.Split(',')[0] + ".dll".ToLower();

            string asmFile = Path.Combine(directory,filename);
            try
            {
                return AssemblyLoadContext.GetLoadContext(eventArgs.RequestingAssembly)
                    .LoadFromAssemblyPath(asmFile);
                //return Assembly.LoadFrom(asmFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION ON RESOLVE: " + ex);
                return null;
            }
        };
            
        OnInit(directory);
        return 0;
    }

    private static void OnExchange(Variant subject, VariantMethodArgs args, VariantArgBuilder ret)
    {
        GodotCLR.Godot.Print(subject.AsString());
        if (subject.SequenceEquals("party_toggle"))
        {
            ret.Add("YEAH");
        }
    }

    private static GhostRunner runner;
    private static bool runnerLoopResult;
    
    private static void OnInit(string directory)
    {
        Console.WriteLine("Program - OnInit");
        
        runner = GhostInit.Launch(sc =>
        {
            // ...
            sc.ExecutiveStorage = new ExecutiveStorage(new LocalStorage(directory));
            sc.UserStorage = new LocalStorage(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            ).GetSubStorage("GameHostExp");
            sc.ModuleStorage = new ModuleCollectionStorage(new MultiStorage
            {
                sc.ExecutiveStorage.GetSubStorage("Modules"),
                sc.UserStorage.GetSubStorage("Modules")
            });
        }, sc => new EntryModule(sc));
        runnerLoopResult = true;
    }

    public static void Main()
    {
        throw new InvalidOperationException("PataNext.Export.Godot shouldn't be called via command line");
    }

    private static void OnUpdate()
    {
        runnerLoopResult = runner.Loop();
        
        /*var resolver = (runner.Scope.DependencyResolver as SchedulerDependencyResolver)!;
        var list = new List<IDependencyCollection>();
        resolver.GetQueuedCollections(ref list);
        foreach (var collection in list)
        {
            Console.WriteLine($"{(collection as DependencyCollection).Source.ToString()}");
            foreach (var dep in collection.Dependencies)
            {
                Console.WriteLine($"  :: {dep.ToString()}");
            }
        }*/
    }

    private static void OnClean()
    {
        runnerLoopResult = false;
        runner.Dispose();
    }
}