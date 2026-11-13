using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

public class LimbUnlocks : MonoBehaviour
{
    public static LimbUnlocks instance;
    private static Body vitals;
    private static bool livingLast;
    public static Dictionary<int, int> limbUpgrades = new Dictionary<int, int>()
    {
        {0,0},
        {1,0},
        {2,0},
        {3,0},
        {4,0},
        {5,0},
        {6,0},
        {7,0},
        {8,0},
        {9,0},
        {10,0},
        {11,0},
        {12,0},
        {13,0},
        {14,0}
    };

    private static Dictionary<string, int> ReadableLimbToID = new Dictionary<string, int>()
    {
        {"Head",0},
        {"Thorax",1},
        {"Abdomen",2},
        {"Upper Right Arm",3},
        {"Upper right arm",3},
        {"Right Forearm",4},
        {"Right forearm",4},
        {"Right Hand",5},
        {"Right hand",5},
        {"Upper Left Arm",6},
        {"Upper left arm",6},
        {"Left Forearm",7},
        {"Left forearm",7},
        {"Left Hand",8},
        {"Left hand",8},
        {"Right Thigh",9},
        {"Right thigh",9},
        {"Right Crus",10},
        {"Right crus",10},
        {"Right Foot",11},
        {"Right foot",11},
        {"Left Thigh",12},
        {"Left thigh",12},
        {"Left Crus",13},
        {"Left crus",13},
        {"Left Foot",14},
        {"Left foot",14}
    };

    private void Start()
    {
        instance = this;
        vitals = this.gameObject.GetComponent<Body>();
        var options = APClientClass.slotdata;
        if (options.TryGetValue("LimbUnlocks", out object limbsoption))
        {
            switch (Convert.ToInt32(limbsoption))
            {
                case 1:
                    Startup.Logger.LogWarning("Limb Unlocks are disabled, destroying script");
                    DestroyImmediate(this);
                    return;
                case 2:
                    livingLast = false;
                    break;
                case 3:
                    livingLast = true;
                    break;
            }
            CheckLimbUnlocks();
        }
    }

    private async void CheckLimbUnlocks()
    {
        WoodPegAllLimbs();
        foreach (KeyValuePair<int, int> unlocks in limbUpgrades)
        {
            if (unlocks.Value > 0)
            {
                Limb limbObject = vitals.limbs[unlocks.Key];
                int unlocksLeft = unlocks.Value;
                while (unlocksLeft > 0)
                {
                    GiveNextProsthesisToLimb(limbObject.fullName);
                    unlocksLeft--;
                }
            }
        }
    }

    private void WoodPegAllLimbs()
    {
        foreach (Limb limb in vitals.limbs)
        {
            SetLimbToProsthesis(limb, 0); // bypasses readable name translation
        }
    }

    // sets the given limb to a specific prosthesis
    private void SetLimbToProsthesis(Limb limbObject, int prosthesis) // 0 = Wood, 1 = Steel, 2 = P.E.O., 3 = Living
    {
        limbObject.Reattach();
        switch (prosthesis)
        {
            case 0:
                limbObject.prostheticInfo = Item.PegLimbInfo(); // only the peg has stored stats in the base game (for some reason???)
                break;
            case 1:
                limbObject.prostheticInfo = new Limb.ProstheticInfo
                {
                    hasProsthetic = true,
                    itemId = "steelprosthesis",
                    forceChange = 0f,
                    armorChange = 0.2f,
                    destroyItem = "scrapmetal",
                    appearancePath = "Sprites/steelprosthesisoverlay",
                    maxHoles = 15,
                    r = 0.5f,
                    g = 0.5f,
                    b = 0.5f
                };
                break;
            case 2:
                limbObject.prostheticInfo = new Limb.ProstheticInfo
                {
                    hasProsthetic = true,
                    itemId = "peo",
                    forceChange = 0.25f,
                    armorChange = 0.6f,
                    destroyItem = "titaniumsheet",
                    appearancePath = "Sprites/peooverlay",
                    maxHoles = 20,
                    r = 0.361f,
                    g = 0.729f,
                    b = 0.729f
                };
                break;
            case 3:
                limbObject.prostheticInfo = new Limb.ProstheticInfo // this is actually how the vanilla game sets these values
                {
                    hasProsthetic = false,
                    itemId = "",
                    forceChange = 0,
                    armorChange = 0,
                    destroyItem = "",
                    appearancePath = "",
                    maxHoles = 0,
                    r = 0,
                    g = 0,
                    b = 0
                };
                break;
        }
        AccessTools.FieldRef<Limb, Material> mat = AccessTools.FieldRefAccess<Limb, Material>("mat");
        if (prosthesis == 3)
        {
            mat(limbObject).SetTexture("_OverlayTex", null);
            mat(limbObject).SetFloat("_OverlayVis", 0f);
        }
        else
        {
            mat(limbObject).SetTexture("_OverlayTex", Resources.Load<Sprite>(limbObject.prostheticInfo.appearancePath).texture);
            mat(limbObject).SetFloat("_OverlayVis", 1f);
        }
    }
    private void SetLimbToProsthesis(string limbName, int prosthesis) // string variant (readable names)
    {
        if (ReadableLimbToID.TryGetValue(limbName, out int limbID))
        {
            Limb wantedLimb = vitals.limbs[limbID];
            SetLimbToProsthesis(wantedLimb, prosthesis);
        }
        else
        {
            Startup.Logger.LogError($"Ran the string version of SetLimbToProstheis on an invalid limb! ({limbName})");
        }
    }

    // reads the given limb's prosthesis, and upgrades it to the next one, accounting for livingLast
    private void GiveNextProsthesisToLimb(Limb limbObject, int upgrade)
    {
        switch (upgrade)
        {
            case 0: // peglimb
                if (livingLast)
                {
                    SetLimbToProsthesis(limbObject, 1); // set to steel
                }
                else
                {
                    SetLimbToProsthesis(limbObject, 3); // set to living
                }
                break;
            case 1: // steel
                SetLimbToProsthesis(limbObject, 2); // set to P.E.O. regardless of option
                break;
            case 2: // peo
                if (livingLast)
                {
                    SetLimbToProsthesis(limbObject, 3); // set to living
                }
                else
                {
                    Startup.Logger.LogError($"Received too many unlocks for {limbObject.fullName}!");
                }
                break;
            case 3: // living
                if (livingLast)
                {
                    Startup.Logger.LogError($"Received too many unlocks for {limbObject.fullName}!");
                }
                else
                {
                    SetLimbToProsthesis(limbObject, 1); // set to steel
                }
                break;
        }
    }
    public static void GiveNextProsthesisToLimb(string limbName) // string variant (readable names)
    {
        if (ReadableLimbToID.TryGetValue(limbName, out int limbID) && limbUpgrades.TryGetValue(limbID, out int upgrade))
        {
            Limb wantedLimb = vitals.limbs[limbID];
            instance.GiveNextProsthesisToLimb(wantedLimb, upgrade - 1);
        }
        else
        {
            Startup.Logger.LogError($"Ran the string version of GiveNextProsthesisToLimb on an invalid limb! ({limbName})");
        }
    }
}

[HarmonyPatch(typeof(Limb), "Dismember")]
// Attaches the above script to all Elder Thornbacks after worldgen
class VitalDismembermentPatch
{
    static bool Prefix(Limb __instance)
    {
        if (__instance.isAbdomen || __instance.isHead || __instance.isVital)
        {
            // if this is a limb that cannot be dismembered, set health to a low value and skip amputation
            __instance.muscleHealth = 1;
            return false;
        }
        return true;
    }
}