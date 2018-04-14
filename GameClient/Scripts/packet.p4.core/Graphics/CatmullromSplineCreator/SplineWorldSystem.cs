using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guerro.Utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Experimental.U2D;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace P4.Core.Graphics
{
    [UpdateAfter(
        typeof(PreLateUpdate.DirectorUpdateAnimationEnd))]
    //< Update after the 'LateUpdate', so all animations can be finished
    public class SplineWorldSystem : JobComponentSystem
    {
        struct Group
        {
            [ReadOnly] public ComponentDataArray<DSplineWorldData>    Components;
            public            ComponentArray<SplineWorldConfigurator> Configurators;
            public            EntityArray                             Entities;
            public            int                                     Length;
        }

        struct ControlPointGroup
        {
            public TransformAccessArray                             Transforms;
            public ComponentDataArray<DSplineControlPointWorldData> Components;
            public int                                              Length;
        }

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
            [ReadOnly]                            public ComponentDataArray<DSplineWorldData> Components;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<float3>                  Points;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int>                     PointsIndexes;
            [WriteOnly]                           public NativeList<float3>                   FinalFillerArray;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<int> FinalFillerArrayIndexes;

            public int MaxLength;

            // Unity jobs
            public void Execute()
            {
                for (int index = 0; index < Components.Length; index++)
                {
                    var component               = Components[index];
                    var currentPointsIndex      = PointsIndexes[index];
                    var currentFillerArrayIndex = FinalFillerArrayIndexes[index];
                    var sliceValue              = component.Step;
                    var tensionValue            = component.Tension;
                    var isLoopingValue          = component.IsLooping == 1 ? true : false;

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

        [Inject] private Group                m_group;
        [Inject] private ControlPointGroup    m_controlPointsGroup;
        private          NativeList<float3>   m_finalFillerArray;
        private          TransformAccessArray m_orderedPoints;
        private          EntityChange         m_EntityChange;

        public JobHandle lastJobHandle;

        public SplineWorldSystem()
        {
            Camera.onPreCull               += OnCameraPreCull;
            m_finalFillerArray             =  new NativeList<float3>(0, Allocator.Persistent);
            m_orderedPoints                =  new TransformAccessArray(0);
            JobsUtility.JobCompilerEnabled =  true;

            m_EntityChange                        = new EntityChange(0);
            m_EntityChange.NeedEntitiesList       = false;
            m_EntityChange.NeedToCheckReplacement = false;
            m_EntityChange.NeedToCheckRemoval     = false;
            m_EntityChange.NeedToAddEntities      = false;
        }

        private void OnCameraPreCull(Camera cam)
        {
            lastJobHandle.Complete();

            var currentFillerArrayLength = 0;
            for (int i = 0; i < m_group.Length; i++)
            {
                var component    = m_group.Components[i];
                var configurator = m_group.Configurators[i];

                var fillArray_addLength =
                    CGraphicalCatmullromSplineUtility.GetFormula(component.Step, configurator.Points.Length);
                var maxFillerArrayIndexes = currentFillerArrayLength + fillArray_addLength;
                var count                 = maxFillerArrayIndexes - currentFillerArrayLength;

                if (configurator._lastLineRendererPositionCount != count)
                {
                    configurator.LineRenderer.positionCount     = count;
                    configurator._lastLineRendererPositionCount = count;
                }

                for (int j = currentFillerArrayLength, posIndex = 0; j < maxFillerArrayIndexes; j++, posIndex++)
                {
                    configurator.LineRenderer.SetPosition(posIndex, m_finalFillerArray[j]);
                }

                currentFillerArrayLength += fillArray_addLength;
            }
        }

        private bool m_hadEntityChange;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Profiler.BeginSample("Get changes");
            m_EntityChange.Update(ref m_group.Entities);
            Profiler.EndSample();

            while (m_orderedPoints.length > m_controlPointsGroup.Length)
                m_orderedPoints.RemoveAtSwapBack(m_orderedPoints.length - 1);

            var hasChange = m_EntityChange.EntityReplaceCount > 0 || m_EntityChange.EntitySpawnCount > 0;
            if (hasChange || m_hadEntityChange) //< for some reason, we need to do that or else ugly thing will happen
            {
                m_hadEntityChange = hasChange;

                for (int i = 0, total = 0, totalLength = 0;
                    i != m_group.Length;
                    i++)
                {
                    var configurator = m_group.Configurators[i];
                    var length       = configurator.Points.Length;
                    totalLength += length;

                    for (int j = 0;
                        j != length;
                        j++, total++)
                    {
                        if (m_orderedPoints.length <= totalLength)
                            m_orderedPoints.Add(configurator.Points[j]);
                        else
                        {
                            m_orderedPoints[total] = configurator.Points[j];
                        }
                    }
                }
            }

            int fillerArrayLength = 0,
                currentCount      = 0,
                transformCount    = 0;

            var pointsToConvert = new NativeArray<float3>(m_controlPointsGroup.Length, Allocator.TempJob);
            var convertPointsJob = new JobConvertPoints()
            {
                Result = pointsToConvert
            };
            inputDeps = convertPointsJob.Schedule(m_orderedPoints, inputDeps);

            var pointsIndexes           = new NativeArray<int>(m_group.Length, Allocator.TempJob);
            var finalFillerArrayIndexes = new NativeArray<int>(m_group.Length, Allocator.TempJob);
            for (int i = 0; i != m_group.Length; i++)
            {
                var mod1  = m_group.Components[i];
                var conf1 = m_group.Configurators[i];

                var fillArray_addLength = CGraphicalCatmullromSplineUtility.GetFormula(mod1.Step, conf1.Points.Length);

                pointsIndexes[i]           =  currentCount;
                finalFillerArrayIndexes[i] =  fillerArrayLength;
                currentCount               += conf1.Points.Length;
                fillerArrayLength          += fillArray_addLength;
            }

            var fillArrayJob = new JobFillArray();
            fillArrayJob.Components              = m_group.Components;
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
