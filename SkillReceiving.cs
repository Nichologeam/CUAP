using Archipelago.MultiClient.Net;
using System;
using System.Collections;
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
        APCanvas.skillsanityEnabled = true;
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        Startup.Logger.LogMessage("Skillsanity is monitoring exp...");
        StartCoroutine(ResetSkills());
    }

    private void Update()
    {
        if (worldgen.loadingObject.activeSelf)
        {
            return;
        }
        if (playerSkills == null)
        {
            playerSkills = gameObject.GetComponent<Body>().skills;
        }
        playerSkills.UpdateExpBoundaries();
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
        playerSkills = gameObject.GetComponent<Body>().skills;
        playerSkills.expSTR = 0;
        playerSkills.expRES = 0;
        playerSkills.expINT = 0;
        playerSkills.STR = APClientClass.MaxSTR;
        playerSkills.RES = APClientClass.MaxRES;
        playerSkills.INT = APClientClass.MaxINT;
    }
}