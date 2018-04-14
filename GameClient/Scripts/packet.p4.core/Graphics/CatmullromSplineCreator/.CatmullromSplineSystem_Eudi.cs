using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using EudiFramework;
using EudiFramework.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace Assets.Scripts.Graphics
{
    public class CatmullromSplineSystem : EudiSystemBehaviour
    {
        [InjectTuples(0)] public EudiMatchArray<CatmullromSplineModule> _catmullRomSplineModules;

        [InjectTuples(1)] public EudiMatchArray<CatmullromSplineClassConfigurator> _catmullRomSplineClassConfigurators;

        private Contract _contract;
        
        public const int DivideChunk = 64;

        private TransformAccessArray m_pointsTransforms;
        
        protected override void InternalSystemAwake()
        {            
            base.UnityAwake();

            Eudi.Globals.SetBindingFromInstance<CatmullromSplineSystem>(this);

            Camera.onPreCull += UnityOnCameraPreCull;
            
            m_pointsTransforms = new TransformAccessArray(0);

            _contract = new Contract()
            {
                Jobs = new EudiJobContractGroup<ContractUpdateSplines>(null)
            };
            AddWorker(new Worker(ref _contract));
        }

        protected override void SystemOnEntityAdded(long entityId, EudiEntity entity)
        {
            Debug.Log("new entity lol");
            
            var configurator = entity.GetComponent<CatmullromSplineClassConfigurator>();
            for (int j = 0; j < configurator.PointsLength; j++)
            {
                m_pointsTransforms.Add(configurator.Points[j]);
            }
        }

        protected override void UnityLateUpdate()
        {
            base.UnityLateUpdate();
                
            var matchLength = MatchEntities.Length;
            if (matchLength == 0)
                return;
            
            // ---- ---- ---- ---- ---- ---- ---- ---- ----
            // Update transformAccess and points indexes
            // ---- ---- ---- ---- ---- ---- ---- ---- ----
            var pointsIndexes = new EudiStructList<int>(matchLength);
            var currentCount = 0;
            for (int i = 0; i < matchLength; i++)
            {
                var configurator = _catmullRomSplineClassConfigurators[i];
                pointsIndexes[i] = currentCount;
                currentCount += configurator.PointsLength;
            }

            // ---- ---- ---- ---- ---- ---- ---- ---- ----
            // Schedule the job to update the points positions
            // ---- ---- ---- ---- ---- ---- ---- ---- ----
            var points = new EudiStructList<Vector3>(m_pointsTransforms.Length);
            var jobUpdatePointsPos = new JobUpdatePointsPosition()
            {
                PointsToUpdate = points,
            };
                
            AddDependency(jobUpdatePointsPos.Schedule(m_pointsTransforms, GetDependency()));
            GetDependency().Complete();
            
            Debug.Log(m_pointsTransforms.Length);

            var currentChunk = -1;
            for (int chunk = 0, i = 0; i < matchLength; i++, chunk++)
            {
                if (chunk >= DivideChunk || currentChunk == -1)
                {
                    currentChunk++;
                    chunk = 0;
                    
                    _contract.Jobs.AddJob(new ContractUpdateSplines()
                    {
                        PointsIndexes = pointsIndexes,
                        Points = points,
                        Entities = MatchEntities,
                        CurrentChunk = currentChunk,
                        Iteration = i,
                        MaxLength = Mathf.Clamp(DivideChunk - 1, i, matchLength)
                    });
                }
            }
        }

        private void DoModuleWork(ref CatmullromSplineModule module)
        {
            Profiler.BeginSample("add job");
            // Add a new job
            //_contract.JobsContract.AddJob(new JobContract(ref module));

            Profiler.EndSample();
        }

        protected virtual void UnityOnCameraPreCull(Camera cam)
        {
            _contract.Jobs.ForceFinishAll();
            
            if (!GetDependency().IsCompleted)
            {
                //GetDependency().Complete();
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 120, 120), _contract.Jobs.Jobs.Length.ToString());
        }

        // -------- -------- -------- -------- -------- -------- -------- -------- 
        // Workers and contracts code part
        // -------- -------- -------- -------- -------- -------- -------- --------

        private class Contract : EudiComponentContract
        {
            public EudiJobContractGroup<ContractUpdateSplines> Jobs;
        }
        
        private struct ContractUpdateSplines : IEudiJobContract<ContractUpdateSplines>
        {
            public bool ForcedToFinish { get; set; }

            public EudiJobReportExecution ReportExecutionOnForceFinish => EudiJobReportExecution.KeepRunning;

            public int Iteration;
            public int CurrentChunk;
            public int MaxLength;
            
            public EudiStructList<Vector3> Points;
            public EudiStructList<int> PointsIndexes;
            public EudiStructList<EudiEntity> Entities;
            
            public void Execute(ref EudiJobHandle<ContractUpdateSplines> handle, bool wasForcedToFinish = false)
            {
                for (int i = Iteration; i < MaxLength; i++)
                {
                    var currentPointsIndex = PointsIndexes[i];
                    
                    var entity = Entities[i];
                    var moduleIndex = entity.FastGet<CatmullromSplineModule>();
                    ref var spline = ref EudiModuleGroup<CatmullromSplineModule>.Get(entity).ListObject[moduleIndex];
                    var usedPoints = new EudiStructList<Vector3>(spline.PointsLength);
                    
                    for (int j = 0; j < usedPoints.Length; j++)
                    {
                        var p = Points[currentPointsIndex + j];
                        usedPoints[j] = p;
                        Debug.Log("points");
                    }
                    
                    CatmullromSplineUtility.CalculateSpline(usedPoints,
                        ref spline.FillerArray,
                        spline.UsableSegments,
                        spline.Tension,
                        spline.IsLooping);
                    
                    Debug.Log(spline.FillerArray.Length);

                    for (int j = 0; j < spline.FillerArray.Length; j++)
                    {
                        Debug.Log(spline.FillerArray[j]);
                    }
                    
                    usedPoints.Dispose();
                }
            }
        }

        private class Worker : EudiComponentWorker
        {
            private Contract _contract;
            
            public Worker(ref Contract contract)
            {
                _contract = contract;
                
                SetThreadCount(32);
                for (int i = 0; i < ThreadCount; i++)
                {
                    SetThreadShareParam(i, EudiThreading.GetThreadGroup<Worker>(i));
                }
            }

            protected override void OnNewWorkerTask(WorkerTask workerTask, bool firstCreation)
            {
                workerTask.Updater.RefreshRate = 1;
                workerTask.SynchronizationType = EudiSynchronizationType.TrueMultiThreading;
            }

            protected override void WorkerUpdate(EudiWorkerUpdateEvent ev)
            {
                if (_contract.IsLocked)
                    return;

                if (_contract.Jobs.StartNextJob(out var jobHandle))
                {
                    _contract.Lock();
                    jobHandle.Schedule(ref jobHandle.Job);    
                    _contract.Jobs.CurrentJobFinish();
                    _contract.Unlock();
                }
            }
        }
        
        private struct JobUpdateSplines : IJobParallelFor
        {
            [DeallocateOnJobCompletion] public EudiStructList<Vector3> Points;
            [DeallocateOnJobCompletion] public EudiStructList<int> PointsIndexes;
            
            public EudiMatchArray<CatmullromSplineModule> Splines;
            
            // Unity jobs
            public void Execute(int index)
            {
                var currentPointsIndex = PointsIndexes[index];
                
                var spline = Splines[index];
                var usedPoints = new EudiStructList<Vector3>(spline.PointsLength);
                for (int i = 0; i < usedPoints.Length; i++)
                {
                    usedPoints[i] = Points[currentPointsIndex + i];
                }
                
                CatmullromSplineUtility.CalculateSpline(usedPoints,
                    ref spline.FillerArray,
                    spline.UsableSegments,
                    spline.Tension,
                    spline.IsLooping);
                
                usedPoints.Dispose();
            }
        }
        
        private struct JobUpdatePointsPosition : IJobParallelForTransform
        {
            [WriteOnly] public EudiStructList<Vector3> PointsToUpdate;
            
            public void Execute(int index, TransformAccess transform)
            {
                PointsToUpdate[index] = transform.localPosition;
            }
        }
    }
}