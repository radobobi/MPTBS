using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKeyboardControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

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
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.position -= new Vector3(0, 0, speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            transform.position += new Vector3(0, 0, speed * Time.deltaTime);
        }
    }
}
