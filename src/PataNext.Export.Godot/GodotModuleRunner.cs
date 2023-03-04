using System.Diagnostics;
using System.Runtime;
using System.Runtime.Loader;
using GDNative;
using GodotCLR;
using GodotCLR.HighLevel;
using PataNext.Export.Godot.GodotComponents;
using revghost;
using revghost.Domains;
using revghost.IO.Storage;
using revghost.Module;
using revghost.Module.Storage;
using revghost.Shared.Threading.Tasks;
using revghost.Threading;

namespace PataNext.Export.Godot;

public class GodotModuleRunner
{
    private static GhostRunner _runner;

    private static void Load(string moduleGroup, string moduleName, string directoryPath)
    {
        Console.WriteLine($"Load module: '{moduleGroup}/{moduleName}'");
        Console.WriteLine($"Directory path: '{directoryPath}'");

        SetDirectoryPath(directoryPath);

        _runner = GhostInit.Launch(
            sc =>
            {
                // ...
                sc.ExecutiveStorage = new ExecutiveStorage(new LocalStorage(directoryPath));
                sc.UserStorage = new LocalStorage(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                ).GetSubStorage("GameHostExp");
                sc.ModuleStorage = new ModuleCollectionStorage(new MultiStorage
                {
                    sc.ExecutiveStorage.GetSubStorage("Modules"),
                    sc.UserStorage.GetSubStorage("Modules")
                });
            },
            sc => new HelperRootModule(sc)
        );
        while (_loadModuleAction == null)
        {
            if (!_runner.Loop())
                throw new InvalidOperationException("loop broken");
        }

        _loadModuleAction(moduleGroup, moduleName);
    }

    private static void SetDirectoryPath(string directory)
    {
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
    }

    private static long previousMemSize;
    private static TimeSpan now = DateTime.Now.TimeOfDay;
    
    private static bool Loop()
    {
        /*var mem = GC.GetTotalMemory(false);
        var delta = mem - previousMemSize;
        previousMemSize = mem;

        if (delta != 0)
        {
            Console.Write("Memory Delta: ");
            Console.Write(delta);
            Console.Write("b");
            Console.WriteLine();
        }

        if (DateTime.Now.TimeOfDay > now.Add(TimeSpan.FromSeconds(4)) 
            && GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
        {
            Console.WriteLine("Entering No GC zone");
            GC.TryStartNoGCRegion(int.MaxValue);
        }*/

        Debug.Assert(_runner != null, "_runner != null");
        return _runner.Loop();
    }

    public static void Create()
    {
        GD.RegisterClass<int>(nameof(GodotModuleRunner), "Node");
        GD.AddMethod<int>(
            nameof(GodotModuleRunner),
            "load_module",
            (ref byte _, ref int _, VariantMethodArgs args) =>
            {
                Load(
                    args[0].AsString(),
                    args[1].AsString(),
                    args[2].AsString()
                );
                return default;
            },
            new (Variant.EType, string)[]
            {
                (Variant.EType.STRING, "module_group"),
                (Variant.EType.STRING, "module_name"),
                (Variant.EType.STRING, "directory_path")
            },
            Variant.EType.NIL,
            GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_STATIC
        );
        GD.AddMethod<int>(
            nameof(GodotModuleRunner),
            "loop",
            (ref byte _, ref int _, VariantMethodArgs args) =>
            {
                return new Variant {Type = Variant.EType.BOOL, Bool = Loop()};
            },
            default,
            Variant.EType.NIL,
            GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_STATIC
        );

        SplineModel.Load();
    }

    private static Action<string, string> _loadModuleAction;

    private class HelperRootModule : HostModule
    {
        public HelperRootModule(HostRunnerScope scope) : base(scope)
        {
        }

        protected override void OnInit()
        {
            Console.WriteLine($"{GetModuleGroupName(typeof(EntryModule))}/{GetModuleName(typeof(EntryModule))}");
            
            RegisterModule(sc => new EntryModule(sc));
            _loadModuleAction = LoadModule;
        }
    }
}