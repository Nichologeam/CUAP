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
using System.Linq;

namespace CUAP;

public class DepthChecks : MonoBehaviour
{
    public static ApClient Client;
    public static ConcurrentQueue<long> CheckHandler;
    private WorldGeneration worldgen;
    private int RoundedMeters;
    private int CheckID;
    private List<int> AlreadySentChecks = new List<int>();

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
        if (RoundedMeters > 1500)
        {
            CheckHandler.Enqueue(-966813081); // goal location
            Destroy(this); // no need for this script after the player goals, it would just spam goal every frame.
        }
        if (RoundedMeters % 100 == 0)
        {
            CheckID = RoundedMeters / 100;
            CheckID = -966813096 + CheckID - 1; // don't ask me why the check id is a random negative number, best guess is an integer over/underflow
            if (AlreadySentChecks.Contains(CheckID)) // whatever, just adds to the jank of this implimentation. for the immersion, for the love of the game!
            {
                return; // Avoid spamming the server by not even attempting to send a check we already have sent.
            }
            Startup.Logger.LogMessage("Queueing check for " + RoundedMeters + "m");
            CheckHandler.Enqueue(CheckID);
            AlreadySentChecks.Add(CheckID);
        }
    }
}