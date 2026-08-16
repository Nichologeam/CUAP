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
    }
    // here comes a funny function name
    private static void RegisterEveryReasonablySpawnableObjectInsideTheGiftingLibraryBKCloseTraitParserToMakeSureThatAnyPlayerPlayingTheIndieGameCasualtiesUnknownCreatedByIndieDeveloperOrsoniksCanSendOrRecieveGiftsToAnyOtherVideoGameThatSupportsBothTheArchipelagoMultiGameRandomizerAndTheGiftingAPI()
    {
        // Gifting API specifications say that a Quality and Duration of 1.0 is considered "average" for that game
        // "Duration" doesn't really mean anything inside of Casualties, so all Casualties items will have a Duration of 1.0
        giftParser.RegisterAvailableGift("12gauge", [ // 12-Gauge buckshot
            // "Ammo" itself is not a "common" trait exposed by the API, but I feel as if it's generic enough
            // The "quality" of Ammo determines its damage...
            // 12-Gauge Shotguns do ~50-65 damage per pellet, Rifles do ~140-150 damage, Pistols do ~75-90 damage
            // 12-Gauge is in the middle of those ranges (since it has multiple pellets), so it will be of "average" quality (don't tell the multiplayer mod pvp community)
            new GiftTrait { Trait = "Ammo", Quality = 1.0 },
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Damage", Quality = 3.0 } // high damage (compared to other sources) so high quality
        ]);
        giftParser.RegisterAvailableGift("556round", [ // 5.56 round
            new GiftTrait { Trait = "Ammo", Quality = 1.5 },
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Damage", Quality = 3.5 }
        ]);
        giftParser.RegisterAvailableGift("9mmround", [ // 9mm round
            new GiftTrait { Trait = "Ammo", Quality = 0.5 },
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Damage", Quality = 2.5 }
        ]);
        giftParser.RegisterAvailableGift("adhesivebandage", [ // Adhesive bandage
            new GiftTrait { Trait = "Consumeable" },
            // I will be using "Heal" to determine how good an item is at stopping bleeding or restoring skin/muscle health
            new GiftTrait { Trait = "Heal", Quality = 0.5 } // lowers bleeding by 0.63l/m (if staggered properly)
        ]);
        giftParser.RegisterAvailableGift("aed", [ // Automated external defibrillator
            new GiftTrait { Trait = "Consumeable" },
            // I will use "Life" for items that can heal any critical conditions. In this case, that's fibrillation
            new GiftTrait { Trait = "Life", Quality = 2 } // higher quality than the manual defib
        ]);
        giftParser.RegisterAvailableGift("alcohol", [ // Bottle of alcohol
            new GiftTrait { Trait = "Consumeable" },
            // I will be using "Cure" to determine how good an item is at disinfecting limbs or increasing body immunity
            new GiftTrait { Trait = "Cure", Quality = 2 } // full disinfect, four times
        ]);
        giftParser.RegisterAvailableGift("alginate", [ // Alginate dressing
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Heal", Quality = 4 }, // 1l/m+ of bleeding, and restores skin health from 0->100 in a single go
            new GiftTrait { Trait = "Cure", Quality = 1 } // can disinfect, but it's not the best at it
        ]);
        giftParser.RegisterAvailableGift("amiodarone", [ // Amiodarone bottle
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Life", Quality = 0.5 } // fixes fibrillation. works slowly and requires a syringe to use. not a great choice
        ]);
        giftParser.RegisterAvailableGift("analgesicgauze", [ // Medical gauze
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Heal", Quality = 1.5 } // 0.75l/m of bleeding + opioids for pain relief
        ]);
        giftParser.RegisterAvailableGift("animalflesh", [ // Animal flesh
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Food", Quality = 0 }, // 7 hunger, downside of 4% sickness. arguably worst food item in the game
            new GiftTrait { Trait = "Meat", Quality = 1 } // can be used as meat in cooking based recipes
        ]);
        giftParser.RegisterAvailableGift("antibiotics", [ // Antibiotics bottle
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Cure", Quality = 3 }, // 5 uses, each boosting body immunity
            // Things that have a lasting effect will have the "Buff" trait
            new GiftTrait { Trait = "Buff" }
        ]);
        giftParser.RegisterAvailableGift("antidepressants", [ // Antidepressants
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Buff" } // 7 uses, each gradually increasing mood
        ]);
        giftParser.RegisterAvailableGift("antirad", [ // Anti-rad bottle
            // I don't really have a good way to seperate this from other anti items, but I'm open to suggestions
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Buff" } // 5 uses, each gradually decreasing radiation sickness
        ]);
        giftParser.RegisterAvailableGift("antisepticmush", [ // Antiseptic mush
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Cure", Quality = 4 } // single use, but fully disinfects all adjacent limbs
        ]);
        giftParser.RegisterAvailableGift("antiserum", [ // Antiserum syringe
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Cure", Quality = 2.5 } // increases immunity and decreases sepsis
        ]);
        giftParser.RegisterAvailableGift("applejuice", [ // Apple juice carton
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Drink", Quality = 1 }, // 9 thirst, 8 uses
            new GiftTrait { Trait = "Fruit" } // apple
        ]);
        giftParser.RegisterAvailableGift("aquapple", [ // Aquapple
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Drink", Quality = 0.25 } // 7 thirst, 1 use, decays rapidly
        ]);
        giftParser.RegisterAvailableGift("armwarmers", [ // Arm warmers
            // I will be using "Armor" to determine how good a wearable item is
            new GiftTrait { Trait = "Armor", Quality = 1 } // 6 hour decay time, 25% protection, 10% insulation
        ]);
        giftParser.RegisterAvailableGift("autoinjector", [ // Auto-injector
            // Since the API was built off of Stardew Valley, items used in crafting or construction are called materials
            new GiftTrait { Trait = "Material" },
            new GiftTrait { Trait = "Life", Quality = 1 } // Auto-injector is used in medical recipes
        ]);
        giftParser.RegisterAvailableGift("autopump", [ // Auto-pump
            new GiftTrait { Trait = "Armor", Quality = 0 }, // doesn't actually give any defense (trade off for its live-saving ability)
            new GiftTrait { Trait = "Life", Quality = 3 } // Auto-pump keeps your blood pressure livable as long as it has power
        ]);
        giftParser.RegisterAvailableGift("autozoomgoggles", [ // Auto-zoom googles
            new GiftTrait { Trait = "Armor", Quality = 0 }, // no defense
            new GiftTrait { Trait = "Buff", Quality = 1.5 } // lasting effect (2 hours, as long as its powered)
        ]);
        giftParser.RegisterAvailableGift("balaclava", [ // Balaclava
            new GiftTrait { Trait = "Armor", Quality = 2 } // no decay time, 25% protection, 20% insulation
        ]);
        giftParser.RegisterAvailableGift("banana", [ // Banana
            new GiftTrait { Trait = "Consumeable" },
            new GiftTrait { Trait = "Food", Quality = 0.5 }, // 9 hunger, 2 uses
            new GiftTrait { Trait = "Drink", Quality = 0.3 }, // 4 thirst, 2 uses
            new GiftTrait { Trait = "Fruit" } // banana
        ]);
        giftParser.RegisterAvailableGift("bananaplant", [ // Banana-plant
            new GiftTrait { Trait = "Trap" },
            new GiftTrait { Trait = "Damage" },
            new GiftTrait { Trait = "Fruit" } // i debated also adding the Vegetable trait, but nah
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

/* Below is a list of every spawnable object in the game
bandage
bandolier
bellyarmor
belt
bigpack
bikehelmet
bleach
blindfold
blobflesh
bloodbag
bloodbaghuman
bloodcoagulant
bloodcrystalshard
bloodsac
boneweldingtool
bowlofcereal
box
boxof12gauge
braingrow
bread
brokenbag
browncap
brownshroom
bruisekit
bucketofchicken
bucketofnochicken
bulbskin
bundleofwires
burger
cactusflesh
cake
campfire
candybar
canteen
canvas
carapace
carcass
carcasssplint
casing
cavetick
ceftriaxone
ceilingrye
cereal
charcoal
chestdrain
chips
chocolatebar
chocolatemilk
circuitboard
claws
climbingclaws
climbingrope
clottingmush
coffee
combatpen
cookies
crudecleaver
digestioncrystalshard
disinfectant
dogfood
drillrepairkit
droppings
dryfoliage
duffelbag
dustmask
dynamite
emergencylight
emissivecrystalshard
energydrink
experimentflesh
exposedcore
fannypack
fentanyl
filterstraw
firestarter
flammablepowder
flashlight
fleshchunk
flexiglass
flimsyknife
foliage
foliagebag
foliagemeal
frigiantfruit
funguschunk
geigercounter
geofruit
glowplantfruit
grapplinghook
gravbag
handcrank
hardcandy
headlamp
heavydrill
helluce
heroin
holidayhat
hoodie
hydreed
icepack
icetea
ilmenitechunk
internalorgans
jetpack
keratinbooster
ketchup
kneepads
lantern
largebattery
largecarcass
latexgloves
lcdscreen
legpouch
lemonade
lightbulb
lighter
limbwraps
liquidcentrifuge
liquidpouch
lockpickingkit
lrd
machete
magazinebase
makeshiftdiggingtool
makeshiftheadlamp
makeshifthelmet
makeshiftlrd
makeshiftrifle
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
mushroomdropper
mushroomplatform
mushroomtree
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
reinforceddoor
reinforcedrope
reliefcrystalshard
rifle
riflemagazine
ringersolution
riothelmet
rippeddressing
rope
roselight
rosepod
ryebulb
ryeflour
safetyglasses
saline
scaffoldingpack
scarf
scrapcube
scrapeater
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
xalorissponge
*/