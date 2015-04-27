using UnityEngine;
using System.Collections;

public class SelfRotate : MonoBehaviour {

    public float speed = 100f;

    private float scale = 30f;
    
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward * speed * Time.deltaTime);
	}

    public void Zoom(int zoom) {
        transform.localScale = new Vector3(transform.localScale.x + zoom * scale, transform.localScale.y + zoom * scale, transform.localScale.z + zoom * scale);
    }

    public void TurnOver(int turn) {
        transform.Rotate(turn * Vector3.down * speed * 1.5f);
    }
}
