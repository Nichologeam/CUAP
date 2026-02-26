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
        limbs[3].gameObject.SetActive(false);
        limbs[4].dismembered = true; // DownArmF
        limbs[4].gameObject.SetActive(false);
        limbs[5].dismembered = true; // HandF
        limbs[5].gameObject.SetActive(false);
        limbs[6].dismembered = true; // UpArmB
        limbs[6].gameObject.SetActive(false);
        limbs[7].dismembered = true; // DownArmB
        limbs[7].gameObject.SetActive(false);
        limbs[8].dismembered = true; // HandB
        limbs[8].gameObject.SetActive(false);
    }

    public void RestoreLimbs()
    {
        RestoreArm(6, APClientClass.leftArmUnlocks);  // Left arm (B)
        RestoreArm(3, APClientClass.rightArmUnlocks); // Right arm (F)
    }
    // this technically breaks if you get more than 3 progressive arm items, but you would need to be using the server console or !getitem for that to happen anyway
    private void RestoreArm(int startLimb, int unlocks)
    {
        for (int i = 0; i < unlocks; i++)
        {
            var thisLimb = limbs[startLimb + i];
            thisLimb.gameObject.SetActive(true);
            thisLimb.dismembered = false;
            // Limbs still take damage when dismembered. Since you normally can't regain limbs, this bug goes unseen in the normal game
            // But since you CAN regain limbs in Archipelago, I have to fix this, else being sent a Progressive Arm can give you a septic shock jumpscare
            thisLimb.muscleHealth = 100f;
            thisLimb.skinHealth = 100f;
            thisLimb.boneHealTimer = 0f;
            thisLimb.dislocationTimer = 0f;
            thisLimb.infectionAmount = 0f;
            thisLimb.bleedAmount = 0f;
            thisLimb.pain = 0f;
            thisLimb.shrapnel = 0;
            thisLimb.infected = false;
            if (thisLimb == limbs[8] || thisLimb == limbs[5])
            {
                vitals.clawHealth = 100f; // fix basegame oversight (restore claws when unamputating hands)
            }
        }
    }

}
