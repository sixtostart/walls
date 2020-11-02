using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseProjectionController : MonoBehaviour {

    [System.Serializable]
    public class PoseConnection
    {
        public GameObject Source;
        public GameObject Target;
    }

    [System.Serializable]
    public class Tube
    {
        public GameObject Start;
        public GameObject End;
    }

    public Transform Parent;
    public GameObject TubePrefab;
    public PoseConnection[] Connections;
    public Tube[] Tubes;

    private GameObject[] TubeObjects;

	void Start () {
        TubeObjects = new GameObject[Tubes.Length];

		for (int i = 0; i < Tubes.Length; i++)
        {
            TubeObjects[i] = Instantiate(TubePrefab, Parent);
        }
	}
	
	void Update () {
        foreach (PoseConnection pc in Connections)
        {
            Vector3 sourcePosition = pc.Source.transform.position;
            pc.Target.transform.localPosition = new Vector3(sourcePosition.x, sourcePosition.y);
        }

        for (int i = 0; i < Tubes.Length; i++)
        {
            Tube t = Tubes[i];
            Vector3 t1 = t.Start.transform.position;
            Vector3 t2 = t.End.transform.position;
            GameObject to = TubeObjects[i];
            PositionTube(to, t1, t2);
        }
    }

    public static void PositionTube(GameObject tube, Vector3 t1, Vector3 t2)
    {
        Vector3 td = t2 - t1;
        Vector3 midpoint = Vector3.Lerp(t1, t2, 0.5f);
        float angle = Mathf.Atan2(td.y, td.x) * Mathf.Rad2Deg;
        float distance = Vector3.Distance(t1, t2);
        tube.transform.SetPositionAndRotation(midpoint, Quaternion.Euler(0, 0, angle + 90f));
        tube.transform.localScale = new Vector3(0.01f, distance * 0.5f, 0.01f);
    }
}
