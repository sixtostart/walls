using UnityEngine;
using System.Collections.Generic;

public interface IHost
{
    List<Pose> GetPoses();
    bool Aligned();
    void Pulse();
    void RelocateTargets();
    void RelocateTargets(Pose pose);
    void AddScore(int amount);
    AudioSource GetAudioSource();
    void UpdatePoseTime(float poseTime, float scaler);
    void StopGame();
    void SetTimeRemaining(float length);
}
