using System;
using Packet.Guerro.Shared.Game;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace P4.Core.Graphics
{
    [Serializable]
    public struct DSplineData : IComponentData
    {
        public float Tension;
        public int   Step;
        public int   IsLooping;
    }
    
    public class SplineRendererBehaviour : CGameEntityCreatorBehaviour<SplineRendererCreatorSystem>
    {
        public int Step = 6;
        public float Tension = 0.5f;
        public bool IsLooping;
        
        public Transform[] Points;
        public LineRenderer LineRenderer;

        private int m_CurrentPointsLength;
        
        internal int LastLineRendererPositionCount;

        protected override void AwakeAfterFilling()
        {
            m_CurrentPointsLength = Points.Length;
        }

        private void OnValidate()
        {
            var goEntity = GetComponent<GameObjectEntity>();
            if (!UnityEngine.Application.isPlaying || goEntity?.EntityManager == null)
                return;
            if (Points.Length != m_CurrentPointsLength)
            {
                Debug.LogError("Can't add/remove points of a spline while playing!");
                return;
            }
            
            goEntity.EntityManager.SetComponentData(goEntity.Entity, GetData());
            
            World.Active.GetExistingManager<SplineWorldSystem>().SendUpdateEvent(goEntity.Entity);
        }

        public DSplineData GetData()
        {
            return new DSplineData
            {
                Step      = Step,
                Tension   = Tension,
                IsLooping = IsLooping ? 1 : 0
            };
        }

        public Vector3 GetPoint(int i)
        {
            return Points[i].position;
        }

        public void SetPoint(int i, Vector3 value)
        {
            Points[i].position = value;
        }
    }

    public class SplineRendererCreatorSystem : CGameEntityCreatorSystem
    {
        protected override void OnUpdate()
        {
        }

        public override void FillEntityData(GameObject gameObject, Entity entity)
        {
            var component = gameObject.GetComponent<SplineRendererBehaviour>();
            
            AddComponentData(entity, component.GetData());
            //AddFixedArray<DSplinePositionData>(entity, component.Points.Length);
        }
    }
}
