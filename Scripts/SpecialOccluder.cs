/*******************************************************************************
File:      SpecialOccluder.cs
Author:    Tyler Swan
Email:     Tswanpro@gmail.com
Date:      10/17/2020
Course:    CS176
Section:   A

Description:
    A script to enable objects to handle their own occlusion if they have special occlusion rules.
    
*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialOccluder : MonoBehaviour
{
    //Object References
    public MeshRenderer MR;
    public Material OccludedTexture;
    public Material NormalTexture;

    private bool IsOccluding = false;

    private void Start()
    {
        //On start grabs a reference to own MeshRenderer.
        MR = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        //If the object is infront of the camera, turns self transparent.
        if(IsOccluding)
        {
            MR.material = OccludedTexture;
        }
        else
        {
            MR.material = NormalTexture;
        }
        IsOccluding = false;
    }

    //Function for other scripts to use for easy handling of special occlusion.
    public void HandleOcclusion()
    {
        IsOccluding = true;
    }
}
