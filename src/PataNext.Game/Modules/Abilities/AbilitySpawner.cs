using System.Runtime.CompilerServices;
using DefaultEcs;
using PataNext.Game.Modules.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs.Core;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Utility;

namespace PataNext.Game.Modules.Abilities;

public delegate UEntityHandle SpawnAbility(Entity data);

public class AbilitySpawner : SimulationSystem
{
    private World _world;

    public AbilitySpawner(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
    }
    
    private EntityMultiMap<ComponentType> _providerMap;

    protected override void OnInit()
    {
        _providerMap = _world.GetEntities()
            .With<SpawnAbility>()
            .AsMultiMap<ComponentType>();
    }

    public Entity Register(ComponentType type, BaseRhythmAbilityProvider provider)
    {
        HostLogger.Output.Info($"Register {Simulation.ComponentTypeBoard.Names[type.Handle]} ability");
        
        var hostEntity = _world.CreateEntity();
        hostEntity.Set(type);
        hostEntity.Set(new SpawnAbility(provider.SpawnEntity));

        return hostEntity;
    }

    public bool TrySpawnAbility(ComponentType componentType, Entity data, out UEntityHandle entity)
    {
        if (!data.Has<CreateAbility>())
            throw new InvalidOperationException($"{data} should have '{nameof(CreateAbility)}' component");
        
        Unsafe.SkipInit(out entity);
        if (!_providerMap.TryGetEntities(componentType, out var hostEntities))
            return false;

        foreach (var hostEntity in hostEntities)
        {
            entity = hostEntity.Get<SpawnAbility>()(data);
            return true;
        }

        entity = default;
        return false;
    }
    
    public bool TrySpawnAbility(ComponentType componentType, UEntitySafe owner, out UEntityHandle entity)
    {
        using var data = _world.CreateEntity();
        data.Set(new CreateAbility(owner));
        
        return TrySpawnAbility(componentType, data, out entity);
    }
}