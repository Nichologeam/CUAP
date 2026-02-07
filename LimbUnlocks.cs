using Archipelago.MultiClient.Net;
using System;
using UnityEngine;

namespace CUAP;

public class LimbUnlocks : MonoBehaviour
{
    public static LimbUnlocks instance;
    private Body vitals;
    private Limb[] limbs;

    private void Start()
    {
        instance = this;
        vitals = this.gameObject.GetComponent<Body>();
        limbs = vitals.limbs;
        var options = APClientClass.slotdata;
        if (options.TryGetValue("LimbUnlocks", out object limbsoption))
        {
            if (!Convert.ToBoolean(limbsoption)) // option is disabled
            {
                Startup.Logger.LogWarning("Limb Unlocks are disabled, destroying script");
                DestroyImmediate(this);
                return;
            }
            AmputateAllLimbs();
            RestoreLimbs();
        }
    }

    private void AmputateAllLimbs()
    {
        limbs[3].dismembered = true; // UpArmF
        limbs[4].dismembered = true; // DownArmF
        limbs[5].dismembered = true; // HandF
        limbs[6].dismembered = true; // UpArmB
        limbs[7].dismembered = true; // DownArmB
        limbs[8].dismembered = true; // HandB
    }

    public void RestoreLimbs()
    {
        RestoreArm(3, APClientClass.leftArmUnlocks);  // Left arm (F)
        RestoreArm(6, APClientClass.rightArmUnlocks); // Right arm (B)
    }
    // this technically breaks if you get more than 3 progressive arm items, but you would need to be using the server console or !getitem for that to happen anyway
    private void RestoreArm(int startLimb, int unlocks)
    {
        for (int i = 0; i < unlocks; i++)
        {
            limbs[startLimb + i].dismembered = false;
        }
    }

}
