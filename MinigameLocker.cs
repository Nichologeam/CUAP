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
        { typeof(AmputationMinigame), ("Amputation Minigame", APLocale.Get("amputation", APLocale.APLanguageType.UI)) },
        { typeof(BandageMinigame), ("Bandage Minigame", APLocale.Get("bandages", APLocale.APLanguageType.UI)) },
        { typeof(DislocationMinigame), ("Dislocation Minigame", APLocale.Get("dislocations", APLocale.APLanguageType.UI)) },
        { typeof(KeypadMinigame), ("Keypad Minigame", APLocale.Get("keypads", APLocale.APLanguageType.UI)) },
        { typeof(LockpingMinigame), ("Lockpicking Minigame", APLocale.Get("lockpicking", APLocale.APLanguageType.UI)) },
        { typeof(ShrapnelMinigame), ("Shrapnel Minigame", APLocale.Get("shrapnel", APLocale.APLanguageType.UI)) },
        { typeof(SyringeMinigame), ("Syringe Minigame", APLocale.Get("injection", APLocale.APLanguageType.UI)) },
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
                plrcam.DoAlert($"{APLocale.Get("minigames", APLocale.APLanguageType.UI)}{APCanvas.coloredAPText}");
                return false; // player doesn't have the item

            case 3:
                if (MinigameMap.TryGetValue(minigame.GetType(), out var data))
                {
                    if (minigamesUnlocked.Contains(data.itemName))
                    {
                        return true; // player has the respective item
                    }
                    plrcam.DoAlert($"{data.message}{ APLocale.Get("locked", APLocale.APLanguageType.UI)} {APCanvas.coloredAPText}");
                    return false; // player does not
                }
                return true; // unknown minigame (should only be Suicide and Self Harm)

            default: // unset?
                APCanvas.EnqueueArchipelagoNotification($"{APLocale.Get("minigameUnhandled", APLocale.APLanguageType.Errors)}{APClientClass.minigameRandom}", 3);
                Startup.Logger.LogError($"minigameRandom is an unhandled value! {APClientClass.minigameRandom}");
                return false;
        }
    }
}