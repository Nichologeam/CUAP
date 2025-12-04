using CreepyUtil.Archipelago;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace CUAP;

public class SkillChecks : MonoBehaviour
{
    public static ApClient Client;
    public Skills playerSkills;
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
        playerSkills = gameObject.GetComponent<Body>().skills;
        playerSkills.expSTR = 0;
        playerSkills.expRES = 0;
        playerSkills.expINT = 0;
        playerSkills.STR = apMaxStr;
        playerSkills.RES = apMaxRes;
        playerSkills.INT = apMaxInt;
        playerSkills.maxSTR = Skills.GetExperienceForLevel(sendStr + 1);
        playerSkills.maxRES = Skills.GetExperienceForLevel(sendRes + 1);
        playerSkills.maxINT = Skills.GetExperienceForLevel(sendInt + 1);
        Startup.Logger.LogMessage("Skillsanity is monitoring exp...");
    }

    private void Update()
    {
        playerSkills.STR = apMaxStr; // set the level to the AP level
        playerSkills.RES = apMaxRes;
        playerSkills.INT = apMaxInt;
        if (playerSkills.expSTR >= Skills.GetExperienceForLevel(sendStr)) // check if we reached a basegame level
        {
            sendStr = IncreaseSendExp(sendStr); // simulate level up without increasing stats
            int checkID = -966812163 + EXPRequirementToLevel.Get(sendStr); // find the level
            if (alreadySentChecks.Contains(checkID))
            {
                return;
            }
            APClientClass.ChecksToSendQueue.Enqueue(checkID);
            alreadySentChecks.Add(checkID);
        }
        if (playerSkills.expRES >= Skills.GetExperienceForLevel(sendRes))
        {
            sendRes = IncreaseSendExp(sendRes);
            int checkID = -966812148 + EXPRequirementToLevel.Get(sendRes);
            if (alreadySentChecks.Contains(checkID))
            {
                return;
            }
            APClientClass.ChecksToSendQueue.Enqueue(checkID);
            alreadySentChecks.Add(checkID);
        }
        if (playerSkills.expINT >= Skills.GetExperienceForLevel(sendInt))
        {
            sendInt = IncreaseSendExp(sendInt);
            int checkID = -966812133 + EXPRequirementToLevel.Get(sendInt);
            if (alreadySentChecks.Contains(checkID))
            {
                return;
            }
            APClientClass.ChecksToSendQueue.Enqueue(checkID);
            alreadySentChecks.Add(checkID);
        }
        playerSkills.maxSTR = Skills.GetExperienceForLevel(sendStr + 1);
        playerSkills.maxRES = Skills.GetExperienceForLevel(sendRes + 1);
        playerSkills.maxINT = Skills.GetExperienceForLevel(sendInt + 1);
    }

    int IncreaseSendExp(int exptype)
    { // Replicating the game's actual Skills.GetExperienceForLevel function
        if (exptype < 600) // less than level 10
        {
            return exptype + 60;
        }
        else if (exptype == 600) // exactly level 10
        {
            return exptype + 100;
        }
        else if (exptype > 600 && exptype < 1485) // between level 10 and 15
        {
            return (int)(exptype * 1.18);
        }
        return 999999999; // above level 15 (simple way to never send these checks)
    }
}