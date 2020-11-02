using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passage : ScriptableObject {

    [System.Serializable]
    public class Step
    {
        public int Beat;
        public float LX;
        public float LY;
        public float LS = 1;
        public float RX;
        public float RY;
        public float RS = 1;
    }

    public string Name;
    public AnimationClip SourceClip;
    public Step[] Steps;
}
