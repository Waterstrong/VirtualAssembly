using UnityEngine;
using System.Collections;

public class NUIElementPress : MonoBehaviour {    
    public ButtonUICtrl switchButtonCtrl = null;
    public ButtonUICtrl assembleButtonCtrl = null;
    public ButtonUICtrl showButtonCtrl = null;
    public ButtonUICtrl studyButtonCtrl = null;

    public HandUIAdaptive handUIAdaptive = null;

    private bool isGestureCtrolling = true;
    public bool IsGestureCtrolling
    {
        get { return isGestureCtrolling; }
    }

    private bool isAssembling = false;

    GameObject audiCar;

    SelfRotate selfRotate = null;

    GameObject[] carComponents;

    GameObject component;

    Quaternion rawRotation;
    

	// Use this for initialization
    void Start()
    {
        audiCar = GameObject.Find("/Car/Car_Audi");

        selfRotate = audiCar.GetComponent<SelfRotate>();

        carComponents = GameObject.FindGameObjectsWithTag("car");

        component = GameObject.FindGameObjectWithTag("component");
        component.SetActive(isAssembling);

        rawRotation = audiCar.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Rotate(int rotate) {
        selfRotate.speed = rotate*Mathf.Abs(selfRotate.speed);
        selfRotate.enabled = true;
    }

    public void Zoom(int zoom) {
        selfRotate.Zoom(zoom);
    }

    public void TurnOver(int turn) {
        selfRotate.TurnOver(turn);
    }


    public void StopRotate() {
        selfRotate.enabled = false;
    }

    public void OnTestElementPress()
    {
        print("press");
    }

    public void OnCourseStudyPress() {
        print("study");
    }

    public void OnDemoShowPress() {
        print("Show");
    }

    /// <summary>
    /// 切换按钮
    /// </summary>
    public void OnSwitchInteractionPress()
    {
        isGestureCtrolling = !isGestureCtrolling;
        bool res = isGestureCtrolling ? switchButtonCtrl.ChangeCaption("切换为\n动 作") : switchButtonCtrl.ChangeCaption("切换为\n手 势");
        assembleButtonCtrl.ShowSelf(isGestureCtrolling);
        showButtonCtrl.ShowSelf(isGestureCtrolling);
        studyButtonCtrl.ShowSelf(isGestureCtrolling);
        handUIAdaptive.ShowSelf(isGestureCtrolling);

        //foreach (GameObject gameobject in carComponents)
        //{
        //    gameobject.SetActive(true);
        //}
        //SelfRotate sr = audiCar.GetComponent<SelfRotate>();
        //sr.enabled = !sr.enabled;
    }

    /// <summary>
    /// 开始装配
    /// </summary>
    public void OnCarAssemblePress()
    {
        //audiCar.transform.rotation = rawRotation;
        isAssembling = !isAssembling;
        bool res = isAssembling ? assembleButtonCtrl.ChangeCaption("还 原") : assembleButtonCtrl.ChangeCaption("装 配");

        foreach (GameObject gameobject in carComponents)
        {
            gameobject.SetActive(!isAssembling);
        }

        component.SetActive(isAssembling);

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
