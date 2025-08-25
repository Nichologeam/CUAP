using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Linq;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ApClient Client;
    private Body Vitals;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Vitals = this.gameObject.GetComponent<Body>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    public void ProcessTraps(string TrapName)
    {
        if (TrapName == "Depressed Trap")
        {
            Startup.Logger.LogMessage("Happiness lowered.");
            Vitals.happiness =- 20;
        }
        if (TrapName == "Hearing Loss Trap" && Vitals.hearingLoss < 50) // It's a trap item, so let's not lower the player's hearing loss.
        {
            Startup.Logger.LogMessage("Setting hearing loss to 50.");
            Vitals.hearingLoss = 50;
        }
    }
}