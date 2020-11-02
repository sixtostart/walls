using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(PassageProcessor))]
public class PassageProcessorEditor : Editor
{
    SerializedProperty clipProperty;

    private void OnEnable()
    {
        clipProperty = serializedObject.FindProperty("ClipToProcess");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(clipProperty);
        serializedObject.ApplyModifiedProperties();
        bool clicked = GUILayout.Button("Process clip");
        if (clicked)
        {
            ProcessClip();
        }
    }

    private void ProcessClip()
    {
        Debug.Log("Target is: " + target);

        AnimationClip clip = clipProperty.objectReferenceValue as AnimationClip;
        Debug.Log("Clip is: " + clip);

        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);

        AnimationCurve LX = null, LY = null, RX = null, RY = null;

        Dictionary<int, float[]> keyMap = new Dictionary<int, float[]>();

        foreach (EditorCurveBinding binding in bindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

            if (binding.path == "Left Cursor" && binding.propertyName == "m_LocalPosition.x")
            {
                LX = curve;
                ProcessKeys(LX, 0, keyMap);
            }

            if (binding.path == "Left Cursor" && binding.propertyName == "m_LocalPosition.y")
            {
                LY = curve;
                ProcessKeys(LY, 1, keyMap);
            }

            if (binding.path == "Right Cursor" && binding.propertyName == "m_LocalPosition.x")
            {
                RX = curve;
                ProcessKeys(RX, 2, keyMap);
            }

            if (binding.path == "Right Cursor" && binding.propertyName == "m_LocalPosition.y")
            {
                RY = curve;
                ProcessKeys(RY, 3, keyMap);
            }
        }

        if (LX != null && RX != null && LY != null && RY != null)
        {
            Debug.Log("Got all curves");
        }
        else
        {
            Debug.LogError("Curve missing! " + LX + " " + LY + " " + RX + " " + RY);
        }

        List<int> moments = new List<int>(keyMap.Keys);
        moments.Sort();

        Passage passage;
        string path = "Assets/Data/Passages/" + clip.name + ".asset";
        bool loadedExisting = false;

        passage = AssetDatabase.LoadAssetAtPath<Passage>(path);
        if (passage != null)
        {
            loadedExisting = true;
        }
        else
        {
            passage = ScriptableObject.CreateInstance<Passage>();
        }

        passage.name = clip.name;
        passage.SourceClip = clip;
        passage.Steps = new Passage.Step[moments.Count];

        int i = 0;
        foreach (int time in moments)
        {
            Debug.LogFormat("Moment {0} beat {1} LX {2} LY {3} RX {4} RY {5}", i, time, keyMap[time][0], keyMap[time][1], keyMap[time][2], keyMap[time][3]);
            passage.Steps[i] = new Passage.Step();
            passage.Steps[i].Beat = time;
            passage.Steps[i].LX = keyMap[time][0];
            passage.Steps[i].LY = keyMap[time][1];
            passage.Steps[i].RX = keyMap[time][2];
            passage.Steps[i].RY = keyMap[time][3];
            i++;
        }

        if (loadedExisting)
        {
            // Nothing more to do
        }
        else
        {
            AssetDatabase.CreateAsset(passage, path);
            Debug.Log("Asset created / updated");
        }
    }

    private void ProcessKeys(AnimationCurve curve, int index, Dictionary<int, float[]> keyMap)
    {
        foreach (Keyframe k in curve.keys)
        {
            int time = Mathf.RoundToInt(k.time);

            if (time != k.time)
            {
                Debug.LogError("Non-integer beat! " + k.time);
            }

            if (!keyMap.ContainsKey(time))
            {
                keyMap[time] = new float[4];
            }

            keyMap[time][index] = k.value;
        }
    }
}