using System;
using P4.Core.RythmEngine;
using Packages.pack.guerro.shared.Scripts.Modding;
using Unity.Entities;

namespace P4.Core
{
    public class Bootstrap : CModBootstrap
    {
        protected override void OnRegister()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            var entity = entityManager.CreateEntity(typeof(DRythmBeatData), typeof(DRythmTimeData));
            entityManager.SetComponentData(entity, new DRythmBeatData()
            {
                Interval = 0.5f
            });
        }

        protected override void OnUnregister()
        {
            
        }
    }
}