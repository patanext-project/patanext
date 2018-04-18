using Guerro.Utilities;
using Packages.pack.guerro.shared.Scripts.Utilities;
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
    public class SplineSystem : JobComponentSystem
    {
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Groups
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        struct Group
        {
            [ReadOnly] public ComponentDataArray<DSplineData>       SplineData;
            [ReadOnly] public ComponentDataArray<DSplineBoundsData> SplineBoundsData;

            [ReadOnly] public ComponentArray<SplineRendererBehaviour> SplineRenderers;

            //[ReadOnly] public FixedArrayArray<DSplinePositionData>     Positions; // Useless?
            public EntityArray Entities;
            public int         Length;
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Fields
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [Inject] private Group                          m_Group;
        [Inject] private EndFrameBarrier                m_EndFrameBarrier;
        private          NativeList<float3>             m_FinalFillerArray;
        
        private          NativeArray<DSplineBoundsData> m_JobBoundsDatas;
        private          NativeArray<DSplineData>       m_JobDatas;
        private          NativeArray<float>             m_JobBoundsOutline;
        private NativeArray<int> m_jobFormulaAddLength;
        private NativeArray<EActivationZone> m_refreshTypes;
        
        private          TransformAccessArray           m_OrderedPoints;
        private          EntityArchetype                m_EventArchetype;
        private          int                            m_PointsLength;
        private          int                            m_Events;

        public int IgnoredSplines;

        private JobHandle LastJobHandle;

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Jobs
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        [ComputeJobOptimization]
        struct JobConvertPoints : IJobParallelForTransform
        {
            [WriteOnly] public NativeArray<float3> Result;
            [WriteOnly] public NativeArray<float3> WorldResult;

            public void Execute(int index, TransformAccess transform)
            {
                Result[index]      = transform.localPosition;
                WorldResult[index] = transform.position;
            }
        }

        [ComputeJobOptimization]
        struct JobFillArray : IJob
        {
            [ReadOnly] public                            NativeArray<DSplineData>       Datas;
            public                                       NativeArray<DSplineBoundsData> BoundsDatas;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<float3>            WorldPoints;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<float3>            Points;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<int>               PointsIndexes;
            public                                       NativeList<float3>             FinalFillerArray;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<int> FinalFillerArrayIndexes;

            public int MaxLength;

            // Unity jobs
            public void Execute()
            {
                for (int index = 0; index < Datas.Length; index++)
                {
                    var data       = Datas[index];
                    var boundsData = BoundsDatas[index];

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

                    for (int pointIndex = currentPointsIndex; pointIndex != maxPointsIndexes; pointIndex++)
                    {
                        var point = WorldPoints[pointIndex];

                        if (pointIndex == currentPointsIndex)
                        {
                            boundsData.Min = point.xy;
                            boundsData.Max = point.xy;
                        }
                        
                        var min   = boundsData.Min;
                        var max   = boundsData.Max;
                        boundsData.Min = math.min(point.xy, min);
                        boundsData.Max = math.max(point.xy, max);
                    }

                    BoundsDatas[index] = boundsData;
                    FinalFillerArray[currentFillerArrayIndex]   = Points[currentPointsIndex];
                    FinalFillerArray[maxFillerArrayIndexes - 1] = Points[maxPointsIndexes - 1];
                }
            }
        }

        struct JobCheckInterestects : IJobParallelFor
        {
            [ReadOnly]  public NativeArray<DSplineBoundsData> BoundsDatas;
            [ReadOnly]  public NativeArray<Bounds>            CameraBounds;
            [ReadOnly]  public NativeArray<float>             BoundsOutline;
            [WriteOnly] public NativeArray<bool1>             Results;

            public void Execute(int index)
            {
                var cb = CameraBounds[0];
                var bd = BoundsDatas[index];
                
                bd.Min -= BoundsOutline[index];
                bd.Max += BoundsOutline[index];

                Results[index] = cb.min.x <= bd.Max.x && cb.max.x >= bd.Min.x
                                                      && cb.min.y <= bd.Max.y && cb.max.y >= bd.Min.y;
            }
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        // Methods
        // -------- -------- -------- -------- -------- -------- -------- -------- -------- /.
        public void SendUpdateEvent(Entity entity)
        {
            /*var eventEntity = EntityManager.CreateEntity(m_EventArchetype);
            EntityManager.SetComponentData(eventEntity, new InitEvent {Id = entity});*/ // It's a lot bugged for now
            m_Events++;
        }

        protected override void OnCreateManager(int capacity)
        {
            Camera.onPreCull   += OnCameraPreCull;
            m_FinalFillerArray =  new NativeList<float3>(0, Allocator.Persistent);

            m_JobBoundsDatas      = new NativeArray<DSplineBoundsData>(m_Group.Length, Allocator.Persistent);
            m_JobBoundsOutline    = new NativeArray<float>(m_Group.Length, Allocator.Persistent);
            m_JobDatas            = new NativeArray<DSplineData>(m_Group.Length, Allocator.Persistent);
            m_jobFormulaAddLength = new NativeArray<int>(m_Group.Length, Allocator.Persistent);
            m_refreshTypes = new NativeArray<EActivationZone>(m_Group.Length, Allocator.Persistent);

            m_OrderedPoints = new TransformAccessArray(0);

            //m_EventArchetype = EntityManager.CreateArchetype(typeof(InitEvent));

            SendUpdateEvent(Entity.Null);
        }

        protected override void OnDestroyManager()
        {
            LastJobHandle.Complete();
            
            Camera.onPreCull -= OnCameraPreCull;
            m_FinalFillerArray.Dispose();
            m_JobBoundsDatas.Dispose();
            m_JobBoundsOutline.Dispose();
            m_JobDatas.Dispose();
            m_jobFormulaAddLength.Dispose();
            m_refreshTypes.Dispose();
            m_OrderedPoints.Dispose();
        }

        private void OnCameraPreCull(Camera cam)
        {
            IgnoredSplines = 0;

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Finish the current job
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            var cameraBounds = new NativeArray<Bounds>(1, Allocator.TempJob);
            cameraBounds[0] = new Bounds(cam.transform.position, cam.GetExtents()).Flat2D();
            var resultsInterestBounds = new NativeArray<bool1>(m_Group.Length, Allocator.Temp);

            Profiler.BeginSample("Job");
            LastJobHandle.Complete();
            new JobCheckInterestects()
            {
                BoundsDatas   = m_JobBoundsDatas,
                CameraBounds  = cameraBounds,
                BoundsOutline = m_JobBoundsOutline,
                Results       = resultsInterestBounds
            }.Run(m_Group.Length);
            Profiler.EndSample();

            Profiler.BeginSample("Loop");
            var currentFillerArrayLength = 0;
            for (int i = 0; i < m_Group.Length; i++)
            {
                var fillArrayAddLength = m_jobFormulaAddLength[i];
                if (m_refreshTypes[i] == EActivationZone.Bounds)
                {
                    if (!resultsInterestBounds[i])
                    {
                        IgnoredSplines++;
                        currentFillerArrayLength += fillArrayAddLength;
                        continue;
                    }
                }

                var data     = m_Group.SplineData[i];
                var renderer = m_Group.SplineRenderers[i];

                if (renderer.CameraRenderCount > 0)
                {
                    currentFillerArrayLength += fillArrayAddLength;
                    continue;
                }

                renderer.CameraRenderCount++;

                var maxFillerArrayIndexes = currentFillerArrayLength + fillArrayAddLength;
                var count                 = maxFillerArrayIndexes - currentFillerArrayLength;

                if (renderer.LastLineRendererPositionCount != count)
                {
                    renderer.LineRenderer.positionCount    = count;
                    renderer.LastLineRendererPositionCount = count;
                }

                for (int j = currentFillerArrayLength, posIndex = 0; j < maxFillerArrayIndexes; j++, posIndex++)
                {
                    renderer.LineRenderer.SetPosition(posIndex, m_FinalFillerArray[j]);
                }

                currentFillerArrayLength += fillArrayAddLength;
            }
            Profiler.EndSample();

            resultsInterestBounds.Dispose();
            cameraBounds.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Check for any changes
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            var hasChange = m_Events > 0;
            if (hasChange)
            {
                m_OrderedPoints.Dispose();
                m_OrderedPoints = new TransformAccessArray(0);

                m_PointsLength = 0;
                for (int i = 0, total = 0, totalLength = 0;
                    i != m_Group.Length;
                    ++i)
                {
                    var renderer = m_Group.SplineRenderers[i];
                    var length   = renderer.Points.Length;
                    totalLength += length;

                    for (int j = 0;
                        j != length;
                        ++j, ++total)
                    {
                        if (m_OrderedPoints.length <= totalLength)
                            m_OrderedPoints.Add(renderer.Points[j]);
                        else
                            m_OrderedPoints[total] = renderer.Points[j];

                        ++m_PointsLength;
                    }
                }
            }

            m_Events = 0;

            if (m_Group.Length != m_JobBoundsDatas.Length)
            {
                m_JobBoundsDatas.Dispose();
                m_JobDatas.Dispose();
                m_JobBoundsOutline.Dispose();
                m_jobFormulaAddLength.Dispose();
                m_refreshTypes.Dispose();
                m_JobBoundsDatas   = new NativeArray<DSplineBoundsData>(m_Group.Length, Allocator.Persistent);
                m_JobDatas         = new NativeArray<DSplineData>(m_Group.Length, Allocator.Persistent);
                m_JobBoundsOutline = new NativeArray<float>(m_Group.Length, Allocator.Persistent);
                m_jobFormulaAddLength = new NativeArray<int>(m_Group.Length, Allocator.Persistent);
                m_refreshTypes = new NativeArray<EActivationZone>(m_Group.Length, Allocator.Persistent);
            }

            // If our transform access array was too big, we remove some old and useless items...
            /*while (m_orderedPoints.length > m_PointsLength)
                m_orderedPoints.RemoveAtSwapBack(m_orderedPoints.length - 1);*/
            //...

            //< -------- -------- -------- -------- -------- -------- -------- ------- //
            // Create variables and jobs
            //> -------- -------- -------- -------- -------- -------- -------- ------- //
            // Create variables
            int fillerArrayLength = 0,
                currentCount      = 0,
                transformCount    = 0;
            // Create the job that will convert the UTransforms positions into the right components. 
            var localPointsToConvert = new NativeArray<float3>(m_PointsLength, Allocator.TempJob);
            var worldPointsToConvert = new NativeArray<float3>(m_PointsLength, Allocator.TempJob);
            var convertPointsJob = new JobConvertPoints
            {
                Result      = localPointsToConvert,
                WorldResult = worldPointsToConvert
            };
            // ...
            // Schedule the job
            inputDeps = convertPointsJob.Schedule(m_OrderedPoints, inputDeps);

            var pointsIndexes           = new NativeArray<int>(m_Group.Length, Allocator.TempJob);
            var finalFillerArrayIndexes = new NativeArray<int>(m_Group.Length, Allocator.TempJob);
            for (int i = 0; i != m_Group.Length; i++)
            {
                var data       = m_Group.SplineData[i];
                var boundsData = m_Group.SplineBoundsData[i];
                var renderer   = m_Group.SplineRenderers[i];

                renderer.CameraRenderCount = 0; //< Reset camera renders

                var fillArrayAddLength =
                    CGraphicalCatmullromSplineUtility.GetFormula(data.Step, renderer.Points.Length);

                pointsIndexes[i]           =  currentCount;
                finalFillerArrayIndexes[i] =  fillerArrayLength;
                currentCount               += renderer.Points.Length;
                fillerArrayLength          += fillArrayAddLength;

                m_JobDatas[i]         = data;
                m_JobBoundsDatas[i]   = boundsData;
                m_JobBoundsOutline[i] = renderer.RefreshBoundsOutline;
                m_jobFormulaAddLength[i] = fillArrayAddLength;
                m_refreshTypes[i] = renderer.RefreshType;
            }

            var fillArrayJob = new JobFillArray();
            fillArrayJob.Datas                   = m_JobDatas;
            fillArrayJob.BoundsDatas             = m_JobBoundsDatas;
            fillArrayJob.Points                  = localPointsToConvert;
            fillArrayJob.WorldPoints = worldPointsToConvert;
            fillArrayJob.PointsIndexes           = pointsIndexes;
            fillArrayJob.FinalFillerArray        = m_FinalFillerArray;
            fillArrayJob.FinalFillerArrayIndexes = finalFillerArrayIndexes;
            fillArrayJob.MaxLength               = fillerArrayLength;

            inputDeps = LastJobHandle = fillArrayJob.Schedule(inputDeps);

            return inputDeps;
        }
    }
}