using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ApClient Client;
    private Body Vitals;
    private WorldGeneration worldgen;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Vitals = this.gameObject.GetComponent<Body>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    public void ProcessTraps(string TrapName)
    {
        if (TrapName == "Depression Trap")
        {
            Vitals.happiness =- 20;
        }
        if (TrapName == "Hearing Loss Trap" && Vitals.hearingLoss < 50) // It's a trap item, so let's not lower the player's hearing loss.
        {
            Vitals.hearingLoss = 50;
        }
        if (TrapName == "Earthquake Trap")
        {
            worldgen.earthquakeDelay = 0; // start an earthquake
            worldgen.earthquakeIntensity = 2; // twice as intense as basegame earthquake
            worldgen.earthquakeTime = 15; // for 15 seconds
        }
        if (TrapName == "Reverse Controls Trap")
        {
            StartCoroutine(ReverseControls());
        }
        if (TrapName == "Sleep Trap")
        {
            Vitals.sleeping = true;
        }
    }
    IEnumerator ReverseControls()
    {
        Vitals.reversedControls = true;
        yield return new WaitForSecondsRealtime(10);
        Vitals.reversedControls = false;
    }
}