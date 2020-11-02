using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour {

    public Transform DrawingPlane;
    public Transform WallDistancer;
    public Transform LeftCursor;
    public Transform RightCursor;
    public Transform LeftHandReference;
    public Transform RightHandReference;
    public List<TargetController> Targets = new List<TargetController>();
    public Animator Animator;
    public Sequence.Wall Wall;

    public float Tolerance;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        MoveCursor(LeftCursor, LeftHandReference);
        MoveCursor(RightCursor, RightHandReference);

        // Update each target with whether or not a cursor is inside them
        foreach (TargetController target in Targets)
        {
            float targetTolerance = Tolerance * target.Size;

            target.SetInside(Vector3.Distance(target.transform.localPosition, LeftCursor.localPosition) < targetTolerance ||
                Vector3.Distance(target.transform.localPosition, RightCursor.localPosition) < targetTolerance);
        }
    }

    private void MoveCursor(Transform cursor, Transform hand)
    {
        // Create plane for wall
        Plane plane = new Plane(DrawingPlane.position, DrawingPlane.position);

        // Project ray in the direction perpendicular to the wall's front plane
        //Ray ray = new Ray(hand.position, DrawingPlane.position);

        // Determine where the ray intersects the wall's front plane
        //float distance = 0;
        //bool hit = plane.Raycast(ray, out distance);

        // Move the cursor to the hit location
        cursor.position = plane.ClosestPointOnPlane(hand.position);

    }
}
