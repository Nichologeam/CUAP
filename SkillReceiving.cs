using Archipelago.MultiClient.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

public class SkillReceiving : MonoBehaviour
{
    public static ArchipelagoSession Client;
    public static Skills playerSkills;
    private WorldGeneration worldgen;
    private void OnEnable()
    {
        Client = APClientClass.session;
        var options = APClientClass.slotdata;
        if (options.TryGetValue("Skillsanity", out var skillsanityoption)) // check if skillsanity is enabled.
        {
            if (!Convert.ToBoolean(skillsanityoption))
            {
                APCanvas.skillsanityEnabled = false;
                Startup.Logger.LogWarning("Skillsanity is disabled, destroying script.");
                DestroyImmediate(this);
                return;
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
        playerSkills.STR = APClientClass.MaxSTR;
        playerSkills.expSTR = Skills.GetExperienceForLevel(APClientClass.MaxSTR - 1);
        playerSkills.RES = APClientClass.MaxRES;
        playerSkills.expRES = Skills.GetExperienceForLevel(APClientClass.MaxRES - 1);
        if (TrapHandler.mindwipeActive) return;
        playerSkills.INT = APClientClass.MaxINT;
        playerSkills.expINT = Skills.GetExperienceForLevel(APClientClass.MaxINT - 1);
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
        playerSkills.STR = APClientClass.MaxSTR;
        playerSkills.RES = APClientClass.MaxRES;
        playerSkills.INT = APClientClass.MaxINT;
        playerSkills.UpdateExpBoundaries();
    }
}