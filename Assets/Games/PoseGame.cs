using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class PoseGame : IGame
{
    AudioSource audio;
    Routine routine;
    int stepIndex = 0;
    Routine.Step step;
    AudioMixerSnapshot[] winning;
    AudioMixerSnapshot[] losing;

    private float PoseTimeRemaining()
    {
        float nextPoseTime = step.Time;
        Debug.Log("Audio time:" + audio.time);
        return nextPoseTime - audio.time;
    }

    private void UpdatePose(IHost host)
    {
        host.RelocateTargets(step.Pose);
        host.UpdatePoseTime(PoseTimeRemaining(), -1);
    }

    public void Begin(IHost host)
    {
        routine = HoleConfiguration.INSTANCE.PoseRoutine;
        step = routine.Steps[stepIndex];

        audio = host.GetAudioSource();
        AudioMixer mixer = audio.outputAudioMixerGroup.audioMixer;
        winning = new AudioMixerSnapshot[] { mixer.FindSnapshot("Unfiltered") };
        losing = new AudioMixerSnapshot[] { mixer.FindSnapshot("Filtered") };

        audio.clip = routine.Music;
        audio.Play();

        host.SetTimeRemaining(routine.Music.length);
        UpdatePose(host);
    }

    public void Update(IHost host, float timeRemaining, float poseTime)
    {
        if (poseTime == 0)
        {
            AudioMixerSnapshot[] mixerState = losing;
            if (host.Aligned())
            {
                host.Pulse();
                host.AddScore(1);
                mixerState = winning;
            }

            audio.outputAudioMixerGroup.audioMixer.TransitionToSnapshots(mixerState, new float[] { 1f }, 0.2f);

            stepIndex++;
            if (stepIndex >= routine.Steps.Count)
            {
                host.StopGame();
                return;
            }
            step = routine.Steps[stepIndex];

            if (timeRemaining > 0 && PoseTimeRemaining() > 0)
            {
                UpdatePose(host);
            }
            else
            {
                host.StopGame();
            }
        }
    }

    public void End(IHost host)
    {
        audio.Stop();
    }
}
