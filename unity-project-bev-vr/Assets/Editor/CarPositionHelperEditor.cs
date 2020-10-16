using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CarPositionHelper))]
public class CarPositionHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CarPositionHelper myScript = (CarPositionHelper)target;
        if (GUILayout.Button("Move car to start"))
        {
            myScript.MoveCarToStartPosition();
        }
        if (GUILayout.Button("Move car to highway"))
        {
            myScript.MoveCarToHighwayPosition();
        }
        if (GUILayout.Button("Move car to debug"))
        {
            myScript.MoveCarToDebugPosition();
        }

    }



}
