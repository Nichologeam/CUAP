using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using Archipelago.MultiClient.Net;
using System.Collections.Generic;

namespace CUAP;

public class Gifting
{
    private static ArchipelagoSession Client;
    public static GiftingService giftService;
    private static ICloseTraitParser<string> giftParser = new BKTreeCloseTraitParser<string>();
    public static bool shouldGiftBoxExist = false;
    private static Dictionary<string, string> ItemNameToPrefabName = new Dictionary<string, string>() // List of an object's name inside EN.json to the prefab name
    {   // Sorted by prefab name (dict value)
        {"12-Gauge buckshot", "12gauge"},
        {"5.56 round", "556round"},
        {"9mm round", "9mmround"},
        {"Adhesive bandage", "adhesivebandage"},
        {"Automated external defibrillator", "aed"},
        {"Bottle of alcohol", "alcohol"},
        {"Alginate dressing", "alginate"},
        {"Amiodarone bottle", "amiodarone"},
        {"Medical gauze", "analgesicgauze"},
        {"Animal flesh", "animalflesh"},
        {"Antibiotics bottle", "antibiotics"},
        {"Antidepressants", "antidepressants"},
        {"Anti-rad bottle", "antirad"},
        {"Antiseptic mush", "antisepticmush"},
        {"Antiserum syringe", "antiserum"},
        {"Apple juice carton", "applejuice"},
        {"Aquapple", "aquapple"},
        {"Arm warmers", "armwarmers"},
        {"Auto-injector", "autoinjector"},
        {"Auto-pump", "autopump"},
        {"Auto-zoom googles", "autozoomgoggles"},
        {"Balaclava", "balaclava"},
        {"Banana", "banana"},
        {"Banana-plant", "bananaplant"},
        {"Dressing", "bandage"},
        {"Bandolier", "bandolier"},
        {"Belly armor", "bellyarmor"},
        {"Belt", "belt"},
        {"Backpack", "bigpack"},
        {"Bicycle helmet", "bikehelmet"},
        {"Bleach bottle", "bleach"},
        {"Blindfold", "blindfold"},
        {"Blobflesh", "blobflesh"},
        {"Blood bag", "bloodbag"},
        {"Red blood bag", "bloodbaghuman"},
        {"Procoagulant syringe", "bloodcoagulant"},
        {"Blood crystal shard", "bloodcrystalshard"},
        {"Blood sac", "bloodsac"},
        {"Bone welding tool", "boneweldingtool"},
        {"Bowl of cereal", "bowlofcereal"},
        {"Cardboard box", "box"},
        {"Box of 12-Gauge", "boxof12gauge"},
        {"Braingrow bottle", "braingrow"},
        {"Bread", "bread"},
        {"Ripped bag", "brokenbag"},
        {"Browncap", "browncap"},
        {"Bruise kit", "bruisekit"},
        {"Bucket of chicken", "bucketofchicken"},
        {"Bucket of no chicken", "bucketofnochicken"},
        {"Popskin", "bulbskin"},
        {"Bundle of wires", "bundleofwires"},
        {"Burger", "burger"},
        {"Cactus flesh", "cactusflesh"},
        {"Cake", "cake"},
        {"Campfire", "campfire"},
        {"Candy bar", "candybar"},
        {"Canteen", "canteen"},
        {"Canvas", "canvas"},
        {"Carapace", "carapace"},
        {"Stiff carcass", "carcass"},
        {"Carcass splint", "carcasssplint"},
        {"Cave tick", "cavetick"},
        {"Ceftriaxone syringe", "ceftriaxone"},
        {"Cereal", "cereal"},
        {"Charcoal", "charcoal"},
        {"Chest drain", "chestdrain"},
        {"Chips", "chips"},
        {"Chocolate bar", "chocolatebar"},
        {"Chocolate milk bottle", "chocolatemilk"},
        {"Circuit board", "circuitboard"},
        {"Claw pack", "claws"},
        {"Climbing claws", "climbingclaws"},
        {"Climbing rope", "climbingrope"},
        {"Clotting mush", "clottingmush"},
        {"Coffee cup", "coffee"},
        {"Combat pen", "combatpen"},
        {"Box of cookies", "cookies"},
        {"Crude cleaver", "crudecleaver"},
        {"Digestion crystal shard", "digestioncrystalshard"},
        {"Antiseptic", "disinfectant"},
        {"Dog food", "dogfood"},
        {"Drill repair kit", "drillrepairkit"},
        {"Dried foliage", "dryfoliage"},
        {"Duffel bag", "duffelbag"},
        {"Dust mask", "dustmask"},
        {"Dynamite", "dynamite"},
        {"Emergency light", "emergencylight"},
        {"Emissive crystal shard", "emissivecrystalshard"},
        {"Energy drink can", "energydrink"},
        {"Yellow flesh", "experimentflesh"},
        {"Fanny pack", "fannypack"},
        {"Fentanyl syringe", "fentanyl"},
        {"Filter straw", "filterstraw"},
        {"Firestarter", "firestarter"},
        {"Flammable powder", "flammablepowder"},
        {"Flashlight", "flashlight"},
        {"Big flesh chunk", "fleshchunk"},
        {"Flexiglass", "flexiglass"},
        {"Flimsy knife", "flimsyknife"},
        {"Foliage", "foliage"},
        {"Foliage bag", "foliagebag"},
        {"Foliage meal", "foliagemeal"},
        {"Frigiant fruit", "frigiantfruit"},
        {"Fungus chunk", "funguschunk"},
        {"Geiger counter", "geigercounter"},
        {"Geofruit", "geofruit"},
        {"Glowplant fruit", "glowplantfruit"},
        {"Grappling hook", "grapplinghook"},
        {"Grav-bag", "gravbag"},
        {"Hand crank charger", "handcrank"},
        {"Hard candy", "candy"},
        {"Headlamp", "headlamp"},
        {"Heavy drill", "heavydrill"},
        {"Helluce", "helluce"},
        {"Heroin syringe", "heroin"},
        {"Holiday hat", "holidayhat"},
        {"Hoodie", "hoodie"},
        {"Ice pack", "icepack"},
        {"Iced tea bottle", "icetea"},
        {"Ilmenite chunk", "ilmenitechunk"},
        {"Internal organs", "internalorgans"},
        {"Jetpack", "jetpack"},
        {"Keratin booster", "keratinbooster"},
        {"Ketchup bottle", "ketchup"},
        {"Kneepads", "kneepads"},
        {"Lantern", "lantern"},
        {"Large battery", "largebattery"},
        {"Large carcass", "largecarcass"},
        {"Latex gloves", "latexgloves"},
        {"LCD screen", "lcdscreen"},
        {"Leg pouch", "legpouch"},
        {"Lemonade jar", "lemonade"},
        {"Lightbulb", "lightbulb"},
        {"Lighter", "lighter"},
        {"Limb wraps", "limbwraps"},
        {"Liquid centrifuge", "liquidcentrifuge"},
        {"Liquid pouch", "liquidpouch"},
        {"Lock-picking kit", "lockpickingkit"},
        {"L.R.D.", "lrd"},
        {"Machete", "machete"},
        {"Magazine base", "magazinebase"},
        {"Makeshift digging tool", "makeshiftdiggingtool"},
        {"Makeshift headlamp", "makeshiftheadlamp"},
        {"Makeshift helmet", "makeshifthelmet"},
        {"Makeshift L.R.D.", "makeshiftlrd"},
        {"Makseshift rifle", "makeshiftrifle"},
        /* Below objects are untested. Gifting support will not be finished until all objects have been tested.
        makeshiftwrench
        manualdefibrillator
        materialpouch
        medicalsuture
        mediumbattery
        medkit
        milk
        mindwipe
        minilaserdrill
        morphine
        musharm
        mushpear
        mushtail
        nails
        naloxone
        naltrexone
        neuralbooster
        nopopcorn
        nutrientbar
        opium
        overgrowntick
        oxygencrystalshard
        paincream
        painkillers
        pancake
        paprikash
        pemmican
        pickaxe
        pistol
        pitchfork
        pizzaslice
        plasmacutter
        plasticbag
        plasticbandage
        plasticchunk
        plushie
        popcorn
        popfruit
        pouch
        primitivediggingtool
        processedcopper
        purse
        rag
        rake
        rangefinder
        rawcopper
        reinforcedrope
        reliefcrystalshard
        rifle
        riflemagazine
        ringersolution
        riothelmet
        rippeddressing
        rope
        rosepod
        ryebulb
        ryeflour
        safetyglasses
        saline
        scaffoldingpack
        scarf
        scrapcube
        scrapmetal
        scrappanel
        scraptube
        scubadivinggear
        shadecrawler
        shotgun
        shovel
        sickle
        sledgehammer
        sleepingbag
        sleepingpills
        slingbag
        smallbattery
        smallmagazine
        smallpack
        sneakers
        snowstrider
        sodabottle
        sodacan
        sodiumnitroprusside
        soothingcrystalshard
        soup
        spacedrain
        splint
        spraybottle
        steak
        sterilizedbandage
        stick
        stonefruitclosed
        streptokinase
        striderpelt
        string
        syringe
        tacticalboots
        tacticalgloves
        terrainscanner
        titaniummachete
        titaniummultitool
        titaniumpickaxe
        titaniumrod
        titaniumsheet
        titaniumslab
        toolbox
        torch
        tornshirt
        tourniquet
        trashbag
        traumarig
        trowel
        turbulentcrystalshard
        tweezers
        vasopressin
        venomgland
        wallbiter
        watch
        waterbottle
        waterjug
        woodcube
        wooddiggingtool
        woodpanel
        woodpickaxe
        woodpitchfork
        woodsandals
        woodscraps
        woodshovel
        woodsickle
        woodtrowel
        woundglue
        wrench
        xalorissponge*/
    };
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
        if (!APCanvas.InGame) // this should hopefully never trigger, since the box should be closed when not in-game anyway
        {
            giftService.RefundGift(gift);
            giftService.CloseGiftBox();
            Startup.Logger.LogError("Received a gift while not in-game! The Giftbox wasn't closed properly somewhere!");
            APCanvas.EnqueueArchipelagoNotification(APLocale.Get("giftOutOfGame", APLocale.APLanguageType.Errors), 3);
            return;
        }
        List<string> matches = giftParser.FindClosestAvailableGift(gift.Traits); // get the closest available items that have the traits this gift does
        if (matches == null || matches.Count == 0)
        {
            giftService.RefundGift(gift); // no matches could be found, refund this gift
            string sender = Client.Players.GetPlayerName(gift.SenderSlot);
            Startup.Logger.LogWarning($"No matches found for received gift {gift.ItemName} from {sender}. Refunding..."); // for the bepinex log
            CommandPatch.LogToConsole($"<color=#FF0000>CUAP: No matches found for received gift {gift.ItemName} from {sender}. Refunding...</color>"); // for the ingame console
            return;
        }
        UnityEngine.Debug.Log("found match: " + matches[0]);
        // todo: spawn the matched item (get the parsing working first)
        // use ItemNameToPrefabName
    }
    // here comes a funny function name
    private static void RegisterEveryReasonablySpawnableObjectInsideTheGiftingLibraryBKCloseTraitParserToMakeSureThatAnyPlayerPlayingTheIndieGameCasualtiesUnknownCreatedByIndieDeveloperOrsoniksCanSendOrRecieveGiftsToAnyOtherVideoGameThatSupportsBothTheArchipelagoMultiGameRandomizerAndTheGiftingAPI()
    {
        // TODO: go through all the things with the "Damage" trait and scale by raw DPS instead of just feel to make it less subjective.
        
        // Gifting API specifications say that a Quality and Duration of 1.0 is considered "average" for that game
        // "Duration" doesn't really mean anything inside of Casualties, so all Casualties items will have a Duration of 1.0
        giftParser.RegisterAvailableGift("12-Gauge buckshot", [ // 12gauge
            // "Ammo" itself is not a "common" trait exposed by the API, but I feel as if it's generic enough
            // The "quality" of Ammo determines its damage...
            // 12-Gauge Shotguns do ~50-65 damage per pellet, Rifles do ~140-150 damage, Pistols do ~75-90 damage
            // 12-Gauge is in the middle of those ranges (since it has multiple pellets), so it will be of "average" quality (don't tell the multiplayer pvp community)
            new GiftTrait { Trait = "Ammo", Quality = 1.0 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 3.0 } // high damage (compared to other sources) so high quality
        ]);
        giftParser.RegisterAvailableGift("5.56 round", [ // 556round
            new GiftTrait { Trait = "Ammo", Quality = 1.5 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 3.5 }
        ]);
        giftParser.RegisterAvailableGift("9mm round", [ // 9mmround
            new GiftTrait { Trait = "Ammo", Quality = 0.5 },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 }
        ]);
        giftParser.RegisterAvailableGift("Adhesive bandage", [ // adhesivebandage
            new GiftTrait { Trait = "Consumable" },
            // I will be using "Heal" to determine how good an item is at stopping bleeding or restoring skin/muscle health
            new GiftTrait { Trait = "Heal", Quality = 0.5 } // lowers bleeding by 0.63l/m (if staggered properly)
        ]);
        giftParser.RegisterAvailableGift("Automated external defibrillator", [ // aed
            new GiftTrait { Trait = "Consumable" },
            // I will use "Life" for items that can heal any critical conditions. In this case, that's fibrillation
            new GiftTrait { Trait = "Life", Quality = 2 } // higher quality than the manual defib
        ]);
        giftParser.RegisterAvailableGift("Bottle of alcohol", [ // alcohol
            new GiftTrait { Trait = "Consumable" },
            // I will be using "Cure" to determine how good an item is at disinfecting limbs or increasing body immunity
            new GiftTrait { Trait = "Cure", Quality = 1.5 } // full disinfect, four times
        ]);
        giftParser.RegisterAvailableGift("Alginate dressing", [ // alginate
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 4 }, // 1l/m+ of bleeding, and restores skin health from 0->100 in a single go
            new GiftTrait { Trait = "Cure", Quality = 0.5 } // can disinfect, but it's not the best at it
        ]);
        giftParser.RegisterAvailableGift("Amiodarone bottle", [ // amiodarone
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 0.5 } // fixes fibrillation. works slowly and requires a syringe to use. not a great choice
        ]);
        giftParser.RegisterAvailableGift("Medical gauze", [ // analgesicgauze
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1.5 } // 0.75l/m of bleeding + opioids for pain relief
        ]);
        giftParser.RegisterAvailableGift("Animal flesh", [ // animalflesh
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 7 hunger, downside of 4% sickness
            new GiftTrait { Trait = "Meat", Quality = 1 } // can be used as meat in cooking based recipes
        ]);
        giftParser.RegisterAvailableGift("Antibiotics bottle", [ // antibiotics
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 3 }, // 5 uses, each boosting body immunity
            // Things that have a lasting effect will have the "Buff" trait
            new GiftTrait { Trait = "Buff" }
        ]);
        giftParser.RegisterAvailableGift("Antidepressants", [ // antidepressants
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" } // 7 uses, each gradually increasing mood
        ]);
        giftParser.RegisterAvailableGift("Anti-rad bottle", [ // antirad
            // I don't really have a good way to seperate this from other anti items, but I'm open to suggestions
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" } // 5 uses, each gradually decreasing radiation sickness
        ]);
        giftParser.RegisterAvailableGift("Antiseptic mush", [ // antisepticmush
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 3 } // single use, but fully disinfects all adjacent limbs
        ]);
        giftParser.RegisterAvailableGift("Antiserum syringe", [ // antiserum
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 2.5 } // increases immunity and decreases sepsis
        ]);
        giftParser.RegisterAvailableGift("Apple juice carton", [ // applejuice
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1 }, // 9 thirst, 8 uses
            new GiftTrait { Trait = "Fruit" } // apple
        ]);
        giftParser.RegisterAvailableGift("Aquapple", [ // aquapple
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.25 } // 7 thirst, 1 use, decays rapidly
        ]);
        giftParser.RegisterAvailableGift("Arm warmers", [ // armwarmers
            // I will be using "Armor" to determine how good a wearable item is
            new GiftTrait { Trait = "Armor", Quality = 1 } // 6 hour decay time, 25% protection, 10% insulation
        ]);
        giftParser.RegisterAvailableGift("Auto-injector", [ // autoinjector
            // Since the API was built off of Stardew Valley, items used in crafting or construction are called materials
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Life", Quality = 1 } // Auto-injector is used in medical recipes
        ]);
        giftParser.RegisterAvailableGift("Auto-pump", [ // autopump
            new GiftTrait { Trait = "Armor", Quality = 0 }, // doesn't actually give any defense (trade off for its live-saving ability)
            new GiftTrait { Trait = "Life", Quality = 3 } // Auto-pump keeps your blood pressure livable as long as it has power
        ]);
        giftParser.RegisterAvailableGift("Auto-zoom googles", [ // autozoomgoggles
            new GiftTrait { Trait = "Armor", Quality = 0 }, // no defense
            new GiftTrait { Trait = "Buff", Quality = 1.5 } // lasting effect (2 hours, as long as its powered)
        ]);
        giftParser.RegisterAvailableGift("Balaclava", [ // balaclava
            new GiftTrait { Trait = "Armor", Quality = 1 } // no decay time, 25% protection, 20% insulation
        ]);
        giftParser.RegisterAvailableGift("Banana", [ // banana
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 9 hunger, 2 uses
            new GiftTrait { Trait = "Drink", Quality = 0.3 }, // 4 thirst, 2 uses
            new GiftTrait { Trait = "Fruit" } // banana
        ]);
        giftParser.RegisterAvailableGift("Banana-plant", [ // bananaplant
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Damage" },
            new GiftTrait { Trait = "Fruit" } // i debated also adding the Vegetable trait, but nah
        ]);
        giftParser.RegisterAvailableGift("Dressing", [ // bandage
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 } // 0.7l/m of bleeding
        ]);
        giftParser.RegisterAvailableGift("Bandolier", [ // bandolier
            new GiftTrait { Trait = "Armor", Quality = 0 }, // no defense
            // Container trait is self explanatory. This item can hold other items
            new GiftTrait { Trait = "Container", Quality = 0.25 } // 5hr decay, 3u capacity, 0.05u per item (made for bullets only)
        ]);
        giftParser.RegisterAvailableGift("Belly armor", [ // bellyarmor
            new GiftTrait { Trait = "Armor", Quality = 1.5 }, // no decay, 50% defense, 8% insulation
        ]);
        giftParser.RegisterAvailableGift("Belt", [ // belt
            new GiftTrait { Trait = "Container", Quality = 0.75 } // >24hr decay time, 4u capacity, 4u per item
        ]);
        giftParser.RegisterAvailableGift("Backpack", [ // bigpack
            new GiftTrait { Trait = "Armor", Quality = 0.1 }, // no defense, 7% insulation
            new GiftTrait { Trait = "Container", Quality = 1 } // 1hr decay time, 7.5u capacity, 5u per item
        ]);
        giftParser.RegisterAvailableGift("Bicycle helmet", [ // bikehelmet
            new GiftTrait { Trait = "Armor", Quality = 1.75 }, // no decay, 50% defense, 8% insulation (higher quality because it protects the head)
        ]);
        giftParser.RegisterAvailableGift("Bleach bottle", [ // bleach
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 3 } // full disinfect, 15 times
        ]);
        giftParser.RegisterAvailableGift("Blindfold", [ // blindfold
            new GiftTrait { Trait = "Armor", Quality = 0.5 }, // no decay, 10% defense, 10% insulation
            new GiftTrait { Trait = "Trap" } // downside of blinding you
        ]);
        giftParser.RegisterAvailableGift("Blobflesh", [ // blobflesh
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.1 }, // 6 hunger, downside of 8% sickness
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Blood bag", [ // bloodbag
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 },
            new GiftTrait { Trait = "Life", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Red blood bag", [ // bloodbaghuman
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1 },
            new GiftTrait { Trait = "Life", Quality = 1 },
            new GiftTrait { Trait = "Trap" } // eh? not sure how to seperate them otherwise
        ]);
        giftParser.RegisterAvailableGift("Procoagulant syringe", [ // bloodcoagulant
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2 } // slows/stops bleeding bodywide
        ]);
        giftParser.RegisterAvailableGift("Blood crystal shard", [ // bloodcrystalshard
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2 }, // adds blood volume
            new GiftTrait { Trait = "Buff" } // gradually restores over 20 minutes
        ]);
        giftParser.RegisterAvailableGift("Blood sac", [ // bloodsac
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Heal", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Bone welding tool", [ // boneweldingtool
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Life", Quality = 1 } // fixes broken bones, downside of causing bleeding
        ]);
        giftParser.RegisterAvailableGift("Bowl of cereal", [ // bowlofcereal
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.75 }, // 6 hunger, 5 times
            new GiftTrait { Trait = "Drink", Quality = 0.5 } // 8 thirst, 5 times
        ]);
        giftParser.RegisterAvailableGift("Cardboard box", [ // box
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 30min decay, 4u capacity, 2u per item
        ]);
        giftParser.RegisterAvailableGift("Box of 12-Gauge", [ // boxof12gauge
            new GiftTrait { Trait = "Ammo", Quality = 3.5 }, // 16 shells, so higher quality
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Damage", Quality = 3.0 }
        ]);
        giftParser.RegisterAvailableGift("Braingrow bottle", [ // braingrow
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 3 } // regenerates brain health
        ]);
        giftParser.RegisterAvailableGift("Bread", [ // bread
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1 }, // 10 hunger, 3 times
            new GiftTrait { Trait = "Drink", Quality = 0.1 } // 2 thirst, 3 times (for some reason?)
        ]);
        giftParser.RegisterAvailableGift("Backpack", [ // bigpack
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 1hr decay time, 6.25u capacity, 2.25u per item
        ]);
        giftParser.RegisterAvailableGift("Browncap", [ // browncap
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 10 hunger, downside of a random effect
            // Items with the Vegetable trait have the Produce trait ingame
            new GiftTrait { Trait = "Vegetable" }
        ]);
        giftParser.RegisterAvailableGift("Bruise kit", [ // bruisekit
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2.5 } // 20 skin health, 10 muscle health
        ]);
        giftParser.RegisterAvailableGift("Bucket of chicken", [ // bucketofchicken
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.25 }, // 10 hunger, 5 times, downside of weight+hands only
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Bucket of no chicken", [ // bucketofnochicken
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 20min decay time, 3u capacity, 3u per item
        ]);
        giftParser.RegisterAvailableGift("Popskin", [ // bulbskin
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Thirst", Quality = 0.25 }, // 4 thirst, 3 times
            new GiftTrait { Trait = "Heal", Quality = 0.25 } // 0.12l/m bleeding, no skin or muscle health
        ]);
        giftParser.RegisterAvailableGift("Bundle of wires", [ // bundleofwires
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" } // copper
        ]);
        giftParser.RegisterAvailableGift("Burger", [ // burger
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.5 }, // 15 hunger, 3 times
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Cactus flesh", [ // cactusflesh
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 4 hunger
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 4 thirst
            new GiftTrait { Trait = "Vegetable" }
        ]);
        giftParser.RegisterAvailableGift("Cake", [ // cake
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1 }, // 8 hunger, 10 times
        ]);
        giftParser.RegisterAvailableGift("Campfire", [ // campfire
            new GiftTrait { Trait = "Fire" } // literally a fire
        ]);
        giftParser.RegisterAvailableGift("Candy bar", [ // candybar
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 6 hunger but lasts a while
        ]);
        giftParser.RegisterAvailableGift("Canteen", [ // canteen
            new GiftTrait { Trait = "Drink" }, // Container that holds a drink
            new GiftTrait { Trait = "Container", Quality = 1 } // 300ml capacity
        ]);
        giftParser.RegisterAvailableGift("Canvas", [ // canvas
            new GiftTrait { Trait = "Material" }
        ]);
        giftParser.RegisterAvailableGift("Carapace", [ // carapace
            new GiftTrait { Trait = "Armor", Quality = 2 } // 50% protection, 15% insulation, on a critical limb
        ]);
        giftParser.RegisterAvailableGift("Stiff carcass", [ // carcass
            new GiftTrait { Trait = "Material" },
            // The "Monster" trait is used when this item is dropped by an enemy
            // The trait's Quality determines how easy that enemy is to kill
            // This trait is also used when the enemies themselves are gifted, though it will also have the "Trap" trait
            new GiftTrait { Trait = "Monster", Quality = 0.75 } // dropped from an early-game enemy
        ]);
        giftParser.RegisterAvailableGift("Carcass splint", [ // carcasssplint
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 1.5 }, // splint. heals broken bones
        ]);
        giftParser.RegisterAvailableGift("Cave tick", [ // cavetick
            new GiftTrait { Trait = "Trap" },
            // In this instance, the "Monster" trait is used to determine how difficult this enemy is to kill
            new GiftTrait { Trait = "Monster", Quality = 0.25 }, // very easy to kill
        ]);
        giftParser.RegisterAvailableGift("Ceftriaxone syringe", [ // ceftriaxone
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 2 }, // bodywide immunity boost, downside of pain
        ]);
        giftParser.RegisterAvailableGift("Cereal", [ // cereal
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 6 hunger, 5 times, downside of -3 thirst each time
        ]);
        giftParser.RegisterAvailableGift("Charcoal", [ // charcoal
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Fire", Quality = 0.5 } // can make a fire
        ]);
        giftParser.RegisterAvailableGift("Chest drain", [ // chestdrain
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Life", Quality = 0.5 } // solves hemothorax
        ]);
        giftParser.RegisterAvailableGift("Chips", [ // chips
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 3 hunger, 10 times, downside of 3% sickness each time
        ]);
        giftParser.RegisterAvailableGift("Chocolate bar", [ // chocolatebar
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0 } // 7 hunger, 3 times, downside of 20% sickness each time. arguably worst food item in the game
        ]);
        giftParser.RegisterAvailableGift("Chocolate milk bottle", [ // chocolatemilk
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.5 }, // 8 thirst, 10 times, downside of 12% sickness each time
            new GiftTrait { Trait = "Food", Quality = 0 } // 2 hunger, 10 times (for some reason?)
            // Fun fact: Despite the item description saying that this increases happiness, it doesn't!
        ]);
        giftParser.RegisterAvailableGift("Circuit board", [ // circiutboard
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Energy", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Claw pack", [ // claws
            new GiftTrait { Trait = "Tool", Quality = 1 }, // cutting tool
            // All weapons will have the tool trait, but not all tools will have the weapon trait
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.25 } // barely better than normal damage, but really fast
        ]);
        giftParser.RegisterAvailableGift("Climbing claws", [ // climbingclaws
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Buff" } // movement buff
        ]);
        giftParser.RegisterAvailableGift("Climbing rope", [ // climbingrope
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Consumable" } // 2 uses
        ]);
        giftParser.RegisterAvailableGift("Clotting mush", [ // clottingmush
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }, // used in crafting
            new GiftTrait { Trait = "Heal", Quality = 2 } // 1.18l/m of bleeding
        ]);
        giftParser.RegisterAvailableGift("Coffee cup", [ // coffee
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.8 }, // 12 thirst, 3 times, downside of 15% sickness each time
            new GiftTrait { Trait = "Buff" } // energizing
        ]);
        giftParser.RegisterAvailableGift("Combat pen", [ // combatpen
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Life", Quality = 4 }, // epinephrine, stimulant, and oxyline all at once at safe doses
            new GiftTrait { Trait = "Buff" } // 60ml of Medical-grade stimulant
        ]);
        giftParser.RegisterAvailableGift("Box of cookies", [ // cookies
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 3 hunger, 10 times, downside of 3% sickness each time
        ]);
        giftParser.RegisterAvailableGift("Crude cleaver", [ // crudecleaver
            new GiftTrait { Trait = "Tool", Quality = 0.5 } // cutting tool
        ]);
        giftParser.RegisterAvailableGift("Digestion crystal shard", [ // digestioncrystalshard
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 0.5 }, // increases immunity by decreasing sickness slowly
            new GiftTrait { Trait = "Buff" }
        ]);
        giftParser.RegisterAvailableGift("Antiseptic", [ // disinfectant
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Cure", Quality = 1.5 }, // 20 uses
        ]);
        giftParser.RegisterAvailableGift("Dog food", [ // dogfood
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.8 }, // 6 hunger, 5 times
        ]);
        giftParser.RegisterAvailableGift("Drill repair kit", [ // drillrepairkit
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Resource" }
        ]);
        giftParser.RegisterAvailableGift("Dried foliage", [ // dryfoliage
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.2 }, // 6 hunger, 5% sickness
        ]);
        giftParser.RegisterAvailableGift("Duffel bag", [ // duffelbag
            new GiftTrait { Trait = "Material", Quality = 0.5 }, // source of ripped dressing (2)
            new GiftTrait { Trait = "Container", Quality = 1 } // 2hr decay time, 5u capacity, 2.8u per item
        ]);
        giftParser.RegisterAvailableGift("Dust mask", [ // dustmask
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Armor", Quality = 0.3 } // 10% protection, 8% insulation, on head
        ]);
        giftParser.RegisterAvailableGift("Dynamite", [ // dynamite
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Consumable" }
        ]);
        giftParser.RegisterAvailableGift("Emergency light", [ // emergencylight
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 0.75 }
        ]);
        giftParser.RegisterAvailableGift("Emissive crystal shard", [ // emissivecrystalshard
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Light", Quality = 2 },
            new GiftTrait { Trait = "Material" } // used in crafting the lantern
        ]);
        giftParser.RegisterAvailableGift("Energy drink can", [ // energydrink
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 0.70 }, // 10 thirst, 3 times, downside of 20% sickness each time
            new GiftTrait { Trait = "Buff" } // energizing
        ]);
        giftParser.RegisterAvailableGift("Yellow flesh", [ // experimentflesh
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.4 }, // 12 hunger, downside of 16% sickness
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Fanny pack", [ // fannypack
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.25 } // 2hr decay time, 2u capacity, 0.8u per item
        ]);
        giftParser.RegisterAvailableGift("Fentanyl syringe", [ // fentanyl
            new GiftTrait { Trait = "Consumable" },
            // The concept of opiods being classified as Mana sounds absurd on paper and was originally a joke
            // but it made more and more sense the more I thought about it, so now it's staying
            new GiftTrait { Trait = "Mana", Quality = 3 } // highly potent
        ]);
        giftParser.RegisterAvailableGift("Filter straw", [ // filterstraw
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Drink" }
        ]);
        giftParser.RegisterAvailableGift("Firestarter", [ // firestarter
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Fire", Quality = 0.75 } // can create fire
        ]);
        giftParser.RegisterAvailableGift("Flammable powder", [ // flammablepowder
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Fire" }
        ]);
        giftParser.RegisterAvailableGift("Flashlight", [ // flashlight
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 1.25 }
        ]);
        giftParser.RegisterAvailableGift("Big flesh chunk", [ // fleshchunk
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1.5 }, // 15 hunger, 5 times
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Flexiglass", [ // flexiglass
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        giftParser.RegisterAvailableGift("Flimsy knife", [ // flimsyknife
            new GiftTrait { Trait = "Tool", Quality = 1 }, // cutting tool
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 0.5 } // worse than normal damage, terrible durability, but a little faster
        ]);
        giftParser.RegisterAvailableGift("Foliage", [ // foliage
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 5 hunger, downside of 20% sickness
            new GiftTrait { Trait = "Material" }
        ]);
        giftParser.RegisterAvailableGift("Foliage bag", [ // foliagebag
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.5 } // 25min decay time, 4u capacity, 2u per item
        ]);
        giftParser.RegisterAvailableGift("Foliage meal", [ // foliagemeal
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 1 }, // 18 hunger, 2 times, downside of 3% sickness each
            new GiftTrait { Trait = "Drink", Quality = 0.25 } // 2 thirst, 2 times
        ]);
        giftParser.RegisterAvailableGift("Frigiant fruit", [ // frigiantfruit
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.25 }, // 2 hunger, 2 times
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 3 thirst, 2 times
            new GiftTrait { Trait = "Ice", Quality = 1.5 } // cools you down by 3.5 degrees instantly
        ]);
        giftParser.RegisterAvailableGift("Fungus chunk", [ // funguschunk
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 10 hunger, downside of -20 thirst
        ]);
        giftParser.RegisterAvailableGift("Geiger counter", [ // geigercounter
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Life" } // helps prevent radiation
        ]);
        giftParser.RegisterAvailableGift("Geofruit", [ // geofruit
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.4 }, // 5 hunger, 2 times
            new GiftTrait { Trait = "Drink", Quality = 0.25 }, // 3 thirst, 2 times
            new GiftTrait { Trait = "Fruit" }, // its in the name
            new GiftTrait { Trait = "Vegetable" } // produce trait
        ]);
        giftParser.RegisterAvailableGift("Glowplant fruit", [ // glowplantfruit
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Light", Quality = 0.25 },
            new GiftTrait { Trait = "Cure", Quality = 0.5 } // can disinfect
        ]);
        giftParser.RegisterAvailableGift("Grappling hook", [ // grapplinghook
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Buff" }, // movement buff
            new GiftTrait { Trait = "Speed" } // eh?
        ]);
        giftParser.RegisterAvailableGift("Grav-pack", [ // gravpack
            new GiftTrait { Trait = "Container", Quality = 2 } // 5u capacity, 4u per item, zero weight when powered
        ]);
        giftParser.RegisterAvailableGift("Hand crank charger", [ // handcrank
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Energy" } // recharges batteries
        ]);
        giftParser.RegisterAvailableGift("Candy", [ // hardcandy
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.1 }, // 1 hunger, 5 times, downside of 3% sickness each
        ]);
        giftParser.RegisterAvailableGift("Headlamp", [ // headlamp
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 1.3 } // same as normal light. higher quality since it doesn't take a slot
        ]);
        giftParser.RegisterAvailableGift("Heavy drill", [ // heavydrill
            new GiftTrait { Trait = "Tool", Quality = 1 }, // hammering tool
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 } // 60 damage and really fast, but little range
        ]);
        giftParser.RegisterAvailableGift("Helluce", [ // helluce
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Fire", Quality = 0.25 } // increases body temperature by 2 degrees instantly
        ]);
        giftParser.RegisterAvailableGift("Heroin syringe", [ // heroin
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Mana", Quality = 1 } // about mid-range
        ]);
        giftParser.RegisterAvailableGift("Holiday hat", [ // holidayhat
            new GiftTrait { Trait = "Armor", Quality = 0.6 } // 3hr decay, 10% defense, 20% insulation
        ]);
        giftParser.RegisterAvailableGift("Hoodie", [ // hoodie
            new GiftTrait { Trait = "Armor", Quality = 1.3 }, // 8hr decay, 20% defense to most of upper body, 60% insulation
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 8hr decay time, 2u capacity, 2u per item
        ]);
        giftParser.RegisterAvailableGift("Ice pack", [ // icepack
            new GiftTrait { Trait = "Ice", Quality = 1 }, // cools you down by 1 degree instantly, and chills a given limb
            new GiftTrait { Trait = "Heal", Quality = 1.5 } // increases muscle healing speed. reusable
        ]);
        giftParser.RegisterAvailableGift("Iced tea bottle", [ // icetea
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1 } // 10 thirst, 5 times, downside of 3% sickness each time
        ]);
        giftParser.RegisterAvailableGift("Ilmenite chunk", [ // ilmenitechunk
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Metal" },
            new GiftTrait { Trait = "Ore" } // I don't think I need to explain this one
        ]);
        giftParser.RegisterAvailableGift("Internal organs", [ // internalorgans
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.2 }, // 15 hunger, 3 times, downside of 32% sickness and ~-22 mood each time
            new GiftTrait { Trait = "Meat", Quality = 1 }
        ]);
        giftParser.RegisterAvailableGift("Jetpack", [ // jetpack
            new GiftTrait { Trait = "Armor", Quality = 0.2 }, // no decay, 0% defense, 5% insulation
            new GiftTrait { Trait = "Container", Quality = 1 }, // no decay time, 2.5u capacity, 2u per item
            new GiftTrait { Trait = "Buff" } // movement buff
        ]);
        giftParser.RegisterAvailableGift("Keratin booster", [ // keratinbooster
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Buff" }
        ]);
        giftParser.RegisterAvailableGift("Ketchup bottle", [ // ketchup
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 8 hunger, 3 times, downside of 5% sickness each time
            new GiftTrait { Trait = "Drink", Quality = 0.3 }, // 6 thirst, 3 times, downside of 5% sickness each time
            new GiftTrait { Trait = "Fruit" }
        ]);
        giftParser.RegisterAvailableGift("Kneepads", [ // kneepads
            new GiftTrait { Trait = "Armor", Quality = 1.2 } // no decay, 50% defense, 5% insulation. Lowered quality because it's on a non-vital limb
        ]);
        giftParser.RegisterAvailableGift("Lantern", [ // lantern
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Light", Quality = 2 }
        ]);
        giftParser.RegisterAvailableGift("Large battery", [ // largebattery
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Energy", Quality = 2 } // duh
        ]);
        giftParser.RegisterAvailableGift("Large carcass", [ // largecarcass
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Monster", Quality = 1.25 }
        ]);
        giftParser.RegisterAvailableGift("Latex gloves", [ // latexgloves
            new GiftTrait { Trait = "Armor", Quality = 0.1 }, // 5hr decay, no defense, 8% insulation
        ]);
        giftParser.RegisterAvailableGift("LCD screen", [ // lcdscreen
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Material" }
        ]);
        giftParser.RegisterAvailableGift("Leg pouch", [ // legpouch
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Container", Quality = 0.75 } // 3hr decay time, 3.8u capacity, 3u per item
        ]);
        giftParser.RegisterAvailableGift("Lemonade jar", [ // lemonadejar
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Drink", Quality = 1.5 } // 9 thirst, 8 times
        ]);
        giftParser.RegisterAvailableGift("Lightbulb", [ // lightbulb
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Light", Quality = 1.25 }
        ]);
        giftParser.RegisterAvailableGift("Lighter", [ // lighter
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Fire", Quality = 1.5 } // can create fire (and is really good at it)
        ]);
        giftParser.RegisterAvailableGift("Limb wraps", [ // limbwraps
            new GiftTrait { Trait = "Material", Quality = 0.25 }, // source of ripped dressing (1)
            new GiftTrait { Trait = "Armor", Quality = 1.5 } // 5hr decay, 20% defense, 7% insulation. Higher quality because it covers multiple limbs
        ]);
        giftParser.RegisterAvailableGift("Liquid centrifuge", [ // liquidcentrifuge
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Drink" } // a tool that is liquid related (kind of a stretch, I know)
        ]);
        giftParser.RegisterAvailableGift("Liquid pouch", [ // liquidpouch
            new GiftTrait { Trait = "Drink" }, // Container that holds a drink
            new GiftTrait { Trait = "Container", Quality = 1.5 } // 1000ml capacity AND a wearble instead of taking a slot (canteen)
        ]);
        giftParser.RegisterAvailableGift("Lock-picking kit", [ // lockpickingkit
            new GiftTrait { Trait = "Tool" },
            new GiftTrait { Trait = "Key" } // how else would I use this trait?
        ]);
        giftParser.RegisterAvailableGift("L.R.D.", [ // lrd
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 2 }, // do i need to explain these numbers? L.R.D. my goat
            new GiftTrait { Trait = "Life", Quality = 2 },
            new GiftTrait { Trait = "Cure", Quality = 2 }
        ]);
        giftParser.RegisterAvailableGift("Machete", [ // machete
            new GiftTrait { Trait = "Tool", Quality = 2 }, // cutting AND hammering tool. can also amputate
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.5 } // 36 damage, slightly slower than normal, high durability
        ]);
        giftParser.RegisterAvailableGift("Magazine base", [ // magazinebase
            new GiftTrait { Trait = "Ammo" },
            new GiftTrait { Trait = "Material" }
        ]);
        giftParser.RegisterAvailableGift("Makeshift digging tool", [ // makeshiftdiggingtool
            new GiftTrait { Trait = "Tool", Quality = 1.2 }, // cutting AND hammering tool. cannot amputate
            new GiftTrait { Trait = "Weapon" },
            new GiftTrait { Trait = "Damage", Quality = 1.75 } // 62 damage, slightly slower than normal, low durability
        ]);
        giftParser.RegisterAvailableGift("Makeshift headlamp", [ // makeshiftheadlamp
            new GiftTrait { Trait = "Light", Quality = 0.75 } // low light, but it doesn't take up a slot
        ]);
        giftParser.RegisterAvailableGift("Makeshift helmet", [ // makeshifthelmet
            new GiftTrait { Trait = "Armor", Quality = 0.75 } // 2hr decay, 43% defense, 4% insulation, covers a critical limb
        ]);
        giftParser.RegisterAvailableGift("Makeshift L.R.D.", [ // makeshiftlrd
            new GiftTrait { Trait = "Consumable" },
            new GiftTrait { Trait = "Heal", Quality = 1.75 }, // slightly lower than the normal L.R.D. because makeshift has 2 uses while the normal one has 3
            new GiftTrait { Trait = "Life", Quality = 1.75 },
            new GiftTrait { Trait = "Cure", Quality = 1.75 }
        ]);
        giftParser.RegisterAvailableGift("Makeshift rifle", [ // makeshiftrifle
            // Need I explain this trait?
            new GiftTrait { Trait = "Gun", Quality = 0.5 }, // lower durability, jams easier. loaded with 9mm.
            new GiftTrait { Trait = "Weapon" }
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