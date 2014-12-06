using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text; 

// Wrapper class that holds the various structs, vand dll imports
// needed to set up a model with the Kinect.
public class KinectWrapper : MonoBehaviour
{
	// Kinect-given Variables to keep track of the skeleton's joints.
	public enum SkeletonJoint
	{ 
		//NONE = 0,
		HEAD = NuiSkeletonPositionIndex.Head,
        NECK = NuiSkeletonPositionIndex.ShoulderCenter, 
        SPINE = NuiSkeletonPositionIndex.Spine,  // TORSO_CENTER
		HIPS = NuiSkeletonPositionIndex.HipCenter,  // WAIST

		LEFT_COLLAR = -1,
		LEFT_SHOULDER = NuiSkeletonPositionIndex.ShoulderLeft,
        LEFT_ELBOW = NuiSkeletonPositionIndex.ElbowLeft,
        LEFT_WRIST = NuiSkeletonPositionIndex.WristLeft,
        LEFT_HAND = NuiSkeletonPositionIndex.HandLeft,
        LEFT_FINGERTIP = -1,

        RIGHT_COLLAR = -1,
		RIGHT_SHOULDER = NuiSkeletonPositionIndex.ShoulderRight,
		RIGHT_ELBOW = NuiSkeletonPositionIndex.ElbowRight,
		RIGHT_WRIST = NuiSkeletonPositionIndex.WristRight,
		RIGHT_HAND = NuiSkeletonPositionIndex.HandRight,
        RIGHT_FINGERTIP = -1,

        LEFT_HIP = NuiSkeletonPositionIndex.HipLeft,
        LEFT_KNEE = NuiSkeletonPositionIndex.KneeLeft,
        LEFT_ANKLE = NuiSkeletonPositionIndex.AnkleLeft,
        LEFT_FOOT = NuiSkeletonPositionIndex.FootLeft,

        RIGHT_HIP = NuiSkeletonPositionIndex.HipRight,
		RIGHT_KNEE = NuiSkeletonPositionIndex.KneeRight,
        RIGHT_ANKLE = NuiSkeletonPositionIndex.AnkleRight,
		RIGHT_FOOT = NuiSkeletonPositionIndex.FootRight,
		
		END 
	};
	
	public static class Constants
	{
		public static int NuiSkeletonCount = 6;
    	public static int NuiSkeletonMaxTracked = 2;
    	public static int NuiSkeletonInvalidTrackingID = 0;
		
		public static float NuiDepthHorizontalFOV = 58.5f;
		public static float NuiDepthVerticalFOV = 45.6f;
		
		public static int ImageWidth = 640;
		public static int ImageHeight = 480;
		public static NuiImageResolution ImageResolution = NuiImageResolution.resolution640x480;
	}
	
	/// <summary>
	///Structs and constants for interfacing C# with the Kinect.dll 
	/// </summary>

    [Flags]
    public enum NuiInitializeFlags : uint
    {
        UsesDepthAndPlayerIndex = 0x00000001,
        UsesColor = 0x00000002,
        UsesSkeleton = 0x00000008,
        UsesDepth = 0x00000020
    }

    public enum NuiSkeletonPositionIndex : int
    {
        HipCenter = 0,
        Spine,
        ShoulderCenter,
        Head,
        ShoulderLeft,
        ElbowLeft,
        WristLeft,
        HandLeft,
        ShoulderRight,
        ElbowRight,
        WristRight,
        HandRight,
        HipLeft,
        KneeLeft,
        AnkleLeft,
        FootLeft,
        HipRight,
        KneeRight,
        AnkleRight,
        FootRight,
        Count
    }

    public enum NuiSkeletonPositionTrackingState
    {
        NotTracked = 0,
        Inferred,
        Tracked
    }

    public enum NuiSkeletonTrackingState
    {
        NotTracked = 0,
        PositionOnly,
        SkeletonTracked
    }
	
	public enum NuiImageType
	{
		DepthAndPlayerIndex = 0,	// USHORT
		Color,						// RGB32 data
		ColorYUV,					// YUY2 stream from camera h/w, but converted to RGB32 before user getting it.
		ColorRawYUV,				// YUY2 stream from camera h/w.
		Depth						// USHORT
	}
	
	public enum NuiImageResolution
	{
		resolutionInvalid = -1,
		resolution80x60 = 0,
		resolution320x240,
		resolution640x480,
		resolution1280x1024                        // for hires color only
	}
	
	//KK addition - flags for image stream in order to enable near mode
	public enum NuiImageStreamFlags
	{
		None = 0x00000000,
		SupressNoFrameData = 0x0001000,
		EnableNearMode = 0x00020000,
		TooFarIsNonZero = 0x0004000
	}
	// end of KK addition
	
	
    /*public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }*/

    public struct NuiSkeletonData
    {
        public NuiSkeletonTrackingState eTrackingState;
        public uint dwTrackingID;
        public uint dwEnrollmentIndex_NotUsed;
        public uint dwUserIndex;
        public Vector4 Position;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public Vector4[] SkeletonPositions;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonPositionTrackingState[] eSkeletonPositionTrackingState;
        public uint dwQualityFlags;
    }
	
    public struct NuiSkeletonFrame
    {
        public Int64 liTimeStamp;
        public uint dwFrameNumber;
        public uint dwFlags;
        public Vector4 vFloorClipPlane;
        public Vector4 vNormalToGravity;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonData[] SkeletonData;
    }
	
	public struct NuiTransformSmoothParameters
	{
		public float fSmoothing;
		public float fCorrection;
		public float fPrediction;
		public float fJitterRadius;
		public float fMaxDeviationRadius;
	}
	
    public struct NuiSkeletonBoneRotation
    {
        public Matrix4x4 rotationMatrix;
        public Quaternion rotationQuaternion;
    }

    public struct NuiSkeletonBoneOrientation
    {
        public NuiSkeletonPositionIndex endJoint;
        public NuiSkeletonPositionIndex startJoint;
        public NuiSkeletonBoneRotation hierarchicalRotation;
        public NuiSkeletonBoneRotation absoluteRotation;
    }
	
	public struct NuiImageViewArea
	{
	    int eDigitalZoom_NotUsed;
	    long lCenterX_NotUsed;
	    long lCenterY_NotUsed;
	}
	
	public class NuiImageBuffer
	{
		public int m_Width;
		public int m_Height;
		public int m_BytesPerPixel;
		public IntPtr m_pBuffer;
	}
	
	public struct NuiImageFrame
	{
		public Int64 liTimeStamp;
		public uint dwFrameNumber;
		public NuiImageType eImageType;
		public NuiImageResolution eResolution;
		//[MarshalAsAttribute(UnmanagedType.Interface)]
		public IntPtr pFrameTexture;
		public uint dwFrameFlags_NotUsed;
		public NuiImageViewArea ViewArea_NotUsed;
	}
	
	public struct ColorCust
	{
		public byte b;
		public byte g;
		public byte r;
		public byte a;
	}
	
	public struct NuiLockedRect
	{
		public int pitch;
		public int size;
		//[MarshalAsAttribute(UnmanagedType.U8)] 
		public IntPtr pBits; 
		
	}
	
	public struct ColorBuffer
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 640 * 480, ArraySubType = UnmanagedType.Struct)]
		public ColorCust[] pixels;
	}
	
	public struct DepthBuffer
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 640 * 480, ArraySubType = UnmanagedType.I2)]
		public short[] pixels;
	}
	
	public struct NuiSurfaceDesc
	{
		uint width;
		uint height;
	}
	
	[Guid("13ea17f5-ff2e-4670-9ee5-1297a6e880d1")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport()]
	public interface INuiFrameTexture
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int BufferLen();
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int Pitch();
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int LockRect(uint Level,ref NuiLockedRect pLockedRect,IntPtr pRect, uint Flags);
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetLevelDesc(uint Level, ref NuiSurfaceDesc pDesc);
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int UnlockRect(uint Level);
	}
	
	/* 
	 * kinect NUI (general) functions
	 */
    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiInitialize")]
    public static extern int NuiInitialize(NuiInitializeFlags dwFlags);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiShutdown")]
    public static extern void NuiShutdown();
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiCameraElevationSetAngle")]
	public static extern int NuiCameraSetAngle(long angle);
	
	/*
	 * kinect skeleton functions
	 */
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiSkeletonTrackingEnable")]
    public static extern int NuiSkeletonTrackingEnable(IntPtr hNextFrameEvent, uint dwFlags);
	
    [DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiSkeletonGetNextFrame")]
    public static extern int NuiSkeletonGetNextFrame(uint dwMillisecondsToWait, ref NuiSkeletonFrame pSkeletonFrame);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiTransformSmooth")]
    public static extern int NuiTransformSmooth(ref NuiSkeletonFrame pSkeletonFrame,ref NuiTransformSmoothParameters pSmoothingParams);
	
    [DllImport(@"Kinect10.dll", EntryPoint = "NuiSkeletonCalculateBoneOrientations")]
    public static extern int NuiSkeletonCalculateBoneOrientations(ref NuiSkeletonData pSkeletonData, NuiSkeletonBoneOrientation[] pBoneOrientations);

	/*
	 * kinect video functions
	 */
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamOpen")]
    public static extern int NuiImageStreamOpen(NuiImageType eImageType, NuiImageResolution eResolution, uint dwImageFrameFlags_NotUsed, uint dwFrameLimit, IntPtr hNextFrameEvent, ref IntPtr phStreamHandle);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamGetNextFrame")]
    public static extern int NuiImageStreamGetNextFrame(IntPtr phStreamHandle, uint dwMillisecondsToWait, ref IntPtr ppcImageFrame);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamReleaseFrame")]
    public static extern int NuiImageStreamReleaseFrame(IntPtr phStreamHandle, IntPtr ppcImageFrame);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageResolutionToSize")]
    public static extern int NuiImageResolutionToSize(NuiImageResolution eResolution,out uint frameWidth,out uint frameHeight);
	
	[DllImportAttribute(@"Kinect10.dll", EntryPoint = "NuiImageStreamSetImageFrameFlags")]
	public static extern int NuiImageStreamSetImageFrameFlags (IntPtr phStreamHandle, NuiImageStreamFlags dvImageFrameFlags);


	public static int GetDepthWidth()
	{
		return Constants.ImageWidth;
	}
	
	public static int GetDepthHeight()
	{
		return Constants.ImageHeight;
	}

	public static bool PollSkeleton(NuiTransformSmoothParameters smoothParameters, ref NuiSkeletonFrame skeletonFrame)
	{
		bool newSkeleton = false;
		
		int hr = KinectWrapper.NuiSkeletonGetNextFrame(0, ref skeletonFrame);
		if(hr == 0)
		{
			newSkeleton = true;
		}
		
		if(newSkeleton)
		{
			hr = KinectWrapper.NuiTransformSmooth(ref skeletonFrame,ref smoothParameters);
			if(hr != 0)
			{
				Debug.Log("Skeleton Data Smoothing failed");
			}
		}
		
		return newSkeleton;
	}
	
	public static bool PollColor(IntPtr colorStreamHandle, ref Color32[] colorImage)
	{
		IntPtr imageFramePtr = IntPtr.Zero;
		bool newColor = false;
	
		int hr = KinectWrapper.NuiImageStreamGetNextFrame(colorStreamHandle, 0, ref imageFramePtr);
		if (hr == 0)
		{
			newColor = true;
			
			NuiImageFrame imageFrame = (NuiImageFrame)Marshal.PtrToStructure(imageFramePtr, typeof(NuiImageFrame));
			INuiFrameTexture frameTexture = (INuiFrameTexture)Marshal.GetObjectForIUnknown(imageFrame.pFrameTexture);
			
			NuiLockedRect lockedRectPtr = new NuiLockedRect();
			IntPtr r = IntPtr.Zero;
			
			frameTexture.LockRect(0, ref lockedRectPtr, r, 0);
			ExtractColorImage(lockedRectPtr, ref colorImage);
			
			frameTexture.UnlockRect(0);
			hr = KinectWrapper.NuiImageStreamReleaseFrame(colorStreamHandle, imageFramePtr);
		}
		
		return newColor;
	}
	
	public static bool PollDepth(IntPtr depthStreamHandle, bool isNearMode, ref short[] depthPlayerData)
	{
		IntPtr imageFramePtr = IntPtr.Zero;
		bool newDepth = false;

		if (isNearMode)
		{
			KinectWrapper.NuiImageStreamSetImageFrameFlags(depthStreamHandle, NuiImageStreamFlags.EnableNearMode);
		}
		else
		{
			KinectWrapper.NuiImageStreamSetImageFrameFlags(depthStreamHandle, NuiImageStreamFlags.None);
		}
		
		int hr = KinectWrapper.NuiImageStreamGetNextFrame(depthStreamHandle, 0, ref imageFramePtr);
		if (hr == 0)
		{
			newDepth = true;
			
			NuiImageFrame imageFrame = (NuiImageFrame)Marshal.PtrToStructure(imageFramePtr, typeof(NuiImageFrame));
			INuiFrameTexture frameTexture = (INuiFrameTexture)Marshal.GetObjectForIUnknown(imageFrame.pFrameTexture);
			
			NuiLockedRect lockedRectPtr = new NuiLockedRect();
			IntPtr r = IntPtr.Zero;
			
			frameTexture.LockRect(0,ref lockedRectPtr,r,0);
			depthPlayerData = ExtractDepthImage(lockedRectPtr);
			
			frameTexture.UnlockRect(0);
			hr = KinectWrapper.NuiImageStreamReleaseFrame(depthStreamHandle, imageFramePtr);
		}
		
		return newDepth;
	}
	
	private static void ExtractColorImage(NuiLockedRect buf, ref Color32[] colorImage)
	{
		ColorBuffer cb = (ColorBuffer)Marshal.PtrToStructure(buf.pBits, typeof(ColorBuffer));
		int totalPixels = Constants.ImageWidth * Constants.ImageHeight;
		
		for (int pix = 0; pix < totalPixels; pix++)
		{
			int ind = totalPixels - pix - 1;
			
			colorImage[ind].r = cb.pixels[pix].r;
			colorImage[ind].g = cb.pixels[pix].g;
			colorImage[ind].b = cb.pixels[pix].b;
			colorImage[ind].a = 255;
		}
	}
	
	private static short[] ExtractDepthImage(NuiLockedRect lockedRect)
	{
		DepthBuffer db = (DepthBuffer)Marshal.PtrToStructure(lockedRect.pBits, typeof(DepthBuffer));
		return db.pixels;
	}
	
    private static Vector3 GetPositionBetweenIndices(ref Vector3[] jointsPos, NuiSkeletonPositionIndex p1, NuiSkeletonPositionIndex p2) 
	{
		Vector3 pVec1 = jointsPos[(int)p1];
		Vector3 pVec2 = jointsPos[(int)p2];
		
		pVec1.z = -pVec1.z;
		pVec2.z = -pVec2.z;

        return pVec2 - pVec1;
    }
           
    //populate matrix using the columns
    private static void PopulateMatrix(ref Matrix4x4 jointOrientation, Vector3 xCol, Vector3 yCol, Vector3 zCol) 
	{
    	jointOrientation.SetColumn(0, xCol);
    	jointOrientation.SetColumn(1, yCol);
    	jointOrientation.SetColumn(2, zCol);
    }

    //constructs an orientation from a vector that specifies the x axis
    private static void MakeMatrixFromX(Vector3 v1, ref Matrix4x4 jointOrientation, bool flip) 
	{
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set first column to the vector between the previous joint and the current one, this sets the two degrees of freedom
        xCol = v1.normalized;

        //set second column to an arbitrary vector perpendicular to the first column
        yCol.x = 0.0f;
        yCol.y = !flip ? xCol.z : -xCol.z;
        yCol.z = !flip ? -xCol.y : xCol.y;

        yCol.Normalize();

        //third column is fully determined by the first two, and it must be their cross product
        zCol = Vector3.Cross(xCol, yCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from a vector that specifies the y axis
    private static void MakeMatrixFromY(Vector3 v1, ref Matrix4x4 jointOrientation) 
	{
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set first column to the vector between the previous joint and the current one, this sets the two degrees of freedom
        yCol = v1.normalized;

        //set second column to an arbitrary vector perpendicular to the first column
        xCol.x = yCol.y;
        xCol.y = -yCol.x;
        xCol.z = 0.0f;

        xCol.Normalize();

        //third column is fully determined by the first two, and it must be their cross product
        zCol = Vector3.Cross(xCol, yCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
   
    //constructs an orientation from a vector that specifies the x axis
    private static void MakeMatrixFromZ(Vector3 v1, ref Matrix4x4 jointOrientation) 
	{
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

         //set first column to the vector between the previous joint and the current one, this sets the two degrees of freedom
        zCol = v1.normalized;

        //set second column to an arbitrary vector perpendicular to the first column
        xCol.x = zCol.y;
        xCol.y = -zCol.x;
        xCol.z = 0.0f;

        xCol.Normalize();

        //third column is fully determined by the first two, and it must be their cross product
        yCol = Vector3.Cross(zCol, xCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from 2 vectors: the first specifies the x axis, and the next specifies the y axis
    //uses the first vector as x axis, then constructs the other axes using cross products
    private static void MakeMatrixFromXY(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation) 
	{
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set up the three different columns to be rearranged and flipped
        xCol = xUnnormalized.normalized;
        zCol = Vector3.Cross(xCol, yUnnormalized.normalized).normalized;
        yCol = Vector3.Cross(zCol, xCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
   
    //constructs an orientation from 2 vectors: the first specifies the x axis, and the next specifies the y axis
    //uses the second vector as y axis, then constructs the other axes using cross products
    private static void MakeMatrixFromYX(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation) 
	{
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set up the three different columns to be rearranged and flipped
        yCol = yUnnormalized.normalized;
        zCol = Vector3.Cross(xUnnormalized.normalized, yCol).normalized;
        xCol = Vector3.Cross(yCol, zCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }
   
    //constructs an orientation from 2 vectors: the first specifies the x axis, and the next specifies the y axis
    //uses the second vector as y axis, then constructs the other axes using cross products
    private static void MakeMatrixFromYZ(Vector3 yUnnormalized, Vector3 zUnnormalized, ref Matrix4x4 jointOrientation) 
	{
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set up the three different columns to be rearranged and flipped
        yCol = yUnnormalized.normalized;
        xCol = Vector3.Cross(yCol, zUnnormalized.normalized).normalized;
        zCol = Vector3.Cross(xCol, yCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    public static void GetSkeletonJointOrientation(ref Vector3[] jointsPos, ref bool[] jointsTracked, ref Matrix4x4 [] jointOrients)
    {
        Vector3 vx;
        Vector3 vy;
        Vector3 vz;

	    // NUI_SKELETON_POSITION_HIP_CENTER
		if(jointsTracked[(int)NuiSkeletonPositionIndex.HipCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Spine])
		{
			vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipCenter, NuiSkeletonPositionIndex.Spine);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipCenter]);
			
			// make a correction of about 40 degrees back to the front
			Matrix4x4 mat = jointOrients[(int)NuiSkeletonPositionIndex.HipCenter];
			Quaternion quat = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
			quat *= Quaternion.Euler(-40, 0, 0);
			jointOrients[(int)NuiSkeletonPositionIndex.HipCenter].SetTRS(Vector3.zero, quat, Vector3.one);
		}
       
	    // NUI_SKELETON_POSITION_SPINE
		if(jointsTracked[(int)NuiSkeletonPositionIndex.Spine] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.ShoulderCenter);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Spine]);
		}
       
	    // NUI_SKELETON_POSITION_SHOULDER_CENTER
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderCenter, NuiSkeletonPositionIndex.Head);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderCenter]);
		}

	    // NUI_SKELETON_POSITION_HEAD
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderCenter] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderCenter, NuiSkeletonPositionIndex.Head);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Head]);
	        //MakeMatrixFromY(vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Head]);
		}
       
	    // NUI_SKELETON_POSITION_SHOULDER_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft])
		{
	        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ElbowLeft);
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowLeft, NuiSkeletonPositionIndex.WristLeft);
	        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderLeft]);
		}
       
	    // NUI_SKELETON_POSITION_ELBOW_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft])
		{
	        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowLeft, NuiSkeletonPositionIndex.WristLeft);
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ElbowLeft);
	        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowLeft]);
		}
       
	    // NUI_SKELETON_POSITION_WRIST_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft])
		{
	        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristLeft, NuiSkeletonPositionIndex.HandLeft);
	        MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.WristLeft], false);
		}
			
	    // NUI_SKELETON_POSITION_HAND_LEFT:
		if(jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft])
		{
	        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristLeft, NuiSkeletonPositionIndex.HandLeft);
	        MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.HandLeft], false);
		}
       
	    // NUI_SKELETON_POSITION_SHOULDER_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight])
		{
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderRight, NuiSkeletonPositionIndex.ElbowRight);
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowRight, NuiSkeletonPositionIndex.WristRight);
	        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderRight]);
		}
       
	    // NUI_SKELETON_POSITION_ELBOW_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight] && jointsTracked[(int)NuiSkeletonPositionIndex.WristRight])
		{
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowRight, NuiSkeletonPositionIndex.WristRight);
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderRight, NuiSkeletonPositionIndex.ElbowRight);
	        MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowRight]);
		}
       
	    // NUI_SKELETON_POSITION_WRIST_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight])
		{
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristRight, NuiSkeletonPositionIndex.HandRight);
	        MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.WristRight], true);
		}
       
	    // NUI_SKELETON_POSITION_HAND_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight])
		{
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristRight, NuiSkeletonPositionIndex.HandRight);
	        MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.HandRight], true);
		}
        
	    // NUI_SKELETON_POSITION_HIP_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.HipLeft);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipLeft]);
		}
       
	    // NUI_SKELETON_POSITION_KNEE_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft])
		{
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
	        vz = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.AnkleLeft, NuiSkeletonPositionIndex.FootLeft);
	        MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeLeft]);
		}
       
	    // NUI_SKELETON_POSITION_ANKLE_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
		{
	        vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootLeft, NuiSkeletonPositionIndex.AnkleLeft);
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
	        MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleLeft]);
	        //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleLeft]);
		}
       
	    // NUI_SKELETON_POSITION_FOOT_LEFT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
		{
	        vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootLeft, NuiSkeletonPositionIndex.AnkleLeft);
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
	        MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootLeft]);
	        //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootLeft]);
		}
       
	    // NUI_SKELETON_POSITION_HIP_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.HipRight] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight])
		{
	        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.HipRight);
	        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
	        MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipRight]);
		}
       
	    // NUI_SKELETON_POSITION_KNEE_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight])
		{
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
	        vz = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.AnkleRight, NuiSkeletonPositionIndex.FootRight);
	        MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeRight]);
		}
       
	    // NUI_SKELETON_POSITION_ANKLE_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] && jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
		{
	        vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootRight, NuiSkeletonPositionIndex.AnkleRight);
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
	        MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleRight]);
	        //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleRight]);
		}
       
	    // NUI_SKELETON_POSITION_FOOT_RIGHT
		if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] && jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
		{
	        vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootRight, NuiSkeletonPositionIndex.AnkleRight);
	        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
	        MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootRight]);
	        //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootRight]);
		}
    }

}