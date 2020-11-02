using System.Collections.Generic;
using UnityEngine;

public class PlayController : MonoBehaviour, IHost {

    public GameObject HeadMarker;
    public GameObject LeftHandMarker;
    public GameObject RightHandMarker;

    public GameObject HeadController;
    public GameObject LeftHandController;
    public GameObject RightHandController;

    public GameObject Targets;
    public GameObject RightHandTarget;
    public GameObject LeftHandTarget;
    public Renderer RightHandRay;
    public Renderer LeftHandRay;

    public Color MissColor;
    public Color HitColor;

    public TextMesh ScoreText;
    public TextMesh TimeRemainingText;
    public TextMesh PoseTimeText;

    /// <summary>
    /// The center of the circle in which to place right hand targets.
    /// </summary>
    public Vector2 RightHandCenter = new Vector2(0.3f, 1.3f);
    /// <summary>
    /// The center of the circle in which to place left hand targets.
    /// </summary>
    public Vector2 LeftHandCenter = new Vector2(-0.3f, 1.3f);
    /// <summary>
    /// The maximum distance hand targets can be displaced.
    /// </summary>
    public float HandRadius = 0.7f;
    /// <summary>
    /// The distance beneath which a marker will be considered aligned with a target.
    /// </summary>
    public float HitDistance = 0.05f;

    public float TimeRemaining = 0;
    public int TimeRemainingSeconds = 0;
    public float PoseTime = 0;
    public float PoseTimeDeciseconds = 0;
    public float PoseTimeScaler = 1f;
    public float RoundLength = 30f;
    public int Score = 0;

    public bool RightHit = false;
    public bool LeftHit = false;

    public PoseController PoseController;
    public List<Pose> PoseRegister;
    public Transform PoseLeftHand;
    public Transform PoseRightHand;

    public AudioSource AudioSource;

    IGame Game;

    // Use this for initialization
    void Start () {
        Targets.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        Project();
        
        if (Game != null)
        {
            TimeRemaining = Mathf.Max(TimeRemaining - Time.deltaTime, 0);
            PoseTime = Mathf.Max(PoseTime + (Time.deltaTime * PoseTimeScaler), 0);

            int seconds = Mathf.CeilToInt(TimeRemaining);
            if (seconds != TimeRemainingSeconds)
            {
                TimeRemainingSeconds = seconds;
                TimeRemainingText.text = string.Format("{0}", seconds);
            }

            int deciseconds = Mathf.CeilToInt(PoseTime * 10);
            if (deciseconds != PoseTimeDeciseconds)
            {
                PoseTimeDeciseconds = deciseconds;
                PoseTimeText.text = string.Format("{0:f1}", deciseconds/10f);
            }

            Game.Update(this, TimeRemaining, PoseTime);
        }
    }

    #region Projection

    private void Project()
    {
        Project(HeadController, HeadMarker);
        Project(LeftHandController, LeftHandMarker);
        Project(RightHandController, RightHandMarker);

        LeftHit = CheckHit(LeftHandMarker, LeftHandTarget);
        RightHit = CheckHit(RightHandMarker, RightHandTarget);

        LeftHandRay.material.SetColor("_EmissionColor", LeftHit ? HitColor : MissColor);
        RightHandRay.material.SetColor("_EmissionColor", RightHit ? HitColor : MissColor);
    }

    /// <summary>
    /// Given a source and destination GameObject, set the destination's X and Y coords to match the source's.
    /// </summary>
    /// <param name="source">The object to read</param>
    /// <param name="destination">The object to move on the X/Y plane</param>
    private void Project(GameObject source, GameObject destination)
    {
        Vector3 sourcePosition = source.transform.position;
        destination.transform.localPosition = new Vector3(sourcePosition.x, sourcePosition.y); 
    }

    #endregion

    #region Targets

    public List<Pose> GetPoses()
    {
        return PoseRegister;
    }

    public AudioSource GetAudioSource()
    {
        return AudioSource;
    }

    public void RelocateTargets(Pose pose)
    {
        Targets.SetActive(true);
        // Apply the pose
        PoseController.ApplyPose(pose);
        // Move targets
        RelocateTarget(LeftHandTarget, PoseLeftHand.transform.position);
        RelocateTarget(RightHandTarget, PoseRightHand.transform.position);
    }

    public void RelocateTargets()
    {
        Targets.SetActive(true);
        RelocateTarget(LeftHandTarget, LeftHandCenter, HandRadius);
        RelocateTarget(RightHandTarget, RightHandCenter, HandRadius);
    }

    /// <summary>
    /// Move the given target somewhere within the given circle
    /// </summary>
    private void RelocateTarget(GameObject target, Vector2 center, float radius)
    {
        Vector2 point = Random.insideUnitCircle;
        target.transform.localPosition = new Vector3(
            center.x + (point.x * radius),
            center.y + (point.y * radius));
    }

    private void RelocateTarget(GameObject target, Vector3 position)
    {
        target.transform.localPosition = new Vector3(position.x, position.y);
    }

    public bool Aligned()
    {
        return LeftHit && RightHit;
    }

    private bool CheckHit(GameObject marker, GameObject target)
    {
        return Vector3.Distance(marker.transform.position, target.transform.position) < HitDistance;
    }

    #endregion

    #region Haptics

    public void Pulse()
    {
        Pulse(LeftHandController);
        Pulse(RightHandController);
    }

    private void Pulse(GameObject controller)
    {
        int input = (int)controller.GetComponent<SteamVR_TrackedObject>().index;
        StartCoroutine(LongVibration(input, 0.1f, 0.2f));
    }

    System.Collections.IEnumerator LongVibration(int controllerIndex, float length, float strength)
    {
        for (float i = 0; i < length; i += Time.deltaTime)
        {
            SteamVR_Controller.Input(controllerIndex).TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
            yield return null;
        }
    }

    #endregion

    #region Rounds

    public void SkillRound()
    {
        UpdateScore();
        TimeRemaining = RoundLength;
        Game = new WallGame();
        Game.Begin(this);
    }

    public void SpeedRound()
    {
        UpdateScore();
        TimeRemaining = RoundLength;
        Game = new SpeedGame();
        Game.Begin(this);
    }

    public void PoseRound()
    {
        UpdateScore();
        Game = new PoseGame();
        Game.Begin(this);
    }

    public void StopGame()
    {
        Game.End(this);
        Game = null;

        Targets.SetActive(false);
    }

    public void AddScore(int score)
    {
        UpdateScore(Score + score);
    }

    public void UpdatePoseTime(float poseTime, float scaler)
    {
        PoseTime = poseTime;
        PoseTimeScaler = scaler;
    }
    
    public void SetTimeRemaining(float remaining)
    {
        TimeRemaining = remaining;
    }

    void UpdateScore(int score = 0)
    {
        Score = score;
        ScoreText.text = string.Format("{0}", Score);
    }

    #endregion

}
