using UnityEngine;
using System.Collections;

public class WallGame : IGame
{
    public void Begin(IHost host)
    {
        host.RelocateTargets();
        host.UpdatePoseTime(HoleConfiguration.INSTANCE.SkillTime, -1);
    }

    public void Update(IHost host, float timeRemaining, float poseTime)
    {
        if (poseTime == 0)
        {
            if (host.Aligned())
            {
                host.Pulse();
                host.AddScore(1);
            }

            if (timeRemaining > 0)
            {
                host.RelocateTargets();
                host.UpdatePoseTime(HoleConfiguration.INSTANCE.SkillTime, -1);
            }
            else
            {
                host.StopGame();
            }
        }
    }

    public void End(IHost host)
    {

    }
}
