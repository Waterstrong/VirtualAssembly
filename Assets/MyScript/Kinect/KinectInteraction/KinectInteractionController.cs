using UnityEngine;
using System.Collections;

using NUI.Motion;

using JointType = Kinect.NuiSkeletonPositionIndex;

public class KinectInteractionController : MonoBehaviour{

    private SkeletonWrapper sw;

    public int player = 0; // which player controlling

    public PromptUICtrl promptUiCtrl = null;

    public HandUIAdaptive handUiAdaptive = null; // 

    MotionSuper motion = null;

    private bool handStatus = false; // get the adaptive result
    private HandWave handWave = new HandWave();

    GameObject audiCar = null;
    
    // Use this for initialization
    void Start()
    {
        //handCtr = gameObject.GetComponent<HandCtrl>();

        sw = GameObject.Find("/KinectPrefab").GetComponent<SkeletonWrapper>();

        audiCar = GameObject.Find("/Car/Car_Audi");

        motion = MotionFactory.CreateHandleChain();

    }
    

    // Update is called once per frame
	void Update() {
    
        //update the data from the kinect if necessary
        if (sw.pollSkeleton())
		{
            switch (sw.playerState[player]) //Check which player state
            {
                case 0: // Tracking is ok
                    {
                        if (promptUiCtrl != null) promptUiCtrl.Hide();

                        handStatus = handUiAdaptive.Adaptive(sw, player);

                        FeatureData feature = new FeatureData();
                        feature.SetRelativeJoints(sw.rawBonePos, player);

                        MotionType mt = motion.HandleDataEx(feature);
                        SelfRotate sr = null;
                        switch (mt)
                        {
                            case MotionType.None:
                                break;
                            case MotionType.Jump: // 起跳
                                Debug.Log("Jump");
                                break;
                            case MotionType.HandLeftSlide: // 左滑动
                                Debug.Log("LeftSlide");
                                sr = audiCar.GetComponent<SelfRotate>();
                                sr.speed = Mathf.Abs(sr.speed);
                                sr.enabled = true;
                                break;
                            case MotionType.HandRightSlide: // 右滑动
                                Debug.Log("RightSlide");
                                sr = audiCar.GetComponent<SelfRotate>();
                                sr.speed = -Mathf.Abs(sr.speed);
                                sr.enabled = true;
                                break;
                            case MotionType.HandsUp: // 举手
                                Debug.Log("HandsUp");
                                break;
                            case MotionType.HandsMiddle: // 双手水平展开
                                Debug.Log("HandsMiddle");
                                break;
                            case MotionType.HandsFold: // 双手水平收缩
                                Debug.Log("HandsFold");
                                break;
                            case MotionType.HandLeftUp: // 左手向上
                            case MotionType.HandRightUp: // 右手向上
                                Debug.Log("Hand L&R Up");
                                sr = audiCar.GetComponent<SelfRotate>();
                                sr.enabled = false;
                                break;
                            case MotionType.HandLeftCircle: // 左手划圆
                                Debug.Log("HandLeftCircle");
                                break;
                            case MotionType.HandRightCircle: // 右手划圆
                                Debug.Log("HandRightCircle");
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case -1: // too right
                    if (promptUiCtrl != null) promptUiCtrl.Show("Too Right");
                    //gameRunUi.ShowMessage("请左移");
                    break;
                case 1: // too left
                    if (promptUiCtrl != null) promptUiCtrl.Show("Too Left");
                    //gameRunUi.ShowMessage("请右移");
                    break;
                case -3: // too near
                    if (promptUiCtrl != null) promptUiCtrl.Show("Too Near");
                    //gameRunUi.ShowMessage("请后退");
                    break;
                case 3: // too far
                    if (promptUiCtrl != null) promptUiCtrl.Show("Too Far");
                    //gameRunUi.ShowMessage("请靠近");
                    break;
                case -4: // no user, waiting
                    if (promptUiCtrl != null) promptUiCtrl.Show("No User", false);
                    //gameRunUi.HideMessagePanel();
                    return;
                default: break;
            }

            if (!(sw.playerState[player] == 0 && handStatus) && handWave.Recognise(sw, 1-player))// wave to swap player to the other one
            {
                sw.trackedPlayers[player] = sw.trackedPlayers[1 - player];
                sw.trackedPlayers[1 - player] = -1;
            }

        }
    }

}
