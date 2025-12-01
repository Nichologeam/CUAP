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
    private PlayerCamera plrcam;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Vitals = this.gameObject.GetComponent<Body>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    public void ProcessTraps(string TrapName)
    {
        if (TrapName == "Depression Trap")
        {
            Vitals.happiness =- 20;
        }
        if (TrapName == "Hearing Loss Trap")
        {
            Vitals.hearingLoss =+ 50;
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
        if (TrapName == "Unchipped Trap")
        {
            StartCoroutine(UnchippedToggle());
        }
        if (TrapName == "Elder Thornback Trap")
        {
            if (UnityEngine.Random.Range(0, 1) == 0)
            {
                plrcam.currentThreatTheme = 15; // play the Elder Thornback first phase theme
            }
            else
            {
                plrcam.currentThreatTheme = 10; // play the Elder Thornback second phase theme
            }
            plrcam.threatMusicTime = 3000; // for 3000 frames (50 seconds)
        }
        if (TrapName == "Cave Ticks Trap")
        {
            Instantiate(Resources.Load<GameObject>("caveticks"), gameObject.transform.position, Quaternion.identity);
        }
        if (TrapName == "Bad Rep Trap")
        {
            foreach (var trader in FindObjectsOfType<TraderScript>())
            {
                trader.hostility = 500;
            }
        }
        if (TrapName == "Disfigured Trap" && !Vitals.disfigured)
        {
            StartCoroutine(Disfigurement());
        }
    }
    IEnumerator ReverseControls()
    {
        Vitals.reversedControls = true;
        yield return new WaitForSecondsRealtime(10);
        Vitals.reversedControls = false;
    }
    IEnumerator UnchippedToggle()
    {
        worldgen.unchippedMode = true;
        yield return new WaitForSecondsRealtime(90);
        worldgen.unchippedMode = false;
    }
    IEnumerator Disfigurement()
    {
        Vitals.disfigured = true;
        yield return new WaitForSecondsRealtime(180);
        Vitals.disfigured = false;
    }
}