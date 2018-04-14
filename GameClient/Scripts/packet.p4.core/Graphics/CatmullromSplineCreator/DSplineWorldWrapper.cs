using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace P4.Core.Graphics
{
    [Serializable]
    public struct DSplineWorldData : IComponentData
    {
        public float Tension;
        public int Step;
        public int IsLooping;
    }

    public class DSplineWorldWrapper : ComponentDataWrapper<DSplineWorldData>
    {
        private void OnValidate()
        {
            var goEntity = GetComponent<GameObjectEntity>();
            if (!UnityEngine.Application.isPlaying || goEntity.EntityManager == null)
                return;
            goEntity.EntityManager.SetComponentData(goEntity.Entity, Value);
        }
    }
}
