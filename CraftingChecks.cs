using CreepyUtil.Archipelago.ApClient;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Enums;
using UnityEngine.UIElements.Collections;
using System.IO;

namespace CUAP;

public class CraftingChecks : MonoBehaviour
{
    AssetBundle bundle;
    private Sprite aplogo;
    public static ApClient Client;
    private static List<string> RecievedRecipes;
    public static List<int> AlreadySentChecks = new List<int>();
    private int lastFrameRecipeCount = 0;
    public static bool freesamples = false;
    private int RecipeNum = 0;
    public static int CraftedRecipes = 0;
    private bool removeBlueprints = false;
    private static HashSet<string> AppliedRecipes = new();
    private LocationScoutsPacket blueprintsPacket = new LocationScoutsPacket()
    {
        Locations = Enumerable.Range(22318500, 22318612 - 22318500 + 1)
                            .Select(i => (long)i)
                            .ToArray(),
        CreateAsHint = 0
    };
    public static Dictionary<long, string> BlueprintToPlayerName = new Dictionary<long, string>();
    public static Dictionary<long, string> BlueprintToItemName = new Dictionary<long, string>();
    public static Dictionary<string, string> CheckNameToItem = new Dictionary<string, string>()
    {
        {"Foliage rope Recipe","rope"},
        // {"Foliage Recipe", "foliage"} Technically exists in the game files, but is impossible to craft right now
        {"String Recipe","string"},
        {"Canvas Recipe","canvas"},
        {"Wood scraps Recipe","woodscraps"},
        {"Ripped dressing Recipe","rippeddressing"},
        {"Sterilized dressing Recipe","sterilizedbandage"},
        {"Bio-chem fluid Recipe","biochem"},
        {"Opium Recipe","opium"},
        {"Morphine Recipe","morphine"},
        {"Fentanyl Recipe","fentanyl"},
        {"Painkillers Recipe","painkillers"},
        {"Neural booster Recipe","neuralbooster"},
        {"Canteen Recipe","canteen"},
        {"Foliage bag Recipe","foliagebag"},
        {"Sling bag Recipe","slingbag"},
        {"Scrap cube Recipe","scrapcube"},
        {"Scrap panel Recipe","scrappanel"},
        {"Scrap tube Recipe","scraptube"},
        {"Nails Recipe","nails"},
        {"Wood cube Recipe","woodcube"},
        {"Wood panel Recipe","woodpanel"},
        {"Stick Recipe","stick"},
        {"Flimsy knife Recipe","flimsyknife"},
        {"Processed copper Recipe","processedcopper"},
        {"Bundle of wires Recipe","bundleofwires"},
        {"Titanium slab Recipe","titaniumslab"},
        {"Titanium sheet Recipe","titaniumsheet"},
        {"Titanium rod Recipe","titaniumrod"},
        {"Alien blood Recipe","alienblood"},
        {"Blood Recipe","blood"},
        {"Makeshift wrench Recipe","makeshiftwrench"},
        {"Wrench Recipe","wrench"},
        {"Crude cleaver Recipe","crudecleaver"},
        {"Flammable powder Recipe","flammablepowder"},
        {"Casing Recipe","casing"},
        {"9mm round Recipe","9mmround"},
        {"5.56 round Recipe","556round"},
        {"12-Gauge buckshot Recipe","12gauge"},
        {"Magazine base Recipe","magazinebase"},
        {"Small magazine Recipe","smallmagazine"},
        {"Rifle magazine Recipe","riflemagazine"},
        {"Box of 12-Guage Recipe","boxof12gauge"},
        {"Dynamite Recipe","dynamite"},
        {"Large carcass Recipe","largecarcass"},
        {"Circuit board Recipe","circuitboard"},
        {"Small battery Recipe","smallbattery"},
        {"Medium battery Recipe","mediumbattery"},
        {"Large battery Recipe","largebattery"},
        {"Flashlight Recipe","flashlight"},
        {"Headlamp Recipe","headlamp"},
        {"LCD screen Recipe","lcdscreen"},
        {"Flexiglass Recipe","flexiglass"},
        {"Lightbulb Recipe","lightbulb"},
        {"Limb wraps Recipe","limbwraps"},
        {"Makeshift lamp Recipe","makeshiftheadlamp"},
        {"Bicycle helmet Recipe","bikehelmet"},
        {"Makeshift helmet Recipe","makeshifthelmet"},
        {"Makeshift digging tool Recipe","makeshiftdiggingtool"},
        {"Makeshift rifle Recipe","makeshiftrifle"},
        {"Mini laser drill Recipe","minilaserdrill"},
        {"Dressing Recipe","bandage"},
        {"Lantern Recipe","lantern"},
        {"Terrain scanner Recipe","terrainscanner"},
        {"Advanced scuba diving gear Recipe","scubadivinggear"},
        {"Pickaxe Recipe","pickaxe"},
        {"Scaffolding pack Recipe","scaffoldingpack"},
        {"Backpack Recipe","bigpack"},
        {"Bowl of cereal Recipe","bowlofcereal"},
        {"Fat Recipe","fat"},
        {"Soap Recipe","soap"},
        {"Clotting mush Recipe","clottingmush"},
        {"Naltrexone Recipe","naltrexone"},
        {"Antidepressants Recipe","antidepressants"},
        {"Auto-injector Recipe","autoinjector"},
        {"Auto-auto-pump Recipe","autoautopump"},
        {"Antiseptic mush Recipe","antisepticmush"},
        {"Plastic dressing Recipe","plasticbandage"},
        {"Tourniquet Recipe","tourniquet"},
        {"Bone welding tool Recipe","boneweldingtool"},
        {"Tweezers Recipe","tweezers"},
        {"Blood bag Recipe","bloodbag"},
        {"Antiseptic Recipe","disinfectant"},
        {"Relief cream Recipe","reliefcream"},
        {"Splint Recipe","splint"},
        {"Bruise kit Recipe","bruisekit"},
        {"Carcass splint Recipe","carcasssplint"},
        {"Makeshift L.R.D. Recipe","makeshiftlrd"},
        {"L.R.D. Recipe","lrd"},
        {"L.R.D. Serum Recipe","lrdserum"},
        {"Produce juice Recipe","producejuice"},
        {"Refined juice Recipe","refinedjuice"},
        {"Drill repair kit Recipe","drillrepairkit"},
        {"Procoagulant Recipe","procoagulant"},
        {"Antiseptic bottle Recipe","disinfectant"},
        {"Firestarter Recipe","firestarter"},
        {"Campfire Recipe","campfire"},
        {"Water Recipe","water"},
        {"Charcoal Recipe","charcoal"},
        {"Bread Recipe","bread"},
        {"Rye flour Recipe","ryeflour"},
        {"Torch Recipe","torch"},
        {"Torch (relight) Recipe","torch"},
        {"Nutrient bar Recipe","nutrientbar"},
        {"Pemmican Recipe","pemmican"},
        {"Foliage meal Recipe","foliagemeal"},
        {"Burger Recipe","burger"},
        {"Soup Recipe","soup"},
        {"Ice pack Recipe","icepack"},
        {"Scarf Recipe","scarf"},
        {"Titanium pickaxe Recipe","titaniumpickaxe"},
        {"Titanium machete Recipe","titaniummachete"},
        {"Titanium multitool Recipe","titaniummultitool"},
        {"Climbing rope Recipe","climbingrope"}

    };
    private static Dictionary<string, int> RecipeToINTRequirement = new Dictionary<string, int>()
    {
        {"Foliage rope Recipe",1},
        // {"Foliage Recipe",4}, Technically exists in the game files, but is impossible to craft right now
        {"String Recipe",5},
        {"Canvas Recipe",2},
        {"Wood scraps Recipe",3},
        {"Ripped dressing Recipe",0},
        {"Sterilized dressing Recipe",1},
        {"Bio-chem fluid Recipe",4},
        {"Opium Recipe",7},
        {"Morphine Recipe",9},
        {"Fentanyl Recipe",11},
        {"Painkillers Recipe",10},
        {"Neural booster Recipe",18}, // current highest INT cost in the game
        {"Canteen Recipe",6},
        {"Foliage bag Recipe",5},
        {"Sling bag Recipe",7},
        {"Scrap cube Recipe",6},
        {"Scrap panel Recipe",5},
        {"Scrap tube Recipe",5},
        {"Nails Recipe",5},
        {"Wood cube Recipe",4},
        {"Wood panel Recipe",4},
        {"Stick Recipe",4},
        {"Flimsy knife Recipe",4},
        {"Processed copper Recipe",10},
        {"Bundle of wires Recipe",10},
        {"Titanium slab Recipe",15},
        {"Titanium sheet Recipe",15},
        {"Titanium rod Recipe",15},
        {"Alien blood Recipe",2},
        {"Blood Recipe",8},
        {"Makeshift wrench Recipe",9},
        {"Wrench Recipe",12},
        {"Crude cleaver Recipe",9},
        {"Flammable powder Recipe",8},
        {"Casing Recipe",10},
        {"9mm round Recipe",12},
        {"5.56 round Recipe",12},
        {"12-Gauge buckshot Recipe",12},
        {"Magazine base Recipe",12},
        {"Small magazine Recipe",12},
        {"Rifle magazine Recipe",12},
        {"Box of 12-Guage Recipe",8},
        {"Dynamite Recipe",10},
        {"Large carcass Recipe",4},
        {"Circuit board Recipe",9},
        {"Small battery Recipe",11},
        {"Medium battery Recipe",12},
        {"Large battery Recipe",13},
        {"Flashlight Recipe",10},
        {"Headlamp Recipe",8},
        {"LCD screen Recipe",11},
        {"Flexiglass Recipe",9},
        {"Lightbulb Recipe",11},
        {"Limb wraps Recipe",6},
        {"Makeshift lamp Recipe",4},
        {"Bicycle helmet Recipe",13},
        {"Makeshift helmet Recipe",8},
        {"Makeshift digging tool Recipe",7},
        {"Makeshift rifle Recipe",12},
        {"Mini laser drill Recipe",16},
        {"Dressing Recipe",5},
        {"Lantern Recipe",10},
        {"Terrain scanner Recipe",12},
        {"Advanced scuba diving gear Recipe",12},
        {"Pickaxe Recipe",12},
        {"Scaffolding pack Recipe",14},
        {"Backpack Recipe",13},
        {"Bowl of cereal Recipe",2},
        {"Fat Recipe",8},
        {"Soap Recipe",8},
        {"Clotting mush Recipe",10},
        {"Naltrexone Recipe",10},
        {"Antidepressants Recipe",10},
        {"Auto-injector Recipe",12},
        {"Auto-auto-pump Recipe",12},
        {"Antiseptic mush Recipe",8},
        {"Plastic dressing Recipe",14},
        {"Tourniquet Recipe",9},
        {"Bone welding tool Recipe",14},
        {"Tweezers Recipe",9},
        {"Blood bag Recipe",12},
        {"Antiseptic Recipe",9},
        {"Relief cream Recipe",9},
        {"Splint Recipe",8},
        {"Bruise kit Recipe",10},
        {"Carcass splint Recipe",6},
        {"Makeshift L.R.D. Recipe",10},
        {"L.R.D. Recipe",15},
        {"L.R.D. Serum Recipe",11},
        {"Produce juice Recipe",2},
        {"Refined juice Recipe",4},
        {"Drill repair kit Recipe",9},
        {"Procoagulant Recipe",14},
        {"Antiseptic bottle Recipe",7},
        {"Firestarter Recipe",8},
        {"Campfire Recipe",4},
        {"Water Recipe",4},
        {"Charcoal Recipe",7},
        {"Bread Recipe",7},
        {"Rye flour Recipe",6},
        {"Torch Recipe",5},
        {"Torch (relight) Recipe",5},
        {"Nutrient bar Recipe",9},
        {"Pemmican Recipe",6},
        {"Foliage meal Recipe",7},
        {"Burger Recipe",6},
        {"Soup Recipe",9},
        {"Ice pack Recipe",14},
        {"Scarf Recipe",10},
        {"Titanium pickaxe Recipe",16},
        {"Titanium machete Recipe",16},
        {"Titanum multitool Recipe",16},
        {"Climbing rope Recipe",8},
    };
    private static Dictionary<int, string> RecipeIDToCheckName = new Dictionary<int, string>()
    {
        {0,"Recipe - Foliage rope"},
        // {1,"Recipe - Foliage"},
        {2,"Recipe - String"},
        {3,"Recipe - Canvas"},
        {4,"Recipe - Wood scraps"},
        {5,"Recipe - Ripped dressing"},
        {6,"Recipe - Sterilized dressing"},
        {7,"Recipe - Bio-chem fluid"},
        {8,"Recipe - Opium"},
        {9,"Recipe - Morphine"},
        {10,"Recipe - Fentanyl"},
        {11,"Recipe - Painkillers"},
        {12,"Recipe - Neural booster"},
        {13,"Recipe - Canteen"},
        {14,"Recipe - Foliage bag"},
        {15,"Recipe - Sling bag"},
        {16,"Recipe - Scrap cube"},
        {17,"Recipe - Scrap panel"},
        {18,"Recipe - Scrap tube"},
        {19,"Recipe - Nails"},
        {20,"Recipe - Wood cube"},
        {21,"Recipe - Wood panel"},
        {22,"Recipe - Stick"},
        {23,"Recipe - Flimsy knife"},
        {24,"Recipe - Processed copper"},
        {25,"Recipe - Bundle of wires"},
        {26,"Recipe - Titanium slab"},
        {27,"Recipe - Titanium sheet"},
        {28,"Recipe - Titanium rod"},
        {29,"Recipe - Alien blood"},
        {30,"Recipe - Blood"},
        {31,"Recipe - Makeshift wrench"},
        {32,"Recipe - Wrench"},
        {33,"Recipe - Crude cleaver"},
        {34,"Recipe - Flammable powder"},
        {35,"Recipe - Casing"},
        {36,"Recipe - 9mm round"},
        {37,"Recipe - 5.56 round"},
        {38,"Recipe - 12-Gauge buckshot"},
        {39,"Recipe - Magazine base"},
        {40,"Recipe - Small magazine"},
        {41,"Recipe - Rifle magazine"},
        {42,"Recipe - Box of 12-Guage"},
        {43,"Recipe - Dynamite"},
        {44,"Recipe - Large carcass"},
        {45,"Recipe - Circuit board"},
        {46,"Recipe - Small battery"},
        {47,"Recipe - Medium battery"},
        {48,"Recipe - Large battery"},
        {49,"Recipe - Flashlight"},
        {50,"Recipe - Headlamp"},
        {51,"Recipe - LCD screen"},
        {52,"Recipe - Flexiglass"},
        {53,"Recipe - Lightbulb"},
        {54,"Recipe - Limb wraps"},
        {55,"Recipe - Makeshift lamp"},
        {56,"Recipe - Bicycle helmet"},
        {57,"Recipe - Makeshift helmet"},
        {58,"Recipe - Makeshift digging tool"},
        {59,"Recipe - Makeshift rifle"},
        {60,"Recipe - Mini laser drill"},
        {61,"Recipe - Dressing"},
        {62,"Recipe - Lantern"},
        {63,"Recipe - Terrain scanner"},
        {64,"Recipe - Advanced scuba diving gear"},
        {65,"Recipe - Pickaxe"},
        {66,"Recipe - Scaffolding pack"},
        {67,"Recipe - Backpack"},
        {68,"Recipe - Bowl of cereal"},
        {69,"Recipe - Fat"},
        {70,"Recipe - Soap"},
        {71,"Recipe - Clotting mush"},
        {72,"Recipe - Naltrexone"},
        {73,"Recipe - Antidepressants"},
        {74,"Recipe - Auto-injector"},
        {75,"Recipe - Auto-auto-pump"},
        {76,"Recipe - Antiseptic mush"},
        {77,"Recipe - Plastic dressing"},
        {78,"Recipe - Tourniquet"},
        {79,"Recipe - Bone welding tool"},
        {80,"Recipe - Tweezers"},
        {81,"Recipe - Blood bag"},
        {82,"Recipe - Antiseptic"},
        {83,"Recipe - Relief cream"},
        {84,"Recipe - Splint"},
        {85,"Recipe - Bruise kit"},
        {86,"Recipe - Carcass splint"},
        {87,"Recipe - Makeshift L.R.D."},
        {88,"Recipe - L.R.D."},
        {89,"Recipe - L.R.D. Serum"},
        {90,"Recipe - Produce juice"},
        {91,"Recipe - Refined juice"},
        {92,"Recipe - Drill repair kit"},
        {93,"Recipe - Procoagulant"},
        {94,"Recipe - Antiseptic bottle"},
        {95,"Recipe - Firestarter"},
        {96,"Recipe - Campfire"},
        {97,"Recipe - Water"},
        {98,"Recipe - Charcoal"},
        {99,"Recipe - Bread"},
        {100,"Recipe - Rye flour"},
        {101,"Recipe - Torch"},
        {102,"Recipe - Torch (relight)"},
        {103,"Recipe - Nutrient bar"},
        {104,"Recipe - Pemmican"},
        {105,"Recipe - Foliage meal"},
        {106,"Recipe - Burger"},
        {107,"Recipe - Soup"},
        {108,"Recipe - Ice pack"},
        {109,"Recipe - Scarf"},
        {110,"Recipe - Titanium pickaxe"},
        {111,"Recipe - Titanium machete"},
        {112,"Recipe - Titanium multitool"},
        {113,"Recipe - Climbing rope"}
    };
    private static Dictionary<string, int> CheckNameToRecipeID = new Dictionary<string, int>()
    {   // Same order as items.py, and the interal recipe order in-game
        {"Foliage rope Recipe",0},
        // {"Foliage Recipe",1}, Technically exists in the game files, but is impossible to craft right now
        {"String Recipe",2},
        {"Canvas Recipe",3},
        {"Wood scraps Recipe",4},
        {"Ripped dressing Recipe",5},
        {"Sterilized dressing Recipe",6},
        {"Bio-chem fluid Recipe",7},
        {"Opium Recipe",8},
        {"Morphine Recipe",9},
        {"Fentanyl Recipe",10},
        {"Painkillers Recipe",11},
        {"Neural booster Recipe",12},
        {"Canteen Recipe",13},
        {"Foliage bag Recipe",14},
        {"Sling bag Recipe",15},
        {"Scrap cube Recipe",16},
        {"Scrap panel Recipe",17},
        {"Scrap tube Recipe",18},
        {"Nails Recipe",19},
        {"Wood cube Recipe",20},
        {"Wood panel Recipe",21},
        {"Stick Recipe",22},
        {"Flimsy knife Recipe",23},
        {"Processed copper Recipe",24},
        {"Bundle of wires Recipe",25},
        {"Titanium slab Recipe",26},
        {"Titanium sheet Recipe",27},
        {"Titanium rod Recipe",28},
        {"Alien blood Recipe",29},
        {"Blood Recipe",30},
        {"Makeshift wrench Recipe",31},
        {"Wrench Recipe",32},
        {"Crude cleaver Recipe",33},
        {"Flammable powder Recipe",34},
        {"Casing Recipe",35},
        {"9mm round Recipe",36},
        {"5.56 round Recipe",37},
        {"12-Gauge buckshot Recipe",38},
        {"Magazine base Recipe",39},
        {"Small magazine Recipe",40},
        {"Rifle magazine Recipe",41},
        {"Box of 12-Guage Recipe",42},
        {"Dynamite Recipe",43},
        {"Large carcass Recipe",44},
        {"Circuit board Recipe",45},
        {"Small battery Recipe",46},
        {"Medium battery Recipe",47},
        {"Large battery Recipe",48},
        {"Flashlight Recipe",49},
        {"Headlamp Recipe",50},
        {"LCD screen Recipe",51},
        {"Flexiglass Recipe",52},
        {"Lightbulb Recipe",53},
        {"Limb wraps Recipe",54},
        {"Makeshift lamp Recipe",55},
        {"Bicycle helmet Recipe",56},
        {"Makeshift helmet Recipe",57},
        {"Makeshift digging tool Recipe",58},
        {"Makeshift rifle Recipe",59},
        {"Mini laser drill Recipe",60},
        {"Dressing Recipe",61},
        {"Lantern Recipe",62},
        {"Terrain scanner Recipe",63},
        {"Advanced scuba diving gear Recipe",64},
        {"Pickaxe Recipe",65},
        {"Scaffolding pack Recipe",66},
        {"Backpack Recipe",67},
        {"Bowl of cereal Recipe",68},
        {"Fat Recipe",69},
        {"Soap Recipe",70},
        {"Clotting mush Recipe",71},
        {"Naltrexone Recipe",72},
        {"Antidepressants Recipe",73},
        {"Auto-injector Recipe",74},
        {"Auto-auto-pump Recipe",75},
        {"Antiseptic mush Recipe",76},
        {"Plastic dressing Recipe",77},
        {"Tourniquet Recipe",78},
        {"Bone welding tool Recipe",79},
        {"Tweezers Recipe",80},
        {"Blood bag Recipe",81},
        {"Antiseptic Recipe",82},
        {"Relief cream Recipe",83},
        {"Splint Recipe",84},
        {"Bruise kit Recipe",85},
        {"Carcass splint Recipe",86},
        {"Makeshift L.R.D. Recipe",87},
        {"L.R.D. Recipe",88},
        {"L.R.D. Serum Recipe",89},
        {"Produce juice Recipe",90},
        {"Refined juice Recipe",91},
        {"Drill repair kit Recipe",92},
        {"Procoagulant Recipe",93},
        {"Antiseptic bottle Recipe",94},
        {"Firestarter Recipe",95},
        {"Campfire Recipe",96},
        {"Water Recipe",97},
        {"Charcoal Recipe",98},
        {"Bread Recipe",99},
        {"Rye flour Recipe",100},
        {"Torch Recipe",101},
        {"Torch (relight) Recipe",102},
        {"Nutrient bar Recipe",103},
        {"Pemmican Recipe",104},
        {"Foliage meal Recipe",105},
        {"Burger Recipe",106},
        {"Soup Recipe",107},
        {"Ice pack Recipe",108},
        {"Scarf Recipe",109},
        {"Titanium pickaxe Recipe",110},
        {"Titanium machete Recipe",111},
        {"Titanum multitool Recipe",112},
        {"Climbing rope Recipe",113},
    };
    public static Dictionary<int, bool> RecipeCraftedBefore = new Dictionary<int, bool>();

    private void OnEnable()
    {
        Client = APClientClass.Client;
        RecievedRecipes = APClientClass.RecipeUnlockDictionary;
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("RandomizeRecipes", out var recipesoption)) // check if recipe randomization is enabled.
        {
            if (Convert.ToInt16(recipesoption) == 1) // disabled
            {
                Startup.Logger.LogWarning("Recipe Randomization is disabled, destroying script.");
                DestroyImmediate(this);
                return;
            }
            else // enabled. both need to learn recupes, so we'll do that first
            {
                foreach (var recipe in Recipes.recipes)
                {
                    recipe.INT = 999; // Unlearn every recipe. We will recieve them with items later.
                }
                if (Convert.ToInt16(recipesoption) == 3) // blueprint locations enabled
                {
                    APClientClass.session.Socket.SendPacket(blueprintsPacket);
                    SetupAPBlueprint();
                    var loaded = AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b => b.name == "apassets");
                    if (loaded == null) // only load the bundle if we haven't already
                    {
                        bundle = AssetBundle.LoadFromFile(Path.Combine(BepInEx.Paths.PluginPath, "CUAP", "apassets")); // load assetbundle
                        aplogo = bundle.LoadAsset<Sprite>("aplogopixel"); // load custom blueprint asset replacement
                    }
                }
                else // blueprint locations aren't enabled. mark that for later
                {
                    removeBlueprints = true;
                }
            }
        }
        if (options.TryGetValue("FreeSamples", out var samples))
        {
            freesamples = Convert.ToBoolean(samples);
        }
        if (APClientClass.selectedGoal == 4)
        {
            APClientClass.session.Socket.SendPacket(new GetPacket {Keys = new[]{"crafted_blueprints"}});
        }
    }
    private void Update()
    {
        if (RecievedRecipes.Count > lastFrameRecipeCount)
        {
            foreach (string gotrecipe in RecievedRecipes)
            {
                if (!AppliedRecipes.Add(gotrecipe)) continue; // already in the list
                CheckNameToRecipeID.TryGetValue(gotrecipe, out int recipeToLearn); // get the recipe we're learning
                RecipeToINTRequirement.TryGetValue(gotrecipe, out int recipeRequiredINT); // get its int requried to craft
                Recipes.recipes[recipeToLearn].INT = recipeRequiredINT; // set the int to the vanilla value
                Recipes.recipes[recipeToLearn].specialKnown = true; // force it visible
                if (APClientClass.selectedGoal == 4)
                {
                    RecipeCraftedBefore.Add(RecipeNum++, false);
                }
            }
            lastFrameRecipeCount = RecievedRecipes.Count;
        }
        if (APClientClass.selectedGoal == 4)
        {
            for (int i = 0; i < Recipes.recipes.Count; i++)
            {
                var recipe = Recipes.recipes[i]; // check each recipe. the order of recipes in Recipes.recipes will always match RecipeCraftedBefore
                if (!recipe.hasMadeBefore) continue; // have we made it before? if not, ignore
                if (RecipeCraftedBefore.TryGetValue(i, out bool alreadyCrafted) && alreadyCrafted) continue; // has it been added before? if so, ignore
                RecipeCraftedBefore[i] = true; // update the dictionary
                CraftedRecipes++; // increase our local recipes crafted number
                APClientClass.session.Socket.SendPacket(new SetPacket // save the data to AP in case of disconnects
                {
                    Key = "crafted_blueprints",
                    Operations = new[]
                    {
                    new OperationSpecification
                    {
                        OperationType = OperationType.Update,
                        Value = JToken.FromObject(new Dictionary<int, bool>
                        {
                            [i] = true
                        })
                    }
                }
                });
            }
            if (RecipeCraftedBefore.Count == Recipes.recipes.Count && CraftedRecipes == Recipes.recipes.Count) // we have all the recipes and have crafted them all
            {
                Client.Goal();
            }
        }
        try
        {
            var blueprints = GameObject.FindObjectsOfType<GameObject>()
                .Where(o => o.name == "blueprint(Clone)")
                .ToList();
            foreach (GameObject bp in blueprints)
            {
                if (removeBlueprints) // settings make blueprints not needed. remove them
                {
                    Destroy(bp);
                    continue;
                }
                bp.GetComponent<SpriteRenderer>().sprite = aplogo;
                var blueprint = bp.GetComponent<BlueprintScript>();
                var recipeId = blueprint.recipeIndex;
                if (AlreadySentChecks.Contains(recipeId))
                {
                    bp.gameObject.GetComponent<BlueprintScript>().recipeIndex = UnityEngine.Random.Range(0, 112); // rerandomize it
                    continue; // the game internally only spawns blueprints up to the amount that are in the game,
                    // since I remove them to randomize them, we need to rerandomize up to all 112, because the game doesn't
                }
            }
            if (GameObject.Find("blueprint(Clone)")) // does at least one blueprint still exist?
            {
                var closest = blueprints.OrderBy(o => (o.transform.position - GameObject.Find("Experiment/Body").transform.position).sqrMagnitude)
                           .FirstOrDefault(); // find the closest one
                var item = closest.gameObject.GetComponent<Item>();
                var recipeId = closest.gameObject.GetComponent<BlueprintScript>().recipeIndex;
                item.Stats.description = "Six multicolored circles held together by an invisible force. Use it to send <v1> their <v2>.";
                item.Stats.description = item.Stats.description.Replace("<v1>", BlueprintToPlayerName.Get(recipeId));
                item.Stats.description = item.Stats.description.Replace("<v2>", BlueprintToItemName.Get(recipeId));
            }
        }
        catch
        {
            return; // no blueprints currently exist in the world
        }
    }
    void SetupAPBlueprint() // rebuild the basegame blueprint functions, but with AP code integrated.
    {
        Item.GlobalItems.Remove("blueprint");
        string itemname = "blueprint";
        ItemInfo patchAPinfo = new ItemInfo
        {
            category = "utility",
            slotRotation = 0f,
            usable = true,
            usableOnLimb = false,
            destroyAtZeroCondition = true,
            weight = 0.2f,
            useAction = delegate (Body body, Item item)
            {
                item.condition = 0f;
                body.skills.AddExp(2, 35f);
                CraftingChecks.SendBlueprintLocation(item.gameObject.GetComponent<BlueprintScript>().recipeIndex);
                PlayerCamera.main.DoAlert("Item sent to <color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o!", false);
                Sound.Play("combine", item.transform.position, false, true, null, 1f, 1f, false, false);
            },
            value = 0, // i think setting this to 0 makes it unsellable? makes it useless to regardless
            fullName = "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Item",
            rec = new Recognition(0)
        };
        Item.GlobalItems.Add(itemname, patchAPinfo);
        patchAPinfo.SetTags();
        ItemLootPool.InitializePool();
    }

    public static void SendBlueprintLocation(int recipeIndex)
    {
        RecipeIDToCheckName.TryGetValue(recipeIndex, out string CheckName);
        APClientClass.ChecksToSendQueue.Enqueue(CheckName);
        AlreadySentChecks.Add(recipeIndex);
    }
}