using DefaultEcs;
using PataNext.Game.Client.RhythmEngineAudio.Systems;
using Quadrum.Game.Modules.Simulation.Application;
using revghost;
using revghost.Injection.Dependencies;

namespace PataNext.Game.Client.RhythmEngineAudio.BGM.Directors;

public abstract class BgmDirectorSystemBase<TDirector, TLoader> : PresentationRhythmEngineSystemBase
    where TDirector : BgmDirectorBase
    where TLoader : BgmSamplesLoaderBase
{
    private World world;

    public BgmDirectorSystemBase(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref world);
    }

    private EntitySet directorSet;

    protected override void OnInit()
    {
        base.OnInit();

        Disposables.Add(
            directorSet = world.GetEntities()
                .With<BgmDirectorBase>()
                .AsSet()
        );
    }

    protected override void OnUpdatePass(EngineQuery.Iteration engine, GameTime gameTime)
    {
        if (directorSet.Count == 0)
            return;

        var directorEnt = directorSet.GetEntities()[0];
        if (directorEnt.Get<BgmDirectorBase>() is not TDirector director)
            return;

        OnUpdatePass(engine, gameTime, director, (TLoader) director.Loader);
    }

    protected abstract void OnUpdatePass(EngineQuery.Iteration engine, GameTime gameTime,
        TDirector director,
        TLoader loader);
}