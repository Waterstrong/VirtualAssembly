using UnityEngine;
using System.Collections;

public class NUIElementPress : MonoBehaviour {

    GameObject audiCar;

    GameObject[] carComponents;

    Quaternion rawRotation;
    

	// Use this for initialization
    void Start()
    {
        audiCar = GameObject.Find("/Car/Car_Audi");
        carComponents = GameObject.FindGameObjectsWithTag("car");

        rawRotation = audiCar.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTestElementPress()
    {
        print("press");
    }

    /// <summary>
    /// 切换按钮
    /// </summary>
    public void OnSwitchPress()
    {
        foreach (GameObject gameobject in carComponents)
        {
            gameobject.SetActive(true);
        }
        SelfRotate sr = audiCar.GetComponent<SelfRotate>();
        sr.enabled = !sr.enabled;
    }

    /// <summary>
    /// 开始装配
    /// </summary>
    public void StartAssembly()
    {
        //audiCar.transform.rotation = rawRotation;

        foreach (GameObject gameobject in carComponents)
        {
            gameobject.SetActive(false);
        }
    }

    int index = 0;
    public void ShowCar()
    {
        if (index > 5)
        {
            return;
        }

        if (index == 1)
        {
            index++;
        }
        carComponents[index++].SetActive(true);
        
        if (index == 5)
        {
            foreach (GameObject gameobject in carComponents)
            {
                gameobject.SetActive(true);
            }
        }
    }
}
