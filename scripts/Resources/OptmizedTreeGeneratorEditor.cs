using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OptimizedTreeGenerator))]
public class OptimizedTreeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        OptimizedTreeGenerator generator = (OptimizedTreeGenerator)target;

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Trees", GUILayout.Height(40)))
        {
            generator.GenerateTrees();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Clear Trees", GUILayout.Height(30)))
        {
            generator.ClearTrees();
        }

        GUI.backgroundColor = Color.white;
    }
}