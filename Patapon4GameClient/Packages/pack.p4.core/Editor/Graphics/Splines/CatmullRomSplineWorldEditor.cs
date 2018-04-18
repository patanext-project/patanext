using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P4.Core.Graphics;
using UnityEditor;
using P4Main.Graphics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace P4Main.Graphics.Editor
{
#if UNITY_EDITOR && false == true
    [CustomEditor(typeof(SplineRendererBehaviour)), CanEditMultipleObjects]
    public class CatmullRomSplineWorldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            serializedObject.Update();

            foreach (SplineRendererBehaviour inspected in targets)
            {
                EditorGUILayout.LabelField("Inspecting: " + inspected.name);

                // Write Field 'Points'
                var array = serializedObject.FindProperty("Points");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(array, true);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                // Write Field 'LineRenderer'
                var lineRenderer = serializedObject.FindProperty("LineRenderer");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(lineRenderer, true);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
                // Write Field 'RefreshType'
                inspected.RefreshType = (EActivationZone)EditorGUILayout.EnumFlagsField("Refresh type", inspected.RefreshType);
                // Write Field 
                inspected.RefreshBounds = EditorGUILayout.BoundsField("Refresh Bounds", inspected.RefreshBounds);
                // Write Field 'Is Looping'
                inspected.IsLooping = EditorGUILayout.Toggle("Is Looping", inspected.IsLooping);
                // Write Field 'Tension'
                inspected.Tension = EditorGUILayout.FloatField("Tension", inspected.Tension);
                // Write Field 'Usable segments/Step'
                inspected.Step = EditorGUILayout.IntField("Line Step", inspected.Step);

                EditorGUILayout.Space();
            }
        }

        public virtual void OnSceneGUI()
        {
            var inspected = (SplineRendererBehaviour) target;
            var positions = new NativeArray<float3>(inspected.Points.Length, Allocator.TempJob);

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i != positions.Length; i++)
            {
                if (inspected.Points[i] == null)
                    continue;
                positions[i] = Handles.PositionHandle(inspected.GetPoint(i), quaternion.identity);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(inspected, "(Spline) Change control point position");

                for (int i = 0; i != positions.Length; i++)
                {
                    if (inspected.Points[i] == null)
                        continue;
                    inspected.SetPoint(i, positions[i]);
                }
            }

            positions.Dispose();
        }
    }
#endif
}
