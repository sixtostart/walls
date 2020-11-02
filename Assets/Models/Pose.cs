using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MyPose", menuName = "Hole/Pose", order = 1)]
public class Pose : ScriptableObject {
    public string PoseName = "New Pose";
    public List<Quaternion> Rotations;
}
