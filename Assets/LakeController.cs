using System;
using System.Collections.Generic;
using UnityEngine;

public class LakeController : MonoBehaviour, MusicController.IListener {

    public Sequence SequenceToPlay;
    public MusicController MusicController;
    public GameObject WallPrefab;
    public GameObject TargetPrefab;
    public Transform GameRoot;
    public Transform ScoreRotator;
    public TextMesh ScoreText;
    public SteamVR_TrackedController LeftController;
    public SteamVR_TrackedController RightController;

    // The number of beats in advance of the actual beat to begin the target animation.
    // The extra beats give the animation time to display the target and timing cursors.
    public int TargetAnimationLead = 10;
    public int WallAnimationLead = 8;
    public int ScoreAnimationLead = 4;

    public Dictionary<int, List<WallController>> WallMap = new Dictionary<int, List<WallController>>();
    public Dictionary<int, List<TargetController>> TargetMap = new Dictionary<int, List<TargetController>>();

    // Rotation smoothing parameters
    private Quaternion scoreTarget = Quaternion.identity;
    private float scoreSmooth = 0.3F;
    private float scoreVelocity = 0.0F;

    // Scoring
    private int targetsTotal = 0;
    private int targetsHit = 0;

    // Use this for initialization
    void Start () {
        LeftController.Gripped += delegate
        {
            Debug.Log("Left Gripped");
            BeginSequence(SequenceToPlay);
        };
        RightController.Gripped += delegate
        {
            Debug.Log("Right Gripped");
            BeginSequence(SequenceToPlay);
        };
        Debug.Log("Grip delegates configured");
	}

    void Update ()
    {
        float yAngle = Mathf.SmoothDampAngle(ScoreRotator.eulerAngles.y, scoreTarget.eulerAngles.y, ref scoreVelocity, scoreSmooth);
        ScoreRotator.rotation = Quaternion.Euler(0, yAngle, 0);
    }
    
    void BeginSequence(Sequence sequence)
    {
        Debug.Log("Destroying...");
        DestroyLevel();

        Debug.Log("Building level...");
        BuildLevel(SequenceToPlay);

        Debug.Log("Playing...");
        MusicController.Play(sequence, this);
    }

    // Given a Sequence, instantiate all of the prefabs required to play the sequence.
    void BuildLevel(Sequence sequence)
    {
        targetsTotal = 0;
        targetsHit = 0;

        if (sequence.Walls.Length > 0)
        {
            scoreTarget = Quaternion.Euler(0, sequence.Walls[0].Angle, 0);
            Debug.Log("Rotating to first wall at " + sequence.Walls[0].Angle);
        }

        foreach (Sequence.Wall wall in sequence.Walls)
        {
            Vector2 lastLeft = Vector2.zero;
            Vector2 lastRight = Vector2.zero;

            // Instantiate and configure a wall
            GameObject wallObj = Instantiate(WallPrefab, GameRoot);
            WallController wallController = wallObj.GetComponent<WallController>();
            wallController.Wall = wall;
            wallController.Tolerance = sequence.Tolerance;
            AddToWallMap(wall, wallController);

            wallObj.name = "Wall for " + wall.Measure + " " + wall.Passage.Name;
            wallObj.transform.Rotate(Vector3.up, wall.Angle);

            Transform drawingPlane = wallController.DrawingPlane;

            foreach (Passage.Step step in wall.Passage.Steps)
            {
                // Instantiate and configure targets for left and right hands

                Vector2 left = new Vector2(step.LX, step.LY);
                Vector2 right = new Vector2(step.RX, step.RY);

                if (left != Vector2.zero)
                {
                    lastLeft = MakeTarget(sequence, wall, wallController, step, drawingPlane, left, step.LS, lastLeft);
                    targetsTotal++;
                }

                if (right != Vector2.zero)
                {
                    lastRight = MakeTarget(sequence, wall, wallController, step, drawingPlane, right, step.RS, lastRight);
                    targetsTotal++;
                }
            }
        }
    }

    Vector2 MakeTarget(
        Sequence sequence, 
        Sequence.Wall wall, 
        WallController wallController, 
        Passage.Step step, 
        Transform drawingPlane,
        Vector2 position,
        float size,
        Vector2 last)
    {
        GameObject target = Instantiate(TargetPrefab, drawingPlane);
        TargetController targetController = target.GetComponent<TargetController>();
        wallController.Targets.Add(targetController);
        targetController.Step = step;
        targetController.HitClip = sequence.HitSound;
        targetController.MissClip = sequence.MissSound;
        targetController.Update(position, size, last);
        target.name = step.Beat + " left";
        AddToTargetMap(wall, step, targetController);

        return position;
    }

    int CalculateMonobeat(Sequence.Wall wall, Passage.Step step)
    {
        int measure = Mathf.FloorToInt(wall.Measure);
        int beat = Mathf.RoundToInt((wall.Measure - measure) * 10);
        return CalculateMonobeat(measure, beat) + ((step == null)? 0 : step.Beat);
    }

    int CalculateMonobeat(int measure, int beat)
    {
        return measure * SequenceToPlay.BeatsPerMeasure + beat;
    }

    void AddToWallMap(Sequence.Wall wall, WallController wc)
    {
        int key = CalculateMonobeat(wall, null);
        if (!WallMap.ContainsKey(key))
        {
            WallMap.Add(key, new List<WallController>());
        }
        WallMap[key].Add(wc);
    }

    void AddToTargetMap(Sequence.Wall wall, Passage.Step step, TargetController target)
    {
        int key = CalculateMonobeat(wall, step);
        if (!TargetMap.ContainsKey(key))
        {
            TargetMap.Add(key, new List<TargetController>());
        }
        TargetMap[key].Add(target);
    }

    void DestroyLevel()
    {
        foreach (List<WallController> w in WallMap.Values)
        {
            foreach (WallController wc in w)
            {
                Destroy(wc.gameObject);
            }
        }

        foreach (List<TargetController> l in TargetMap.Values)
        {
            foreach (TargetController tc in l)
            {
                Destroy(tc.gameObject);
            }
        }

        WallMap.Clear();
        TargetMap.Clear();
    }

    void MusicController.IListener.OnBeat(int measure, int beat)
    {
        // Key for targets that are due, and that player must be aligned with
        int beatKey = CalculateMonobeat(measure, beat);
        if (TargetMap.ContainsKey(beatKey))
        {
            foreach (TargetController tc in TargetMap[beatKey])
            {
                if (tc.Beat())
                {
                    targetsHit++;
                }
            }
        }

        // Key for targets that should be animated ahead of becoming due
        int startKey = beatKey + TargetAnimationLead;
        if (TargetMap.ContainsKey(startKey))
        {
            foreach (TargetController tc in TargetMap[startKey])
            {
                tc.Animator.SetTrigger("start");
            }
        }

        // Key for walls that should be animated ahead of becoming due
        int wallStartKey = beatKey + WallAnimationLead;
        if (WallMap.ContainsKey(wallStartKey))
        {
            foreach (WallController wc in WallMap[wallStartKey])
            {
                wc.Animator.SetTrigger("Enter");
            }
        }

        // Key for walls that the score should move to ahead of becoming due
        int scoreStartKey = beatKey + ScoreAnimationLead;
        if (WallMap.ContainsKey(scoreStartKey))
        {
            foreach (WallController wc in WallMap[scoreStartKey])
            {
                scoreTarget = Quaternion.Euler(0, wc.Wall.Angle, 0);
            }
        }

        ScoreText.text = targetsHit + "/" + targetsTotal;
    }
}
