using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CUAP;

[HarmonyPatch(typeof(WorldGeneration), "FinishWorldGeneration")]
// Dynamically replaces Survivor note texts with Archipelago hints
class NoteHints
{
    private static FieldInfo textInput = AccessTools.Field(typeof(SurvivorNote), "loreString");
    private static FieldInfo image = AccessTools.Field(typeof(SurvivorNote), "loreSprite");
    static async void Postfix(WorldGeneration __instance)
    {
        Startup.Logger.LogWarning("Postfix Entered");
        var missing = APClientClass.session.Locations.AllMissingLocations.ToList(); // get all missing locations
        if (missing == null || missing.Count == 0)
        {
            return; // there are no locations left, so keep the contents of this note vanilla
        }
        foreach (var note in UnityEngine.Object.FindObjectsOfType<SurvivorNote>(true))
        {
            Startup.Logger.LogWarning($"processing note at {note.gameObject.transform.position}");
            int chosenLoc = UnityEngine.Random.Range(0, missing.Count);
            long locID = missing[chosenLoc]; // pick a random location
            try
            {
                var scouted = await APClientClass.session.Locations.ScoutLocationsAsync(locID); // scout it
                var itemInfo = scouted[locID]; // ScoutLocationsAsync returns a dictionary, but we don't care for that. just pick out the info and leave the dictionary
                string constructedString = APLocale.GetRandomNote(itemInfo.Flags); // get a random message
                constructedString = constructedString.Replace("<plr>", itemInfo.Player.Name); // dynamic replacements...
                constructedString = constructedString.Replace("<item>", itemInfo.ItemName);
                constructedString = constructedString.Replace("<loc>", itemInfo.LocationName);
                textInput.SetValue(note, constructedString); // set the note to display the custom message
                image.SetValue(note, Startup.apassets.LoadAsset<Sprite>("aplogo200")); // load the Archipelago logo as the image used in the note
            }
            catch (Exception ex)
            {
                Startup.Logger.LogError($"Scouting failed during NoteHint! {ex}");
                textInput.SetValue(note, APLocale.Get("noteScoutError", APLocale.APLanguageType.Errors)); // display error message in the note
            }
        }
    }
}