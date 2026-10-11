using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using Archipelago.MultiClient.Net;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

public class Gifting
{
    public class GiftDefinition
    {
        public string Name; // item's in-game display name
        public string PrefabName; // item's prefab/internal name
        public GiftTrait[] Traits; // item traits
    }
    private static ArchipelagoSession Client;
    public static GiftingService giftService;
    private static ICloseTraitParser<string> giftParser = new BKTreeCloseTraitParser<string>();
    public static bool shouldGiftBoxExist = false;
    public static Dictionary<string, GiftDefinition> Gifts = new Dictionary<string, GiftDefinition>();
    public static void SetupGifting()
    {
        if (giftService != null)
        {
            Startup.Logger.LogWarning("Attempted to set up Gifting more than once. Probably after a reconnect?");
            return;
        }
        Client = APClientClass.session;
        if (Client == null || !Client.Socket.Connected)
        {
            Startup.Logger.LogError("Attempted to set up Gifting without being connected!");
            return;
        }
        giftService = new GiftingService(Client);
        shouldGiftBoxExist = true;
        giftService.CloseGiftBox(); // close the box if it's open
        RegisterEveryReasonablySpawnableObjectInsideTheGiftingLibraryBKCloseTraitParserToMakeSureThatAnyPlayerPlayingTheIndieGameCasualtiesUnknownCreatedByIndieDeveloperOrsoniksCanSendOrRecieveGiftsToAnyOtherVideoGameThatSupportsBothTheArchipelagoMultiGameRandomizerAndTheGiftingAPI();
        giftService.OnNewGift += OnGiftReceived;
        // todo: set up sending gifts (get receiving working first)
        // send gifts using a new command: apgift [inventorySlot] [playerAlias]
        // take the item in the slot specified and send it to that player. If the slot is empty, do nothing.
    }
    private static void OnGiftReceived(Gift gift)
    {
        Startup.Logger.LogDebug("Received gift from " + Client.Players.GetPlayerAlias(gift.SenderSlot) + " in slot " + gift.SenderSlot);
        Startup.Logger.LogDebug("Gifted item name: " + gift.ItemName);
        Startup.Logger.LogDebug("Is refund?: " + gift.IsRefund);
        if (gift.Traits == null || gift.Traits.Length == 0)
        {
            Startup.Logger.LogDebug("Gifted item has no traits.");
        }
        else
        {
            Startup.Logger.LogDebug("Gifted item traits: ");
            foreach (GiftTrait trait in gift.Traits)
            {
                // Casualties: Unknown doesn't use the duration of traits, so it's ignored in the debugging code as well
                Startup.Logger.LogDebug(trait.Trait + " with a quality of " + trait.Quality);
            }
        }
        if (!APCanvas.InGame) // this should hopefully never trigger, since the box should be closed when not in-game anyway
        {
            giftService.RefundGift(gift);
            giftService.CloseGiftBox();
            Startup.Logger.LogError("Received a gift while not in-game! The Giftbox wasn't closed properly somewhere!");
            APCanvas.EnqueueArchipelagoNotification(APLocale.Get("giftOutOfGame", APLocale.APLanguageType.Errors), 3);
            return;
        }
        // TODO: skip trait parsing if an exact name match exists
        List<string> matches = giftParser.FindClosestAvailableGift(gift.Traits); // get the closest available items that have the traits this gift does
        if (matches == null || matches.Count == 0)
        {
            giftService.RefundGift(gift); // no matches could be found, refund this gift
            string sender = Client.Players.GetPlayerName(gift.SenderSlot);
            Startup.Logger.LogWarning($"No matches found for received gift {gift.ItemName} from {sender}. Refunding..."); // for the bepinex log
            CommandPatch.LogToConsole($"<color=#FF0000>CUAP: No matches found for received gift {gift.ItemName} from {sender}. Refunding...</color>"); // for the ingame console
            return;
        }
        string match = matches[0]; // only use the first match
        Startup.Logger.LogDebug("Found match for gifted item: " + match); // log what item was matched
        if (Gifts.TryGetValue(match, out GiftDefinition info)) // get the prefab to spawn
        {
            Vector2 playerPos = GameObject.Find("Experiment/Body").transform.position; // player's current position (at the body's center)
            Object.Instantiate(Resources.Load<GameObject>(info.PrefabName), playerPos, Quaternion.identity); // spawn the object there
            APCanvas.EnqueueArchipelagoNotification($"You've been gifted a {match} from {Client.Players.GetPlayerAlias(gift.SenderSlot)}!", 1);
        }
        else // couldn't find the item inside the list
        {
            Startup.Logger.LogError("Gifted item is not inside the prefab list!");
            giftService.RefundGift(gift); // refund the gift since the game can't spawn the item
        }
    }
    private static void RegisterGift(string name, string prefabName, GiftTrait[] traits)
    {
        Gifts[name] = new GiftDefinition
        {
            Name = name,
            PrefabName = prefabName,
            Traits = traits
        };
        giftParser.RegisterAvailableGift(name, traits);
    }
    // here comes a funny function name
    private static void RegisterEveryReasonablySpawnableObjectInsideTheGiftingLibraryBKCloseTraitParserToMakeSureThatAnyPlayerPlayingTheIndieGameCasualtiesUnknownCreatedByIndieDeveloperOrsoniksCanSendOrRecieveGiftsToAnyOtherVideoGameThatSupportsBothTheArchipelagoMultiGameRandomizerAndTheGiftingAPI()
    {
        // TODO: go through all the things with the "Damage" trait and scale by raw DPS instead of just feel to make it less subjective.
        // TODO: certain tools deal higher damage against living enemies than if used on blocks. only titanium tools were tested for this.
        // TODO: go through all the things with the "Cure" trait and scale by their in-game "Antiseptic" quality.

        // Gifting API specifications say that a Quality and Duration of 1.0 is considered "average" for that game
        // "Duration" doesn't really mean anything inside of Casualties, so all Casualties items will have a Duration of 1.0
        RegisterGift("12-Gauge buckshot", "12gauge", [
            // "Ammo" itself is not a "common" trait exposed by the API, but I feel as if it's generic enough
            // The "quality" of Ammo determines its damage...
            // 12-Gauge Shotguns do ~50-65 damage per pellet, Rifles do ~140-150 damage, Pistols do ~75-90 damage
            // 12-Gauge is in the middle of those ranges (since it has multiple pellets), so it will be of "average" quality (don't tell the multiplayer pvp community)
            new GiftTrait { Trait = "Ammo", Quality = 1.0 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 3.0 } // high damage (compared to other sources) so high quality
        ]);
        RegisterGift("5.56 round", "556round", [
            new GiftTrait { Trait = "Ammo", Quality = 1.5 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 3.5 }
        ]);
        RegisterGift("9mm round", "9mmround", [
            new GiftTrait { Trait = "Ammo", Quality = 0.5 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 }
        ]);
        RegisterGift("Adhesive bandage", "adhesivebandage", [
            new GiftTrait { Trait = "Consumable" },
            // I will be using "Heal" to determine how good an item is at stopping bleeding or restoring skin/muscle health
            new GiftTrait { Trait = "Heal", Quality = 0.5 } // lowers bleeding by 0.63l/m (if staggered properly)
        ]);
        RegisterGift("Automated external defibrillator", "aed", [
            new GiftTrait { Trait = "Consumable" },
            // I will use "Life" for items that can heal any critical conditions. In this case, that's fibrillation
            new GiftTrait { Trait = "Life", Quality = 2 } // higher quality than the manual defib
        ]);
        RegisterGift("Bottle of alcohol", "alcohol", [
            new GiftTrait { Trait = "Consumable" },
            // I will be using "Cure" to determine how good an item is at disinfecting limbs or increasing body immunity
            new GiftTrait { Trait = "Cure", Quality = 1.5 } // full disinfect, four times
        ]);
        RegisterGift("Alginate dressing", "alginate", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 4 }, // 1l/m+ of bleeding, and restores skin health from 0->100 in a single go
            new GiftTrait { Trait = "Cure", Quality = 0.5 } // can disinfect, but it's not the best at it
        ]);
        RegisterGift("Amiodarone bottle", "amiodarone", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 0.5 } // fixes fibrillation. works slowly and requires a syringe to use. not a great choice
        ]);
        RegisterGift("Medical gauze", "analgesicgauze", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1.5 } // 0.75l/m of bleeding + opioids for pain relief
        ]);
        RegisterGift("Animal flesh", "animalflesh", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 7 hunger, downside of 4% sickness
            new GiftTrait { Trait = "Meat", Quality = 1 } // can be used as meat in cooking based recipes
        ]);
        RegisterGift("Antibiotics bottle", "antibiotics", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 3 }, // 5 uses, each boosting body immunity
            // Things that have a lasting effect will have the "Buff" trait
            new GiftTrait { Trait = "Buff" }
        ]);
        RegisterGift("Antidepressants", "antidepressants", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" } // 7 uses, each gradually increasing mood
        ]);
        RegisterGift("Anti-rad bottle", "antirad", [
            // I don't really have a good way to seperate this from other anti items, but I'm open to suggestions
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" } // 5 uses, each gradually decreasing radiation sickness
        ]);
        RegisterGift("Antiseptic mush", "antisepticmush", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 3 } // single use, but fully disinfects all adjacent limbs
        ]);
        RegisterGift("Antiserum syringe", "antiserum", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 2.5 } // increases immunity and decreases sepsis
        ]);
        RegisterGift("Apple juice carton", "applejuice", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1 }, // 9 thirst, 8 uses
            new GiftTrait { Trait = "Fruit" } // apple
        ]);
        RegisterGift("Aquapple", "aquapple", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.25 } // 7 thirst, 1 use, decays rapidly
        ]);
        RegisterGift("Arm warmers", "armwarmers", [
            // I will be using "Armor" to determine how good a wearable item is
            new GiftTrait { Trait = "Armor", Quality = 1 } // 6 hour decay time, 25% protection, 10% insulation
        ]);
        RegisterGift("Auto-injector", "autoinjector", [
            // Since the API was built off of Stardew Valley, items used in crafting or construction are called materials
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Life", Quality = 1 } // Auto-injector is used in medical recipes
        ]);
        RegisterGift("Auto-pump", "autopump", [
            new GiftTrait { Trait = "Armor", Quality = 0 }, // doesn't actually give any defense (trade off for its live-saving ability)
            new GiftTrait { Trait = "Life", Quality = 3 } // Auto-pump keeps your blood pressure livable as long as it has power
        ]);
        RegisterGift("Auto-zoom googles", "autozoomgoggles", [
            new GiftTrait { Trait = "Armor", Quality = 0 }, // no defense
            new GiftTrait { Trait = "Buff", Quality = 1.5 } // lasting effect (2 hours, as long as its powered)
        ]);
        RegisterGift("Balaclava", "balaclava", [
            new GiftTrait { Trait = "Armor", Quality = 1 } // no decay time, 25% protection, 20% insulation
        ]);
        RegisterGift("Banana", "banana", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 9 hunger, 2 uses
            new GiftTrait { Trait = "Drink", Quality = 0.3 }, // 4 thirst, 2 uses
            new GiftTrait { Trait = "Fruit" } // banana
        ]);
        RegisterGift("Banana-plant", "bananaplant", [
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Damage" },
            new GiftTrait { Trait = "Fruit" } // i debated also adding the Vegetable trait, but nah
        ]);
        RegisterGift("Dressing", "bandage", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 } // 0.7l/m of bleeding
        ]);
        RegisterGift("Bandolier", "bandolier", [
            new GiftTrait { Trait = "Armor", Quality = 0 }, // no defense
            // Container trait is self explanatory. This item can hold other items
            new GiftTrait { Trait = "Container", Quality = 0.25 } // 5hr decay, 3u capacity, 0.05u per item (made for bullets only)
        ]);
        RegisterGift("Belly armor", "bellyarmor", [
            new GiftTrait { Trait = "Armor", Quality = 1.5 }, // no decay, 50% defense, 8% insulation
        ]);
        RegisterGift("Belt", "belt", [
            new GiftTrait { Trait = "Container", Quality = 0.75 } // >24hr decay time, 4u capacity, 4u per item
        ]);
        RegisterGift("Backpack", "bigpack", [
            new GiftTrait { Trait = "Armor", Quality = 0.1 }, // no defense, 7% insulation
            new GiftTrait { Trait = "Container", Quality = 1 } // 1hr decay time, 7.5u capacity, 5u per item
        ]);
        RegisterGift("Bicycle helmet", "bikehelmet", [
            new GiftTrait { Trait = "Armor", Quality = 1.75 }, // no decay, 50% defense, 8% insulation (higher quality because it protects the head)
        ]);
        RegisterGift("Bleach bottle", "bleach", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 3 } // full disinfect, 15 times
        ]);
        RegisterGift("Blindfold", "blindfold", [
            new GiftTrait { Trait = "Armor", Quality = 0.5 }, // no decay, 10% defense, 10% insulation
            new GiftTrait { Trait = "Trap" } // downside of blinding you
        ]);
        RegisterGift("Blobflesh", "blobflesh", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.1 }, // 6 hunger, downside of 8% sickness
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        RegisterGift("Blood bag", "bloodbag", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 },
            new GiftTrait { Trait = "Life", Quality = 1 }
        ]);
        RegisterGift("Red blood bag", "bloodbaghuman", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 },
            new GiftTrait { Trait = "Life", Quality = 1 },
            new GiftTrait { Trait = "Trap" } // eh? not sure how to seperate them otherwise
        ]);
        RegisterGift("Procoagulant syringe", "bloodcoagulant", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2 } // slows/stops bleeding bodywide
        ]);
        RegisterGift("Blood crystal shard", "bloodcrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2 }, // adds blood volume
            new GiftTrait { Trait = "Buff" } // gradually restores over 20 minutes
        ]);
        RegisterGift("Blood sac", "bloodsac", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Heal", Quality = 1 }
        ]);
        RegisterGift("Bone welding tool", "boneweldingtool", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Life", Quality = 1 } // fixes broken bones, downside of causing bleeding
        ]);
        RegisterGift("Bowl of cereal", "bowlofcereal", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.75 }, // 6 hunger, 5 times
            new GiftTrait { Trait = "Drink", Quality = 0.5 } // 8 thirst, 5 times
        ]);
        RegisterGift("Cardboard box", "box", [
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 30min decay, 4u capacity, 2u per item
        ]);
        RegisterGift("Box of 12-Gauge", "boxof12gauge", [
            new GiftTrait { Trait = "Ammo", Quality = 3.5 }, // 16 shells, so higher quality
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 3.0 }
        ]);
        RegisterGift("Braingrow bottle", "braingrow", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 3 } // regenerates brain health
        ]);
        RegisterGift("Bread", "bread", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1 }, // 10 hunger, 3 times
            new GiftTrait { Trait = "Drink", Quality = 0.1 } // 2 thirst, 3 times (for some reason?)
        ]);
        RegisterGift("Backpack", "bigpack", [
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 1hr decay time, 6.25u capacity, 2.25u per item
        ]);
        RegisterGift("Browncap", "browncap", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 10 hunger, downside of a random effect
            // Items with the Vegetable trait have the Produce trait ingame
            new GiftTrait { Trait = "Vegetable" }
        ]);
        RegisterGift("Bruise kit", "bruisekit", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2.5 } // 20 skin health, 10 muscle health
        ]);
        RegisterGift("Bucket of chicken", "bucketofchicken", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.25 }, // 10 hunger, 5 times, downside of weight+hands only
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        RegisterGift("Bucket of no chicken", "bucketofnochicken", [
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 20min decay time, 3u capacity, 3u per item
        ]);
        RegisterGift("Popskin", "bulbskin", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Thirst", Quality = 0.25 }, // 4 thirst, 3 times
            new GiftTrait { Trait = "Heal", Quality = 0.25 } // 0.12l/m bleeding, no skin or muscle health
        ]);
        RegisterGift("Bundle of wires", "bundleofwires", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" } // copper
        ]);
        RegisterGift("Burger", "burger", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.5 }, // 15 hunger, 3 times
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        RegisterGift("Cactus flesh", "cactusflesh", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 4 hunger
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 4 thirst
            new GiftTrait { Trait = "Vegetable" }
        ]);
        RegisterGift("Cake", "cake", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1 }, // 8 hunger, 10 times
        ]);
        RegisterGift("Campfire", "campfire", [
            new GiftTrait { Trait = "Fire" } // literally a fire
        ]);
        RegisterGift("Candy bar", "candybar", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 6 hunger but lasts a while
        ]);
        RegisterGift("Canteen", "canteen", [
            new GiftTrait { Trait = "Drink" }, // Container that holds a drink
            new GiftTrait { Trait = "Container", Quality = 1 } // 300ml capacity
        ]);
        RegisterGift("Canvas", "canvas", [
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Carapace", "carapace", [
            new GiftTrait { Trait = "Armor", Quality = 2 } // 50% protection, 15% insulation, on a critical limb
        ]);
        RegisterGift("Stiff carcass", "carcass", [
            new GiftTrait { Trait = "Material" },
            // The "Monster" trait is used when this item is dropped by an enemy
            // The trait's Quality determines how easy that enemy is to kill
            // This trait is also used when the enemies themselves are gifted, though it will also have the "Trap" trait
            new GiftTrait { Trait = "Monster", Quality = 0.75 } // dropped from an early-game enemy
        ]);
        RegisterGift("Carcass splint", "carcasssplint", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 1.5 }, // splint. heals broken bones
        ]);
        RegisterGift("Cave tick", "cavetick", [
            new GiftTrait { Trait = "Trap" },
            // In this instance, the "Monster" trait is used to determine how difficult this enemy is to kill
            new GiftTrait { Trait = "Monster", Quality = 0.25 }, // very easy to kill
        ]);
        RegisterGift("Ceftriaxone syringe", "ceftriaxone", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 2 }, // bodywide immunity boost, downside of pain
        ]);
        RegisterGift("Cereal", "cereal", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 6 hunger, 5 times, downside of -3 thirst each time
        ]);
        RegisterGift("Charcoal", "charcoal", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Fire", Quality = 0.5 } // can make a fire
        ]);
        RegisterGift("Chest drain", "chestdrain", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Life", Quality = 0.5 } // solves hemothorax
        ]);
        RegisterGift("Chips", "chips", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 3 hunger, 10 times, downside of 3% sickness each time
        ]);
        RegisterGift("Chocolate bar", "chocolatebar", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0 } // 7 hunger, 3 times, downside of 20% sickness each time. arguably worst food item in the game
        ]);
        RegisterGift("Chocolate milk bottle", "chocolatemilk", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.5 }, // 8 thirst, 10 times, downside of 12% sickness each time
            new GiftTrait { Trait = "Food", Quality = 0 } // 2 hunger, 10 times (for some reason?)
            // Fun fact: Despite the item description saying that this increases happiness, it doesn't!
        ]);
        RegisterGift("Circuit board", "circiutboard", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Energy", Quality = 1 }
        ]);
        RegisterGift("Claw pack", "claws", [
            new GiftTrait { Trait = "Tool", Quality = 1 }, // cutting tool
            // All weapons will have the tool trait, but not all tools will have the weapon trait
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.25 } // barely better than normal damage, but really fast
        ]);
        RegisterGift("Climbing claws", "climbingclaws", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Buff" } // movement buff
        ]);
        RegisterGift("Climbing rope", "climbingrope", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Consumable" } // 2 uses
        ]);
        RegisterGift("Clotting mush", "clottingmush", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }, // used in crafting
            new GiftTrait { Trait = "Heal", Quality = 2 } // 1.18l/m of bleeding
        ]);
        RegisterGift("Coffee cup", "coffee", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.8 }, // 12 thirst, 3 times, downside of 15% sickness each time
            new GiftTrait { Trait = "Buff" } // energizing
        ]);
        RegisterGift("Combat pen", "combatpen", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 4 }, // epinephrine, stimulant, and oxyline all at once at safe doses
            new GiftTrait { Trait = "Buff" } // 60ml of Medical-grade stimulant
        ]);
        RegisterGift("Box of cookies", "cookies", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 3 hunger, 10 times, downside of 3% sickness each time
        ]);
        RegisterGift("Crude cleaver", "crudecleaver", [
            new GiftTrait { Trait = "Tool", Quality = 0.5 } // cutting tool
        ]);
        RegisterGift("Digestion crystal shard", "digestioncrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 0.5 }, // increases immunity by decreasing sickness slowly
            new GiftTrait { Trait = "Buff" }
        ]);
        RegisterGift("Antiseptic", "disinfectant", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 1.5 }, // 20 uses
        ]);
        RegisterGift("Dog food", "dogfood", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.8 }, // 6 hunger, 5 times
        ]);
        RegisterGift("Drill repair kit", "drillrepairkit", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Resource" }
        ]);
        RegisterGift("Dried foliage", "dryfoliage", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.2 }, // 6 hunger, 5% sickness
        ]);
        RegisterGift("Duffel bag", "duffelbag", [
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Container", Quality = 1 } // 2hr decay time, 5u capacity, 2.8u per item
        ]);
        RegisterGift("Dust mask", "dustmask", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Armor", Quality = 0.3 } // 10% protection, 8% insulation, on head
        ]);
        RegisterGift("Dynamite", "dynamite", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Consumable" }
        ]);
        RegisterGift("Emergency light", "emergencylight", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 0.75 }
        ]);
        RegisterGift("Emissive crystal shard", "emissivecrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Light", Quality = 2 },
            new GiftTrait { Trait = "Material" } // used in crafting the lantern
        ]);
        RegisterGift("Energy drink can", "energydrink", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.70 }, // 10 thirst, 3 times, downside of 20% sickness each time
            new GiftTrait { Trait = "Buff" } // energizing
        ]);
        RegisterGift("Yellow flesh", "experimentflesh", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.4 }, // 12 hunger, downside of 16% sickness
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        RegisterGift("Fanny pack", "fannypack", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.25 } // 2hr decay time, 2u capacity, 0.8u per item
        ]);
        RegisterGift("Fentanyl syringe", "fentanyl", [
            new GiftTrait { Trait = "Consumable" },
            // The concept of opiods being classified as Mana sounds absurd on paper and was originally a joke
            // but it made more and more sense the more I thought about it, so now it's staying
            new GiftTrait { Trait = "Mana", Quality = 3 } // quality is based on the in-game quality per ml
        ]);
        RegisterGift("Filter straw", "filterstraw", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Drink" }
        ]);
        RegisterGift("Firestarter", "firestarter", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Fire", Quality = 0.75 } // can create fire
        ]);
        RegisterGift("Flammable powder", "flammablepowder", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Fire" }
        ]);
        RegisterGift("Flashlight", "flashlight", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 1.25 }
        ]);
        RegisterGift("Big flesh chunk", "fleshchunk", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.5 }, // 15 hunger, 5 times
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        RegisterGift("Flexiglass", "flexiglass", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Flimsy knife", "flimsyknife", [
            new GiftTrait { Trait = "Tool", Quality = 1 }, // cutting tool
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 0.5 } // worse than normal damage, terrible durability, but a little faster
        ]);
        RegisterGift("Foliage", "foliage", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 5 hunger, downside of 20% sickness
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Foliage bag", "foliagebag", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 25min decay time, 4u capacity, 2u per item
        ]);
        RegisterGift("Foliage meal", "foliagemeal", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1 }, // 18 hunger, 2 times, downside of 3% sickness each
            new GiftTrait { Trait = "Drink", Quality = 0.25 } // 2 thirst, 2 times
        ]);
        RegisterGift("Frigiant fruit", "frigiantfruit", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 2 hunger, 2 times
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 3 thirst, 2 times
            new GiftTrait { Trait = "Ice", Quality = 1.5 } // cools you down by 3.5 degrees instantly
        ]);
        RegisterGift("Fungus chunk", "funguschunk", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 10 hunger, downside of -20 thirst
        ]);
        RegisterGift("Geiger counter", "geigercounter", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Life" } // helps prevent radiation
        ]);
        RegisterGift("Geofruit", "geofruit", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.4 }, // 5 hunger, 2 times
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 3 thirst, 2 times
            new GiftTrait { Trait = "Fruit" }, // its in the name
            new GiftTrait { Trait = "Vegetable" } // produce trait
        ]);
        RegisterGift("Glowplant fruit", "glowplantfruit", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Light", Quality = 0.25 },
            new GiftTrait { Trait = "Cure", Quality = 0.5 } // can disinfect
        ]);
        RegisterGift("Grappling hook", "grapplinghook", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Buff" }, // movement buff
            new GiftTrait { Trait = "Speed" } // eh?
        ]);
        RegisterGift("Grav-pack", "gravpack", [
            new GiftTrait { Trait = "Container", Quality = 2 } // 5u capacity, 4u per item, zero weight when powered
        ]);
        RegisterGift("Hand crank charger", "handcrank", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Energy" } // recharges batteries
        ]);
        RegisterGift("Candy", "hardcandy", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.1 }, // 1 hunger, 5 times, downside of 3% sickness each
        ]);
        RegisterGift("Headlamp", "headlamp", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 1.3 } // same as normal light. higher quality since it doesn't take a slot
        ]);
        RegisterGift("Heavy drill", "heavydrill", [
            new GiftTrait { Trait = "Tool", Quality = 1 }, // hammering tool
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 } // 60 damage and really fast, but little range
        ]);
        RegisterGift("Helluce", "helluce", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Fire", Quality = 0.25 } // increases body temperature by 2 degrees instantly
        ]);
        RegisterGift("Heroin syringe", "heroin", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Mana", Quality = 1.5 }
        ]);
        RegisterGift("Holiday hat", "holidayhat", [
            new GiftTrait { Trait = "Armor", Quality = 0.6 } // 3hr decay, 10% defense, 20% insulation
        ]);
        RegisterGift("Hoodie", "hoodie", [
            new GiftTrait { Trait = "Armor", Quality = 1.3 }, // 8hr decay, 20% defense to most of upper body, 60% insulation
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 8hr decay time, 2u capacity, 2u per item
        ]);
        RegisterGift("Ice pack", "icepack", [
            new GiftTrait { Trait = "Ice", Quality = 1 }, // cools you down by 1 degree instantly, and chills a given limb
            new GiftTrait { Trait = "Heal", Quality = 1.5 } // increases muscle healing speed. reusable
        ]);
        RegisterGift("Iced tea bottle", "icetea", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1 } // 10 thirst, 5 times, downside of 3% sickness each time
        ]);
        RegisterGift("Ilmenite chunk", "ilmenitechunk", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" },
            new GiftTrait { Trait = "Ore" } // I don't think I need to explain this one
        ]);
        RegisterGift("Internal organs", "internalorgans", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.2 }, // 15 hunger, 3 times, downside of 32% sickness and ~-22 mood each time
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        RegisterGift("Jetpack", "jetpack", [
            new GiftTrait { Trait = "Armor", Quality = 0.2 }, // no decay, 0% defense, 5% insulation
            new GiftTrait { Trait = "Container", Quality = 1 }, // no decay time, 2.5u capacity, 2u per item
            new GiftTrait { Trait = "Buff" } // movement buff
        ]);
        RegisterGift("Keratin booster", "keratinbooster", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" }
        ]);
        RegisterGift("Ketchup bottle", "ketchup", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 8 hunger, 3 times, downside of 5% sickness each time
            new GiftTrait { Trait = "Drink", Quality = 0.3 }, // 6 thirst, 3 times, downside of 5% sickness each time
            new GiftTrait { Trait = "Fruit" }
        ]);
        RegisterGift("Kneepads", "kneepads", [
            new GiftTrait { Trait = "Armor", Quality = 1.2 } // no decay, 50% defense, 5% insulation. Lowered quality because it's on a non-vital limb
        ]);
        RegisterGift("Lantern", "lantern", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 2 }
        ]);
        RegisterGift("Large battery", "largebattery", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Energy", Quality = 2 } // duh
        ]);
        RegisterGift("Large carcass", "largecarcass", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Monster", Quality = 1.25 }
        ]);
        RegisterGift("Latex gloves", "latexgloves", [
            new GiftTrait { Trait = "Armor", Quality = 0.1 }, // 5hr decay, no defense, 8% insulation
        ]);
        RegisterGift("LCD screen", "lcdscreen", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Leg pouch", "legpouch", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 3hr decay time, 3.8u capacity, 3u per item
        ]);
        RegisterGift("Lemonade jar", "lemonadejar", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1.5 } // 9 thirst, 8 times
        ]);
        RegisterGift("Lightbulb", "lightbulb", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Light", Quality = 1.25 }
        ]);
        RegisterGift("Lighter", "lighter", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Fire", Quality = 1.5 } // can create fire (and is really good at it)
        ]);
        RegisterGift("Limb wraps", "limbwraps", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Armor", Quality = 1.5 } // 5hr decay, 20% defense, 7% insulation. Higher quality because it covers multiple limbs
        ]);
        RegisterGift("Liquid centrifuge", "liquidcentrifuge", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Drink" } // a tool that is liquid related (kind of a stretch, I know)
        ]);
        RegisterGift("Liquid pouch", "liquidpouch", [
            new GiftTrait { Trait = "Drink" }, // Container that holds a drink
            new GiftTrait { Trait = "Container", Quality = 1.5 } // 1000ml capacity AND a wearble instead of taking a slot (canteen)
        ]);
        RegisterGift("Lock-picking kit", "lockpickingkit", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Key" } // how else would I use this trait?
        ]);
        RegisterGift("L.R.D.", "lrd", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2 }, // do i need to explain these numbers? L.R.D. my goat
            new GiftTrait { Trait = "Life", Quality = 2 },
            new GiftTrait { Trait = "Cure", Quality = 2 }
        ]);
        RegisterGift("Machete", "machete", [
            new GiftTrait { Trait = "Tool", Quality = 2 }, // cutting AND hammering tool. can also amputate
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.5 } // 36 damage, slightly slower than normal, high durability
        ]);
        RegisterGift("Magazine base", "magazinebase", [
            new GiftTrait { Trait = "Ammo" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Makeshift digging tool", "makeshiftdiggingtool", [
            new GiftTrait { Trait = "Tool", Quality = 1.2 }, // cutting AND hammering tool. cannot amputate
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.75 } // 62 damage, slightly slower than normal, low durability
        ]);
        RegisterGift("Makeshift headlamp", "makeshiftheadlamp", [
            new GiftTrait { Trait = "Light", Quality = 0.75 } // low light, but it doesn't take up a slot
        ]);
        RegisterGift("Makeshift helmet", "makeshifthelmet", [
            new GiftTrait { Trait = "Armor", Quality = 0.75 } // 2hr decay, 43% defense, 4% insulation, covers a critical limb
        ]);
        RegisterGift("Makeshift L.R.D.", "makeshiftlrd", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1.75 }, // slightly lower than the normal L.R.D. because makeshift has 2 uses while the normal one has 3
            new GiftTrait { Trait = "Life", Quality = 1.75 },
            new GiftTrait { Trait = "Cure", Quality = 1.75 }
        ]);
        RegisterGift("Makeshift rifle", "makeshiftrifle", [
            // Need I explain this trait?
            new GiftTrait { Trait = "Gun", Quality = 0.5 }, // lower durability, jams easier. loaded with 9mm.
            new GiftTrait { Trait = "Weapon" }
        ]);
        RegisterGift("Makeshift wrench", "makeshiftwrench", [
            new GiftTrait { Trait = "Tool", Quality = 0.8 } // hammering tool. low durability. cannot help with dislocations
        ]);
        RegisterGift("Manual defibrillator", "aed", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 1.5 } // lower quality than the automatic defib
        ]);
        RegisterGift("Material pouch", "materialpouch", [
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 5hr decay, 5u capacity, 0.25u per item
        ]);
        RegisterGift("Medical suture", "medicalsuture", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 3 } // 1.27l/m of bleeding, 50 skin health
        ]);
        RegisterGift("Medium battery", "mediumbattery", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Energy", Quality = 1.5 } // duh
        ]);
        RegisterGift("Med-kit", "medkit", [
            new GiftTrait { Trait = "Container", Quality = 1 } // 4hr decay time, 4u capacity, 1.5u per item
        ]);
        RegisterGift("Milk carton", "milk", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.9 }, // 3 hunger, 10 times
            new GiftTrait { Trait = "Drink", Quality = 1.75 } // 8 thirst, 10 times
        ]);
        RegisterGift("Mindwipe", "mindwipe", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Mana", Quality = 0.15 }, // contains 30ml of Morphine, which could be extracted with a centrifuge?
            new GiftTrait { Trait = "Life", Quality = 0.25 } // can cure brain damage, but comes at the downside of removing all skills and dialogue
            // This is such a unique item that it's hard to give it proper traits
            // Should this have the Trap trait? It's not forced on the player when they get gifted it, but it's hard to say this is exactly a good item to receive in most cases
        ]);
        RegisterGift("Mini laser drill", "minilaserdrill", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.5 } // 25 damage, about twice as fast than normal, zero stamina usage
        ]);
        RegisterGift("Morphine syringe", "morphine", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Mana", Quality = 1 }
        ]);
        RegisterGift("Wrapvine", "musharm", [ // (yes, it's still internally called that)
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }, // can be used as foliage
            new GiftTrait { Trait = "Heal", Quality = 0.25 } // 0.11l/m of bleeding
        ]);
        RegisterGift("Numberry", "mushpear", [ // (same with this one)
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.3 }, // 5 hunger, downside of 7% sickness
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 3 thirst, downside of 7% sickness
            new GiftTrait { Trait = "Mana", Quality = 0.2 } // contains some opiates
            // Not sure how to show the weight loss effect through traits
        ]);
        RegisterGift("Naptail", "mushtail", [ // (and this one too)
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Slowness" } // not sure how else to describe this one
        ]);
        RegisterGift("Nails", "nails", [
            new GiftTrait { Trait = "Metal" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Naloxone syringe", "naloxone", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life" } // anti-overdose
        ]);
        RegisterGift("Neural booster", "neuralbooster", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Speed" },
            new GiftTrait { Trait = "Life", Quality = 1.5 } // increases brain health by 10
            // fun fact! the 'heal' command doesn't remove the flag of having used this! learned that the hard way by accidentally jumpscaring myself
        ]);
        RegisterGift("No popcorn", "nopopcorn", [
            new GiftTrait { Trait = "Container", Quality = 0.3 } // 20min decay time, 2u capacity, 2u per item
        ]);
        RegisterGift("Nutrient bar", "nutrientbar", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.5 } // 12 hunger, 3 times, doesn't spoil
        ]);
        RegisterGift("Opium syringe", "opium", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Mana", Quality = 0.5 }
        ]);
        RegisterGift("Overgrown tick", "overgrowntick", [
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Monster", Quality = 0.75 }
        ]);
        RegisterGift("Oxygen crystal shard", "oxygencrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" } // stamina regen
        ]);
        RegisterGift("Relief cream bottle", "paincream", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 1.5 }
        ]);
        RegisterGift("Painkiller bottle", "painkillers", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Mana", Quality = 1 }
        ]);
        RegisterGift("Pancakes", "pancake", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.75 } // 10 hunger, 4 times, downside of 1% sickness each time
        ]);
        RegisterGift("Paprikash", "paprikash", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Meat" }, // has meat quality
            new GiftTrait { Trait = "Food", Quality = 1.5 } // 12 hunger, 2 times, downside of 2% sickness each time
        ]);
        RegisterGift("Pemmican", "pemmican", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Meat" }, // has meat quality
            new GiftTrait { Trait = "Food", Quality = 2 } // 12 hunger, 4 times
        ]);
        RegisterGift("Pickaxe", "pickaxe", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 2 } // 106 damage, quite slow
        ]);
        RegisterGift("Pistol", "pistol", [
            new GiftTrait { Trait = "Gun", Quality = 1 }, // loaded with 9mm
            new GiftTrait { Trait = "Weapon" }
        ]);
        RegisterGift("Metal pitchfork", "pitchfork", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.2 } // 38 damage, slightly slower than normal
        ]);
        RegisterGift("Pizza slice", "pizzaslice", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Meat" }, // has meat quality
            new GiftTrait { Trait = "Food", Quality = 1.8 } // 12 hunger, 3 times
        ]);
        RegisterGift("Plasma cutter", "plasmacutter", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 }, // 10 damage, nearly instant, no stamina usage
            new GiftTrait { Trait = "Heal", Quality = 2.5 } // decreases bleeding by 80% on the selected limb, downside of pain
        ]);
        RegisterGift("Plastic bag", "plasticbag", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 45min decay time, 3.5u capacity, 3.5u per item
        ]);
        RegisterGift("Plastic dressing", "plasticbandage", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1.5 } // 1.1l/m of bleeding, 30 skin health
        ]);
        RegisterGift("Chunk of plastic", "plasticchunk", [
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Plushie", "plushie", [
            new GiftTrait { Trait = "Animal" } // the gifting API says that Animal can also mean "Companion" (can you tell I have no ideas for this one?)
            // new GiftTrait { Trait = "weh" }
        ]);
        RegisterGift("Popcorn", "popcorn", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.75 } // 9 hunger, 5 times, leaves a useable container afterwards
        ]);
        RegisterGift("Popfruit", "popfruit", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 4 hunger, 2 times, downside of radiation
            new GiftTrait { Trait = "Drink", Quality = 0.15 }, // 2 thirst, 2 times, downside of radiation
        ]);
        RegisterGift("Pouch", "pouch", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.4 } // 1hr decay time, 2.5u capacity, 1u per item
        ]);
        RegisterGift("Metal digging tool", "primitivediggingtool", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.5 } // 72 damage, slightly slower than normal
        ]);
        RegisterGift("Processed copper", "processedcopper", [
            new GiftTrait { Trait = "Metal" },
            new GiftTrait { Trait = "Ore" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Purse", "purse", [
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 2hr decay time, 2.5u capacity, 1.25u per item
        ]);
        RegisterGift("Old rag", "rag", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 0.25 } // 0.18l/m of bleeding
        ]);
        RegisterGift("Rake", "rake", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1 } // 33 damage, half normal speed
        ]);
        RegisterGift("Rangefinder", "rangefinder", [
            new GiftTrait { Trait = "Tool" } // yeah, I got no idea
        ]);
        RegisterGift("Raw copper", "rawcopper", [
            new GiftTrait { Trait = "Metal" },
            new GiftTrait { Trait = "Ore", Quality = 0.5 },
            new GiftTrait { Trait = "Material" }
        ]);
        //RegisterGift("???", "reinforcedrope", [
            // This item DOES exist in the spawn list and can be spawned, but has no item data, no name, no description, and no functionality. Putting this here for futureproofing.
            //new GiftTrait { Trait = "sorry nothing" }
        //]);
        RegisterGift("Relief crystal shard", "reliefcrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" } // pain reduction
        ]);
        RegisterGift("Rifle", "rifle", [
            new GiftTrait { Trait = "Gun", Quality = 1.25 }, // loaded with 5.56
            new GiftTrait { Trait = "Weapon" }
        ]);
        RegisterGift("Rifle magazine", "riflemagazine", [
            new GiftTrait { Trait = "Ammo", Quality = 1.5 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Container" }
        ]);
        RegisterGift("Small IV bag", "ringersolution", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 2.5 }, // 60 thirst
            new GiftTrait { Trait = "Life", Quality = 1.5 } // 0.9l blood
        ]);
        RegisterGift("Riot helmet", "riothelmet", [
            new GiftTrait { Trait = "Armor", Quality = 3 } // 67% protection, 20% insulation, on a critical limb
        ]);
        RegisterGift("Ripped dressing", "rippeddressing", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 0.3 } // 0.28l/m of bleeding
        ]);
        RegisterGift("Rope", "rope", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Rose pod", "rosepod", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Light", Quality = 2.5 }
            // Only the closed pod can be gifted/received, so the recipient can use it when they're ready
        ]);
        RegisterGift("Rye bulb", "ryebulb", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Rye flour", "ryeflour", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Safety glasses", "safetyglasses", [
            new GiftTrait { Trait = "Armor", Quality = 1.5 } // 20% protection, on a critical limb
        ]);
        RegisterGift("IV bag", "saline", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 2.5 }, // 60 thirst
            new GiftTrait { Trait = "Life", Quality = 1.75 } // 1l blood
        ]);
        RegisterGift("Scaffolding pack", "scaffoldingpack", [
            new GiftTrait { Trait = "Tool" } // yeah, I got no idea
        ]);
        RegisterGift("Scarf", "scarf", [
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Armor", Quality = 0.4 } // 6hr decay time, 15% insulation
        ]);
        RegisterGift("Scrap metal", "scrapmetal", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" }
        ]);
        RegisterGift("Scarf", "scarf", [
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Armor", Quality = 0.4 } // 6hr decay time, 15% insulation
        ]);
        RegisterGift("Advanced scuba diving gear", "scubadivinggear", [
            new GiftTrait { Trait = "Speed" }, // manuverability increase underwater
            new GiftTrait { Trait = "Armor", Quality = 2.5 } // 2hr decay time, 44% protection across multiple limbs, including critical ones
        ]);
        RegisterGift("Shade crawler", "shadecrawler", [
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Monster", Quality = 0.5 }
        ]);
        RegisterGift("Pump-action shotgun", "shotgun", [
            new GiftTrait { Trait = "Gun", Quality = 2 }, // loaded with 12-gauge
            new GiftTrait { Trait = "Weapon" }
        ]);
        RegisterGift("Metal shovel", "shovel", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.5 } // 43 damage, normal speed
        ]);
        RegisterGift("Metal sickle", "sickle", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.1 } // 33 damage, normal speed
        ]);
        RegisterGift("Sledgehammer", "sledgehammer", [
            new GiftTrait { Trait = "Tool", Quality = 3 }, // 1000 hammering quality
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.2 } // 86 damage, very slow
        ]);
        RegisterGift("Sleeping bag", "sleepingbag", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Slowness" },
            new GiftTrait { Trait = "Tool" }
        ]);
        RegisterGift("Sleeping pills packet", "sleepingpills", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Slowness" }
        ]);
        RegisterGift("Sling bag", "slingbag", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 1 } // 50min decay time, 4u capacity, 2.5u per item
        ]);
        RegisterGift("Small battery", "smallbattery", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Energy", Quality = 1 } // duh
        ]);
        RegisterGift("Small magazine", "smallmagazine", [
            new GiftTrait { Trait = "Ammo", Quality = 0.5 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Container" }
        ]);
        RegisterGift("Small backpack", "smallpack", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.8 } // 3hr decay time, 3.25u capacity, 2u per item
        ]);
        RegisterGift("Sneakers", "sneakers", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Armor", Quality = 0.4 } // 5hr decay time, 50% protection, 7% insulation
        ]);
        RegisterGift("Snow strider", "snowstrider", [
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Monster", Quality = 1 }
        ]);
        RegisterGift("Soda bottle", "sodabottle", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1.75 } // 9 thirst, 10 times, downside of 5% sickness each time
        ]);
        RegisterGift("Soda can", "sodacan", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.75 } // 9 thirst, 3 times, downside of 5% sickness each time
        ]);
        RegisterGift("Sodium nitroprusside bottle", "sodiumnitroprusside", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 0.5 } // lowers blood pressure
        ]);
        RegisterGift("Soothing crystal shard", "soothingcrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" }
        ]);
        RegisterGift("Soup bowl", "soup", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.5 }, // 10 hunger, 3 times
            new GiftTrait { Trait = "Drink", Quality = 1.5 }, // 9 thirst, 3 times
        ]);
        RegisterGift("Space-drain", "spacedrain", [
            new GiftTrait { Trait = "Tool" } // yeah, I got no idea
        ]);
        RegisterGift("Splint", "splint", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 2 }, // splint. heals broken bones. more durable than carcass variant
        ]);
        RegisterGift("Spray bottle", "spraybottle", [
            new GiftTrait { Trait = "Tool", Quality = 2 } // required to make some antiseptics work properly
        ]);
        RegisterGift("Steak", "steak", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Meat" }, // has meat quality
            new GiftTrait { Trait = "Food", Quality = 3 } // 15 hunger, 3 times
        ]);
        RegisterGift("Sterilized dressing", "sterilizedbandage", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 }, // 0.7l/m of bleeding
            new GiftTrait { Trait = "Cure" } // disinfects
        ]);
        RegisterGift("Stick", "stick", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool", Quality = 0.1 }, // very bad hammering tool
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Fire" } // flammable
        ]);
        RegisterGift("Stuck stone fruit", "stonefruitclosed", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Fruit" },
            new GiftTrait { Trait = "Food", Quality = 0.75 } // 8 hunger, downside of pain and -5 thirst
        ]);
        RegisterGift("Streptokinase syringe", "streptokinase", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 0.4 }, // lowers blood viscosity
        ]);
        RegisterGift("Strider pelt", "striderpelt", [
            new GiftTrait { Trait = "Armor", Quality = 2.25 }, // 29% protection, 40% insulation, on multiple critial limbs
            new GiftTrait { Trait = "Monster", Quality = 1 }
        ]);
        RegisterGift("String", "string", [
            new GiftTrait { Trait = "Material" }
        ]);
        RegisterGift("Syringe", "syringe", [
            new GiftTrait { Trait = "Tool", Quality = 2.5 } // very useful to have on hand
        ]);
        RegisterGift("Tactical boots", "tacticalboots", [
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Armor", Quality = 3 } // 67% protection, 30% insulation
        ]);
        RegisterGift("Tactical gloves", "tacticalgloves", [
            new GiftTrait { Trait = "Armor", Quality = 2 } // 50% protection, 15% insulation
        ]);
        RegisterGift("Terrain scanner", "terrainscanner", [
            new GiftTrait { Trait = "Tool" }
        ]);
        RegisterGift("Titanium machete", "titaniummachete", [
            new GiftTrait { Trait = "Tool", Quality = 2 }, // 100 cutting quality
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 } // 77 damage, slightly faster than normal (only against living enemies)
        ]);
        RegisterGift("Titanium multitool", "titaniummultitool", [
            new GiftTrait { Trait = "Tool", Quality = 4 } // 300 cutting and 300 hammering quality
        ]);
        RegisterGift("Titanium pickaxe", "titaniumpickaxe", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 } // 135 damage, half normal speed
        ]);
        RegisterGift("Titanium rod", "titaniumrod", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" }
        ]);
        RegisterGift("Titanium sheet", "titaniumsheet", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" }
        ]);
        RegisterGift("Titanium slab", "titaniumslab", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" }
        ]);
        RegisterGift("Toolbox", "toolbox", [
            new GiftTrait { Trait = "Container", Quality = 3 } // 8hr decay time, 10u capacity, 6u per item, can only hold in hands
        ]);
        RegisterGift("Torch", "torch", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Light", Quality = 2 } // good light, but liquids put it out
        ]);
        RegisterGift("Torn shirt", "tornshirt", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Armor", Quality = 1 } // 20% protection, 10% insulation
        ]);
        RegisterGift("Tourniquet", "tourniquet", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal" } // stops bleeding temporarially, downside of pain if left too long
        ]);
        RegisterGift("Trash bag", "trashbag", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 1 } // 1hr decay time, 4u capacity, 4u per item
        ]);
        RegisterGift("Trauma rig", "traumarig", [
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 3.5 }, // +24hr decay time, 6u capacity, 5u per item
            new GiftTrait { Trait = "Armor", Quality = 2.5 } // +24hr decay time, 44% protection
        ]);
        RegisterGift("Trowel", "trowel", [
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.25 } // 30 damage, normal speed
        ]);
        RegisterGift("Turbulent crystal shard", "turbulentcrystalshard", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" },
            new GiftTrait { Trait = "Speed" } // movement
        ]);
        RegisterGift("Tweezers", "tweezers", [
            new GiftTrait { Trait = "Tool" },
            // Tweezers help with removing shrapnel. When shrapnel is in a limb, it's maximum muscle health is capped to 50
            // The "Heal" trait is used for restoring skin/muscle health, and this is a tool to remove a cap on muscle health
            // Bit of a stretch, I know
            new GiftTrait { Trait = "Heal" }
        ]);
        RegisterGift("Vasopressin bottle", "vasopressin", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 0.4 }, // increases blood pressure
        ]);
        RegisterGift("Venom gland", "venomgland", [
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Cure" } // material that can be processed into an anti-venom
        ]);
        RegisterGift("Sand stalker", "wallbiter", [
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Monster", Quality = 1.5 }
        ]);
        RegisterGift("Electronic watch", "watch", [
            new GiftTrait { Trait = "Tool" }
        ]);
        RegisterGift("Water bottle", "waterbottle", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 2 } // 9 thirst, 5 times
        ]);
        RegisterGift("Water jug", "waterjug", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 4 } // 9 thirst, 30 times
        ]);
        RegisterGift("Wooden digging tool", "wooddiggingtool", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 0.8 } // 47 damage, half speed, terrible durability
        ]);
        RegisterGift("Wooden pickaxe", "woodpickaxe", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.2 } // 181 damage, slow speed, terrible durability
        ]);
        RegisterGift("Wooden pitchfork", "woodpitchfork", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 0.75 } // 33 damage, half speed, terrible durability
        ]);
        RegisterGift("Wood sandals", "woodsandals", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Armor" }
        ]);
        RegisterGift("Wood scraps", "woodscraps", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Fire" } // flammable
        ]);
        RegisterGift("Wooden shovel", "woodshovel", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1 } // 38 damage, slightly slower, terrible durability
        ]);
        RegisterGift("Wooden sickle", "woodsickle", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 0.9 } // 30 damage, slightly slower, terrible durability
        ]);
        RegisterGift("Wooden trowel", "woodtrowel", [
            new GiftTrait { Trait = "Wood" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.1 } // 27 damage, normal speed, terrible durability
        ]);
        RegisterGift("Wound glue", "wouldglue", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 4 }, // 1.84l/m of bleeding, 40 skin health
            new GiftTrait { Trait = "Cure", Quality = 2 } // full disinfect on used limb, 4 times
        ]);
        RegisterGift("Wrench", "wrench", [
            new GiftTrait { Trait = "Tool", Quality = 2.5 }, // 150 hammering quality
            new GiftTrait { Trait = "Life", Quality = 1.3 } // helps with dislocations
        ]);
        RegisterGift("Xaloris sponge", "xalorissponge", [
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0 }, // 8 hunger, downside of 2% sickness and 5 sepsis
            new GiftTrait { Trait = "Cure", Quality = 0.9 } // full disinfect on used limb, downside of 8 sepsis and 5% pain
        ]);

        /* Here is a list of every "common" trait used inside the Gifting API along with a brief description of it (https://github.com/agilbert1412/Archipelago.Gifting.Net/blob/main/Documentation/Gifting%20API.md#gift-traits)
        Speed	    Increases Speed
        Consumable	Can be consumed
        Food	    Can eat
        Drink	    Can drink
        Heal	    Heals
        Mana	    Restores mana
        Key	        Can open a door, a chest, can unlock something
        Trap	    The gift is intended as negative. Other traits should be interpreted as such.
        Buff	    Buffs the player or some aspect of them
        Life	    Grants an extra life or increases max HP
        Weapon	    Is a weapon, can be used for combat
        Armor	    Is an armor or other wearable item, increases defense
        Tool	    Is a tool, can be used to complete tasks
        Fish	    Is a fish, or related to fishing
        Animal	    Is an animal, pet, companion
        Cure	    Cures status effect, illnesses, ailments
        Seed	    Can be planted, farming-related
        Metal	    Material for crafting, iron, copper, steel, etc
        Bomb	    Can explode, be used as a weapon, to destroy things, etc
        Monster	    Enemy NPC, mob, something to be killed
        Resource	An item that can be used to build or purchase something, or complete a task
        Material	An item to be used for crafting, construction
        Wood	    Wood, Lumber, tree-based products
        Stone	    Stone, Rock, Boulder, rock-based products
        Ore         Precious, raw material for a metal or other processed material
        Grass	    Grass, Leaves, related to nature
        Meat	    Subcategory for food related to meats, animal products
        Vegetable	Subcategory for food that are vegetables, salads, etc
        Fruit	    Subcategory for food that are fruits, foraged, etc
        Egg	        Animal Product, container that can spawn something
        Slowness	Status effect, opposite of speed
        Damage	    Deals damage, reduce HP
        Fire	    Status effect, warm, heat, burn
        Ice	        Status effect, cold, freeze, slow
        Currency	Item that can be used as currency. Consider using the Energylink instead of gifts
        Energy	    Stores or is Energy, electricity. Consider using the EnergyLink instead of gifts
        Light	    Is a light source or light itself, can help against darkness
        */
    }
}