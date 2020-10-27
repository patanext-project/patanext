using System.Numerics;
using Collections.Pooled;
using GameHost.Core.Ecs;
using GameHost.Simulation.TabEcs;
using GameHost.Simulation.TabEcs.Interfaces;
using StormiumTeam.GameBase.Physics.Components;
using StormiumTeam.GameBase.Roles.Components;
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
            entityComponents.AddRange(new [] 
            {
                GameWorld.AsComponentType<Owner>(),
                GameWorld.AsComponentType<SpearProjectile>(),
                GameWorld.AsComponentType<Position>(),
                GameWorld.AsComponentType<Velocity>()
            });
        }

        public override void SetEntityData(GameEntity entity, (GameEntity owner, Vector3 pos, Vector3 vel, Vector3 gravity) args)
        {
            GameWorld.GetComponentData<Owner>(entity) = new Owner(args.owner);
            GameWorld.GetComponentData<SpearProjectile>(entity).Gravity = args.gravity;
            GameWorld.GetComponentData<Position>(entity).Value = args.pos;
            GameWorld.GetComponentData<Velocity>(entity).Value = args.vel;
        }
    }
}