using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using GDNative;
using GodotCLR;
using GodotCLR.HighLevel;
using revghost;
using revghost.Domains;
using revghost.Injection;
using revghost.IO.Storage;
using revghost.Module.Storage;

namespace PataNext.Export.Godot;

public unsafe class Program
{
    private static GDNativeInterface* _interface;
    private static void* _library;
    
    [UnmanagedCallersOnly(EntryPoint = "lib_load", CallConvs = new []{typeof(CallConvCdecl)})]
    // the compiler on windows is racist and don't want to compile if we put the struct names instead of void*
    public static byte GodotLoad(void* gdInterfaceVoid, void* gdLibrary, void* gdInitVoid)
    {
        var gdInterface = (GDNativeInterface*) gdInterfaceVoid;
        var gdInit = (GDNativeInitialization*) gdInitVoid;

        gdInit->initialize = &InitializeLevel;
        gdInit->deinitialize = &DeinitializeLevel;
        gdInit->minimum_initialization_level = GDNativeInitializationLevel.GDNATIVE_INITIALIZATION_SCENE;

        _interface = gdInterface;
        _library = gdLibrary;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] {typeof(CallConvCdecl)})]
    private static void InitializeLevel(void* data, GDNativeInitializationLevel level)
    {
        Console.WriteLine($"Init(level={level})");

        if (level == GDNativeInitializationLevel.GDNATIVE_INITIALIZATION_SCENE)
        {
            Native.Load(_interface, _library);
            GodotModuleRunner.Create();
        }
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] {typeof(CallConvCdecl)})]
    private static void DeinitializeLevel(void* data, GDNativeInitializationLevel level)
    {
        Console.WriteLine($"Deinit(level={level})");
    }

    public static void Main()
    {
        throw new InvalidOperationException("PataNext.Export.Godot shouldn't be called via command line");
    }
}