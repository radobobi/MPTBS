using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject mapHolder;

    private Color normalColor = Color.red;
    private Color mouseDownColor = Color.green;
    private Color mouseEnterColor = Color.yellow;
    private Color neighbor = Color.cyan;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void mapclick(GameObject objClicked)
    {
        //Debug.Log("Clicked: " + objClicked.name);
        //print("Clicked: " + objClicked.name);
    }

    public void mapMouseDown(GameObject objClicked)
    {
        //Debug.Log("Pointer Down: " + objClicked.name);

        MeshRenderer mr = objClicked.GetComponent<MeshRenderer>();
        mr.material.color = mouseDownColor;
    }

    public void mapMouseUp(GameObject objClicked)
    {
        //Debug.Log("Pointer Up: " + objClicked.name);

        MeshRenderer mr = objClicked.GetComponent<MeshRenderer>();
        mr.material.color = normalColor;
    }

    public void mapMouseEnter(GameObject objClicked)
    {
        //Debug.Log("Pointer Enter: " + objClicked.name);

        MeshRenderer mr = objClicked.GetComponent<MeshRenderer>();
        mr.material.color = mouseEnterColor;

        FaceManager[] face_managers = mapHolder.GetComponent<DelaunayTerrain>().faces_managers;
        FaceManager current_face = objClicked.GetComponent<FaceManager>();

        for(int i=0; i<current_face.neighbors_ids.Count; ++i) {
            if (current_face.neighbors_ids[i] >= 0)
            {
                face_managers[current_face.neighbors_ids[i]].GetComponent<MeshRenderer>().material.color = neighbor;
            }
        }
}

    public void mapMouseExit(GameObject objClicked)
    {
        //Debug.Log("Pointer Exit: " + objClicked.name);

        MeshRenderer mr = objClicked.GetComponent<MeshRenderer>();
        mr.material.color = normalColor;

        FaceManager[] face_managers = mapHolder.GetComponent<DelaunayTerrain>().faces_managers;
        FaceManager current_face = objClicked.GetComponent<FaceManager>();

        for (int i = 0; i < current_face.neighbors_ids.Count; ++i)
        {
            if (current_face.neighbors_ids[i] >= 0)
            {
                face_managers[current_face.neighbors_ids[i]].GetComponent<MeshRenderer>().material.color = normalColor;
            }
        }
    }
}