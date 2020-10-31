using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using Collections.Pooled;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.EntityQuery;
using GameHost.Simulation.Utility.Resource;
using GameHost.Worlds.Components;
using PataNext.Module.Simulation.Components.GamePlay;
using PataNext.Module.Simulation.Components.Roles;
using PataNext.Module.Simulation.Game.Visuals;
using StormiumTeam.GameBase;
using StormiumTeam.GameBase.GamePlay;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Physics.Systems;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Descriptions;
using StormiumTeam.GameBase.SystemBase;
using StormiumTeam.GameBase.Transform.Components;

namespace PataNext.CoreAbilities.Server
{
    public struct SpearProjectile : IComponentData
    {
        public Vector3 Gravity;
    }

    public class SpearProjectileProvider : BaseProvider<(GameEntity owner, Vector3 pos, Vector3 vel, Vector3 gravity)>
    {
        public SpearProjectileProvider(WorldCollection collection) : base(collection)
        {
        }

        public override void GetComponents(PooledList<ComponentType> entityComponents)
        {
            entityComponents.AddRange(new[]
            {
                GameWorld.AsComponentType<ProjectileDescription>(),
                GameWorld.AsComponentType<Owner>(),
                GameWorld.AsComponentType<EntityVisual>(),
                GameWorld.AsComponentType<SpearProjectile>(),
                GameWorld.AsComponentType<Position>(),
                GameWorld.AsComponentType<Velocity>()
            });
        }

        public override void SetEntityData(GameEntity entity, (GameEntity owner, Vector3 pos, Vector3 vel, Vector3 gravity) args)
        {
            GameWorld.GetComponentData<Owner>(entity)                   = new Owner(args.owner);
            GameWorld.GetComponentData<EntityVisual>(entity)       = default;
            GameWorld.GetComponentData<SpearProjectile>(entity).Gravity = args.gravity;
            GameWorld.GetComponentData<Position>(entity).Value          = args.pos;
            GameWorld.GetComponentData<Velocity>(entity).Value          = args.vel;
        }
    }

    public class SpearProjectileSystem : GameLoopAppSystem, IUpdateSimulationPass
    {
        private PhysicsSystem     physicsSystem;
        private IManagedWorldTime worldTime;

        public SpearProjectileSystem(WorldCollection collection) : base(collection, false)
        {
            DependencyResolver.Add(() => ref physicsSystem);
            DependencyResolver.Add(() => ref worldTime);
        }

        private EntityQuery colliderQuery;
        private Sphere      sphereCollider;

        protected override void OnDependenciesResolved(IEnumerable<object> dependencies)
        {
            base.OnDependenciesResolved(dependencies);

            colliderQuery  = CreateEntityQuery(new[] {typeof(PhysicsCollider), typeof(Position)});
            sphereCollider = new Sphere(0.1f);

            Add((GameEntity ent, ref SpearProjectile proj, ref Position pos, ref Velocity vel) =>
            {
                pos.Value += vel.Value * (float) worldTime.Delta.TotalSeconds;
                vel.Value += proj.Gravity * (float) worldTime.Delta.TotalSeconds;

                var didCollide = pos.Value.Y <= 0; // ground may have a collidable or not, but we don't care
                if (TryGetComponentData<Relative<TeamDescription>>(ent, out var teamRelative)
                    && TryGetComponentBuffer<TeamEnemies>(teamRelative.Target, out var teamEnemies))
                {
                    foreach (var teamEnemy in teamEnemies)
                    {
                        if (!TryGetComponentBuffer<TeamEntityContainer>(teamEnemy.Team, out var container))
                            continue;

                        foreach (var entity in container)
                        {
                            if (!colliderQuery.MatchAgainst(entity.Value))
                                continue;

                            if (!physicsSystem.Sweep(entity.Value, sphereCollider, new RigidPose(pos.Value), new BodyVelocity(vel.Value), out var hit))
                                continue;

                            didCollide = true;
                            break;
                        }

                        if (didCollide) // goto?
                            break;
                    }
                }

                if (didCollide)
                {
                    LoopScheduler.Schedule(GameWorld.RemoveEntity, ent, default);
                }

                return true;
            });
        }

        private EntityQuery entityWithoutBuffer;

        public void OnSimulationUpdate()
        {
            colliderQuery.CheckForNewArchetypes();
            RunExecutors();
        }
    }
}