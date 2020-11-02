using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoseApplier))]
public class PoseApplierEditor : Editor
{
    SerializedProperty pose;

    private void OnEnable()
    {
        pose = serializedObject.FindProperty("PoseToApply");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(pose);
        serializedObject.ApplyModifiedProperties();
        bool clicked = GUILayout.Button("Apply pose");
        if (clicked)
        {
            ApplyPose();
        }
    }

    private void ApplyPose()
    {
        Debug.Log("Target is: " + target);
        Debug.Log("Pose is: " + pose.objectReferenceValue);
        (target as PoseApplier)
            .GetComponent<PoseController>()
            .ApplyPose(pose.objectReferenceValue as Pose);
    }
}