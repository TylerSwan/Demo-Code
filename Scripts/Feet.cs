/*******************************************************************************
File:      Feet.cs
Author:    Tyler Swan
Email:     Tswanpro@gmail.com
Date:      10/17/2020
Course:    CS176
Section:   A

Description:
    Script to handle jumping.
    
*******************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feet : MonoBehaviour
{
    //Sets initial variables.
    public int SurfaceCounter = 0;
    public int AirJumps = 1;

    //Refreshes jump variables.
    private void OnTriggerEnter(Collider other)
    {
        SurfaceCounter += 1;
        AirJumps = 1;
    }

    //Decrements jump variables.
    private void OnTriggerExit(Collider other)
    {
        SurfaceCounter -= 1;
    }

    //Checks if the player is able to jump in the air.
    public bool CanAirJump()
    {
        if (AirJumps > 0)
        {
            AirJumps = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
}
