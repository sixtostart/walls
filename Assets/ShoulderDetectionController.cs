using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderDetectionController : MonoBehaviour {

    [System.Serializable]
    public class Config
    {
        public float Threshold = 0.05f;
        public int PointCount = 20;
        public int ChordStep = 10;
        public int IntersectionStep = 5;

        public float EstimateThreshold = 0.05f;
        public int EstimatePointsRequired = 15;

        public GameObject PointPrefab;
        public GameObject TubePrefab;
        public GameObject MidpointPrefab;
        public GameObject EstimatePrefab;
    }

    [System.Serializable]
    public class Solver
    {
        public Transform Position;
        // The last computed circle center
        public Vector2 Midpoint;
        // The best guess for where the player's shoulder is
        public Vector2 Estimate;
        public int EstimateSamples;
        public Vector3 LastPosition;

        // Point-pairs describing chord bisections of the circle
        public Vector4[] Bisections;
        // Points describing intersections of pairs of chord bisections of the circle
        public Vector2[] Intersections;

        // Position history for the hand
        public GameObject[] Points;
        // Connected points describing chords of the circle
        public GameObject[] Chords;
        // Lines perpendicular to chords, which approximately pass through the circle center
        public GameObject[] Bisectors;
        // Intersections of chords, which approximately locate the circle center 
        public GameObject[] Midpoints;
        // The last computed circle center
        public GameObject Midpointer;
        // Where we think the shoulder is
        public GameObject Shoulder;

        private Config Configuration;
        private int PointPointer = 0;

        public void Setup(string prefix, Transform root, Config configuration)
        {
            Configuration = configuration;
            int PointCount = configuration.PointCount;

            GameObject solverRootObject = MakeRoot(prefix, root);
            Transform solverRoot = solverRootObject.transform;

            Bisections = new Vector4[PointCount];
            Intersections = new Vector2[PointCount];

            Points = new GameObject[PointCount];
            GameObject pointRoot = MakeRoot("Points", solverRoot);

            Chords = new GameObject[PointCount];
            GameObject chordRoot = MakeRoot("Chords", solverRoot);

            Bisectors = new GameObject[PointCount];
            GameObject bisectorRoot = MakeRoot("Bisectors", solverRoot);

            Midpoints = new GameObject[PointCount];
            GameObject midpointRoot = MakeRoot("Midpoints", solverRoot);

            for (int i = 0; i < PointCount; i++)
            {
                Points[i] = Instantiate(Configuration.PointPrefab, pointRoot.transform);
                Points[i].name = "Point " + i;

                Chords[i] = Instantiate(Configuration.TubePrefab, chordRoot.transform);
                Chords[i].name = "Chord " + i;

                Bisectors[i] = Instantiate(Configuration.TubePrefab, bisectorRoot.transform);
                Bisectors[i].name = "Bisector " + i;

                Midpoints[i] = Instantiate(Configuration.PointPrefab, midpointRoot.transform);
                Midpoints[i].name = "Midpoint " + i;
            }

            Midpointer = Instantiate(Configuration.MidpointPrefab, solverRoot);
            Midpointer.name = "Midpoint Estimate";

            Shoulder = Instantiate(Configuration.EstimatePrefab, solverRoot);
            Shoulder.name = "Shoulder Estimate";
        }

        private GameObject MakeRoot(string name, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.parent = parent;
            root.transform.localPosition = Vector3.zero;
            return root;
        }

        public void Update()
        {
            // If either hand has moved more than threshold distance
            if (SufficientMovement())
            {
                // Update hands
                LastPosition = Position.position;
                // Update points
                Points[PointPointer].transform.localPosition =
                    new Vector3(LastPosition.x, LastPosition.y);
                // Increment pointer
                PointPointer = (PointPointer + 1) % Configuration.PointCount;
                // Rerun solver
                Solve();
            }
        }

        private bool SufficientMovement()
        {
            return Vector3.Distance(Position.position, LastPosition) > Configuration.Threshold;
        }

        private void Solve()
        {
            // Calculate bisectors (lines which intersect with the center of the circle)
            for (int i = 0; i < Configuration.PointCount; i++)
            {
                Vector3 P = Points[i].transform.position;
                Vector3 Q = Points[(i + Configuration.ChordStep) % Configuration.PointCount].transform.position;

                PoseProjectionController.PositionTube(Chords[i], P, Q);

                // Chord midpoint
                Vector3 M = (P + Q) / 2f;
                
                Vector3 direction = Q - P;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
                Vector3 N = M + perpendicular;
                Vector3 L = M - perpendicular;

                PoseProjectionController.PositionTube(Bisectors[i], L, N);

                // Two x,y points which both lie on the bisection line
                Bisections[i] = new Vector4(M.x, M.y, N.x, N.y);
            }

            // Calculate intersections of bisectors
            for (int i = 0; i < Configuration.PointCount; i++)
            {
                int i2 = (i + Configuration.IntersectionStep) % Configuration.PointCount;
                float x1 = Bisections[i].x;
                float y1 = Bisections[i].y;
                float x2 = Bisections[i].z;
                float y2 = Bisections[i].w;
                float x3 = Bisections[i2].x;
                float y3 = Bisections[i2].y;
                float x4 = Bisections[i2].z;
                float y4 = Bisections[i2].w;

                // X-coordinate numerator
                float mxn = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
                // Y-coordinate numerator
                float myn = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
                // Common denominator
                float md = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                Vector2 m = new Vector2(mxn / md, myn / md);

                Intersections[i] = m;

                if (!float.IsNaN(m.x) && !float.IsNaN(m.y))
                {
                    Midpoints[i].transform.localPosition = m;
                }
            }

            // Calculate average of intersections
            Vector2 accumulation = Vector2.zero;
            for (int i = 0; i < Configuration.PointCount; i++)
            {
                accumulation += Intersections[i];
            }
            Midpoint = accumulation / Configuration.PointCount;

            // Calculate a new average using points that are within threshold distance of the average
            Vector2 accumulation2 = Vector2.zero;
            int validPoints = 0;
            for (int i = 0; i < Configuration.PointCount; i++)
            {
                if (Vector2.Distance(Intersections[i], Midpoint) < Configuration.EstimateThreshold)
                {
                    accumulation2 += Intersections[i];
                    validPoints++;
                }
            }
            
            if (validPoints >= Configuration.EstimatePointsRequired)
            {
                Midpoint = accumulation2 / validPoints;
                Midpointer.SetActive(true);
                Midpointer.transform.localPosition = Midpoint;

                // Add the new midpoint to the moving average
                Estimate = ((Estimate * EstimateSamples) + Midpoint) / ++EstimateSamples;

                Shoulder.SetActive(true);
                Shoulder.transform.localPosition = Estimate;
            }
            else
            {
                Midpointer.SetActive(false);
            }
        }
    }

    public Transform PointsParent;

    public Solver LeftSolver;
    public Solver RightSolver;

    public Config Configuration;

    public void Start()
    {
        LeftSolver.Setup("Left", PointsParent, Configuration);
        RightSolver.Setup("Right", PointsParent, Configuration);
    }

    public void Update()
    {
        LeftSolver.Update();
        RightSolver.Update();
    }

}
