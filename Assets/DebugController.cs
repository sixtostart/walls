using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour {

    public MusicController Music;
    public TextMesh DebugText;
    public bool ShowText;

    string template;

	// Use this for initialization
	void Start () {
        template = DebugText.text;
	}
	
	// Update is called once per frame
	void Update () {
        DebugText.gameObject.SetActive(ShowText);
        if (ShowText)
        {
            string measure = "None";
            if (Music)
            {
                measure = Music.measure + "." + Music.beat;
            }

            string text = template
                .Replace("$measure", measure);

            DebugText.text = text;
        }
	}
}
