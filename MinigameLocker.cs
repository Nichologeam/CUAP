using HarmonyLib;
using System;
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
    private static readonly Dictionary<Type, (string itemName, string message)> MinigameMap = new()
    {
        { typeof(AmputationMinigame), ("Amputation Minigame", "Amputation is") },
        { typeof(BandageMinigame), ("Bandage Minigame", "Bandages are") },
        { typeof(DislocationMinigame), ("Dislocation Minigame", "Fixing dislocations is") },
        { typeof(KeypadMinigame), ("Keypad Minigame", "Keypads are") },
        { typeof(LockpingMinigame), ("Lockpicking Minigame", "Lockpicking is") },
        { typeof(ShrapnelMinigame), ("Shrapnel Minigame", "Removing shrapnel is") },
        { typeof(SyringeMinigame), ("Syringe Minigame", "Injection is") },
    };
    static bool Prefix(MinigameBase __instance, Minigame minigame, Item item)
    {
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();

        switch (APClientClass.minigameRandom)
        {
            case 1: // disabled
                return true; // allow basegame function
            case 2: // one item mode
                if (minigame is SelfHarmMinigame)
                {
                    return true; // always allow Suicide and Self Harm
                }
                else if (minigameItemObtained)
                {
                    return true; // player has the item
                }
                plrcam.DoAlert($"Minigames are locked by {APCanvas.coloredAPText}");
                return false; // player doesn't have the item

            case 3:
                if (MinigameMap.TryGetValue(minigame.GetType(), out var data))
                {
                    if (minigamesUnlocked.Contains(data.itemName))
                    {
                        return true; // player has the respective item
                    }
                    plrcam.DoAlert($"{data.message} locked by {APCanvas.coloredAPText}");
                    return false; // player does not
                }
                return true; // unknown minigame (should only be Suicide and Self Harm)

            default: // unset?
                APCanvas.EnqueueArchipelagoNotification($"minigameRandom is an unhandled value! {APClientClass.minigameRandom}", 3);
                Startup.Logger.LogError($"minigameRandom is an unhandled value! {APClientClass.minigameRandom}");
                return false;
        }
    }
}