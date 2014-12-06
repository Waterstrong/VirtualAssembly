using UnityEngine;
using System.Collections;

public class SelfRotate : MonoBehaviour {

    public float speed = 100f;
    
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * speed * Time.deltaTime);
	}
}
