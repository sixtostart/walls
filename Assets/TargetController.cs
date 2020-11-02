using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour {

    public Animator Animator;
    public Passage.Step Step;
    public AudioSource HitMissAudio;
    public AudioClip HitClip;
    public AudioClip MissClip;
    public bool Inside;
    public int Size;
    public Transform LineRotator;
    public Transform LinePositioner;
    public SpriteRenderer LineRenderer;
    public SpriteRenderer CircleRenderer;
    public Sprite[] Sprites;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Update(Vector2 position, float size, Vector2 last)
    {
        transform.localPosition = position;
        LineRotator.gameObject.SetActive(last != Vector2.zero);

        Debug.Log("Size: " + size);

        int circleSize = Mathf.Min(Mathf.FloorToInt(size), Sprites.Length - 1);
        Size = circleSize;
        CircleRenderer.sprite = Sprites[circleSize];

        if (last != Vector2.zero)
        {
            float angle = Mathf.Atan2(last.y - position.y, last.x - position.x) * Mathf.Rad2Deg;
            Debug.LogFormat("Found angle {0} between {1} and {2}", angle, position, last);
            LineRotator.localRotation = Quaternion.Euler(0, 0, angle - 90f);

            float distance = Vector2.Distance(position, last);
            LinePositioner.localPosition = new Vector3(0, distance * 0.5f);
            LineRenderer.size = new Vector2(0.028f, distance);
        }
    }

    public void SetInside(bool inside)
    {
        Inside = inside;
        Animator.SetBool("inside", inside);
    }

    public bool Beat()
    {
        Animator.SetTrigger("beat");
        HitMissAudio.clip = Inside ? HitClip : MissClip;
        HitMissAudio.Play();

        return Inside;
    }
}
