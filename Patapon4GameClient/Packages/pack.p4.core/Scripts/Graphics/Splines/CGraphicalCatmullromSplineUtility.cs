using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace P4.Core.Graphics
{
    public static class CGraphicalCatmullromSplineUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFormula(int slices, int nodesLength)
        {
            return (slices * (nodesLength - 1)) + (nodesLength - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateCatmullromSpline(NativeArray<float3> nodes, int nodesStart, int nodesEnd, NativeList<float3> m_fillerArray, int fillerStart, int fillerEnd, int slices, float tension,
            bool loop = false)
        {
            var nodesLength = nodesEnd - nodesStart;
            var formula = (slices * (nodesLength - 1)) + (nodesLength - 1);

            // yield the first point explicitly, if looping the first point
            // will be generated again in the step for loop when interpolating
            // from last point back to the first point
            int last = nodesLength - 1;
            int idx = 0;
            for (int current = 0; loop || current < last; ++current)
            {
                // wrap around when looping
                if (loop && current > last)
                {
                    current = 0;
                }

                // handle edge cases for looping and non-looping scenarios
                // when looping we wrap around, when not looping use start for previous
                // and end for next when you at the ends of the nodes array
                int end = (current == last) ? ((loop) ? 0 : current) : current + 1;
                int next = (end == last) ? ((loop) ? 0 : end) : end + 1;

                var nodePrevious = nodes[((current == 0) ? ((loop) ? last : current) : current - 1) + nodesStart];
                var nodeStart = nodes[(current) + nodesStart];
                var nodeEnd = nodes[(end) + nodesStart];
                var nodeNext = nodes[(next) + nodesStart];

                // adding one guarantees yielding at least the end point
                int stepCount = slices + 1;
                for (int step = 1; step <= stepCount; ++step)
                {
                    if ((idx + fillerStart) >= m_fillerArray.Length)
                        m_fillerArray.Add(CatmullRom(ref nodePrevious,
                        ref nodeStart,
                        ref nodeEnd,
                        ref nodeNext,
                        step, stepCount, tension));
                    else
                        m_fillerArray[idx + fillerStart] = CatmullRom(ref nodePrevious,
                            ref nodeStart,
                            ref nodeEnd,
                            ref nodeNext,
                            step, stepCount, tension);

                    if ((idx + fillerStart) >= fillerEnd)
                        return;

                    idx++;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateCatmullromSpline(Transform[] nodes, int nodesStart, int nodesEnd, List<Vector3> m_fillerArray, int fillerStart, int slices, float tension,
            bool loop = false)
        {
            var nodesLength = nodesEnd - nodesStart;
            var formula = (slices * (nodesLength - 1)) + (nodesLength - 1);

            // yield the first point explicitly, if looping the first point
            // will be generated again in the step for loop when interpolating
            // from last point back to the first point
            int last = nodesLength - 1;
            int idx = 0;
            for (int current = 0; loop || current < last; ++current)
            {
                // wrap around when looping
                if (loop && current > last)
                {
                    current = 0;
                }

                // handle edge cases for looping and non-looping scenarios
                // when looping we wrap around, when not looping use start for previous
                // and end for next when you at the ends of the nodes array
                int end = (current == last) ? ((loop) ? 0 : current) : current + 1;
                int next = (end == last) ? ((loop) ? 0 : end) : end + 1;

                var nodePrevious = nodes[((current == 0) ? ((loop) ? last : current) : current - 1) + nodesStart].localPosition;
                var nodeStart = nodes[(current) + nodesStart].localPosition;
                var nodeEnd = nodes[(end) + nodesStart].localPosition;
                var nodeNext = nodes[(next) + nodesStart].localPosition;

                // adding one guarantees yielding at least the end point
                int stepCount = slices + 1;
                for (int step = 1; step <= stepCount; ++step)
                {
                    if ((idx + fillerStart) >= m_fillerArray.Count)
                        m_fillerArray.Add(CatmullRom(ref nodePrevious,
                            ref nodeStart,
                            ref nodeEnd,
                            ref nodeNext,
                            step, stepCount, tension));
                    else
                        m_fillerArray[idx + fillerStart] = CatmullRom(ref nodePrevious,
                            ref nodeStart,
                            ref nodeEnd,
                            ref nodeNext,
                            step, stepCount, tension);

                    idx++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CalculateCatmullromSpline(LocalPosition[] nodes, int nodesStart, int nodesEnd, Vector3[] m_fillerArray, int fillerStart, int fillerEnd, int slices, float tension,
            bool loop = false)
        {
            var nodesLength = nodesEnd - nodesStart;
            var formula = (slices * (nodesLength - 1)) + (nodesLength - 1);

            // yield the first point explicitly, if looping the first point
            // will be generated again in the step for loop when interpolating
            // from last point back to the first point
            int last = nodesLength - 1;
            int idx = 0;
            for (int current = 0; loop || current < last; current++)
            {
                // wrap around when looping
                if (loop && current > last)
                {
                    current = 0;
                }

                // handle edge cases for looping and non-looping scenarios
                // when looping we wrap around, when not looping use start for previous
                // and end for next when you at the ends of the nodes array
                int end = (current == last) ? ((loop) ? 0 : current) : current + 1;
                int next = (end == last) ? ((loop) ? 0 : end) : end + 1;

                var nodePrevious = nodes[((current == 0) ? ((loop) ? last : current) : current - 1) + nodesStart].Value;
                var nodeStart = nodes[(current) + nodesStart].Value;
                var nodeEnd = nodes[(end) + nodesStart].Value;
                var nodeNext = nodes[(next) + nodesStart].Value;

                // adding one guarantees yielding at least the end point
                int stepCount = slices + 1;
                for (int step = 0; step <= stepCount; step++)
                {
                    if ((idx + fillerStart) >= fillerEnd)
                        return;
                    m_fillerArray[idx + fillerStart] = CatmullRom(ref nodePrevious,
                        ref nodeStart,
                        ref nodeEnd,
                        ref nodeNext,
                        step, stepCount, tension);
                    idx++;
                }
            }
        }

        /**
        * A Vector3 Catmull-Rom CatmullromSpline. Catmull-Rom CatmullromSplines are similar to bezier
        * CatmullromSplines but have the useful property that the generated curve will go
        * through each of the control points.
        *
        * NOTE: The NewCatmullRom() functions are an easier to use alternative to this
        * raw Catmull-Rom implementation.
        *
        * @param previous the point just before the start point or the start point
        *                 itself if no previous point is available
        * @param start generated when elapsedTime == 0
        * @param end generated when elapsedTime >= duration
        * @param next the point just after the end point or the end point itself if no
        *             next point is available
        */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CatmullRom(ref float3 previous, ref float3 start, ref float3 end, ref float3 next,
            float elapsedTime, float duration, float tension = 0.5f)
        {
            // References used:
            // p.266 GemsV1
            //
            // tension is often set to 0.5 but you can use any reasonable value:
            // http://www.cs.cmu.edu/~462/projects/assn2/assn2/catmullRom.pdf
            //
            // bias and tension controls:
            // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/interpolation/

            float percentComplete = elapsedTime / duration;
            float percentCompleteSquared = percentComplete * percentComplete;
            float percentCompleteCubed = percentCompleteSquared * percentComplete;

            var p = -tension * percentCompleteCubed + percentCompleteSquared - tension * percentComplete;
            var px = p * previous.x;
            var py = p * previous.y;
            var pz = p * previous.z;

            var s = 1.5f * percentCompleteCubed + -2.5f * percentCompleteSquared + 1.0f;
            var sx = s * start.x;
            var sy = s * start.y;
            var sz = s * start.z;

            var e = -1.5f * percentCompleteCubed + 2.0f * percentCompleteSquared + tension * percentComplete;
            var ex = e * end.x;
            var ey = e * end.y;
            var ez = e * end.z;

            var n = tension * percentCompleteCubed - tension * percentCompleteSquared;
            var nx = n * next.x;
            var ny = n * next.y;
            var nz = n * next.z;

            return new float3(px + sx + ex + nx,
                py + sy + ey + ny,
                pz + sz + ez + nz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CatmullRom(ref Vector3 previous, ref Vector3 start, ref Vector3 end, ref Vector3 next,
            float elapsedTime, float duration, float tension = 0.5f)
        {
            float percentComplete = elapsedTime / duration;
            float percentCompleteSquared = percentComplete * percentComplete;
            float percentCompleteCubed = percentCompleteSquared * percentComplete;

            var p = -tension * percentCompleteCubed + percentCompleteSquared - tension * percentComplete;
            var px = p * previous.x;
            var py = p * previous.y;
            var pz = p * previous.z;

            var s = 1.5f * percentCompleteCubed + -2.5f * percentCompleteSquared + 1.0f;
            var sx = s * start.x;
            var sy = s * start.y;
            var sz = s * start.z;

            var e = -1.5f * percentCompleteCubed + 2.0f * percentCompleteSquared + tension * percentComplete;
            var ex = e * end.x;
            var ey = e * end.y;
            var ez = e * end.z;

            var n = tension * percentCompleteCubed - tension * percentCompleteSquared;
            var nx = n * next.x;
            var ny = n * next.y;
            var nz = n * next.z;

            return new Vector3(px + sx + ex + nx,
                py + sy + ey + ny,
                pz + sz + ez + nz);
        }
    }
}