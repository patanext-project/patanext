using DefaultEcs;
using PataNext.Game.Client.RhythmEngineAudio.BGM;
using Quadrum.Game.BGM;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared.Threading.Schedulers;
using revghost.Utility;

namespace PataNext.Game.Client.RhythmEngineAudio.Systems;

public class LoadBgmSystem : PresentationRhythmEngineSystemBase
{
    private string currentLoadedBgm;
    private bool isBgmLoaded; // temp

    private IScheduler scheduler;

    private World world;
    private BgmContainerStorage storage;
    
    public LoadBgmSystem(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref scheduler);
        Dependencies.Add(() => ref world);
        Dependencies.Add(() => ref storage);
    }

    protected override void OnUpdatePass(EngineQuery.Iteration engine, GameTime gameTime)
    {
        // TODO: Get the BGM id from somewhere (on the engine entity?)
        // For now we just load one BGM and stop the system
        if (isBgmLoaded)
            return;

        currentLoadedBgm = "ZippedTheme";
        isBgmLoaded = true;

        Task.Run(async () =>
        {
            BgmFile? lastBgm = null;
            await foreach (var bgm in storage.GetBgmAsync(currentLoadedBgm))
            {
                lastBgm = bgm;
            }
            
            if (lastBgm == null) 
                HostLogger.Output.Error($"BGM {currentLoadedBgm} not found!");
            else
            {
                HostLogger.Output.Info($"BGM {currentLoadedBgm} found!");
                scheduler.Add(LoadBgm, lastBgm);
            }
        });
    }

    private void LoadBgm(BgmFile file)
    {
        var director = BgmDirector.Create(file).Result;
        var entity = world.CreateEntity();
        entity.Set(director);

        isBgmLoaded = true;
    }
}