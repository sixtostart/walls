using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MySequence", menuName = "Hole/Sequence", order = 3)]
public class Sequence : ScriptableObject
{
    [System.Serializable]
    public class Wall
    {
        public float Measure;
        public float Angle;
        public Passage Passage;
    }

    public float BeatsPerMinute = 120;
    public int BeatsPerMeasure = 4;
    public AudioClip Music;
    public AudioClip HitSound;
    public AudioClip MissSound;
    public Wall[] Walls;
    public float Tolerance = 0.08f;
}
