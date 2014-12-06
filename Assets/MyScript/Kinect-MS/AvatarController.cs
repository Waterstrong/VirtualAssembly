using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

public class AvatarController : MonoBehaviour
{	
	// Bool that determines whether the avatar is active.
	public bool Active = true;
	
	// Bool that has the characters (facing the player) actions become mirrored. Default true.
	public bool MirroredMovement = true;
	
	// Bool that determines whether the avatar will move or not in space.
	public bool MovesInSpace = true;
	
	// Bool that determines whether the avatar is allowed to jump -- vertical movement
	// can cause some models to behave strangely, so use at your own discretion.
	public bool VerticalMovement = false;
	
	// Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
	public int MoveRate = 1;
	
	// Public variables that will get matched to bones. If empty, the kinect will simply not track it.
	// These bones can be set within the Unity interface.
	public Transform Head;
	public Transform Neck;
	public Transform Spine;
	public Transform Hips;
	
	//public Transform LeftShoulder;
	public Transform LeftArm;
	public Transform LeftElbow; 
	public Transform LeftWrist;
	public Transform LeftHand;
	//public Transform LeftFingers;
	
	//public Transform RightShoulder;
	public Transform RightArm;
	public Transform RightElbow;
	public Transform RightWrist;
	public Transform RightHand;
	//public Transform RightFingers;
	
	public Transform LeftLeg;
	public Transform LeftKnee;
	public Transform LeftFoot;
	public Transform LeftToes;
	
	public Transform RightLeg;
	public Transform RightKnee;
	public Transform RightFoot;
	public Transform RightToes;
	
	public Transform Root;
	
	// A required variable if you want to rotate the model in space.
	public GameObject offsetNode;
	
	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	private Transform[] bones;
	
	// Rotations of the bones when the Kinect tracking starts.
    private Quaternion[] initialRotations;
	
	// Calibration Offset Variables for Character Position.
	bool OffsetCalibrated = false;
	float XOffset, YOffset, ZOffset;
	Quaternion originalRotation;
	

    public void Start()
    {	
		// Holds our bones for later.
		bones = new Transform[25];
		
		// Initial rotations of said bones.
		initialRotations = new Quaternion[25];
		
		// Map bones to the points the Kinect tracks.
		MapBones();

		// Get initial rotations to return to later.
		GetInitialRotations();
		
		// Set the model to the calibration pose.
		if(KinectManager.IsCalibrationNeeded())
		{
	        RotateToCalibrationPose();
		}
    }
	
	// Update the avatar each frame.
    public void UpdateAvatar(uint UserID, bool IsNearMode)
    {	
		bool flipJoint = !MirroredMovement;
		
		// Update Head, Neck, Spine, and Hips normally.
		TransformBone(UserID, KinectWrapper.SkeletonJoint.HIPS, 4, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.SPINE, 3, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.NECK, 2, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.HEAD, 1, flipJoint);
		
		// Beyond this, switch the arms and legs.
		
		// Left Arm --> Right Arm
		TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_COLLAR, !MirroredMovement ? 5 : 11, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_SHOULDER, !MirroredMovement ? 6 : 12, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_ELBOW, !MirroredMovement ? 7 : 13, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_WRIST, !MirroredMovement ? 8 : 14, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_HAND, !MirroredMovement ? 9 : 15, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_FINGERTIP, !MirroredMovement ? 10 : 16, flipJoint);
		
		// Right Arm --> Left Arm
		TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_COLLAR, !MirroredMovement ? 11 : 5, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_SHOULDER, !MirroredMovement ? 12 : 6, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_ELBOW, !MirroredMovement ? 13 : 7, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_WRIST, !MirroredMovement ? 14 : 8, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_HAND, !MirroredMovement ? 15 : 9, flipJoint);
		TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_FINGERTIP, !MirroredMovement ? 16 : 10, flipJoint);
		
		if(!IsNearMode)
		{
			// Left Leg --> Right Leg
			TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_HIP, !MirroredMovement ? 17 : 21, flipJoint);
			TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_KNEE, !MirroredMovement ? 18 : 22, flipJoint);
			TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_ANKLE, !MirroredMovement ? 19 : 23, flipJoint);
			TransformBone(UserID, KinectWrapper.SkeletonJoint.LEFT_FOOT, !MirroredMovement ? 20 : 24, flipJoint);
			
			// Right Leg --> Left Leg
			TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_HIP, !MirroredMovement ? 21 : 17, flipJoint);
			TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_KNEE, !MirroredMovement ? 22 : 18, flipJoint);
			TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_ANKLE, !MirroredMovement ? 23 : 19, flipJoint);
			TransformBone(UserID, KinectWrapper.SkeletonJoint.RIGHT_FOOT, !MirroredMovement ? 24 : 20, flipJoint);	
		}
		
		// If the avatar is supposed to move in the space, move it.
		if (MovesInSpace)
		{
			MoveAvatar(UserID);
		}
    }
	
	// Set bones to initial position.
    public void RotateToInitialPosition()
    {	
		// For each bone that was defined, reset to initial position.
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				bones[i].rotation = initialRotations[i];
			}
		}
    }
	
	// Calibration pose is simply initial position with hands raised up. Rotation must be 0,0,0 to calibrate.
    public void RotateToCalibrationPose()
    {	
		// Reset the rest of the model to the original position.
        RotateToInitialPosition();
		
		// Right Elbow
		if(RightElbow != null)
        	RightElbow.rotation = Quaternion.Euler(0, -90, 90) * 
				initialRotations[(int)KinectWrapper.SkeletonJoint.RIGHT_ELBOW];
		
		// Left Elbow
		if(LeftElbow != null)
        	LeftElbow.rotation = Quaternion.Euler(0, 90, -90) * 
				initialRotations[(int)KinectWrapper.SkeletonJoint.LEFT_ELBOW];
    }
	
	// On the successful calibration of a player, use this method to reset the models position.
	public void SuccessfulCalibration()
	{
		if(offsetNode!=null)
		{
			offsetNode.transform.rotation = originalRotation;
		}
	}
	
	// Apply the rotations tracked by the kinect to the joints.
    void TransformBone(uint userId, KinectWrapper.SkeletonJoint joint, int boneIndex, bool flip)
    {
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null)
			return;
		
		// Grab the bone we're moving.
		int iJoint = (int)joint;
		if(iJoint < 0)
			return;
		
		// Get Kinect joint orientation
		Quaternion jointRotation = KinectManager.Instance.GetJointOrientation(userId, iJoint, flip);
		
		// Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		
		//If an offset node is specified, combine the transform with its
		//orientation to essentially make the skeleton relative to the node
		if (offsetNode != null)
		{
			// Grab the total rotation by adding the Euler and offset's Euler.
			Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
			// Grab our new rotation.
			newRotation = Quaternion.Euler(totalRotation);
		}
		
		// Smoothly transition to our new rotation.
        boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, Time.deltaTime * 20);
	}
	
	// Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
	// Only pulls positional, not rotational.
	void MoveAvatar(uint UserID)
	{
		if(Root == null || Root.parent == null)
			return;
		
        // Get the position of the body and store it.
		Vector3 trans = KinectManager.Instance.GetUserPosition(UserID);
		
		// If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
		if (!OffsetCalibrated)
		{
			OffsetCalibrated = true;
			
			XOffset = trans.x * MoveRate;
			YOffset = trans.y * MoveRate;
			ZOffset = trans.z * MoveRate;
			
			// Change the offsets so we can appropriately add or subtract them to the position of the model.
			if(XOffset < 0)
				XOffset = -XOffset;
			if(YOffset < 0)
				YOffset = -YOffset;
			if(ZOffset < 0)
				ZOffset = -ZOffset;
		}
	
		float xPos;
		float yPos;
		float zPos;
		
		// If movement is mirrored, update normally. If not, reverse it.
		if(MirroredMovement)
		{
			xPos = -trans.x * MoveRate + XOffset;
		}
		else
		{
			xPos = trans.x * MoveRate + XOffset;
		}
		
		yPos = trans.y * MoveRate - YOffset;
		zPos = trans.z * MoveRate + ZOffset;
		
		// If we are tracking vertical movement, update the y. Otherwise leave it alone.
		if(VerticalMovement)
		{
			Root.parent.localPosition = new Vector3(xPos, yPos, zPos);
		}
		else
		{
			Root.parent.localPosition = new Vector3(xPos, 0, zPos);
		}
	}
	
	// If the bones to be mapped have been declared, map that bone to the model.
	void MapBones()
	{
		// If they're not empty, pull in the values from Unity and assign them to the array.
		if(Head != null)
			bones[1] = Head;
		if(Neck != null)
			bones[2] = Neck;
		if(Spine != null)
			bones[3] = Spine;
		if(Hips != null)
			bones[4] = Hips;
		
//		if(LeftShoulder != null)
//			bones[5] = LeftShoulder;
		if(LeftArm != null)
			bones[6] = LeftArm;
		if(LeftElbow != null)
			bones[7] = LeftElbow;
		if(LeftWrist != null)
			bones[8] = LeftWrist;
		if(LeftHand != null)
			bones[9] = LeftHand;
//		if(LeftFingers != null)
//			bones[10] = LeftFingers;
		
//		if(RightShoulder != null)
//			bones[11] = RightShoulder;
		if(RightArm != null)
			bones[12] = RightArm;
		if(RightElbow != null)
			bones[13] = RightElbow;
		if(RightWrist != null)
			bones[14] = RightWrist;
		if(RightHand != null)
			bones[15] = RightHand;
//		if(RightFingers != null)
//			bones[16] = RightFingers;
		
		if(LeftLeg != null)
			bones[17] = LeftLeg;
		if(LeftKnee != null)
			bones[18] = LeftKnee;
		if(LeftFoot != null)
			bones[19] = LeftFoot;
		if(LeftToes != null)
			bones[20] = LeftToes;
		
		if(RightLeg != null)
			bones[21] = RightLeg;
		if(RightKnee != null)
			bones[22] = RightKnee;
		if(RightFoot != null)
			bones[23] = RightFoot;
		if(RightToes!= null)
			bones[24] = RightToes;
	}
	
	// Capture the initial rotations of the model.
	void GetInitialRotations()
	{
		if(offsetNode != null)
		{
            
			// Store the original offset's rotation.
			originalRotation = offsetNode.transform.rotation;
			// Set the offset's rotation to 0.
			offsetNode.transform.rotation = Quaternion.Euler(Vector3.zero);
		}
		
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation;
			}
		}

		if(offsetNode != null)
		{
			// Restore the offset's rotation
			offsetNode.transform.rotation = originalRotation;
		}
		
	}
}