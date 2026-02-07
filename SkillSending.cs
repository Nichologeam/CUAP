using Archipelago.MultiClient.Net;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

[HarmonyPatch(typeof(Skills), "AddExp")]
// Intercepts the game's exp adding system, redirecting it to skillsanity instead
class SkillSending
{
    private static ArchipelagoSession Client;
    private static PlayerCamera plrcam;
    private static int APSTR = 0; // what the current skillsanity STR level is (not the player's actual level, nor received skillsanity levels)
    private static int APRES = 0; // same for RES...
    private static int APINT = 0; // ... and for INT
    private static float toNextSTR = 60; // how much exp is needed to level up the skillsanity STR
    private static float toNextRES = 60; // same for RES...
    private static float toNextINT = 60; // ... and for INT
    private static List<int> EXPRequirementToLevel = new List<int>()
    {
        60, // at level 0, this is how much is required to level up
        120, // at level 1, this is how much
        180, // at level 2, etc
        240, // level 3
        300, // level 4
        360, // level 5
        420, // level 6
        480, // level 7
        540, // level 8
        640, // level 9
        758, // level 10
        897, // level 11
        1062, // level 12
        1255, // level 13
        1484, // level 14. the last check is for reaching level 15 (and is configureable via .yaml option)
        1483 // one less to return -1 when calculating level (see APCanvas.UpdateSkillsanityValues)
    };
    static bool Prefix(Skills __instance, int stat, float xp)
    {
        Client = APClientClass.session;
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        if (Client is null || !APCanvas.skillsanityEnabled)
        {
            return true; // either we're not connected (shouldn't be possible), or skillsanity is disabled. let the basegame function run
        }
        if (APSTR == 0 && APRES == 0 && APINT == 0) // all skills are 0. check if this is a new server
        {
            // >>>>> NOTE: I am aware this system breaks if the skillsanity checks are part of another player's collect. <<<<<
            // >>>>> NOTE: I don't know how I'd solve this, nor do I really care to, since all locations are still reachable. <<<<<
            if (Client.Locations.AllLocationsChecked.Contains(22318800)) // "STR Level 1" location. if found, this is NOT a new server
            {
                APSTR = GetSkillsanityLevelFromCheckedLocations(22318800);
                toNextSTR = EXPRequirementToLevel[APSTR] - EXPRequirementToLevel[APSTR - 1];
                APCanvas.UpdateSkillsanityValues(0, MathF.Round(toNextSTR, 1));
            }
            if (Client.Locations.AllLocationsChecked.Contains(22318815)) // "RES Level 1" location
            {
                APRES = GetSkillsanityLevelFromCheckedLocations(22318815);
                toNextRES = EXPRequirementToLevel[APRES] - EXPRequirementToLevel[APRES - 1];
                APCanvas.UpdateSkillsanityValues(1, MathF.Round(toNextRES, 1));
            }
            if (Client.Locations.AllLocationsChecked.Contains(22318830)) // "INT Level 1" location
            {
                APINT = GetSkillsanityLevelFromCheckedLocations(22318830);
                toNextINT = EXPRequirementToLevel[APINT] - EXPRequirementToLevel[APINT - 1];
                APCanvas.UpdateSkillsanityValues(2, MathF.Round(toNextINT, 1));
            }
        }
        float multexp = xp * Skills.xpGainMult;
        switch (stat)
        {
            case 0: // STR
                if (APSTR >= 15)
                {
                    APCanvas.UpdateSkillsanityValues(0, -1);
                    break; // we've reached all checks for this skill.
                }
                toNextSTR -= multexp; // apply the earned exp to the count
                if (toNextSTR <= 0) // have we reached a new level?
                {
                    APClientClass.ChecksToSend.Add(22318800 + APSTR); // "STR Level 1" ID plus current level. max 15
                    APSTR++; // increase the skillsanity level count
                    plrcam.DoAlert($"Skillsanity STR level up! [{APSTR - 1}] -> [{APSTR}]"); // display popup
                    Sound.Play("music/levelup", Vector2.zero, true, false, null, 1f, 1f, true, true); // play vanilla levelup sound
                    toNextSTR += EXPRequirementToLevel[APSTR] - EXPRequirementToLevel[APSTR - 1]; // set new experience requirement
                }
                APCanvas.UpdateSkillsanityValues(0, MathF.Round(toNextSTR, 1));
                break;
            case 1: // RES
                if (APRES >= 15)
                {
                    APCanvas.UpdateSkillsanityValues(1, -1);
                    break; // we've reached all checks for this skill.
                }
                toNextRES -= multexp; // apply the earned exp to the count
                if (toNextRES <= 0) // have we reached a new level?
                {
                    APClientClass.ChecksToSend.Add(22318815 + APRES); // "RES Level 1" ID plus current level. max 15
                    APRES++; // increase the skillsanity level count
                    plrcam.DoAlert($"Skillsanity RES level up! [{APRES - 1}] -> [{APRES}]"); // display popup
                    Sound.Play("music/levelup", Vector2.zero, true, false, null, 1f, 1f, true, true); // play vanilla levelup sound
                    toNextRES += EXPRequirementToLevel[APRES] - EXPRequirementToLevel[APRES - 1]; // set new experience requirement
                }
                APCanvas.UpdateSkillsanityValues(1, MathF.Round(toNextRES, 1));
                break;
            case 2: // INT
                if (APINT >= 15)
                {
                    APCanvas.UpdateSkillsanityValues(2, -1);
                    break; // we've reached all checks for this skill.
                }
                toNextINT -= multexp; // apply the earned exp to the count
                if (toNextINT <= 0) // have we reached a new level?
                {
                    APClientClass.ChecksToSend.Add(22318830 + APINT); // "INT Level 1" ID plus current level. max 15
                    APINT++; // increase the skillsanity level count
                    plrcam.DoAlert($"Skillsanity INT level up! [{APINT - 1}] -> [{APINT}]"); // display popup
                    Sound.Play("music/levelup", Vector2.zero, true, false, null, 1f, 1f, true, true); // play vanilla levelup sound
                    toNextINT += EXPRequirementToLevel[APINT] - EXPRequirementToLevel[APINT - 1]; // set new experience requirement
                }
                APCanvas.UpdateSkillsanityValues(2, MathF.Round(toNextINT, 1));
                break;
            default: // the basegame just returns INT if the skill is invalid. Not doing that won't cause issues, right Orsoniks?
                Startup.Logger.LogError("Skillsanity Error: SkillSending Harmony patch received an invalid skill! This is (technically) a basegame bug!");
                APCanvas.EnqueueArchipelagoNotification("Skillsanity Error: SkillSending Harmony patch received an invalid skill! This is (technically) a basegame bug!",3);
                return false; // void this invalid exp
        }
        return false; // tell harmony not to run the basegame function
    }
    // Starting at level 1, find the first unchecked Skillsanity location, then assume that's the player's Skillsanity level.
    // Note: Not immune to players goaling on collect-enabled servers, or running !collect manually, but all locations are still reachable.
    private static int GetSkillsanityLevelFromCheckedLocations(long baseLocationID, int maxLevel = 15)
    {
        for (int level = 1; level <= maxLevel; level++)
        {
            if (!Client.Locations.AllLocationsChecked.Contains(baseLocationID + level)) // we don't have this one
            {
                return level - 1; // return that we have the one before it
            }
        }
        return maxLevel; // we have all of them
    }
}