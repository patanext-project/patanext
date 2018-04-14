using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using P4Main.Graphics;
using Unity.Collections;
using Unity.Mathematics;

namespace P4Main.Graphics.Editor
{
    [CustomEditor(typeof(CatmullRomSplineWorldConfigurator))]
    public class CatmullRomSplineWorldConfiguratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        public virtual void OnSceneGUI()
        {
            var inspected = (CatmullRomSplineWorldConfigurator)target;
            var positions = new NativeArray<float3>(inspected.Points.Length, Allocator.TempJob);

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i != positions.Length; i++)
            {
                positions[i] = Handles.PositionHandle(inspected.GetPoint(i), quaternion.identity);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(inspected, "(Spline) Change control point position");

                for (int i = 0; i < positions.Length; i++)
                    inspected.SetPoint(i, positions[i]);
            }
            positions.Dispose();
        }
    }
}
