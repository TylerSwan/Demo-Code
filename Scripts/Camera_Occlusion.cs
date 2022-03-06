/*******************************************************************************
File:      Camera_Occlusion.cs
Author:    Tyler Swan
Email:     Tswanpro@gmail.com
Date:      10/17/2020
Course:    CS176
Section:   A

Description:
    Prevents objects from being able to block the camera view by moving the camera.
    
*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Occlusion : MonoBehaviour
{
    //Object References
    public Transform MovePivot;
    public Transform CameraPos;
    public float PreferredZoom;
    public float ZoomPadding = .2f;
    public float ZoomInterpolant = .15f;
    public float MaxZoom = 1f;
    public float MinZoom = 5f;
    public LayerMask OcclusionMask;

    private void Start()
    {
        //Sets camera to preffered zoom at start.
        transform.localPosition = new Vector3(0f, 0f, PreferredZoom);
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 RayOrigin = MovePivot.position;
        Vector3 RayDir = (transform.position - RayOrigin).normalized;
        Ray OcclusionRay = new Ray(RayOrigin, RayDir);
        float ZCamDist = -PreferredZoom;
        
        //Casts a raycast and stores all hitresults in an array.
        RaycastHit[] results = Physics.RaycastAll(OcclusionRay, Mathf.Abs(PreferredZoom), OcclusionMask);

        //If the raycast hit anything.
        if(results.Length > 0)
        {
            foreach(RaycastHit Result in results)
            {
                //If the hit object is closer to the player than the camera.
                if (Result.distance < Mathf.Abs(PreferredZoom))
                {
                    //Checks if there are special occlusion rules for this object.
                    SpecialOccluder occluder = Result.transform.GetComponent<SpecialOccluder>();

                    //Null guard against SpecialOcclusion
                    if(occluder != null)
                    {
                        //Makes the special occlusion object handle it's own occlusion.
                        occluder.HandleOcclusion();
                    }
                    else
                    {
                        //Sets variables for zooming in to prevent occlusion.
                        ZCamDist = Mathf.Max(ZCamDist, -(Result.distance - ZoomPadding));
                        Mathf.Clamp(ZCamDist, MaxZoom, MinZoom);
                    }
                }
            }
        }
        //Zooms camera in to keep player visible.
        Vector3 TargetCampPos = new Vector3(0f, 0f, ZCamDist);
        transform.localPosition = Vector3.Lerp(transform.localPosition, TargetCampPos, ZoomInterpolant);
    }
}
