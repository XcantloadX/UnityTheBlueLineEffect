using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCube : MonoBehaviour {
    public bool moving = true;
    public float speed = 1;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () 
    {
        if(moving)
            transform.Translate(Random.Range(-10, 10) * speed, Random.Range(-10, 10) * speed, Random.Range(-10, 10) * speed);
	}
}
