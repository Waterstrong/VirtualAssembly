using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

//
//
public class KinectManager : MonoBehaviour
{
	// Public Bool to determine how many players there are. Default of one user.
	public bool TwoUsers = false;
	
	// Public Bool to determine if the sensor is used in near mode.
	public bool NearMode = false;

	// Public Bool to determine whether to display the color map too
	public bool DisplayColorMap = false;
	
	// Public Bool to determine whether to display user map after calibration.
	public bool UserMapAfterCalibration = false;
	
	/// How high off the ground is the sensor (in meters).
	public float SensorHeight = 0;

	// Kinect elevation angle (in degrees)
	public int KinectAngle = 0;
	
	// Bool to keep track of whether Kinect has been initialized
	bool KinectInitialized = false; 
	
	// Bools to keep track of who is currently calibrated.
	bool Player1Calibrated = false;
	bool Player2Calibrated = false;
	
	bool AllPlayersCalibrated = false;
	
	// Values to track which ID (assigned by the Kinect) is player 1 and player 2.
	uint Player1ID;
	uint Player2ID;
	
	// Lists of GameObjects that will be controlled by which player.
	public List<GameObject> Player1Avatars;
	public List<GameObject> Player2Avatars;
	
	// Lists of AvatarControllers that will let the models get updated.
	List<AvatarController> Player1Controllers;
	List<AvatarController> Player2Controllers;
	
	// User Map vars.
	Texture2D usersLblTex;
	Color[] usersMapColors;
	Rect usersMapRect;
	int usersMapSize;

	Texture2D usersClrTex;
	//Color[] usersClrColors;
	Rect usersClrRect;
	
	//short[] usersLabelMap;
	short[] usersDepthMap;
	float[] usersHistogramMap;
	
	// List of all users
	List<uint> allUsers;
	
	// GUI Text to show messages.
	GameObject CalibrationText;
	
	// Image stream handles for the kinect
	private IntPtr colorStreamHandle;
	private IntPtr depthStreamHandle;
	
	// Color image data, if used
	private Color32[] colorImage;
	
	// Skeleton related structures
	private KinectWrapper.NuiSkeletonFrame skeletonFrame;
	private KinectWrapper.NuiTransformSmoothParameters smoothParameters;
	
	// Skeleton tracking states, positions and joints' orientations
	private Vector3 player1Tracked, player2Tracked;
	private Vector3 player1Pos, player2Pos;
	private Matrix4x4 player1Ori, player2Ori;
	private bool[] player1JointsTracked, player2JointsTracked;
	private Vector3[] player1JointsPos, player2JointsPos;
	private Matrix4x4[] player1JointsOri, player2JointsOri;
	private KinectWrapper.NuiSkeletonBoneOrientation[] jointOrientations;
	
	private Matrix4x4 kinectToWorld;
	//Quaternion quatToWorld;
	
	private static KinectManager instance;
	
	
    public static KinectManager Instance
    {
        get
        {
            return instance;
        }
    }
	
	public static bool IsKinectInitialized()
	{
		return instance != null ? instance.KinectInitialized : false;
	}
	
	public static bool IsCalibrationNeeded()
	{
		return false; // Waterstrong
	}
	
	public uint GetPlayer1ID()
	{
		return Player1ID;
	}
	
	public uint GetPlayer2ID()
	{
		return Player2ID;
	}
	
	public Vector3 GetUserPosition(uint UserId)
	{
		if(UserId == Player1ID)
			return player1Pos;
		else if(UserId == Player2ID)
			return player2Pos;
		
		return Vector3.zero;
	}
	
	public Quaternion GetUserOrientation(uint UserId, bool flip)
	{
		if(UserId == Player1ID)
			return ConvertMatrixToQuat(player1Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
		else if(UserId == Player2ID)
			return ConvertMatrixToQuat(player2Ori, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
		
		return Quaternion.identity;
	}
	
	public bool IsJointTracked(uint UserId, int joint)
	{
		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsTracked.Length ? player1JointsTracked[joint] : false;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsTracked.Length ? player2JointsTracked[joint] : false;
		
		return false;
	}
	
	public Vector3 GetJointPosition(uint UserId, int joint)
	{
		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsPos.Length ? player1JointsPos[joint] : Vector3.zero;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsPos.Length ? player2JointsPos[joint] : Vector3.zero;
		
		return Vector3.zero;
	}
	
	public Quaternion GetJointOrientation(uint UserId, int joint, bool flip)
	{
		if(UserId == Player1ID)
			return joint >= 0 && joint < player1JointsOri.Length ? 
				ConvertMatrixToQuat(player1JointsOri[joint], joint, flip) : Quaternion.identity;
		else if(UserId == Player2ID)
			return joint >= 0 && joint < player2JointsOri.Length ? 
				ConvertMatrixToQuat(player2JointsOri[joint], joint, flip) : Quaternion.identity;
		
		return Quaternion.identity;
	}
	
	void Start()
	{
		int hr = 0;
		
		try
		{
			hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex | 
				KinectWrapper.NuiInitializeFlags.UsesSkeleton | 
				KinectWrapper.NuiInitializeFlags.UsesColor);
            if (hr != 0)
			{
            	throw new Exception("NuiInitialize Failed.");
			}
			
			hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);  // 0, 12,8
			if (hr != 0)
			{
				throw new Exception("Cannot initialize Skeleton Data.");
			}
			
			depthStreamHandle = IntPtr.Zero;
			hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.DepthAndPlayerIndex, 
				KinectWrapper.Constants.ImageResolution, 0, 2, IntPtr.Zero, ref depthStreamHandle);
//			if (hr != 0)
//			{
//				throw new Exception("Cannot open depth stream.");
//			}
			
			colorStreamHandle = IntPtr.Zero;
			hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color, 
				KinectWrapper.Constants.ImageResolution, 0, 2, IntPtr.Zero, ref colorStreamHandle);
//			if (hr != 0)
//			{
//				throw new Exception("Cannot open color stream.");
//			}

			// set kinect elevation angle
			KinectWrapper.NuiCameraSetAngle((long)KinectAngle);
			
			// init skeleton structures
			skeletonFrame = new KinectWrapper.NuiSkeletonFrame() 
							{ 
								SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount] 
							};
			
			// values used to pass to smoothing function
			smoothParameters = new KinectWrapper.NuiTransformSmoothParameters();
			smoothParameters.fSmoothing = 0.5f;
			smoothParameters.fCorrection = 0.5f;
			smoothParameters.fJitterRadius = 0.05f;
			smoothParameters.fMaxDeviationRadius = 0.04f;
			smoothParameters.fPrediction = 0.5f;
			
			// create arrays for joint positions and joint orientations
			int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
			player1JointsTracked = new bool[skeletonJointsCount];
			player2JointsTracked = new bool[skeletonJointsCount];
			
			player1JointsPos = new Vector3[skeletonJointsCount];
			player2JointsPos = new Vector3[skeletonJointsCount];
			
			player1JointsOri = new Matrix4x4[skeletonJointsCount];
			player2JointsOri = new Matrix4x4[skeletonJointsCount];
			
			//create the transform matrix that converts from kinect-space to world-space
			Quaternion quat = new Quaternion();
			quat.eulerAngles = new Vector3(-KinectAngle, 0.0f, 0.0f);
			
			// transform matrix - kinect to world
			kinectToWorld.SetTRS(new Vector3(0.0f, SensorHeight, 0.0f), quat, Vector3.one);
			//quatToWorld = Quaternion.LookRotation(kinectToWorld.GetColumn(2), kinectToWorld.GetColumn(1));
			
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		catch (Exception e)
		{
			Debug.Log(e.Message + ", hr=" + hr.ToString());
			return;
		}

        // Initialize depth & label map related stuff
        usersMapSize = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
        usersLblTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
        usersMapColors = new Color[usersMapSize];
		
		/*
        usersMapRect = new Rect(Screen.width - usersLblTex.width / 2, 
			                    Screen.height - usersLblTex.height / 2, 
			                    -usersLblTex.width / 2, 
			                    usersLblTex.height / 2);
			                    */
		
		usersMapRect = new Rect(Screen.width - 10, 
			                    Screen.height - 240 - 10, 
			                    -320, 
			                    240);
		
		if(DisplayColorMap)
		{
	        usersClrTex = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
	        //usersClrColors = new Color[usersMapSize];
	        usersClrRect = new Rect(Screen.width  /**- usersLblTex.width / 2*/ /**- usersClrTex.width / 2*/, Screen.height - usersClrTex.height / 2, -usersClrTex.width / 2, usersClrTex.height / 2);
			usersMapRect.x -= usersLblTex.width / 2;
			
			colorImage = new Color32[usersMapSize];
		}
		
        usersDepthMap = new short[usersMapSize];
        usersHistogramMap = new float[5000];
		
        // Initialize user list to contain ALL users.
        allUsers = new List<uint>();
        
		// Pull the AvatarController from each of the players Avatars.
		Player1Controllers = new List<AvatarController>();
		Player2Controllers = new List<AvatarController>();
		
		// Add each of the avatars' controllers into a list for each player.
		foreach(GameObject avatar in Player1Avatars)
        {
            print(avatar.GetComponent<AvatarController>());
            Player1Controllers.Add(avatar.GetComponent<AvatarController>());
		}
		
		foreach(GameObject avatar in Player2Avatars)
		{
			Player2Controllers.Add(avatar.GetComponent<AvatarController>());
		}
		
		// GUI Text.
		CalibrationText = GameObject.Find("CalibrationText");
		if(CalibrationText != null)
		{
			CalibrationText.guiText.text = "WAITING FOR USERS";
		}
		
		Debug.Log("Waiting for users.");
			
		KinectInitialized = true;
	}
	
	void Update()
	{
		if(KinectInitialized)
		{
	        // If the players aren't all calibrated yet, draw the user map.
			if(!AllPlayersCalibrated || UserMapAfterCalibration)
			{
				if(depthStreamHandle != IntPtr.Zero &&
					KinectWrapper.PollDepth(depthStreamHandle, NearMode, ref usersDepthMap))
				{
		        	UpdateUserMap();
				}
				
				if(colorStreamHandle != IntPtr.Zero &&
					DisplayColorMap && KinectWrapper.PollColor(colorStreamHandle, ref colorImage))
				{
					UpdateColorMap();
				}
			}
			
			if(KinectWrapper.PollSkeleton(smoothParameters, ref skeletonFrame))
			{
				ProcessSkeleton();
			}
			
			// Update player 1's models if he/she is calibrated and the model is active.
			if(Player1Calibrated)
			{
				foreach (AvatarController controller in Player1Controllers)
				{
					if(controller.Active)
					{
						controller.UpdateAvatar(Player1ID, NearMode);
					}
				}
			}
			
			// Update player 2's models if he/she is calibrated and the model is active.
			if(Player2Calibrated)
			{
				foreach (AvatarController controller in Player2Controllers)
				{
					if(controller.Active)
					{
						controller.UpdateAvatar(Player2ID, NearMode);
					}
				}
			}
		}
		
		// Kill the program with ESC.
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
	
	// Make sure to kill the Kinect on quitting.
	void OnApplicationQuit()
	{
		if(KinectInitialized)
		{
			// Shutdown OpenNI
			KinectWrapper.NuiShutdown();
			instance = null;
		}
	}
	
	// Draw the Histogram Map on the GUI.
    void OnGUI()
    {
		if(KinectInitialized)
		{
	        if (!AllPlayersCalibrated || UserMapAfterCalibration)
	        {
	            GUI.DrawTexture(usersMapRect, usersLblTex);
				
				if(DisplayColorMap)
				{
					GUI.DrawTexture(usersClrRect, usersClrTex);
				}
	        }
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
            if ((usersDepthMap[i] & 7) != 0)
            {
                usersHistogramMap[usersDepthMap[i] >> 3]++;
                numOfPoints++;
            }
        }
		
        if (numOfPoints > 0)
        {
            for (i = 1; i < usersHistogramMap.Length; i++)
	        {   
		        usersHistogramMap[i] += usersHistogramMap[i-1];
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
			
			short userMap = (short)(usersDepthMap[i] & 7);
			short userDepth = (short)(usersDepthMap[i] >> 3);
			
            if (userMap == 0)
            {
                usersMapColors[flipIndex] = Color.clear;
            }
            else
            {
                // Create a blending color based on the depth histogram
				float histDepth = usersHistogramMap[userDepth];
                Color c = new Color(histDepth, histDepth, histDepth, 0.9f);
                
				switch(userMap % 4)
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
	
	// Update the Color Map
	void UpdateColorMap()
	{
        usersClrTex.SetPixels32(colorImage);
        usersClrTex.Apply();
	}
	
	// Assign UserId to player 1 or 2.
    void CalibrateUser(uint UserId)
    {
		// If player 1 hasn't been calibrated, assign that UserID to it.
		if(!Player1Calibrated)
		{
			// Check to make sure we don't accidentally assign player 2 to player 1.
			if (UserId != Player2ID)
			{
				Player1Calibrated = true;
				Player1ID = UserId;
				
				allUsers.Add(UserId);
				
                //Waterstrong Show1
                foreach (GameObject avatar in Player1Avatars)
                {
                    avatar.SetActive(true);
                }

				foreach(AvatarController controller in Player1Controllers)
				{
					controller.SuccessfulCalibration();
				}
				
				// If we're not using 2 users, we're all calibrated.
				if(!TwoUsers)
				{
					AllPlayersCalibrated = true;
				}
			}
		}
		// Otherwise, assign to player 2.
		else
		{
			if (UserId != Player1ID)
			{
				Player2Calibrated = true;
				Player2ID = UserId;
				
				allUsers.Add(UserId);
                //Waterstrong show2
                foreach (GameObject avatar in Player2Avatars)
                {
                    avatar.SetActive(true);
                }
				foreach(AvatarController controller in Player2Controllers)
				{
					controller.SuccessfulCalibration();
				}
				
				// All users are calibrated!
				AllPlayersCalibrated = true;
			}
		}
		
		// If all users are calibrated, stop trying to find them.
		if(AllPlayersCalibrated)
		{
            Debug.Log("AllPlayersCalibrated"); // Waterstrong
			
			if(CalibrationText != null)
			{
				CalibrationText.guiText.text = "";
			}
		}
    }
	
	// Remove a lost UserId
	void RemoveUser(uint UserId)
	{
		// If we lose player 1...
		if(UserId == Player1ID)
		{
			// Null out the ID and reset all the models associated with that ID.
			Player1ID = 0;
			Player1Calibrated = false;
            // Waterstrong Add reset player1 code ----disappear1
            foreach (GameObject avatar in Player1Avatars)
            {
                avatar.SetActive(false);
            }
            
			if(IsCalibrationNeeded())
			{
				foreach(AvatarController controller in Player1Controllers)
				{
					controller.RotateToCalibrationPose();
                    Debug.Log("controller.RotateToCalibrationPose();"); // Waterstrong
				}
			}
		}
		
		// If we lose player 2...
		if(UserId == Player2ID)
		{
			// Null out the ID and reset all the models associated with that ID.
			Player2ID = 0;
			Player2Calibrated = false;

            // Waterstrong Add reset player2 code ----disappear2
            foreach (GameObject avatar in Player2Avatars)
            {
                avatar.SetActive(false);
            }

			if(IsCalibrationNeeded())
			{
				foreach(AvatarController controller in Player2Controllers)
				{
					controller.RotateToCalibrationPose();
				}
			}
		}

        // remove from global users list
        allUsers.Remove(UserId);
		
		if(CalibrationText != null)
		{
			CalibrationText.guiText.text = "WAITING FOR USERS";
		}
		
		// Try to replace that user!
		Debug.Log("Waiting for users.");

		AllPlayersCalibrated = false;
	}
	
	// Process the skeleton data
	void ProcessSkeleton()
	{
		List<uint> lostUsers = new List<uint>();
		lostUsers.AddRange(allUsers);
		
		for (int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
		{
			KinectWrapper.NuiSkeletonData skeletonData = skeletonFrame.SkeletonData[i];
			uint userId = skeletonData.dwTrackingID;
			
			if (skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
			{
				if(!AllPlayersCalibrated)
				{
					CalibrateUser(userId);
				}

				//int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked; // Waterstrong
                int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
				
				if(userId == Player1ID)
				{
					// get player position
					player1Pos = kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
					player1Pos.z = -player1Pos.z;
					
					//Debug.Log(String.Format("({0}, {1}, {2})", player1Pos.x, player1Pos.y, player1Pos.z));
					//CalibrationText.guiText.text = String.Format("({0:F1}, {1:F1}, {2:F1})", player1Pos.x, player1Pos.y, player1Pos.z);
					
					// get joints rotation
					for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
					{
						//player1JointsTracked[j] = (int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked; // Waterstrong
                        player1JointsTracked[j] = (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked;
						
						if(player1JointsTracked[j])
						{
							player1JointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
							player1JointsPos[j].z = -player1JointsPos[j].z;
						}
					}
					
					// get joint orientations
					KinectWrapper.GetSkeletonJointOrientation(ref player1JointsPos, ref player1JointsTracked, ref player1JointsOri);
					
					// get player rotation
					player1Ori = player1JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];
				}
				else if(userId == Player2ID)
				{ 
					// get player position
					player2Pos = kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
					player2Pos.z = -player2Pos.z;
					
					// get joints rotation
					for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
					{
						//player2JointsTracked[j] = (int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked; // Waterstrong
                        player2JointsTracked[j] = (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked;
						if(player2JointsTracked[j])
						{
							player2JointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
							player2JointsPos[j].z = -player2JointsPos[j].z;
						}
					}
					
					// get joint orientations
					KinectWrapper.GetSkeletonJointOrientation(ref player2JointsPos, ref player2JointsTracked, ref player2JointsOri);
					
					// get player rotation
					player2Ori = player2JointsOri[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];
				}
				
				lostUsers.Remove(userId);
			}
		}
		
		// remove the lost users if any
		if(lostUsers.Count > 0)
		{
			foreach(uint userId in lostUsers)
			{
				RemoveUser(userId);
			}
			
			lostUsers.Clear();
		}
	}
	
	// convert the matrix to quaternion, taking care of the mirroring
	private Quaternion ConvertMatrixToQuat(Matrix4x4 mOrient, int joint, bool flip)
	{
		Vector4 vZ = mOrient.GetColumn(2);
		Vector4 vY = mOrient.GetColumn(1);

		if(!flip)
		{
			vZ.y = -vZ.y;
			vY.x = -vY.x;
			vY.z = -vY.z;
		}
		else
		{
			vZ.x = -vZ.x;
			vZ.y = -vZ.y;
			vY.z = -vY.z;
		}
		
		return Quaternion.LookRotation(vZ, vY);
	}
	
}


