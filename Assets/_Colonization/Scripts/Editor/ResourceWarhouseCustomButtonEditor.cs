using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceWarhouse))]
public class ResourceWarhouseCustomButtonEditor : Editor
{
    [field: SerializeField] private int _spendCount;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (ResourceWarhouse)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        _spendCount = EditorGUILayout.IntField("Spend Count", _spendCount);

        if (GUILayout.Button($"Spend {_spendCount} Resource"))
        {
            script.TrySpendResource(_spendCount);
        }
    }
}