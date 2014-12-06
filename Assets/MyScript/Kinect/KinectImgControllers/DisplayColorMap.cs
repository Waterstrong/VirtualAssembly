using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

public class DisplayColorMap : MonoBehaviour {

    private Kinect.KinectInterface kinect;

    Texture2D usersClrTex;
    Rect usersClrRect;
    
    private readonly int ColorWidth = 640;
    private readonly int ColorHeight = 480;

    private readonly int DisplayWidth = 320;
    private readonly int DisplayHeight = 240;

    // Use this for initialization
    void Start()
    {
        kinect = GameObject.Find("/KinectPrefab").GetComponent<DeviceOrEmulator>().getKinect();

        usersClrTex = new Texture2D(ColorWidth, ColorHeight); // new Texture2D(ColorWidth , ColorHeight, TextureFormat.ARGB32, false); //

        //usersClrRect = new Rect(Screen.width, Screen.height - usersClrTex.height / 2, -usersClrTex.width / 2, usersClrTex.height / 2);

        usersClrRect = new Rect((Screen.width + DisplayWidth) / 2,
                                (Screen.height - DisplayHeight) / 2,
                                -DisplayWidth,
                                DisplayHeight);
    }


    // Update is called once per frame
    void Update()
    {
        if (kinect.pollColor())
        {
            usersClrTex.SetPixels32(kinect.getColor()); 
            usersClrTex.Apply();
        }
    }
    
    // Draw  GUI.
    void OnGUI()
    {
        GUI.DrawTexture(usersClrRect, usersClrTex);
    }
    
    
    //mipmapImg(kinect.getColor(), ColorWidth, ColorHeight)

    //private Color32[] mipmapImg(Color32[] src, int width, int height)
    //{
    //    int newWidth = width / 2;
    //    int newHeight = height / 2;
    //    Color32[] dst = new Color32[newWidth * newHeight];
    //    for (int yy = 0; yy < newHeight; yy++)
    //    {
    //        for (int xx = 0; xx < newWidth; xx++)
    //        {
    //            int TLidx = (xx * 2) + yy * 2 * width;
    //            int TRidx = (xx * 2 + 1) + yy * width * 2;
    //            int BLidx = (xx * 2) + (yy * 2 + 1) * width;
    //            int BRidx = (xx * 2 + 1) + (yy * 2 + 1) * width;
    //            dst[xx + yy * newWidth] = Color32.Lerp(Color32.Lerp(src[BLidx], src[BRidx], .5F),
    //                                                   Color32.Lerp(src[TLidx], src[TRidx], .5F), .5F);
    //        }
    //    }
    //    return dst;
    //}

}
