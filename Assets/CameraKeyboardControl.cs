using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKeyboardControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private static float zoom_min = 0.5f;
    private static float zoom_max = 2f;

    private float zoom = 1f;
    private float zoom_delta = 0.1f;

    public float speed = 5.0f;
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= new Vector3(0, speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            zoom += zoom_delta;
            GameObject.Find("MapHolder").transform.localScale = (new Vector3(1, 1, 1)) * zoom;
        }
        if (Input.GetKey(KeyCode.E))
        {
            zoom -= zoom_delta;
            GameObject.Find("MapHolder").transform.localScale = (new Vector3(1, 1, 1)) * zoom;
        }
    }
}
