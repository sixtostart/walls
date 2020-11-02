using UnityEngine;
using System.Collections;

public class SpeedGame : IGame
{
    public void Begin(IHost host)
    {
        host.RelocateTargets();
        host.UpdatePoseTime(0, 1);
    }

    public void Update(IHost host, float timeRemaining, float poseTime)
    {
        if (host.Aligned())
        {
            host.Pulse();
            host.AddScore(1);
            host.RelocateTargets();
            host.UpdatePoseTime(0, 1);
        }

        if (timeRemaining <= 0)
        {
            host.StopGame();
        }

    }

    public void End(IHost host)
    {

    }
}
