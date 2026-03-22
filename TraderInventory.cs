using HarmonyLib;

namespace CUAP;

[HarmonyPatch(typeof(TraderScript), "GenerateInventory")]
// Guarentees an Archipelago item will be inside each trader's inventory
class TraderInventory
{
    static void Postfix(TraderScript __instance)
    {
        if (!CraftingChecks.apItems)
        {
            return; // blueprint locations are disabled
        }
        __instance.items.Insert(0, new TraderItem // insert at index 0 to guarentee it's at the top of the list
        {
            preference = 0, // always inside the Wants Trade category, making it near the top and cheap
            id = "blueprint",
            value = 0 // free
        });
    }
}