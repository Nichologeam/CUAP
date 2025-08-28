using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Drawing;
using System.IO;
using System.Collections.Concurrent;
using Archipelago.MultiClient.Net.Models;
using static System.Collections.Specialized.BitVector32;
using Newtonsoft.Json.Linq;

namespace CUAP;

public class CraftingChecks : MonoBehaviour
{
    public static ApClient Client;
    private static List<string> RecievedRecipes;
    private List<int> AlreadySentChecks = new List<int>();
    private static Dictionary<int, int> RecipeIDtoCheckID = new Dictionary<int, int>()
    {
        {0,-966812947},
        {1,-966812946},
        {2,-966812945},
        {3,-966812944},
        {4,-966812943},
        {5,-966812942},
        {6,-966812941},
        {7,-966812940},
        {8,-966812939},
        {9,-966812938},
        {10,-966812937},
        {11,-966812936},
        {12,-966812935},
        {13,-966812934},
        {14,-966812933},
        {15,-966812932},
        {16,-966812931},
        {17,-966812930},
        {18,-966812929},
        {19,-966812928},
        {20,-966812927},
        {21,-966812926},
        {22,-966812925},
        {23,-966812924},
        {24,-966812923},
        {25,-966812922},
        {26,-966812921},
        {27,-966812920},
        {28,-966812919},
        {29,-966812918},
        {30,-966812917},
        {31,-966812916},
        {32,-966812915},
        {33,-966812914},
        {34,-966812913},
        {35,-966812912},
        {36,-966812911},
        {37,-966812910},
        {38,-966812909},
        {39,-966812908},
        {40,-966812907},
        {41,-966812906},
        {42,-966812905},
        {43,-966812904},
        {44,-966812903},
        {45,-966812902},
        {46,-966812901},
        {47,-966812900},
        {48,-966812899},
        {49,-966812898},
        {50,-966812897},
        {51,-966812896},
        {52,-966812895},
        {53,-966812894},
        {54,-966812893},
        {55,-966812892},
        {56,-966812891},
        {57,-966812890},
        {58,-966812889},
        {59,-966812888},
        {60,-966812887},
        {61,-966812886},
        {62,-966812885},
        {63,-966812884},
        {64,-966812883},
        {65,-966812882},
        {66,-966812881},
        {67,-966812880},
        {68,-966812879},
        {69,-966812878},
        {70,-966812877},
        {71,-966812876},
        {72,-966812875},
        {73,-966812874},
        {74,-966812873},
        {75,-966812872},
        {76,-966812871},
        {77,-966812870},
    };
    private static Dictionary<string, int> RecipeNametoID = new Dictionary<string, int>()
    {
        { "Neural booster Recipe", 0 },
        { "Scaffolding Recipe", 1 },
        { "Foliage rope (from foliage) Recipe", 2 },
        { "Foliage rope (from dry foliage) Recipe", 3 },
        { "Foliage (from dry foliage) Recipe", 4 },
        { "Foliage rope (from musharm) Recipe", 5 },
        { "Sterilized dressing Recipe", 6 },
        { "Dressing (from ripped dressing) Recipe", 7 },
        { "Burger Recipe", 8 },
        { "Ripped dressing (from foliage) Recipe", 9 },
        { "Ripped dressing (from dried foliage) Recipe", 10 },
        { "Canteen Recipe", 11 },
        { "Filtered canteen Recipe", 12 },
        { "Large carcass Recipe", 13 },
        { "Carcass splint Recipe", 14 },
        { "Metal digging tool Recipe", 15 },
        { "Pickaxe Recipe", 16 },
        { "Reinforced rope Recipe", 17 },
        { "Makeshift L.R.D. Recipe", 18 },
        { "L.R.D. Recipe", 19 },
        { "Bruise kit Recipe", 20 },
        { "Splint Recipe", 21 },
        { "Foliage bag Recipe", 22 },
        { "Sling bag Recipe", 23 },
        { "Relief cream Recipe", 24 },
        { "Antiseptic Recipe", 25 },
        { "Blood bag (from alloy) Recipe", 26 },
        { "Blood bag (from empty bag) Recipe", 27 },
        { "Blood bag (from filled bag) Recipe", 28 },
        { "Blood bag (from yellow flesh) Recipe", 29 },
        { "Dressing (from musharm) Recipe", 30 },
        { "Opium Recipe", 31 },
        { "Morphine (from opium) Recipe", 32 },
        { "Fentanyl Recipe", 33 },
        { "Lantern Recipe", 34 },
        { "Antidepressants Recipe", 35 },
        { "Turbulent crystal shard Recipe", 36 },
        { "Flammable powder (from stuck fruit) Recipe", 37 },
        { "Flammable powder (from freed fruit) Recipe", 38 },
        { "Dynamite Recipe", 39 },
        { "Magazine base Recipe", 40 },
        { "Small magazine Recipe", 41 },
        { "Rifle magazine Recipe", 42 },
        { "Box of 12-Gauge Recipe", 43 },
        { "Makeshift rifle Recipe", 44 },
        { "Scrap metal Recipe", 45 },
        { "9mm round Recipe", 46 },
        { "12-Gauge buckshot Recipe", 47 },
        { "5.56 round Recipe", 48 },
        { "Tweezers Recipe", 49 },
        { "Metal alloy Recipe", 50 },
        { "Advanced scuba diving gear Recipe", 51 },
        { "Makeshift digging tool Recipe", 52 },
        { "Bone welding tool Recipe", 53 },
        { "Clotting mush Recipe", 54 },
        { "Procoagulant Recipe", 55 },
        { "Mini laser drill Recipe", 56 },
        { "Makeshift helmet Recipe", 57 },
        { "Bicycle helmet Recipe", 58 },
        { "Makeshift headlamp Recipe", 59 },
        { "Duffel bag Recipe", 60 },
        { "Tourniquet Recipe", 61 },
        { "Plastic dressing Recipe", 62 },
        { "Limb wraps Recipe", 63 },
        { "Headlamp Recipe", 64 },
        { "Bowl of cereal Recipe", 65 },
        { "Naltrexone Recipe", 66 },
        { "Blood sac Recipe", 67 },
        { "Antiseptic mush Recipe", 68 },
        { "Morphine (from relief crystal) Recipe", 69 },
        { "Grav-Bag Recipe", 70 },
        { "Terrain scanner Recipe", 71 },
        { "Auto-auto-pump Recipe", 72 },
        { "Climbing claws Recipe", 73 },
        { "Blood bag (from crystal) Recipe", 74 },
        { "Ice pack Recipe", 75 },
        { "Flashlight Recipe", 76 },
        { "Foliage (from fungus chunk) Recipe", 77 },
    };

    private void OnEnable()
    {
        Client = APClientClass.Client;
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("RandomizeRecipes", out var recipesoption)) // check if recipe randomization is enabled.
        {
            if (!Convert.ToBoolean(recipesoption))
            {
                Startup.Logger.LogWarning("Recipe Randomization is disabled, destroying script.");
                Destroy(this);
            }
        }
        foreach (Recipe recipe in Recipes.recipes)
        {
            recipe.alwaysShow = false; // Unlearn every recipe upon first connecting. We will recieve them with items later.
        }
    }
    private void Update()
    {
        RecievedRecipes = APClientClass.RecipeUnlockDictionary;
        foreach (string gotrecipe in RecievedRecipes)
        {
            RecipeNametoID.TryGetValue(gotrecipe, out int recipeID);
            var recipe = Recipes.recipes[recipeID];
            recipe.alwaysShow = true; // Learn the recipe
        }
        try
        {
            BlueprintScript[] blueprints = GameObject.Find("blueprint(Clone)").GetComponentsInChildren<BlueprintScript>();
            foreach (BlueprintScript recipeId in blueprints) // For each recipe item that exists, delete it and send its check.
            {
                if (AlreadySentChecks.Contains(recipeId.recipeIndex))
                {
                    Destroy(recipeId.gameObject);
                    continue; // Avoid spamming the server by not even attempting to send a check we already have sent.
                }
                RecipeIDtoCheckID.TryGetValue(recipeId.recipeIndex, out int CheckID);
                Startup.Logger.LogMessage("Found a blueprint with " + recipeId.recipeIndex + " as its index, which is checkid " + CheckID + ".");
                APClientClass.ChecksToSendQueue.Enqueue(CheckID);
                AlreadySentChecks.Add(recipeId.recipeIndex);
                Destroy(recipeId.gameObject);
            }
        }
        catch
        {
            return; // no blueprints currently exist in the world
        }
    }
}