using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using EudiFramework;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

namespace P4.Core.Graphics
{
    /// <summary>
    /// Hold importants references variables
    /// </summary>
    [Serializable]
    public class SplineWorldConfigurator : MonoBehaviour
    {
        /*struct JobUpdateSpline : IJob
        {
            [ReadOnly] public CatmullRomSplineWorld Component;
            [ReadOnly, DeallocateOnJobCompletion] public NativeArray<float3> Input;
            [WriteOnly] public NativeArray<float3> Output;

            public void Execute()
            {
                SplineUtility.CalculateSpline(Input, 0, Input.Length,
                    Output, 0, Output.Length,
                    Component.Step,
                    Component.Tension,
                    Component.IsLooping == 0 ? false : true);
            }
        }*/

        public Transform[] Points;

        public LineRenderer LineRenderer;

        internal int _lastLineRendererPositionCount;
        /*internal NativeArray<float3> _fillerArray;

        protected override void UnityAwake()
        {
            _fillerArray = new NativeArray<float3>(0, Allocator.Persistent);
        }

        internal JobHandle ScheduleJob(JobHandle dependency, CatmullRomSplineWorld component)
        {
            var formulaLength = SplineUtility.GetFormula(component.Step, PointsLength);
            var input = new NativeArray<float3>(PointsLength, Allocator.TempJob);
            for (int i = 0; i != PointsLength; i++)
            {
                input[i] = GetPoint(i);
            }

            _fillerArray.Dispose();
            _fillerArray = new NativeArray<float3>(formulaLength, Allocator.Persistent);

            return new JobUpdateSpline()
            {
                Component = component,
                Input = input,
                Output = _fillerArray
            }.Schedule(dependency);
        }*/

        public float3 GetPoint(int index)
        {
            /*if (index == 0) return Points_index0;
            else if (index == 1) return Points_index1;
            else if (index == 2) return Points_index2;
            else if (index == 3) return Points_index3;
            else if (index == 4) return Points_index4;
            else if (index == 5) return Points_index5;
            else if (index == 6) return Points_index6;
            else if (index == 7) return Points_index7;
            else if (index == 8) return Points_index8;
            else if (index == 9) return Points_index9;
            return new float3();  */ 
            return Points[index].position;        
        }

        public void SetPoint(int index, float3 value)
        {
            /*if (index == 0) Points_index0 = value;
            else if (index == 1) Points_index1 = value;
            else if (index == 2) Points_index2 = value;
            else if (index == 3) Points_index3 = value;
            else if (index == 4) Points_index4 = value;
            else if (index == 5) Points_index5 = value;
            else if (index == 6) Points_index6 = value;
            else if (index == 7) Points_index7 = value;
            else if (index == 8) Points_index8 = value;
            else if (index == 9) Points_index9 = value;  */ 
            Points[index].position = value;
        }
    }
}
