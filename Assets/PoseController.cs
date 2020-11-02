using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class PoseController : MonoBehaviour {

    public SteamVR_TrackedController RightController;
    public GameObject PlayerPelvis;
    public GameObject ReplicaPelvis;
    // We use the replica's toe position to ensure the replica's
    // feet are planted on the ground after reprojection.
    public GameObject ReplicaToe;

    public bool Recording = false;

    // Use this for initialization
    void Start () {
        RightController.TriggerClicked += TriggerClicked;
    }

    void TriggerClicked(object sender, ClickedEventArgs args)
    {
        RecordPose();
    }

    // Update is called once per frame
    void Update () {
		
	}

    /// <summary>
    /// Copy the player's pose onto the replica
    /// </summary>
    void RecordPose ()
    {
        // The pose is most interestingly defined by the rotation of each of the 
        // dummy's bones.

        // We can flatten the dummy's object heirarchy into a list using BFS, and 
        // then write every quarternion out in order.
        List<Transform> sourceTransforms = BFS(PlayerPelvis);
        List<Quaternion> sourceRotations = ReadQuaternions(sourceTransforms);

        // Then we can project those quaternions onto another dummy, proving that 
        // this reconstructs the pose.
        ApplyPose(ReplicaPelvis, ReplicaToe, sourceRotations);

        // Save the pose.
        if (Application.isEditor && Recording)
        {
            Pose pose = ScriptableObject.CreateInstance<Pose>();
            pose.PoseName = "Recorded Pose";
            pose.Rotations = sourceRotations;
            string fileName = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\-mm\\-ss");
            AssetDatabase.CreateAsset(pose, "Assets/Poses/" + fileName + ".asset");
        }
    }

    public void ApplyPose(Pose pose)
    {
        Debug.Log("Applying pose " + pose.PoseName);
        ApplyPose(ReplicaPelvis, ReplicaToe, pose.Rotations);
    }

    /// <summary>
    /// Given a GameObject, return a list containing the transforms of it and all 
    /// its descendents, as traversed breadth-first.
    /// </summary>
    List<Transform> BFS (GameObject root)
    {
        List<Transform> transforms = new List<Transform>();

        Queue<Transform> toSearch = new Queue<Transform>();
        toSearch.Enqueue(root.transform);

        while (toSearch.Count > 0)
        {
            Transform t = toSearch.Dequeue();
            transforms.Add(t);
            
            for (int i = 0; i < t.childCount; i++)
            {
                toSearch.Enqueue(t.GetChild(i));
            }
        }

        return transforms;
    }

    List<Quaternion> ReadQuaternions(List<Transform> transforms)
    {
        Debug.Log(string.Format("Reading {0} quaternions...", transforms.Count));

        List<Quaternion> qs = new List<Quaternion>(transforms.Count);

        foreach (Transform t in transforms)
        {
            qs.Add(t.localRotation);
        }

        return qs;
    }

    void ApplyPose(GameObject TargetPelvis, GameObject TargetPlant, List<Quaternion> rotations)
    {
        List<Transform> targetTransforms = BFS(TargetPelvis);
        WriteQuaternions(targetTransforms, rotations);

        // Plant the projected pose on the ground by moving the reference toe to y=0
        float plantOffset = TargetPlant.transform.position.y;
        Debug.Log("Target pose Z correction: " + plantOffset);
        Vector3 trp = TargetPelvis.transform.position;
        TargetPelvis.transform.position = new Vector3(trp.x, trp.y - plantOffset, trp.z);
    }

    void WriteQuaternions(List<Transform> destinationTransforms, List<Quaternion> sourceRotations)
    {
        Debug.Log(string.Format("Writing to {0} transforms...", destinationTransforms.Count));

        for (int i = 0; i < destinationTransforms.Count; i++)
        {
            destinationTransforms[i].localRotation = sourceRotations[i];
        }
    }

    public void StartRecording()
    {
        Recording = true;
    }
}
