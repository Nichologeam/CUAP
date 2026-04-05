using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

[HarmonyPatch(typeof(MinigameBase), "StartMinigame")]
// Intercepts the game starting a minigame, first checking if the player has it unlocked before proceeding
class MinigameLocker
{
    private static PlayerCamera plrcam;
    public static bool minigameItemObtained; // only used in one item mode
    public static HashSet<string> minigamesUnlocked = []; // only used in individual mode
    static bool Prefix(MinigameBase __instance, Minigame minigame, Item item)
    {
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        switch (APClientClass.minigameRandom)
        {
            case 1: // disabled
                return true; // run original function
            case 2: // one item mode
                if (minigame is SelfHarmMinigame)
                {
                    return true; // always allow Suicide and Self Harm minigames
                }
                else if (minigameItemObtained)
                {
                    return true; // player has the item. allow original function
                }
                else
                {
                    plrcam.DoAlert($"Minigames are locked by {APCanvas.coloredAPText}");
                    return false; // player does not have the item. don't run function.
                }
            case 3: // individual mode
                switch (minigame)
                {
                    case AmputationMinigame amputation:
                        if (minigamesUnlocked.Contains("Amputation Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Amputation is locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    case BandageMinigame bandage:
                        if (minigamesUnlocked.Contains("Bandage Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Bandages are locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    case DislocationMinigame dislocation:
                        if (minigamesUnlocked.Contains("Dislocation Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Fixing dislocations is locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    case KeypadMinigame keypad:
                        if (minigamesUnlocked.Contains("Keypad Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Keypads are locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    case LockpingMinigame lockpicking: // i have no clue why this is misspelled
                        if (minigamesUnlocked.Contains("Lockpicking Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Lockpicking is locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    case ShrapnelMinigame shrapnel:
                        if (minigamesUnlocked.Contains("Shrapnel Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Removing shrapnel is locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    case SyringeMinigame syringe:
                        if (minigamesUnlocked.Contains("Syringe Minigame"))
                        {
                            return true; // player has this minigame
                        }
                        else
                        {
                            plrcam.DoAlert($"Injection is locked by {APCanvas.coloredAPText}");
                            return false; // player does not
                        }
                    default: // Suicide or Self Harm. always allow these.
                        return true; // run original function
                }
            default: // unset?
                APCanvas.EnqueueArchipelagoNotification($"minigameRandom is an unhandled value! {APClientClass.minigameRandom}",3);
                Startup.Logger.LogError($"minigameRandom is an unhandled value! {APClientClass.minigameRandom}");
                return false;
        }
    }
}