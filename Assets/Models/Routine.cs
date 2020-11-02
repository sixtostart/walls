using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MyRoutine", menuName = "Hole/Routine", order = 2)]
public class Routine : ScriptableObject {

    [System.Serializable]
    public class Step
    {
        public float Time;
        public Pose Pose;
    }

    public string RoutineName = "New Routine";
    public List<Step> Steps;
    public AudioClip Music;
}
