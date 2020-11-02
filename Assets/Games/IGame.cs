using UnityEngine;
using System.Collections;

public interface IGame
{
    void Begin(IHost host);
    void Update(IHost host, float timeRemaining, float poseTime);
    void End(IHost host);
}
