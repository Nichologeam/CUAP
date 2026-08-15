using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using Archipelago.MultiClient.Net;

namespace CUAP;

public class DepthChecks : MonoBehaviour
{
    public static ArchipelagoSession Client;
    public static DepthChecks instance;
    private WorldGeneration worldgen;
    private int RoundedMeters;
    private long CheckID;
    public long GoalDepth;
    private long GoalCheckID;
    private bool loading = false;
    public TextMeshProUGUI DisplayText;
    public List<long> AlreadySentChecks = [];

    private void OnEnable()
    {
        instance = this;
        Client = APClientClass.session;
        worldgen = this.gameObject.GetComponent<WorldGeneration>();
        DisplayText = GameObject.Find("Main Camera/Canvas/TimeScaleShow/Text (TMP)").GetComponent<TextMeshProUGUI>();
        var options = APClientClass.slotdata;
        if (options.TryGetValue("GoalDepth", out object goaldepthoption)) // fetch and store the goal depth. will always be sent even if goal isn't Reach Depth
        {
            if (APClientClass.selectedGoal == 1)
            {
                GoalDepth = (long)goaldepthoption;
                GoalCheckID = 22318000 + (GoalDepth / 100);
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is: Reach " + GoalDepth + "m");
            }
            else if (APClientClass.selectedGoal == 2)
            {
                GoalCheckID = 22318000 + (1500 / 100);
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is: Escape Overgrown Depths");
                GoalDepth = 1534;
            }
            else if (APClientClass.selectedGoal == 3)
            {
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is: Defeat Elder Thornback");
                GoalDepth = long.MaxValue; // not needed for this goal
            }
        }
    }
    private void Update()
    {
        RoundedMeters = Mathf.RoundToInt(worldgen.PlayerTotalDepthMeters());
        if (worldgen.loadingObject.activeSelf && !loading)
        {
            loading = true;
            StartCoroutine(CheckForDepthExtenders());
        }
        if (RoundedMeters > GoalDepth && !worldgen.loadingObject.activeSelf) // fixes a bug with the order the game loads new layers internally
        {
            APClientClass.ChecksToSend.Add(GoalCheckID); // goal location
            DisplayText.text = APLocale.Get("goal", APLocale.APLanguageType.UI);
            Client.SetGoalAchieved();
            Destroy(this); // no need for this script after the player goals, it would just spam goal every frame.
        }
        if (RoundedMeters % 100 == 0)
        {
            CheckID = RoundedMeters / 100;
            CheckID = 22318000 + CheckID - 1;
            if (AlreadySentChecks.Contains(CheckID))
            {
                return; // Avoid spamming the server by not even attempting to send a check we already have sent.
            }
            APClientClass.ChecksToSend.Add(CheckID);
            AlreadySentChecks.Add(CheckID);
        }
    }
    IEnumerator CheckForDepthExtenders()
    {
        int depthToSubtract = 0;
        if (APClientClass.selectedGoal is 1 or 3) // logic for Depth Extenders (goal 1 and 3)
        {
            if (worldgen.doPod && (APClientClass.DepthExtendersRecieved < (RoundedMeters) / 300)) // true if we are using a drillpod and can't afford 2 layers
            {
                depthToSubtract = 614; // double it to go up two layers
            }
            else if (APClientClass.DepthExtendersRecieved < (RoundedMeters) / 300)
            {
                depthToSubtract = 307; // reversing WorldGeneration.IncreaseDepthByLayer (this is worldgen.height * 0.3)
            }
        }
        else if (APClientClass.selectedGoal == 2) // logic for Progressive Layers (goal 2)
        {
            if (worldgen.doPod && (APClientClass.DepthExtendersRecieved < worldgen.biomeDepth)) // true if we are using a drillpod and can't afford 2 layers
            {
                depthToSubtract = 614; // double it to go up two layers
            }
            else if (APClientClass.DepthExtendersRecieved < worldgen.biomeDepth)
            {
                depthToSubtract = 307; // reversing WorldGeneration.IncreaseDepthByLayer (this is worldgen.height * 0.3)
            }
        }
        int depthToSet = worldgen.totalTraveled - depthToSubtract;
        while (worldgen.loadingObject.activeSelf)
        {
            worldgen.totalTraveled = depthToSet;
            yield return null;
        }
        loading = false;
    }
}