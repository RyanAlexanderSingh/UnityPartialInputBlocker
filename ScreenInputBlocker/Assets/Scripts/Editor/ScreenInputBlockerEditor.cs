using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ScreenInputBlocker))]
public class ScreenInputBlockerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ScreenInputBlocker myScript = (ScreenInputBlocker)target;
        if (GUILayout.Button("Test Partial Blocker"))
        {
            myScript.TestPartialBlocker();
        }
    }
}