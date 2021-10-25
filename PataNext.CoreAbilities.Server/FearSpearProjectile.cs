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
    public struct FearSpearProjectile : IComponentData
    {
        public const float RadiusInit      = 0.1f;
        public const float RadiusExplosion = 3.6f;
        
        public Vector3 Gravity;
        public bool    HasExploded;
    }

    public class FearSpearProjectileProvider : BaseProvider<(GameEntity owner, Vector3 pos, Vector3 vel, Vector3 gravity)>
    {
        private IPhysicsSystem physicsSystem;

        private Entity colliderSettings;
        
        private PooledList<ComponentReference> statusRefs;

        public FearSpearProjectileProvider(WorldCollection collection) : base(collection)
        {
            DependencyResolver.Add(() => ref physicsSystem);
            
            colliderSettings = World.Mgr.CreateEntity();
            colliderSettings.Set<Shape>(new CircleShape {Radius = FearSpearProjectile.RadiusInit});
            
            AddDisposable(statusRefs = new PooledList<ComponentReference>());
        }

        public override void GetComponents(PooledList<ComponentType> entityComponents)
        {
            entityComponents.AddRange(new[]
            {
                GameWorld.AsComponentType<ProjectileDescription>(),
                GameWorld.AsComponentType<Owner>(),
                GameWorld.AsComponentType<EntityVisual>(),
                GameWorld.AsComponentType<FearSpearProjectile>(),
                GameWorld.AsComponentType<Position>(),
                GameWorld.AsComponentType<Velocity>(),

                GameWorld.AsComponentType<HitBox>(),
                GameWorld.AsComponentType<HitBoxAgainstEnemies>(),
                GameWorld.AsComponentType<HitBoxHistory>(),

                GameWorld.AsComponentType<DamageFrameDataStatusEffect>(),
                GameWorld.AsComponentType<DamageFrameData>(),
            });
        }

        public override void SetEntityData(GameEntityHandle entity, (GameEntity owner, Vector3 pos, Vector3 vel, Vector3 gravity) args)
        {
            GameWorld.GetComponentData<Owner>(entity)                   = new Owner(args.owner);
            GameWorld.GetComponentData<FearSpearProjectile>(entity).Gravity = args.gravity;
            GameWorld.GetComponentData<Position>(entity).Value          = args.pos;
            GameWorld.GetComponentData<Velocity>(entity).Value          = args.vel;

            GameWorld.GetComponentData<HitBox>(entity) = new HitBox(args.owner, 0);

            if (GameWorld.HasComponent<Relative<TeamDescription>>(args.owner.Handle))
            {
                var team = GameWorld.GetComponentData<Relative<TeamDescription>>(args.owner.Handle);
                GameWorld.AddComponent(entity, new HitBoxAgainstEnemies(team.Target));
            }
            
            var statusEffectBuffer = GameWorld.GetBuffer<DamageFrameDataStatusEffect>(entity);
            {
                statusEffectBuffer.Clear();
                statusRefs.Clear();
                GameWorld.GetComponentOf(args.owner.Handle, GameWorld.AsComponentType<StatusEffectStateBase>(), statusRefs);
                foreach (var componentRef in statusRefs)
                {
                    ref readonly var state = ref GameWorld.GetComponentData<StatusEffectStateBase>(args.owner.Handle, componentRef.Type);
                    statusEffectBuffer.Add(new DamageFrameDataStatusEffect(state.Type, state.CurrentPower));
                }
            }

            GameWorld.GetComponentData<DamageFrameData>(entity) = new DamageFrameData(GameWorld.GetComponentData<UnitPlayState>(args.owner.Handle));

            physicsSystem.AssignCollider(entity, colliderSettings);
        }
    }

    [UpdateBefore(typeof(HitBoxAgainstEnemies))]
    public class FearSpearProjectileSystem : GameLoopAppSystem, IUpdateSimulationPass
    {
        private IManagedWorldTime worldTime;
        private IPhysicsSystem    physicsSystem;

        private Entity    explosionColliderSettings;
        private Scheduler postScheduler = new Scheduler();

        public FearSpearProjectileSystem(WorldCollection collection) : base(collection, false)
        {
            DependencyResolver.Add(() => ref worldTime);
            DependencyResolver.Add(() => ref physicsSystem);

            explosionColliderSettings = World.Mgr.CreateEntity();
            explosionColliderSettings.Set<Shape>(new CircleShape {Radius = FearSpearProjectile.RadiusExplosion});

            AddDisposable(explosionColliderSettings);
        }

        protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
        {
            base.OnDependenciesResolved(dependencies);

            Add((GameEntityHandle ent, ref FearSpearProjectile proj, ref Position pos, ref Velocity vel) =>
            {
                var history = GetBuffer<HitBoxHistory>(ent);
                if (history.Count > 0 || pos.Value.Y <= 0)
                {
                    if (proj.HasExploded)
                    {
                        ref var frameData = ref GetComponentData<DamageFrameData>(ent);
                        frameData.Attack              = (int) (frameData.Attack * 0.75f);
                        frameData.IgnoreDefense       = 0.9f;
                        frameData.IgnoreReceiveDamage = 0.9f;
                        
                        postScheduler.Schedule(onHit, (ent, worldTime.Total.Add(TimeSpan.FromSeconds(1))), default);
                    }
                    else
                    {                        
                        history.Clear();
                        
                        physicsSystem.AssignCollider(ent, explosionColliderSettings);
                        proj.HasExploded = true;
                    }

                    return true;
                }

                var dt = (float) worldTime.Delta.TotalSeconds;
                vel.Value += proj.Gravity * dt;
                pos.Value += vel.Value * (float) worldTime.Delta.TotalSeconds;

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
            postScheduler.Schedule(GameWorld.RemoveEntity, args.ent, default);
        }

        private EntityQuery entityWithoutBuffer;

        public void OnSimulationUpdate()
        {
            RunExecutors();

            postScheduler.Run();
        }
    }
}