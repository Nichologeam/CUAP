using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Drawing;
using System.IO;
using System.Collections.Concurrent;
using Archipelago.MultiClient.Net.Models;
using static System.Collections.Specialized.BitVector32;

namespace CUAP;

public class DepthChecks : MonoBehaviour
{
    public static ApClient Client;
    public static ConcurrentQueue<long> CheckHandler;
    private WorldGeneration worldgen;
    private int RoundedMeters;
    private int CheckID;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        CheckHandler = APClientClass.ChecksToSendQueue;
        worldgen = this.gameObject.GetComponent<WorldGeneration>();
        Startup.Logger.LogMessage("Depth is being read by Archipelago!");
    }
    private void Update()
    {
        RoundedMeters = Mathf.RoundToInt(worldgen.PlayerTotalDepthMeters());
        if (RoundedMeters % 100 == 0)
        {
            Startup.Logger.LogMessage("Reached Depth Milestone.");
            CheckID = RoundedMeters / 100;
            CheckID = -966813096 + CheckID - 1; // don't ask me why the check id is a random negative number, the library i rely on shit itself or something
            Startup.Logger.LogMessage("Queueing check for " + RoundedMeters + "m"); // took me literally hours to diagnose the issue and figure out how to fix it
            CheckHandler.Enqueue(CheckID); // whatever, just adds to the jank of this implimentation. for the immersion, for the love of the game!
        }
    }
}