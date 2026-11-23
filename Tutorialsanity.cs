using HarmonyLib;

namespace CUAP;

[HarmonyPatch(typeof(TutorialHandler), "CompleteTut")]
// Sends a tutorial's respective location after completing it
class TutorialLocationChecker
{
    static void Prefix(int index)
    {
        Startup.Logger.LogMessage($"Tutorial with ID {index} completed.");
        long locationID = 0;
        switch (index)
        {
            case 0: // Basic Training
                locationID = 22319050;
                break;
            case 1: // First Aid Course
                locationID = 22319051;
                break;
            case 2: // Crafting Course
                locationID = 22319052;
                break;
            case 3: // Movement Course
                locationID = 22319053;
                break;
            default:
                Startup.Logger.LogWarning("Unknown tutorial course completed. Probably modded course.");
                break;
        }
        if (locationID != 0)
        {
            APClientClass.ChecksToSend.Add(locationID);
        }
    }
}