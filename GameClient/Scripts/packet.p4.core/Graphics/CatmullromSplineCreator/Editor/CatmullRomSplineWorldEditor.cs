using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using P4Main.Graphics;

namespace P4Main.Graphics.Editor
{
    [CustomEditor(typeof(CatmullRomSplineWorldComponent)), CanEditMultipleObjects]
    public class CatmullRomSplineWorldEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            foreach (CatmullRomSplineWorldComponent inspected in targets)
            {
                EditorGUILayout.LabelField("Inspecting: " + inspected.name);

                var result = inspected.Value;

                // Write Field 'Is Looping'
                result.IsLooping    = EditorGUILayout.Toggle("Is Looping", result.IsLooping == 0 ? false : true) ? 1 : 0;
                // Write Field 'Tension'
                result.Tension      = EditorGUILayout.FloatField("Tension", result.Tension);
                // Write Field 'Usable segments/Step'
                result.Step         = EditorGUILayout.IntField("Line Step", result.Step);
                // Write Field 'Use Depth'
                //result.UseDepth     = EditorGUILayout.Toggle("Use Depth (z)", result.UseDepth == 0 ? false : true) ? 1 : 0;

                inspected.Value = result;

                EditorGUILayout.Space();
            }
        }
    }
}
