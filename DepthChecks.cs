using CreepyUtil.Archipelago;
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
        if (options.TryGetValue("GoalDepth", out var goaldepthoption)) // fetch and store the goal depth. will always be sent even if goal isn't Reach Depth
        {
            if (APClientClass.selectedGoal == 1)
            {
                GoalDepth = (int)goaldepthoption;
                GoalCheckID = -966813196 + (GoalDepth / 100);
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is " + GoalDepth + "m");
            }
            else if (APClientClass.selectedGoal == 2)
            {
                GoalCheckID = -966813196 + (1500 / 100);
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is Escape Overgrown Depths");
                GoalDepth = 1534;
            }
            else if (APClientClass.selectedGoal == 3)
            {
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is Defeat Elder Thornback");
                GoalDepth = 999999; // not needed for this goal
            }
            else if (APClientClass.selectedGoal == 4)
            {
                Startup.Logger.LogMessage("Depth is being read by Archipelago! Goal is Craftsanity");
                GoalDepth = 999999; // not needed for this goal
            }
        }
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
        if (APClientClass.selectedGoal is 1 or 3) // logic for Depth Extenders (goal 1 and 3)
        {
            if (worldgen.doPod && (APClientClass.DepthExtendersRecieved < (RoundedMeters) / 300)) // true if we are using a drillpod and can't afford 2 layers
            {
                worldgen.totalTraveled -= (int)(worldgen.height * 0.3f); // do it a second time
            }
            else if (APClientClass.DepthExtendersRecieved < (RoundedMeters - 300) / 300)
            {
                worldgen.totalTraveled -= (int)(worldgen.height * 0.3f); // reversing WorldGeneration.IncreaseDepthByLayer
            }
        }
        else if (APClientClass.selectedGoal is 2 or 4) // logic for Progressive Layers (goal 2 and 4)
        {
            if (worldgen.doPod && (APClientClass.DepthExtendersRecieved < worldgen.biomeDepth)) // true if we are using a drillpod and can't afford 2 layers
            {
                worldgen.totalTraveled -= (int)(worldgen.height * 0.3f); // do it a second time
            }
            else if (APClientClass.DepthExtendersRecieved < worldgen.biomeDepth)
            {
                worldgen.totalTraveled -= (int)(worldgen.height * 0.3f); // reversing WorldGeneration.IncreaseDepthByLayer
            }
        }
        yield return new WaitUntil(() => !worldgen.loadingObject.activeSelf); // wait until loading is done to not trigger this every frame
    }
}