using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour {

    public interface IListener
    {
        void OnBeat(int measure, int beat);
    }

    IListener Callback;

    public float BeatsPerMinute = 120f;
    public int BeatsPerMeasure = 4;

    public bool Playing = true;
    public int measure = -2;
    public int beat = 0;

    public AudioSource Player;

    int lastMeasure;
    int lastBeat;
    float measuresPerSecond;
    float absoluteBeats;

	// Use this for initialization
	void Start () {
        Reset();
	}
	
    // Update is called once per frame
	void Update () {
        if (!Playing)
        {
            return;
        }

        float beatsAdvance = Time.deltaTime * measuresPerSecond;
        absoluteBeats += beatsAdvance;

        measure = Mathf.FloorToInt(absoluteBeats);

        float measureFraction = absoluteBeats - measure;
        beat = Mathf.FloorToInt(1 + (measureFraction * BeatsPerMeasure));

        if (measure != lastMeasure || beat != lastBeat)
        {
            if (Callback != null)
            {
                Callback.OnBeat(measure, beat);
            }

            if (measure == 1 && beat == 1)
            {
                Player.Play();
            }
        }

        lastMeasure = measure;
        lastBeat = beat;
	}

    public void Reset()
    {
        Player.Stop();
        measure = 0;
        beat = 1;
        measuresPerSecond = BeatsPerMinute / (60f * BeatsPerMeasure);
        lastMeasure = measure - 1;
        lastBeat = beat - 1;
        absoluteBeats = 0f;
        Playing = false;
    }

    public void Play (Sequence sequence, IListener callback)
    {
        Reset();

        BeatsPerMinute = sequence.BeatsPerMinute;
        BeatsPerMeasure = sequence.BeatsPerMeasure;

        Player.clip = sequence.Music;

        Callback = callback;
        Playing = true;
    }
}
