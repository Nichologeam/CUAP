using Archipelago.MultiClient.Net;
using HarmonyLib;

namespace CUAP;
// This file is for any code unique to checking for the game's endings

[HarmonyPatch(typeof(WorldGeneration), "SurfaceBreachCutscene")]
// Checks for completion of the "Complete the Ascent" goal
class ReachSurfacePatch
{
    static void Prefix(WorldGeneration __instance)
    {
        ArchipelagoSession Client = APClientClass.session;
        if (APClientClass.selectedGoal == 4) // Complete the Ascent
        {
            if (APClientClass.goalNeedsAllSawians)
            {
                Startup.CheckHasAllSawians();
            }
            else
            {
                Client.SetGoalAchieved();
            }
        }
    }
}

[HarmonyPatch(typeof(WorldGeneration), "EscapeEnding")]
// Checks for completion of the "Escape Containment" goal
class EscapeEndingPatch
{
    static void Prefix(WorldGeneration __instance)
    {
        ArchipelagoSession Client = APClientClass.session;
        if (APClientClass.selectedGoal == 5) // Escape Containment
        {
            if (APClientClass.goalNeedsAllSawians)
            {
                Startup.CheckHasAllSawians();
            }
            else
            {
                Client.SetGoalAchieved();
            }
        }
    }
}

[HarmonyPatch(typeof(WorldGeneration), "MercEnding")]
// Checks for completion of the "M.E.R.C." goal
class MercEndingPatch
{
    static void Prefix(WorldGeneration __instance)
    {
        ArchipelagoSession Client = APClientClass.session;
        if (APClientClass.selectedGoal == 6) // M.E.R.C
        {
            if (APClientClass.goalNeedsAllSawians)
            {
                Startup.CheckHasAllSawians();
            }
            else
            {
                Client.SetGoalAchieved();
            }
        }
    }
}