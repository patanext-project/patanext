using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Box2D.NetStandard.Collision.Shapes;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core;
using GameHost.Core.Ecs;
using GameHost.Core.Threading;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.GamePlay.Units;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Components.Units;
using PataNext.Module.Simulation.Game.GamePlay.Damage;
using PataNext.Module.Simulation.Game.Visuals;
using PataNext.Module.Simulation.Systems;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.GamePlay.Health;
using StormiumTeam.GameBase.GamePlay.HitBoxes;
using StormiumTeam.GameBase.GamePlay.Projectiles;
using StormiumTeam.GameBase.Physics;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Time.Components;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server
{
    public struct BouncingProjectile : IComponentData
    {
        public Vector3 Gravity;
        public Vector3 Reflection;
        public int     CurrentBounce;
        public int     CurrentHit;
        public int     MaxBounce;
    }

    public struct BouncingProjectileCreate
    {
        public GameEntity                        Owner;
        public Vector3                           Pos, Vel, Gravity, Reflection;
        public TimeSpan                          Duration;
        public int                               MaxBounces;
        public bool                              IsBox;
        public GameResource<GameGraphicResource> Graphic;
    }

    public class BouncingProjectileProvider : BaseProvider<BouncingProjectileCreate>
    {
        private IPhysicsSystem    physicsSystem;
        private IManagedWorldTime worldTime;

        private GameResourceDb<GameGraphicResource> graphicResourceDb;

        private Entity boxSettings;
        private Entity sphereSettings;

        private GameResource<GameGraphicResource> defaultVisualResource;
        private ResPathGen                        resPathGen;

        private PooledList<ComponentReference> statusRefs;

        public BouncingProjectileProvider(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref physicsSystem);
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref graphicResourceDb);
            DependencyResolver.Add(() => ref resPathGen);

            boxSettings = World.Mgr.CreateEntity();
            boxSettings.Set<Shape>(new PolygonShape(1f, 1f));

            sphereSettings = World.Mgr.CreateEntity();
            sphereSettings.Set<Shape>(new CircleShape {Radius = 0.5f});

            AddDisposable(statusRefs = new PooledList<ComponentReference>());
        }

        protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
        {
            base.OnDependenciesResolved(dependencies);

            defaultVisualResource = graphicResourceDb.GetOrCreate(resPathGen.Create(new[] {"Models", "InGame", "Projectiles", "SonicBall", "Projectile"}, ResPath.EType.ClientResource));
        }

        public override void GetComponents(PooledList<ComponentType> entityComponents)
        {
            entityComponents.AddRange(new[]
            {
                GameWorld.AsComponentType<ProjectileDescription>(),
                GameWorld.AsComponentType<Owner>(),
                GameWorld.AsComponentType<EntityVisual>(),
                GameWorld.AsComponentType<BouncingProjectile>(),
                GameWorld.AsComponentType<Position>(),
                GameWorld.AsComponentType<Velocity>(),

                GameWorld.AsComponentType<HitBox>(),
                GameWorld.AsComponentType<HitBoxAgainstEnemies>(),
                GameWorld.AsComponentType<HitBoxHistory>(),

                GameWorld.AsComponentType<DamageFrameData>(),
                GameWorld.AsComponentType<DamageFrameDataStatusEffect>()
            });
        }

        public override void SetEntityData(GameEntityHandle entity, BouncingProjectileCreate args)
        {
            GameWorld.GetComponentData<Owner>(entity) = new Owner(args.Owner);
            GameWorld.GetComponentData<BouncingProjectile>(entity) = new()
            {
                Gravity    = args.Gravity,
                Reflection = args.Reflection,
                MaxBounce  = args.MaxBounces
            };
            GameWorld.GetComponentData<Position>(entity).Value = args.Pos;
            GameWorld.GetComponentData<Velocity>(entity).Value = args.Vel;

            if (args.Duration > TimeSpan.Zero)
                GameWorld.AddComponent(entity, new RemoveEntityEndTime(worldTime.Total + args.Duration));

            GameWorld.GetComponentData<HitBox>(entity) = new HitBox(args.Owner, 0);

            if (GameWorld.HasComponent<Relative<TeamDescription>>(args.Owner.Handle))
            {
                var team = GameWorld.GetComponentData<Relative<TeamDescription>>(args.Owner.Handle);
                GameWorld.AddComponent(entity, new HitBoxAgainstEnemies(team.Target));
            }

            GameWorld.GetComponentData<DamageFrameData>(entity) = new DamageFrameData(GameWorld.GetComponentData<UnitPlayState>(args.Owner.Handle));
            GameWorld.GetComponentData<EntityVisual>(entity)    = new EntityVisual(args.Graphic == default ? defaultVisualResource : args.Graphic);

            var statusEffectBuffer = GameWorld.GetBuffer<DamageFrameDataStatusEffect>(entity);
            {
                statusEffectBuffer.Clear();
                statusRefs.Clear();
                GameWorld.GetComponentOf(args.Owner.Handle, GameWorld.AsComponentType<StatusEffectStateBase>(), statusRefs);
                foreach (var componentRef in statusRefs)
                {
                    ref readonly var state = ref GameWorld.GetComponentData<StatusEffectStateBase>(args.Owner.Handle, componentRef.Type);
                    statusEffectBuffer.Add(new DamageFrameDataStatusEffect(state.Type, state.CurrentPower));
                }
            }

            physicsSystem.AssignCollider(entity, args.IsBox ? boxSettings : sphereSettings);
        }
    }

    [UpdateBefore(typeof(HitBoxAgainstEnemies))]
    public class BouncingProjectileSystem : GameLoopAppSystem, IUpdateSimulationPass
    {
        private IManagedWorldTime worldTime;

        private Scheduler postScheduler = new Scheduler(ex => true);

        public BouncingProjectileSystem(WorldCollection collection) : base(collection, false)
        {
            DependencyResolver.Add(() => ref worldTime);
        }

        private Vector3 Reflect(Vector3 reflection, Vector3 value, Vector3 normal)
        {
            var reflected = Vector3.Reflect(value, normal);
            reflected.X =  Math.Clamp(reflected.X, -Math.Abs(value.X), Math.Abs(value.X));
            reflected   *= reflection;
            return reflected;
        }
        
        protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
        {
            base.OnDependenciesResolved(dependencies);

            Add((GameEntityHandle ent, ref BouncingProjectile proj, ref Position pos, ref Velocity vel) =>
            {
                var dt = (float) worldTime.Delta.TotalSeconds;
                vel.Value += proj.Gravity * dt;
                pos.Value += vel.Value * (float) worldTime.Delta.TotalSeconds;

                var history = GetBuffer<HitBoxHistory>(ent);
                if (history.Count > proj.CurrentHit || pos.Value.Y <= 0)
                {
                    proj.CurrentBounce += 1;
                    
                    if (proj.CurrentBounce >= proj.MaxBounce)
                        postScheduler.Schedule(onHit, (ent, worldTime.Total.Add(TimeSpan.FromSeconds(1))), default);
                    else
                    {
                        if (history.Count > proj.CurrentHit)
                        {
                            vel.Value       = Reflect(proj.Reflection, vel.Value, history[proj.CurrentHit].Normal);
                            proj.CurrentHit = history.Count;
                        }
                        else if (pos.Value.Y <= 0)
                        {
                            vel.Value   = Reflect(proj.Reflection, vel.Value, Vector3.UnitY);
                            pos.Value.Y = 0;
                        }
                    }
                }

                return true;
            }, CreateEntityQuery(none: new[] {typeof(ProjectileEndedTag)}));
        }

        private void onHit((GameEntityHandle ent, TimeSpan time) args)
        {
            GameWorld.AddRemoveMultipleComponent(args.ent, stackalloc[]
            {
                AsComponentType<ProjectileEndedTag>(),
                AsComponentType<ProjectileExplodedEndReason>()
            }, stackalloc[]
            {
                AsComponentType<HitBox>()
            });
            postScheduler.Schedule(removeEntity, Safe(args.ent), default);
        }

        private void removeEntity(GameEntity ent)
        {
            // this can be possible if there was a RemoveEntityEndTime component
            if (!GameWorld.Exists(ent))
                return;
            
            GameWorld.RemoveEntity(ent.Handle);
        }

        private EntityQuery entityWithoutBuffer;

        public void OnSimulationUpdate()
        {
            RunExecutors();

            postScheduler.Run();
        }
    }
}