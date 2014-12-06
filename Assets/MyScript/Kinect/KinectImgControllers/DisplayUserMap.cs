using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

public class DisplayUserMap : MonoBehaviour {

    // User Map vars.
    Texture2D usersLblTex; // 只保留人的深度数据
    Color[] usersMapColors;
    Rect usersMapRect;
    int usersMapSize;

    float[] usersHistogramMap;

    //private DeviceOrEmulator devOrEmu;

    private Kinect.KinectInterface kinect;

    private readonly int DepthWidth = 320;
    private readonly int DepthHeight = 240;
    private readonly int DisplayWidth = 320;
    private readonly int DisplayHeight = 240;

	// Use this for initialization
	void Start () {
        kinect = GameObject.Find("/KinectPrefab").GetComponent<DeviceOrEmulator>().getKinect();

        //kinect = devOrEmu.getKinect();

        // Initialize depth & label map related stuff
        usersMapSize = DepthWidth * DepthHeight;
        usersLblTex = new Texture2D(DepthWidth, DepthHeight);
        usersMapRect = new Rect((Screen.width + DisplayWidth) / 2,
                                10,
                                -DisplayWidth,
                                DisplayHeight);

        usersMapColors = new Color[usersMapSize];
        usersHistogramMap = new float[5000];
	}
	
	// Update is called once per frame
	void Update () {
        if (kinect.pollDepth())
        {
            UpdateUserMap();
        }
	
	}

    // Update the User Map
    void UpdateUserMap()
    {
        // Flip the texture as we convert label map to color array
        int flipIndex, i;
        int numOfPoints = 0;
        Array.Clear(usersHistogramMap, 0, usersHistogramMap.Length);

        // Calculate cumulative histogram for depth
        for (i = 0; i < usersMapSize; i++)
        {
            // Only calculate for depth that contains users
            if ((kinect.getDepth()[i] & 7) != 0)
            {
                usersHistogramMap[kinect.getDepth()[i] >> 3]++;
                numOfPoints++;
            }
        }

        if (numOfPoints > 0)
        {
            for (i = 1; i < usersHistogramMap.Length; i++)
            {
                usersHistogramMap[i] += usersHistogramMap[i - 1];
            }

            for (i = 0; i < usersHistogramMap.Length; i++)
            {
                usersHistogramMap[i] = 1.0f - (usersHistogramMap[i] / numOfPoints);
            }
        }

        // Create the actual users texture based on label map and depth histogram
        for (i = 0; i < usersMapSize; i++)
        {
            flipIndex = usersMapSize - i - 1;

            short userMap = (short)(kinect.getDepth()[i] & 7);
            short userDepth = (short)(kinect.getDepth()[i] >> 3);

            if (userMap == 0)
            {
                usersMapColors[flipIndex] = Color.clear;
            }
            else
            {
                // Create a blending color based on the depth histogram
                float histDepth = usersHistogramMap[userDepth];
                Color c = new Color(histDepth, histDepth, histDepth, 0.9f);

                switch (userMap % 4)
                {
                    case 0:
                        usersMapColors[flipIndex] = Color.red * c;
                        break;
                    case 1:
                        usersMapColors[flipIndex] = Color.green * c;
                        break;
                    case 2:
                        usersMapColors[flipIndex] = Color.blue * c;
                        break;
                    case 3:
                        usersMapColors[flipIndex] = Color.magenta * c;
                        break;
                }
            }
        }

        // Draw it!
        usersLblTex.SetPixels(usersMapColors);
        usersLblTex.Apply();
    }

    // Draw the Histogram Map on the GUI.
    void OnGUI()
    {
        GUI.DrawTexture(usersMapRect, usersLblTex);
    }
}
