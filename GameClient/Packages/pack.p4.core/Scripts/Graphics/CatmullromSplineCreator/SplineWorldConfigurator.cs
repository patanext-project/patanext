using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace P4.Core.Graphics
{
    /* NOT USED ANYMORE
    /// <summary>
    /// Hold importants references variables
    /// </summary>
    [Serializable]
    public class SplineWorldConfigurator : MonoBehaviour
    {
        public Transform[] Points;

        public LineRenderer LineRenderer;

        internal int _lastLineRendererPositionCount;

        private void OnDrawGizmos()
        {            
            var wrapper = GetComponent<DSplineWorldWrapper>();
            if (wrapper == null)
                return;

            var length = CGraphicalCatmullromSplineUtility.GetFormula(wrapper.Value.Step, Points.Length);
            if (length <= 0 || length > 512)
            {
                Debug.LogError($"Length of the spline is not acceptable: {length}");
                return;
            }
            
            var list = new List<Vector3>();
            CGraphicalCatmullromSplineUtility.CalculateCatmullromSpline(Points, 0, Points.Length - 1,
                list, 0, wrapper.Value.Step, wrapper.Value.Tension, false);
            
            LineRenderer.SetPositions(list.ToArray());
            
        }

        public float3 GetPoint(int index)
        {
            return Points[index].position;        
        }

        public void SetPoint(int index, float3 value)
        {
            Points[index].position = value;
        }
    }*/
}
