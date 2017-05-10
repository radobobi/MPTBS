using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTest : MonoBehaviour {

    public Transform textPrefab = null;

    // Use this for initialization
    void Start () {
        Transform text = Instantiate<Transform>(textPrefab, transform.position, transform.rotation);
        text.transform.parent = transform;
        text.transform.localPosition = Vector3.zero;
        text.GetComponent<TextMesh>().text = "BLOH";
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
