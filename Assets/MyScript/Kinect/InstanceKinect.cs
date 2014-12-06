using UnityEngine;
using System.Collections;

public class InstanceKinect : MonoBehaviour {

	public GameObject prefabs = null;
	// Use this for initialization
	void Awake() {

		if (GameObject.Find ("/KinectPrefab") == null) {
		
			Instantiate(prefabs).name = "KinectPrefab";
            
		}
		
	
	}
	
	// Update is called once per frame
	
}
