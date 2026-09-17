using HarmonyLib;
using UnityEngine;

namespace CUAP;
public class ElderThornback : MonoBehaviour
{
    BuildingEntity build;
    private void Start()
    {
        if (APClientClass.selectedGoal != 3)
        {
            Destroy(this);
            return;
        }
        build = gameObject.GetComponent<BuildingEntity>();
    }
    private void OnDestroy()
    {
        if (build.health < 0.5 && APClientClass.selectedGoal == 3) // the game checks at 0.5, so I will too
        {
            APClientClass.session.SetGoalAchieved();
        }
    }
}

[HarmonyPatch(typeof(WorldGeneration), "GenerateWorld")]
// Attaches the above script to all Elder Thornbacks after worldgen
class ElderThornbackGoalAttacher
{
    static void Postfix(WorldGeneration __instance)
    {
        foreach (var elder in Object.FindObjectsOfType<SpiderHandlerTBE>())
        {
            if (!elder.gameObject.GetComponent<ElderThornback>()) // This should never return true since worldgen can only run once per layer, but better safe than sorry
            {
                elder.gameObject.AddComponent<ElderThornback>();
            }
        }
    }
}