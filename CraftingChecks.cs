using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using KaitoKid.ArchipelagoUtilities.AssetDownloader.ItemSprites;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace CUAP;

public class CraftingChecks : MonoBehaviour
{
    private Sprite aplogo;
    private Sprite bgBlueprint; // basegame blueprint
    public static ArchipelagoSession Client;
    private static List<string> RecievedRecipes;
    public static List<int> AlreadySentChecks = new List<int>();
    public static bool apItems = false;
    private int apItemAmount;
    private static int currentAPItemNum;
    private bool randomRecipes = false;
    private bool initialSync = false;
    private static bool updateAllBPs = false;
    private int lastFrameRecipeCount = 0;
    public static bool freesamples = false;
    private int RecipeNum = 0;
    public static int CraftedRecipes = 0;
    private SemaphoreSlim spriteSemaphore = new(1, 1);
    private static readonly long startingRecipeID = 22318500;
    private static HashSet<string> AppliedRecipes = new();
    public static Dictionary<long, string> BlueprintToPlayerName = new Dictionary<long, string>();
    public static Dictionary<long, string> BlueprintToItemName = new Dictionary<long, string>();
    public static Dictionary<string, string> CheckNameToItem = new Dictionary<string, string>()
    {
        {"Foliage rope Recipe","rope"},
        {"Foliage Recipe", "foliage"},
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
        {"Saline Recipe","saline"},
        {"Blood Recipe","blood"},
        {"Makeshift wrench Recipe","makeshiftwrench"},
        {"Wrench Recipe","wrench"},
        {"Crude cleaver Recipe","crudecleaver"},
        {"Machete Recipe","machete"},
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
        {"Lockpicking kit Recipe","lockpickingkit"},
        {"Advanced scuba diving gear Recipe","scubadivinggear"},
        {"Pickaxe Recipe","pickaxe"},
        {"Scaffolding pack Recipe","scaffoldingpack"},
        {"Backpack Recipe","bigpack"},
        {"Wood sandals Recipe","woodsandals"},
        {"Duffel bag Recipe","duffelbag"},
        {"Material bag Recipe","materialpouch"},
        {"Belt Recipe","belt"},
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
        {"Saline bottle Recipe","saline"},
        {"Antiseptic Recipe","disinfectant"},
        {"Relief cream Recipe","reliefcream"},
        {"Splint Recipe","splint"},
        {"Bruise kit Recipe","bruisekit"},
        {"Carcass splint Recipe","carcasssplint"},
        {"Makeshift L.R.D. Recipe","makeshiftlrd"},
        {"L.R.D. Recipe","lrd"},
        {"L.R.D. Serum Recipe","lrdserum"},
        {"Produce juice Recipe","producejuice"},
        {"Milk Recipe","milk"},
        {"Refined juice Recipe","refinedjuice"},
        {"Drill repair kit Recipe","drillrepairkit"},
        {"Procoagulant Recipe","procoagulant"},
        {"Spray bottle Recipe","spraybottle"},
        {"Syringe Recipe","syringe"},
        {"Firestarter Recipe","firestarter"},
        {"Campfire Recipe","campfire"},
        {"Water Recipe","water"},
        {"Charcoal Recipe","charcoal"},
        {"Bread Recipe","bread"},
        {"Pancake Recipe","pancake"},
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
        {"Foliage Recipe",4},
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
        {"Saline Recipe",9},
        {"Blood Recipe",8},
        {"Makeshift wrench Recipe",9},
        {"Wrench Recipe",12},
        {"Crude cleaver Recipe",9},
        {"Machete Recipe",12},
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
        {"Lockpicking kit Recipe",10},
        {"Advanced scuba diving gear Recipe",12},
        {"Pickaxe Recipe",12},
        {"Scaffolding pack Recipe",14},
        {"Backpack Recipe",13},
        {"Wood sandals Recipe",8},
        {"Duffel bag Recipe",11},
        {"Material bag Recipe",10},
        {"Belt Recipe",10},
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
        {"Saline bottle Recipe",11},
        {"Antiseptic Recipe",9},
        {"Relief cream Recipe",9},
        {"Splint Recipe",8},
        {"Bruise kit Recipe",10},
        {"Carcass splint Recipe",6},
        {"Makeshift L.R.D. Recipe",10},
        {"L.R.D. Recipe",15},
        {"L.R.D. Serum Recipe",11},
        {"Produce juice Recipe",2},
        {"Milk Recipe",0},
        {"Refined juice Recipe",4},
        {"Drill repair kit Recipe",9},
        {"Procoagulant Recipe",14},
        {"Spray bottle Recipe",7},
        {"Syringe Recipe",8},
        {"Firestarter Recipe",8},
        {"Campfire Recipe",4},
        {"Water Recipe",4},
        {"Charcoal Recipe",7},
        {"Bread Recipe",7},
        {"Pancake Recipe",5},
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
    private static List<string> CheckNameToRecipeID = new List<string>()
    {   // Same order as items.py, and the interal recipe order in-game
        "Foliage rope Recipe",
        "Foliage Recipe",
        "String Recipe",
        "Canvas Recipe",
        "Wood scraps Recipe",
        "Ripped dressing Recipe",
        "Sterilized dressing Recipe",
        "Bio-chem fluid Recipe",
        "Opium Recipe",
        "Morphine Recipe",
        "Fentanyl Recipe",
        "Painkillers Recipe",
        "Neural booster Recipe",
        "Canteen Recipe",
        "Foliage bag Recipe",
        "Sling bag Recipe",
        "Scrap cube Recipe",
        "Scrap panel Recipe",
        "Scrap tube Recipe",
        "Nails Recipe",
        "Wood cube Recipe",
        "Wood panel Recipe",
        "Stick Recipe",
        "Flimsy knife Recipe",
        "Processed copper Recipe",
        "Bundle of wires Recipe",
        "Titanium slab Recipe",
        "Titanium sheet Recipe",
        "Titanium rod Recipe",
        "Alien blood Recipe",
        "Saline Recipe",
        "Blood Recipe",
        "Makeshift wrench Recipe",
        "Wrench Recipe",
        "Crude cleaver Recipe",
        "Machete Recipe",
        "Flammable powder Recipe",
        "Casing Recipe",
        "9mm round Recipe",
        "5.56 round Recipe",
        "12-Gauge buckshot Recipe",
        "Magazine base Recipe",
        "Small magazine Recipe",
        "Rifle magazine Recipe",
        "Box of 12-Guage Recipe",
        "Dynamite Recipe",
        "Large carcass Recipe",
        "Circuit board Recipe",
        "Small battery Recipe",
        "Medium battery Recipe",
        "Large battery Recipe",
        "Flashlight Recipe",
        "Headlamp Recipe",
        "LCD screen Recipe",
        "Flexiglass Recipe",
        "Lightbulb Recipe",
        "Limb wraps Recipe",
        "Makeshift lamp Recipe",
        "Bicycle helmet Recipe",
        "Makeshift helmet Recipe",
        "Makeshift digging tool Recipe",
        "Makeshift rifle Recipe",
        "Mini laser drill Recipe",
        "Dressing Recipe",
        "Lantern Recipe",
        "Terrain scanner Recipe",
        "Lockpicking kit Recipe",
        "Advanced scuba diving gear Recipe",
        "Pickaxe Recipe",
        "Scaffolding pack Recipe",
        "Backpack Recipe",
        "Wood sandals Recipe",
        "Duffel bag Recipe",
        "Material bag Recipe",
        "Belt Recipe",
        "Bowl of cereal Recipe",
        "Fat Recipe",
        "Soap Recipe",
        "Clotting mush Recipe",
        "Naltrexone Recipe",
        "Antidepressants Recipe",
        "Auto-injector Recipe",
        "Auto-auto-pump Recipe",
        "Antiseptic mush Recipe",
        "Plastic dressing Recipe",
        "Tourniquet Recipe",
        "Bone welding tool Recipe",
        "Tweezers Recipe",
        "Blood bag Recipe",
        "Saline Bottle Recipe",
        "Antiseptic Recipe",
        "Relief cream Recipe",
        "Splint Recipe",
        "Bruise kit Recipe",
        "Carcass splint Recipe",
        "Makeshift L.R.D. Recipe",
        "L.R.D. Recipe",
        "L.R.D. Serum Recipe",
        "Produce juice Recipe",
        "Milk Recipe",
        "Refined juice Recipe",
        "Drill repair kit Recipe",
        "Procoagulant Recipe",
        "Spray bottle Recipe",
        "Syringe Recipe",
        "Firestarter Recipe",
        "Campfire Recipe",
        "Water Recipe",
        "Charcoal Recipe",
        "Bread Recipe",
        "Pancake Recipe",
        "Rye flour Recipe",
        "Torch Recipe",
        "Torch (relight) Recipe",
        "Nutrient bar Recipe",
        "Pemmican Recipe",
        "Foliage meal Recipe",
        "Burger Recipe",
        "Soup Recipe",
        "Ice pack Recipe",
        "Scarf Recipe",
        "Titanium pickaxe Recipe",
        "Titanium machete Recipe",
        "Titanium multitool Recipe",
        "Climbing rope Recipe",
    };
    public static Dictionary<int, bool> RecipeCraftedBefore = new Dictionary<int, bool>();

    private void OnEnable()
    {
        Client = APClientClass.session;
        RecievedRecipes = APClientClass.RecipeUnlockDictionary;
        AppliedRecipes.Clear();
        var options = APClientClass.slotdata;
        if (options.TryGetValue("RandomizeRecipes", out var recipesoption)) // check if recipe randomization is enabled.
        {
            if (!Convert.ToBoolean(recipesoption)) // disabled
            {
                randomRecipes = false;
            }
            else // enabled
            {
                foreach (var recipe in Recipes.recipes)
                {
                    recipe.INT = 999; // Unlearn every recipe. We will recieve them with items later.
                }
                randomRecipes = true;
                initialSync = true;
            }
        }
        if (options.TryGetValue("APItemAmount", out var items))
        {
            apItems = true;
            apItemAmount = Convert.ToInt16(items);
            currentAPItemNum = SkillSending.GetIntFromCheckedLocations(startingRecipeID, Convert.ToInt16(items)); // consider previous sessions
            aplogo = Startup.apassets.LoadAsset<Sprite>("aplogo200"); // load custom blueprint asset replacement
            bgBlueprint = Resources.Load<GameObject>("blueprint").GetComponent<SpriteRenderer>().sprite; // reference basegame asset from prefab
        }
        SetupAPBlueprint(apItems);
        if (options.TryGetValue("FreeSamples", out var samples))
        {
            freesamples = Convert.ToBoolean(samples);
        }
        if (APClientClass.selectedGoal == 4)
        {
            Client.Socket.SendPacket(new GetPacket {Keys = new[]{"crafted_blueprints"}});
        }
    }
    private void Update()
    {
        if ((RecievedRecipes.Count > lastFrameRecipeCount || initialSync) && randomRecipes)
        {
            initialSync = false;
            foreach (string gotrecipe in RecievedRecipes)
            {
                if (!AppliedRecipes.Add(gotrecipe)) continue; // already in the list
                var recipeToLearn = CheckNameToRecipeID.IndexOf(gotrecipe); // get the recipe we're learning
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
            for (int i = 0; i < Recipes.recipes.Count; i++) // >>> FIX: Why are we doing this every frame? I get this is unused code, but still... <<<
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
                Client.SetGoalAchieved();
            }
        }
        if (apItems)
        {
            try
            {
                var blueprints = FindObjectsOfType<GameObject>()
                    .Where(o => o.name == "blueprint(Clone)")
                    .ToList();
                foreach (GameObject bp in blueprints)
                {
                    var renderer = bp.GetComponent<SpriteRenderer>();
                    if (renderer.sprite.name == bgBlueprint.name || updateAllBPs) // do all of this ONLY if it's a new blueprint OR blueprints should be updated
                    {
                        renderer.sprite = aplogo;
                        var blueprint = bp.GetComponent<BlueprintScript>();
                        blueprint.recipeIndex = currentAPItemNum;
                        if (currentAPItemNum > apItemAmount)
                        {
                            Debug.LogWarning("All Archipelago Items have been collected! Disabling apItems flag.");
                            apItems = false;
                            Destroy(bp);
                            break;
                        }
                        var helper = ThreadingHelper.Instance;
                        _ = AssignCustomSprite(renderer, bp.GetComponent<Item>(), currentAPItemNum, helper);
                    }
                    else
                    {
                        continue; // we've already handled this blueprint
                    }
                }
                updateAllBPs = false;
                if (GameObject.Find("blueprint(Clone)")) // does at least one blueprint still exist?
                {
                    var closest = blueprints.OrderBy(o => (o.transform.position - GameObject.Find("Experiment/Body").transform.position).sqrMagnitude)
                               .FirstOrDefault(); // find the closest one
                    var item = closest.gameObject.GetComponent<Item>();
                    var recipeId = closest.gameObject.GetComponent<BlueprintScript>().recipeIndex;
                    item.Stats.description = "A mysterious item from another world. It looks to be <plr>'s <item>.";
                    item.Stats.description = item.Stats.description.Replace("<plr>", BlueprintToPlayerName.Get(recipeId));
                    item.Stats.description = item.Stats.description.Replace("<item>", BlueprintToItemName.Get(recipeId));
                    item.favourited = true;
                }
            }
            catch (Exception ex)
            {
                Startup.Logger.LogError($"Archipelago Blueprint Error: {ex}");
                APCanvas.EnqueueArchipelagoNotification($"Archipelago Blueprint Error: {ex}",3);
                return;
            }
        }
    }
    void SetupAPBlueprint(bool apItemsEnabled) // rebuild the basegame blueprint functions, but with AP code integrated.
    {
        Item.GlobalItems.Remove("blueprint");
        string itemname = "blueprint";
        if (apItemsEnabled)
        {
            ItemInfo patchAPinfo = new ItemInfo
            {
                category = "utility",
                slotRotation = 0f,
                usable = true,
                usableOnLimb = false,
                destroyAtZeroCondition = true,
                weight = 0,
                useAction = delegate (Body body, Item item)
                {
                    item.condition = 0f;
                    body.skills.AddExp(2, 10f);
                    CraftingChecks.SendBlueprintLocation(item.gameObject.GetComponent<BlueprintScript>().recipeIndex);
                    PlayerCamera.main.DoAlert($"Item sent to {APCanvas.coloredAPText}!", false);
                    Sound.Play("combine", item.transform.position, false, true, null, 1f, 1f, false, false);
                },
                value = 0, // i think setting this to 0 makes it unsellable? makes it useless to regardless
                fullName = $"{APCanvas.coloredAPText} Item",
                description = "A mysterious item from another world. It looks to be <plr>'s <item>.",
                rec = new Recognition(0)
            };
            Item.GlobalItems.Add(itemname, patchAPinfo);
            patchAPinfo.SetTags();
        }
        else
        {
            ItemInfo patchAPinfo = new ItemInfo
            {
                category = "utility",
                slotRotation = 0f,
                usable = true,
                usableOnLimb = false,
                destroyAtZeroCondition = true,
                weight = 0,
                useAction = delegate (Body body, Item item)
                {
                    item.condition = 0f;
                    body.skills.AddExp(2, 35f);
                    PlayerCamera.main.DoAlert("Claimed 35 INT Experience!", false);
                    Sound.Play("combine", item.transform.position, false, true, null, 1f, 1f, false, false);
                },
                value = 0,
                fullName = "INT Experience Bundle (35)",
                description = "Archipelago has removed the recipe from this blueprint, but you will still get the INT Experience from it.",
                rec = new Recognition(0)
            };
            Item.GlobalItems.Add(itemname, patchAPinfo);
            patchAPinfo.SetTags();
        }
        ItemLootPool.InitializePool();
    }

    public static void SendBlueprintLocation(int recipeIndex)
    {
        var CheckID = recipeIndex + startingRecipeID;
        APClientClass.ChecksToSend.Add(CheckID);
        currentAPItemNum++;
        updateAllBPs = true; // update other blueprint's sprites and names
    }

    private async Task AssignCustomSprite(SpriteRenderer renderer, Item item, int recipeID, ThreadingHelper helper)
    {
        await spriteSemaphore.WaitAsync();
        if (renderer == null)
        {
            spriteSemaphore.Release();
            return; // object has been destroyed, don't continue
        }
        try
        {
            long locationID = recipeID + startingRecipeID;
            var rawScoutData = await Client.Locations.ScoutLocationsAsync(locationID);
            if (renderer == null)
            {
                spriteSemaphore.Release();
                return; // object has been destroyed, don't continue (doing this again because it's after an await call)
            }
            foreach (var info in rawScoutData)
            {
                var itemInfo = info.Value; // get just the ScoutedItemInfo
                AssetItem formattedInfo = new AssetItem( // convert to AssetItem
                    info.Value.ItemGame,
                    info.Value.ItemName,
                    info.Value.Flags
                );
                var success = SpriteConverter.itemSprites.TryGetCustomAsset(formattedInfo, "Casualties: Unknown", true, true, out ItemSprite sprite);
                if (renderer == null)
                {
                    spriteSemaphore.Release();
                    return; // object has been destroyed, don't continue (doing this again because it's after an async asset check)
                }
                if (success)
                {
                    Startup.Logger.LogDebug($"Custom Sprite FilePath = {sprite.FilePath}");
                    byte[] data = File.ReadAllBytes(sprite.FilePath); // put the file in memory
                    Texture2D texture = new(200, 200); // make a texture
                    if (!texture.LoadImage(data)) // load the data into the texture
                    {
                        Startup.Logger.LogError($"Failed to load image data for {sprite.Game}'s {sprite.Item}! Path = {sprite.FilePath}");
                        continue; // couldn't load this one, onto the next
                    }
                    Sprite finalSprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f), 30); // texture > sprite
                    renderer.sprite = finalSprite; // apply it to the Archipelago item.
                    if (sprite.Item.IsNullOrWhiteSpace())
                    {
                        item.Stats.fullName = info.Value.ItemName;
                    }
                    else
                    {
                        item.Stats.fullName = sprite.Item;
                    }
                }
                else // game is unsupported or the github repo cannot be reached
                {
                    item.Stats.fullName = info.Value.ItemName;
                }
            }
        }
        catch (Exception ex)
        {
            Startup.Logger.LogError($"AssignCustomSprite Failed! {ex}");
            APCanvas.EnqueueArchipelagoNotification($"AssignCustomSprite Failed!<br>{ex}", 3);
        }
        finally
        {
            spriteSemaphore.Release();
        }
    }
}