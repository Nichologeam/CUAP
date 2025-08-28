﻿using CreepyUtil.Archipelago;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace CUAP;

public class DepthChecks : MonoBehaviour
{
    public static ApClient Client;
    public static ConcurrentQueue<long> CheckHandler;
    private WorldGeneration worldgen;
    private int RoundedMeters;
    private int CheckID;
    private int GoalDepth;
    private int GoalCheckID;
    private TextMeshProUGUI DisplayText;
    private List<int> AlreadySentChecks = new List<int>();

    private void OnEnable()
    {
        Client = APClientClass.Client;
        CheckHandler = APClientClass.ChecksToSendQueue;
        worldgen = this.gameObject.GetComponent<WorldGeneration>();
        DisplayText = GameObject.Find("Main Camera/Canvas/TimeScaleShow/Text (TMP)").GetComponent<TextMeshProUGUI>();
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("GoalDepth", out var goaldepthoption)) // fetch and store the goal depth.
        {
            GoalDepth = (int)goaldepthoption;
            GoalCheckID = -966812869 + (GoalDepth / 100);
        }
        Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is " + GoalDepth + "m");
    }
    private void Update()
    {
        RoundedMeters = Mathf.RoundToInt(worldgen.PlayerTotalDepthMeters());
        if (worldgen.loadingObject.activeSelf)
        {
            StartCoroutine(CheckForDepthExtenders());
        }
        // next is handling sending the checks
        if (RoundedMeters > GoalDepth && !worldgen.loadingObject.activeSelf) // fixes a bug with the order the game loads new layers internally
        {
            CheckHandler.Enqueue(GoalCheckID); // goal location
            DisplayText.text = "You have reached your goal!";
            DisplayText.autoSizeTextContainer = true;
            Client.Goal();
            Destroy(this); // no need for this script after the player goals, it would just spam goal every frame.
        }
        if (RoundedMeters % 100 == 0)
        {
            CheckID = RoundedMeters / 100;
            Startup.Logger.LogMessage("Depth read as " + RoundedMeters + "m, which SHOULD be the same as " + CheckID + "00m.");
            CheckID = -966812869 + CheckID - 1; // don't ask me why the check id is a random negative number, best guess is an integer over/underflow
            if (AlreadySentChecks.Contains(CheckID)) // whatever, just adds to the jank of this implimentation. for the immersion, for the love of the game!
            {
                return; // Avoid spamming the server by not even attempting to send a check we already have sent.
            }
            CheckHandler.Enqueue(CheckID);
            AlreadySentChecks.Add(CheckID);
        }
    }
    IEnumerator CheckForDepthExtenders()
    {
        if (APClientClass.DepthExtendersRecieved < (RoundedMeters - 300) / 300) // passes if we don't have enough depth extenders
        {
            worldgen.totalTraveled -= (int)(worldgen.height * 0.3f); // reversing WorldGeneration.IncreaseDepthByLayer
            if (worldgen.doPod) // true if we are using a drillpod
            {
                worldgen.totalTraveled -= (int)(worldgen.height * 0.3f); // do it a second time
            }
        }
        yield return new WaitUntil(() => !worldgen.loadingObject.activeSelf); // wait until loading is done to not trigger this every frame
    }
}