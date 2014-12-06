using UnityEngine;
using System.Collections;

public class DisplayDepthMap : MonoBehaviour {

    private DepthWrapper dw;
    private SkeletonWrapper sw;
    public int player = 0;

    private readonly int DepthWidth = 320;
    private readonly int DepthHeight = 240;
    private readonly int DisplayWidth = 160;
    private readonly int DisplayHeight = 120;

    private readonly Color32 playerColor = Color.green;//new Color32(221, 179, 52, 150);

    // User Map vars.
    Texture2D depthMapTex; 
    Rect depthMapRect;

    // Use this for initialization
    void Start()
    {
        dw = GameObject.Find("/KinectPrefab").GetComponent<DepthWrapper>();
        sw = GameObject.Find("/KinectPrefab").GetComponent<SkeletonWrapper>();

        // Initialize depth map
        depthMapTex = new Texture2D(DepthWidth, DepthHeight); //depthMapTex = new Texture2D(DepthWidth, DepthHeight, TextureFormat.ARGB32, false);
        depthMapRect = new Rect((Screen.width + DisplayWidth) / 2,
                                10, //(Screen.height - DisplayHeight) / 2,
                                -DisplayWidth,
                                DisplayHeight);

        ClearTexture(depthMapTex);
    }

    // Update is called once per frame
    void Update()
    {
        if (sw.pollSkeleton())
        {
            if (sw.playerState[player] != -4)//Check which player state
            {
                if (dw.pollDepth())
                {
                    //depthMapTex.SetPixels32(convertDepthToColor(dw.depthImg));
                    depthMapTex.SetPixels32(convertPlayersToCutout(dw.segmentations, dw.depthImg));
                    depthMapTex.Apply();
                }
            }
            else
            {
                ClearTexture(depthMapTex);
            }
        }
    }
    // Draw the Histogram Map on the GUI.
    void OnGUI()
    {
        GUI.DrawTexture(depthMapRect, depthMapTex);
    }

    /// <summary>
    /// 清空Texture2D
    /// </summary>
    private void ClearTexture(Texture2D tex)
    {
        Color32[] img = new Color32[tex.width * tex.height];
        for (int i = 0; i < img.Length; i++)
        {
            img[i] = Color.clear;
        }
        tex.SetPixels32(img);
        tex.Apply();
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
            img[ind].a = playerColor.a;
            //players[0, pix] | players[1, pix] | players[2, pix] | players[3, pix] | players[4, pix] | players[5, pix]
            if (players[sw.trackedPlayers[player], pix])
            {
                img[ind] = playerColor; // Color.red  Color.green Color.blue  Color.magenta 
            }
        }
        return img;
    }

}
