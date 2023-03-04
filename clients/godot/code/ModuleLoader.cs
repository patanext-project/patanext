using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Godot;
using revghost;
using revghost.Domains;
using revghost.IO.Storage;
using revghost.Module;
using revghost.Module.Storage;
using revghost.Shared.Threading.Tasks;
using revghost.Threading;
using revghost.Utility;
using Environment = System.Environment;

namespace PataNext;

public partial class ModuleLoader : Node
{
    [Export] public string ModuleName;
    [Export] public string ModuleGroup;

    private GhostRunner _runner;

    private void SetDirectory(string path)
    {
        Environment.CurrentDirectory = path;

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

            string asmFile = Path.Combine(path, filename);
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

    private Module _runnerModule;

    public override void _Ready()
    {
        base._Ready();

        HostLogger.Output = (level, line, source, theme) =>
        {
            switch (level)
            {
                case HostLogLevel.Info:
                    GD.Print($"<INFO> [{source}:{theme}] {line}");
                    break;
                case HostLogLevel.Warn:
                    GD.Print($"<WARN> [{source}:{theme}] {line}");
                    GD.PushWarning($"<WARN> [{source}:{theme}] {line}");
                    break;
                case HostLogLevel.Error:
                    GD.Print($"<ERR> [{source}:{theme}] {line}");
                    GD.PushError($"<ERR> [{source}:{theme}] {line}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        };

        var path = Environment.CurrentDirectory;
        // Environment.CurrentDirectory = clients/godot/
        // If we're in the editor, go into /godot/.godot/mono/temp/bin/Debug (not release?)
        // todo: Else, i don't know yet
        if (OS.HasFeature("standalone"))
            path = OS.GetExecutablePath().GetBaseDir();
        else
            path += "/.godot/mono/temp/bin/Debug/";

        SetDirectory(path);

        _runner = GhostInit.Launch(
            sc =>
            {
                // ...
                sc.ExecutiveStorage = new ExecutiveStorage(new LocalStorage(path));
                sc.UserStorage = new LocalStorage(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                ).GetSubStorage("PataNext.Beta");
                sc.ModuleStorage = new ModuleCollectionStorage(new MultiStorage
                {
                    sc.ExecutiveStorage.GetSubStorage("Modules"),
                    sc.UserStorage.GetSubStorage("Modules")
                });

                GD.Print($"{sc.ExecutiveStorage.CurrentPath}");
                GD.Print($"{sc.UserStorage.CurrentPath}");
            },
            sc => { return new Module(sc, ModuleGroup, ModuleName); });
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        _runner.Loop();
    }

    private class Module : HostModule
    {
        private static HostLogger _logger = new("GodotNetLink");

        private readonly string _moduleGroup;
        private readonly string _moduleName;

        public Module(HostRunnerScope scope, string moduleGroup, string moduleName) : base(scope)
        {
            _moduleGroup = moduleGroup;
            _moduleName = moduleName;
            
            // HACK: REQUIRED OR ELSE ConstrainedTaskScheduler will have synchronization issues
            // TODO: (??? we need to investigate how we could fix it properly later)
            SynchronizationContext.SetSynchronizationContext(null);
        }

        protected override void OnInit()
        {
            // Register default modules
            // (User modules will get registered via the .json files in /Modules/ folder)
            RegisterModule(sc => new DefaultFlowModule(sc));
            RegisterModule(sc => new TestServerModule(sc));
            
            _logger.Info(
                $"Received request to load module '{_moduleGroup}/{_moduleName}'",
                "load-module"
            );
            try
            {
                LoadModule(_moduleGroup, _moduleName);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Exception when loading module '{_moduleGroup}/{_moduleName}'\n{ex}",
                    "load-module-failed"
                );
            }
        }
    }
}