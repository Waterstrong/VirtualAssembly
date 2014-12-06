using UnityEngine;
using System.Collections;

using JointType = Kinect.NuiSkeletonPositionIndex;
using System;

public class DisplayDepthHand : MonoBehaviour
{

    private DepthWrapper dw;
    private SkeletonWrapper sw;
    public GUIText gui_text;
    public int player = 0; // which player controlling

    private readonly int DepthWidth = 320;
    private readonly int DepthHeight = 240;
    private readonly int DisplayWidth = 640;
    private readonly int DisplayHeight = 480;

    // User Map vars.
    Texture2D depthMapTex;
    Rect depthMapRect;

    // Use this for initialization
    void Start()
    {
        sw = GameObject.Find("/KinectPrefab").GetComponent<SkeletonWrapper>();
        dw = GameObject.Find("/KinectPrefab").GetComponent<DepthWrapper>();

        // Initialize depth map
        depthMapTex = new Texture2D(DepthWidth, DepthHeight); //depthMapTex = new Texture2D(DepthWidth, DepthHeight, TextureFormat.ARGB32, false);
        depthMapRect = new Rect((Screen.width + DisplayWidth) / 2,
                                (Screen.height - DisplayHeight) / 2, //10, //
                                -DisplayWidth,
                                DisplayHeight);
    }

    // Update is called once per frame
    void Update()
    {
        if (sw.pollSkeleton())
        {
            switch (sw.playerState[player]) //Check which player state
            {
                case 0: // Tracking is ok
                    if (dw.pollDepth())
                    {
                        //depthMapTex.SetPixels32(convertDepthToColor(dw.depthImg));
                        depthMapTex.SetPixels32(convertPlayersToCutout(dw.segmentations, dw.depthImg));
                        depthMapTex.Apply();
                    }
                    break;
                case -1: // too right
                    //gameRunUi.ShowMessage("请左移");
                    break;
                case 1: // too left
                    //gameRunUi.ShowMessage("请右移");
                    break;
                case -3: // too near
                    //gameRunUi.ShowMessage("请后退");
                    break;
                case 3: // too far
                    //gameRunUi.ShowMessage("请靠近");
                    break;
                case -4: // no user, waiting
                    //gameRunUi.HideMessagePanel();
                    break;
                default: break;
            }
        }

        
    }
    // Draw the Histogram Map on the GUI.
    void OnGUI()
    {
        GUI.DrawTexture(depthMapRect, depthMapTex);
    }


    private Color32[] convertDepthToColor(short[] depthBuf)
    {
        Color32[] img = new Color32[depthBuf.Length];
        for (int pix = 0; pix < depthBuf.Length; pix++)
        {
            int ind = depthBuf.Length - pix - 1;
            img[ind].r = (byte)(depthBuf[pix] / 32);
            img[ind].g = (byte)(depthBuf[pix] / 32);
            img[ind].b = (byte)(depthBuf[pix] / 32);
            img[ind].a = (byte)255; // don't forget
        }
        return img;
    }

    private Color32[] convertPlayersToCutout(bool[,] players, short[] depthBuf)
    {
        Color32[] img = new Color32[depthBuf.Length];
        for (int pix = 0; pix < depthBuf.Length; pix++)
        {
            int ind = depthBuf.Length - pix - 1;
            
            img[ind].r = (byte)(depthBuf[pix] / 32);
            img[ind].g = (byte)(depthBuf[pix] / 32);
            img[ind].b = (byte)(depthBuf[pix] / 32);
            img[ind].a = (byte)0;


            if (players[sw.trackedPlayers[player], pix]) // players[0, pix] | players[1, pix] | players[2, pix] | players[3, pix] | players[4, pix] | players[5, pix]
            {
                int diff = depthBuf[pix] - (short)(sw.rawBonePos[player, (int)JointType.WristRight].z * 1000);
                //if (gui_text != null) gui_text.text = diff.ToString();
                //  gui_text.text = (sw.rawBonePos[player, (int)JointType.WristRight].z * 1000).ToString();//
                //if (diff <= 0)
                {
                    img[ind] = Color.white;//Color.magenta; // Color.red  Color.green Color.blue  Color.magenta
                }
               // img[ind] = Color.green;
                //img[ind].a = (byte)255;
            }
            //else
            //{
            //    img[ind].a = (byte)0;
            //}
        }
        return img;
    }

}
