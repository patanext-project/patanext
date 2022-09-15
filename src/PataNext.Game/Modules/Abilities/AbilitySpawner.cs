using System.Runtime.CompilerServices;
using DefaultEcs;
using PataNext.Game.Modules.Abilities.Components;
using PataNext.Game.Modules.Abilities.SystemBase;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Application;
using Quadrum.Game.Modules.Simulation.Common.Systems;
using revecs;
using revecs.Core;
using revecs.Extensions.Generator.Commands;
using revghost;
using revghost.Injection.Dependencies;
using revghost.Shared.Collections;
using revghost.Utility;

namespace PataNext.Game.Modules.Abilities;

public delegate void SpawnAbility(ref UEntityHandle existingHandle, Entity data);

public partial class AbilitySpawner : SimulationSystem
{
    private static readonly HostLogger _logger = new(nameof(AbilitySpawner));
    
    private World _world;

    public AbilitySpawner(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _world);
        
        SubscribeTo<ISimulationUpdateLoopSubscriber>(OnUpdate);
    }
    
    private EntityMultiMap<ComponentType> _providerMap;
    private SpawnQuery _query;
    private Commands _cmd;

    protected override void OnInit()
    {
        _providerMap = _world.GetEntities()
            .With<SpawnAbility>()
            .AsMultiMap<ComponentType>();
        _query = new SpawnQuery(Simulation);
        _cmd = new Commands(Simulation);
    }

    private void OnUpdate(Entity _)
    {
        SynchronizeArchetypes();
        
        using var copyList = new ValueList<UEntityHandle>(0);
        foreach (var entity in _query)
        {
            Console.WriteLine($"add {entity.Handle} {_cmd.Exists(entity.Handle)} {Simulation.GetArchetype(entity.Handle)}");
            copyList.Add(entity.Handle);
        }

        foreach (var req in copyList)
        {
            ref var data = ref _cmd.UpdateSetSpawnAbility(req);
            if (!_cmd.Exists(data.Owner))
            {
                _logger.Warn($"{req} {data.Owner} does not exist anymore (type='{getName(data.AbilityType)}')", "update-spawn");
                _cmd.DestroyEntity(req);
                continue;
            }

            UEntityHandle spawn;
            if (data.Source is { } source)
            {
                if (!_cmd.Exists(data.Owner))
                {
                    _logger.Error($"{data.Source} does not exist anymore");
                    _cmd.DestroyEntity(req);
                    continue;
                }
                
                spawn = source.Handle;
            }
            else
            {
                spawn = Simulation.CreateEntity();
                data.Source = Simulation.Safe(spawn);
            }
            
            if (!TrySpawnAbility(data.AbilityType, data.Owner, ref spawn))
                continue;
            
            _logger.Info($"Spawned ability '{getName(data.AbilityType)}' of {data.Owner}", "update-spawn");
            if (!req.Equals(spawn))
            {
                _cmd.DestroyEntity(req); }
            else
            {
                _cmd.RemoveSetSpawnAbility(req);
            }

            string getName(ComponentType type)
            {
                return Simulation.ComponentTypeBoard.Names[type.Handle];
            }
        }
    }

    public Entity Register(ComponentType type, BaseRhythmAbilityProvider provider)
    {
        HostLogger.Output.Info($"Register {Simulation.ComponentTypeBoard.Names[type.Handle]} ability");
        
        var hostEntity = _world.CreateEntity();
        hostEntity.Set(type);
        hostEntity.Set(new SpawnAbility(provider.SetEntityData));

        return hostEntity;
    }

    public bool TrySpawnAbility(ComponentType componentType, Entity data, ref UEntityHandle handle)
    {
        if (!data.Has<CreateAbility>())
            throw new InvalidOperationException($"{data} should have '{nameof(CreateAbility)}' component");
        
        if (!_providerMap.TryGetEntities(componentType, out var hostEntities))
            return false;

        foreach (var hostEntity in hostEntities)
        {
            if (Unsafe.IsNullRef(ref handle))
                handle = Simulation.CreateEntity();
            
            hostEntity.Get<SpawnAbility>()(ref handle, data);
            return true;
        }

        return false;
    }
    
    public bool TrySpawnAbility(ComponentType componentType, UEntitySafe owner, ref UEntityHandle handle)
    {
        using var data = _world.CreateEntity();
        data.Set(new CreateAbility(owner));
        
        return TrySpawnAbility(componentType, data, ref handle);
    }

    private partial record struct SpawnQuery : IQuery<Read<SetSpawnAbility>>;

    private partial record struct Commands : ICmdEntityAdmin, SetSpawnAbility.Cmd.IAdmin;
}