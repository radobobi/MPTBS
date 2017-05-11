using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceManager : MonoBehaviour {

    public int id;
    public float origin_x;
    public float origin_y;
    public List<DelaunayTerrain.HalfEdge> half_edges;
    public List<int> neighbors_ids;
    //public List<FaceManager> neighbors;
    //public Mesh mesh;
    //public Transform chunk;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
