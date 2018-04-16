using Guerro.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace P4.Core.Graphics
{
    [UpdateAfter(
        typeof(PreLateUpdate.DirectorUpdateAnimationEnd))]
    //< Update after the 'LateUpdate', so all animations can be finished
    public class SplineWorldSystem : JobComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Reactives
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        struct InitEvent : IComponentData
        {
            public Entity Id;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        struct Group
        {
            [ReadOnly] public ComponentDataArray<DSplineData> SplineData;

            [ReadOnly] public ComponentArray<SplineRendererBehaviour> SplineRenderers;

            //[ReadOnly] public FixedArrayArray<DSplinePositionData>     Positions; // Useless?
            public EntityArray Entities;
            public int         Length;
        }

        struct GroupEvents
        {
            public EntityArray                   Entities;
            public ComponentDataArray<InitEvent> Inits;
            public int                           Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private Group                m_group;
        [Inject] private GroupEvents          m_GroupEvents;
        private          NativeList<float3>   m_finalFillerArray;
        private          TransformAccessArray m_orderedPoints;
        private          EntityArchetype      m_EventArchetype;
        private          int                  m_PointsLength;

        public JobHandle lastJobHandle;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Jobs
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [ComputeJobOptimization]
        struct JobConvertPoints : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<float3> Result;

            public void Execute(int index, TransformAccess transform)
            {
                Result[index] = transform.localPosition;
            }
        }

        [ComputeJobOptimization]
        struct JobFillArray : IJob
        {
            [ReadOnly]                            public ComponentDataArray<DSplineData> Datas;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<float3>             Points;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int>                PointsIndexes;
            public                                       NativeList<float3>              FinalFillerArray;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<int> FinalFillerArrayIndexes;

            public int MaxLength;

            // Unity jobs
            public void Execute()
            {
                for (int index = 0; index < Datas.Length; index++)
                {
                    var data                    = Datas[index];
                    var currentPointsIndex      = PointsIndexes[index];
                    var currentFillerArrayIndex = FinalFillerArrayIndexes[index];
                    var sliceValue              = data.Step;
                    var tensionValue            = data.Tension;
                    var isLoopingValue          = data.IsLooping == 1;

                    var maxPointsIndexes = PointsIndexes.Length > index + 1 ? PointsIndexes[index + 1] : Points.Length;
                    var maxFillerArrayIndexes = FinalFillerArrayIndexes.Length > index + 1
                        ? FinalFillerArrayIndexes[index + 1]
                        : MaxLength;

                    CGraphicalCatmullromSplineUtility.CalculateCatmullromSpline(Points, currentPointsIndex,
                        maxPointsIndexes,
                        FinalFillerArray, currentFillerArrayIndex, maxFillerArrayIndexes,
                        sliceValue,
                        tensionValue,
                        isLoopingValue);

                    FinalFillerArray[currentFillerArrayIndex]   = Points[currentPointsIndex];
                    FinalFillerArray[maxFillerArrayIndexes - 1] = Points[maxPointsIndexes - 1];
                }
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public void SendUpdateEvent(Entity entity)
        {
            var eventEntity = EntityManager.CreateEntity(m_EventArchetype);
            EntityManager.SetComponentData(eventEntity, new InitEvent {Id = entity});
        }

        protected override void OnCreateManager(int capacity)
        {
            Camera.onPreCull   += OnCameraPreCull;
            m_finalFillerArray =  new NativeList<float3>(0, Allocator.Persistent);
            m_orderedPoints    =  new TransformAccessArray(0);

            m_EventArchetype = EntityManager.CreateArchetype(typeof(InitEvent));

            SendUpdateEvent(Entity.Null);
        }

        private void OnCameraPreCull(Camera cam)
        {
            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Finish the current job
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            lastJobHandle.Complete();

            var currentFillerArrayLength = 0;
            for (int i = 0; i < m_group.Length; i++)
            {
                var data     = m_group.SplineData[i];
                var renderer = m_group.SplineRenderers[i];

                var fillArray_addLength =
                    CGraphicalCatmullromSplineUtility.GetFormula(data.Step, renderer.Points.Length);
                var maxFillerArrayIndexes = currentFillerArrayLength + fillArray_addLength;
                var count                 = maxFillerArrayIndexes - currentFillerArrayLength;

                if (renderer.LastLineRendererPositionCount != count)
                {
                    renderer.LineRenderer.positionCount    = count;
                    renderer.LastLineRendererPositionCount = count;
                }

                for (int j = currentFillerArrayLength, posIndex = 0; j < maxFillerArrayIndexes; j++, posIndex++)
                {
                    renderer.LineRenderer.SetPosition(posIndex, m_finalFillerArray[j]);
                }

                currentFillerArrayLength += fillArray_addLength;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Check for any changes
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            var hasChange = m_GroupEvents.Length > 0;
            if (hasChange)
            {
                m_PointsLength = 0;
                for (int i = 0, total = 0, totalLength = 0;
                    i != m_group.Length;
                    ++i)
                {
                    var renderer = m_group.SplineRenderers[i];
                    var length   = renderer.Points.Length;
                    totalLength += length;

                    for (int j = 0;
                        j != length;
                        ++j, ++total)
                    {
                        if (m_orderedPoints.length <= totalLength)
                            m_orderedPoints.Add(renderer.Points[j]);
                        else
                            m_orderedPoints[total] = renderer.Points[j];

                        ++m_PointsLength;
                    }
                }
            }

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Reset 'change' event
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            for (int i = 0; i != m_GroupEvents.Length; i++)
            {
                Debug.Log(m_GroupEvents.Length + ", " + i);
                EntityManager.DestroyEntity(m_GroupEvents.Entities[i]);
            }

            // If our transform access array was too big, we remove some old and useless items...
            while (m_orderedPoints.length > m_PointsLength)
                m_orderedPoints.RemoveAtSwapBack(m_orderedPoints.length - 1);
            //...

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Create variables and jobs
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            // Create variables
            int fillerArrayLength = 0,
                currentCount      = 0,
                transformCount    = 0;
            // Create the job that will convert the UTransforms positions into the right components. 
            var pointsToConvert = new NativeArray<float3>(m_PointsLength, Allocator.TempJob);
            var convertPointsJob = new JobConvertPoints
            {
                Result = pointsToConvert
            };
            // ...
            // Schedule the job
            inputDeps = convertPointsJob.Schedule(m_orderedPoints, inputDeps);

            var pointsIndexes           = new NativeArray<int>(m_group.Length, Allocator.TempJob);
            var finalFillerArrayIndexes = new NativeArray<int>(m_group.Length, Allocator.TempJob);
            for (int i = 0; i != m_group.Length; i++)
            {
                var data     = m_group.SplineData[i];
                var renderer = m_group.SplineRenderers[i];

                var fillArray_addLength =
                    CGraphicalCatmullromSplineUtility.GetFormula(data.Step, renderer.Points.Length);

                pointsIndexes[i]           =  currentCount;
                finalFillerArrayIndexes[i] =  fillerArrayLength;
                currentCount               += renderer.Points.Length;
                fillerArrayLength          += fillArray_addLength;
            }

            var fillArrayJob = new JobFillArray();
            fillArrayJob.Datas                   = m_group.SplineData;
            fillArrayJob.Points                  = pointsToConvert;
            fillArrayJob.PointsIndexes           = pointsIndexes;
            fillArrayJob.FinalFillerArray        = m_finalFillerArray;
            fillArrayJob.FinalFillerArrayIndexes = finalFillerArrayIndexes;
            fillArrayJob.MaxLength               = fillerArrayLength;

            inputDeps = lastJobHandle = fillArrayJob.Schedule(inputDeps);

            return inputDeps;
        }
    }
}