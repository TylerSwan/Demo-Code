/*******************************************************************************
File:      Character_Controller.cs
Author:    Tyler Swan
Email:     Tswanpro@gmail.com
Date:      10/17/2020
Course:    CS176
Section:   A

Description:
    Handles player movement and interactions with the environment.
    
*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Character_Controller : MonoBehaviour
{
    // Start is called before the first frame update

    //Player Object References
    Rigidbody RB;
    public Camera PlayerCam;
    public Transform PlayerModelTrans;

    //Yaw object variables
    public GameObject YawPivot;
    public float YawSens;
    private float YawAngle;

    //Pitch Object variables
    public GameObject PitchPivot;
    public float PitchSens;
    private float PitchAngle =0;
    private float MinPitch = -85;
    private float MaxPitch = 85;
    public bool InvertedPitch = false;
    public float CameraZoom;

    //Movement modifiers.
    public float MoveSpeed = 5;
    public float JumpForce = 5;
    public Feet feet;

    //Animator reference
    public Animator PlayerAnimator;

    //Lock on variables
    public GameObject LockOnObj;
    public Quaternion LockRotation;
    public float LockPitchAngle = 40;
    public bool LockedOn = false;
    public float LockOnRange = 10;

    //Grabs needed references at the start
    void Start()
    {
        RB = GetComponent<Rigidbody>();

        CameraZoom = PlayerCam.transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        //Determines if the player is trying to lock on or break lockon.
        if (Input.GetKeyDown(KeyCode.F) && LockedOn)
        {
            LockedOn = false;
        }
        else if (Input.GetKeyDown(KeyCode.F) && LockedOn == false)
        {
            LockOn();

            if (LockOnObj != null)
            {
                LockedOn = true;
            }
            else
            {
                LockedOn = false;
            }
        }

        //Checks if we are using lock on logic or free roam logic
        if (LockedOn)
        {
            LockedCamera();

            if (Input.GetKeyDown(KeyCode.E))
            {
                SelectNextTarget(KeyCode.E);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SelectNextTarget(KeyCode.Q);
            }

            Move();
        }
        else
        {
            SetCameraYaw();

            SetCameraPitch();

            Move();
        }
    }

    //Functions for manipulating the camera based on user input.
    void SetCameraYaw()
    {
        //collects mouse movement data on the X axis and determines wheter the camera should move left or right.
        YawAngle += Input.GetAxis("Mouse X") * YawSens * Time.deltaTime;
        YawPivot.transform.localRotation = Quaternion.Euler(0, YawAngle, 0);
    }
    void RotateRight()
    {
        YawAngle -= .08f;
        YawPivot.transform.localRotation = Quaternion.Euler(0, YawAngle, 0);
    }
    void RotateLeft()
    {
            YawAngle += .08f;
            YawPivot.transform.localRotation = Quaternion.Euler(0, YawAngle, 0);
    }

    //Function for camera controlls if locked on.
    void LockedCamera()
    {
        //Look for a way to control this by setting yawangle rather than an entire quaternion.
        LockRotation = Quaternion.LookRotation(LockOnObj.transform.position - YawPivot.transform.position);
        YawPivot.transform.rotation = Quaternion.Slerp(YawPivot.transform.localRotation, LockRotation, .1f);
        //Sets Yaw angle for free roam camera equal to locked position so that breaking lock does not change camera angle.
        YawAngle = YawPivot.transform.eulerAngles.y;

        //Sets Pitch Angle to locked angle for lockon camera controls.
        PitchAngle = LockPitchAngle;
        PitchPivot.transform.localRotation = Quaternion.Euler(PitchAngle, 0, 0);
    }

    //Camera pitch controlls.
    void SetCameraPitch()
    {
        float MouseDir;

        //Checks if the player wants inverted pitch controlls or not.
        if(InvertedPitch)
        {
            MouseDir = -Input.GetAxis("Mouse Y");
        }
        else
        {
            MouseDir = Input.GetAxis("Mouse Y");
        }
        
        //Adjusts pitch based on Mouse Y axis movement.
        PitchAngle -= MouseDir * PitchSens * Time.deltaTime;
        PitchAngle = Mathf.Clamp(PitchAngle, MinPitch, MaxPitch);
        PitchPivot.transform.localRotation = Quaternion.Euler(PitchAngle, 0, 0);
    }

    //Movement logic.
    void Move()
    {
        bool StrafeRotate = true;
        Vector3 Dir = Vector3.zero;

        //Moves forward
        if (Input.GetKey(KeyCode.W))
        {
            Dir += YawPivot.transform.forward;
            PlayerModelTrans.forward += YawPivot.transform.forward;
            StrafeRotate = false;
        }
        //Moves backwards
        if (Input.GetKey(KeyCode.S))
        {
            Dir -= YawPivot.transform.forward;
            PlayerModelTrans.forward -= YawPivot.transform.forward;
            StrafeRotate = false;
        }
        //Moves left
        if (Input.GetKey(KeyCode.A))
        {
            Dir -= YawPivot.transform.right;
            PlayerModelTrans.forward -= YawPivot.transform.right;
            if (StrafeRotate && !LockedOn)
            {
                RotateRight();
            }
        }
        // Moves Right
        if (Input.GetKey(KeyCode.D))
        {
            Dir += YawPivot.transform.right;
            PlayerModelTrans.forward += YawPivot.transform.right;
            if (StrafeRotate && !LockedOn)
            {
                RotateLeft();
            }
        }
        //Jumps
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Checks if we are using air jumps or not.
            if (feet.SurfaceCounter > 0)
            {
                PlayerAnimator.SetBool("Jump", true);
                RB.velocity += new Vector3(0, JumpForce, 0);
            }
            //Checks that we have air jumps available.
            else if (feet.CanAirJump())
            {
                PlayerAnimator.SetBool("Jump", true);
                RB.velocity += new Vector3(0, JumpForce, 0);
            }
            else
            {
                return;
            }
        }
        //Toggles Jump animation in animator.
        else
        {
            PlayerAnimator.SetBool("Jump", false);
        }
        //Applies new movement vector.
        Dir = Dir.normalized;
        RB.velocity = new Vector3(Dir.x * MoveSpeed, RB.velocity.y, Dir.z * MoveSpeed);
        //Determines if the player is walking or not based on velocity values.
        if (Dir != Vector3.zero)
        {
            PlayerAnimator.SetBool("Is Walking", true);
        }
        else
        {
            PlayerAnimator.SetBool("Is Walking", false);
        }
    }

    //Scans objects in area and choses the closest object.
    void LockOn()
    {
        float ShortestDistance = LockOnRange;
        float tempDist = 0;
        //Collects an array of objects.
        Collider[] colliders = Physics.OverlapSphere(YawPivot.transform.position, LockOnRange);

        for (int i = 0; i < colliders.Length; i++)
        {
            //If the object in the list can be targeted,
            if (colliders[i].GetComponent<Targetable>() != null)
            {
                tempDist = Vector3.Distance(colliders[i].transform.position, YawPivot.transform.position);
                //If the targetable object is closer than the previous closest object,
                if (tempDist < ShortestDistance)
                {
                    //Sets new closest distance and sets new object reference for closest object.
                    ShortestDistance = tempDist;
                    LockOnObj = colliders[i].transform.gameObject;
                }
            }
        }
    }

    void SelectNextTarget(KeyCode key)
    {
        List<TargetData> targetList = new List<TargetData>();
        //Creates an array of colliders in the area.
        Collider[] colliders = Physics.OverlapSphere(YawPivot.transform.position, LockOnRange);

        for (int i = 0; i < colliders.Length; i++)
        {
            //Filters out untargetable objects.
            if (colliders[i].GetComponent<Targetable>() != null)
            {
                //Creates a data entry for the object containing relevant data.
                TargetData tempData = new TargetData();
                Vector3 from = YawPivot.transform.forward;
                Vector3 to = (colliders[i].transform.position - YawPivot.transform.position).normalized;
                //Stores relevant information on current object then adds it to the list.
                tempData.angle = Vector3.SignedAngle(from, to, Vector3.up);
                tempData.obj = colliders[i].gameObject;
                targetList.Add(tempData);
            }
        }
        //Sorts list from left to right, smallest to largest angle values.
        targetList.Sort(TargetData.SmallerAngle);
        //Initializes lockObjIndex.
        int lockObjIndex = 0;
        for (int i = 0; i < targetList.Count; i++)
        {
            //Figures out which object in the array is our current lockOn target and sets LockOnIndex to that position.
            if (targetList[i].obj == LockOnObj)
            {
                lockObjIndex = i;
            }
        }
        //Gets player input to check if we are cycling left or right.
        if (key == KeyCode.E)
        {
            //If we are cycling right, and we are not already the largest object in the list,
            if (lockObjIndex < targetList.Count -1)
            {
                //Locks on to the next closest object to the right.
                LockOnObj = targetList[lockObjIndex + 1].obj;
            }
        }
        if (key == KeyCode.Q)
        {
            //If we are cycling left, checks we are not already the smallest object in the list.
            if (lockObjIndex > 0)
            {
                //Sets lockOnObj to the next closest object to the left.
                LockOnObj = targetList[lockObjIndex - 1].obj;
            }
        }
    }
}
//A class to help organize data in other functions.
public class TargetData
{
    public float angle;
    public GameObject obj;

    //Function for sorting data values in a list.
    public static int SmallerAngle(TargetData a, TargetData b)
    {
        if (a.angle > b.angle)
            return -1;
        if (a.angle < b.angle)
            return 1;
        else
            return 0;
    }
}