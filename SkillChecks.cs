using CreepyUtil.Archipelago;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace CUAP;

public class SkillChecks : MonoBehaviour
{
    public static ApClient Client;
    public Skills playerSkills;
    private WorldGeneration worldgen;
    public static int apMaxStr;
    public static int apMaxRes;
    public static int apMaxInt;
    int sendStr = 60;
    int sendRes = 60;
    int sendInt = 60;
    List<int> alreadySentChecks = new List<int>();
    Dictionary<int, int> EXPRequirementToLevel = new Dictionary<int, int>()
    { // level is subtracted by 1
        {60,0},
        {120,1},
        {180,2},
        {240,3},
        {300,4},
        {360,5},
        {420,6},
        {480,7},
        {540,8},
        {640,9},
        {758,10},
        {897,11},
        {1062,12},
        {1255,13},
        {1484,14},
    };
    private void OnEnable()
    {
        Client = APClientClass.Client;
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("Skillsanity", out var skillsanityoption)) // check if skillsanity is enabled.
        {
            if (!Convert.ToBoolean(skillsanityoption))
            {
                Startup.Logger.LogWarning("Skillsanity is disabled, destroying script.");
                Destroy(this);
            }
        }
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        playerSkills = gameObject.GetComponent<Body>().skills;
        Startup.Logger.LogMessage("Skillsanity is monitoring exp...");
    }

    private void Update()
    {
        if (worldgen.loadingObject.activeSelf && GameObject.Find("ShuttleStarter(Clone)")) // We're in the starting layer of this run
        {
            StartCoroutine(ResetSkills());
            return;
        }
        if (playerSkills.expSTR >= sendStr) // check if we reached a basegame level
        {
            sendStr = Skills.GetExperienceForLevel(playerSkills.STR + 1); // simulate level up without increasing stats
            playerSkills.STR = apMaxStr;
            playerSkills.maxSTR = sendStr;
            int checkID = -966812163 + EXPRequirementToLevel.Get(playerSkills.STR - 1); // find the level
            if (alreadySentChecks.Contains(checkID))
            {
                return;
            }
            APClientClass.ChecksToSendQueue.Enqueue(checkID);
            alreadySentChecks.Add(checkID);
        }
        if (playerSkills.expRES >= sendRes)
        {
            sendRes = Skills.GetExperienceForLevel(playerSkills.RES + 1);
            playerSkills.RES = apMaxRes;
            playerSkills.maxRES = sendRes;
            int checkID = -966812148 + EXPRequirementToLevel.Get(playerSkills.RES - 1);
            if (alreadySentChecks.Contains(checkID))
            {
                return;
            }
            APClientClass.ChecksToSendQueue.Enqueue(checkID);
            alreadySentChecks.Add(checkID);
        }
        if (playerSkills.expINT >= sendInt)
        {
            sendInt = Skills.GetExperienceForLevel(playerSkills.INT + 1);
            playerSkills.INT = apMaxInt;
            playerSkills.maxINT = sendInt;
            int checkID = -966812133 + EXPRequirementToLevel.Get(playerSkills.INT - 1);
            if (alreadySentChecks.Contains(checkID))
            {
                return;
            }
            APClientClass.ChecksToSendQueue.Enqueue(checkID);
            alreadySentChecks.Add(checkID);
        }
    }
    IEnumerator ResetSkills()
    {
        while (worldgen.loadingObject.activeSelf)
        {
            yield return null;
        }
        playerSkills.expSTR = 0;
        playerSkills.expRES = 0;
        playerSkills.expINT = 0;
        sendStr = 60;
        sendRes = 60;
        sendInt = 60;
        playerSkills.STR = apMaxStr;
        playerSkills.RES = apMaxRes;
        playerSkills.INT = apMaxInt;
        playerSkills.UpdateExpBoundaries();
        playerSkills.maxSTR = sendStr;
        playerSkills.maxRES = sendRes;
        playerSkills.maxINT = sendInt;
    }
}