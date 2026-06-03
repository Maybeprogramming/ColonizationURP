using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BotFactory))]
public class BotFactoryCustomButtonEditor : Editor
{    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (BotFactory)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        if (GUILayout.Button($"Spawn bot"))
        {
            script.Spawn();
        }
    }
}