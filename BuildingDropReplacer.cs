using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace CUAP;

[HarmonyPatch(typeof(BuildingEntity), "Start")]
// Guarentees an Archipelago item will drop from all destroyed Buildings, severely helping with Randomize Blueprints on
class BuildingDropReplacer
{
    private static readonly HashSet<string> allowedBuildings = new() // objects not in this list cannot have their drop tables changed
    {// this list is in no particular order
        // containers
        "lifepodchest",
        "containercrate",
        "medcrate",
        "foodbox",
        "dropcapsule",
        // enemies
        "shadecrawler",
        "thornbackyoung",
        "thornbackelder",
        "overgrowntick", // not the normal cave ticks, because that can spam AP items if a Cave Ticks Trap is received
        "snowstrider", // futureproofing (doesn't spawn naturally in the latest demo)
        "wallbiter",
        // traps
        "spikestabber",
        "jumppad",
        "soundcannon",
        "landmine",
        "coil", // not even sure how you'd manage to destroy one of these, but you'd get an AP item out of it if you do!
        "bananaplant",
        "turret",
        "grabberplant",
        "sidestabber", // futureproofing (doesn't spawn naturally in the latest demo)
        "radbarrel", // good luck getting this one!
        "spentfuel", // and this one too!
        "beartrap",
        "stalactite",
        // other
        "trader", // all traders share the same building ID
        "corpse", // same with all corpses
        "glassshards"
    };
    static void Postfix(BuildingEntity __instance)
    {
        if (!allowedBuildings.Contains(__instance.id))
        {
            return; // banned object. do not change.
        }
        if (CraftingChecks.bpLocations) // only do this if Archipelago items over blueprints is enabled
        {// I am aware that this can cause a race condition if a BuildingEntity exists on the frame the scene is loaded, but this shouldn't ever happen (outside of the tutorial)
            var dropsList = __instance.alwaysDrop?.ToList() ?? new List<ItemDrop>(); // copy to list to make editing easier (also make a list if it doesn't exist)
            dropsList.Add(new ItemDrop // Add an item to the list of guarented drops
            {
                id = "blueprint", // replaced with Archipelago item
                conditionMax = 1,
                conditionMin = 1,
            });
            __instance.alwaysDrop = dropsList.ToArray();
        }
    }
}