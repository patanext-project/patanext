using System;
using Quadrum.Game.BGM;
using revghost;
using revghost.Injection.Dependencies;
using revghost.IO.Storage;
using revghost.Module;

namespace PataNext.Game.Client.Resources;

public class Module : HostModule
{
    private BgmContainerStorage bgmContainerStorage;
    
    public Module(HostRunnerScope scope) : base(scope)
    {
        Dependencies.AddRef(() => ref bgmContainerStorage);
    }

    protected override void OnInit()
    {
        bgmContainerStorage.AddStorage(new MultiStorage
        {
            ModuleScope.DllStorage,
            ModuleScope.DataStorage,
        }.GetSubStorage("Bgm"));
    }
}