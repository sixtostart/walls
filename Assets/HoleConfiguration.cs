using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleConfiguration : MonoBehaviour {
    public float HeightScale = 1.00f;
    public float UpperArmScale = 1.00f;
    public float ForearmScale = 1.00f;

    public float SkillTime = 1.00f;

    public Routine PoseRoutine;

    public static HoleConfiguration INSTANCE;

    public HoleConfiguration()
    {
        INSTANCE = this;
    }
}
