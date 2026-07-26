using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using Archipelago.MultiClient.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Analytics;
using static UnityEngine.ExpressionEvaluator;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem.PlaybackState;

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
        SetupGiftParser(); // set up the parser for receiving new gifts
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
    private static void SetupGiftParser() // set up the parser with a list of every item in the game
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
applejuice
aquapple
armwarmers
autoinjector
autopump
autozoomgoggles
balaclava
banana
bananaplant
bandage
bandolier
barbedwirefence
beartrap
bellyarmor
belt
bigpack
bikehelmet
bioterminal
bleach
blindfold
blobflesh
bloodbag
bloodbaghuman
bloodcoagulant
BloodCrystal
bloodcrystalshard
bloodsac
blueprint
boneweldingtool
bouncecap
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
bunchunk
bundleofwires
burger
cactus
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
climbingropeextended
clottingmush
coffee
coil
combatpen
containercrate
cookies
corpse
craftingbottle
crudecleaver
crystalbig
crystalenemy
crystalmassive
crystalmedium
crystalmini
crystalsmall
DigestionCrystal
digestioncrystalshard
disinfectant
dogfood
drillpod
drillpodbroken
drillrepairkit
dropcapsule
droppings
drybush
dryfoliage
duffelbag
dustmask
dynamite
emergencylight
EmissiveCrystal
emissivecrystalshard
energydrink
epda
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
foodbox
frigiant
frigiantfruit
fungalibungali
funguschunk
geigercounter
geofruit
geotree
geyser
glassshards
glowplant
glowplantfruit
glowshroom
grabberplant
grabbershroom
grapplinghook
gravbag
gunmine
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
icestalactite
icestalagmite
icetea
ilmenitechunk
internalorgans
jetpack
jumppad
keratinbooster
ketchup
kneepads
ladder
landmine
lantern
largebattery
largecarcass
latexgloves
lcdscreen
leadbush
legpouch
lemonade
lifepodchest
lifepodheater
LifePodLight
lifepodpump
lifepodshower
lightbulb
lighter
limbwraps
liquidcentrifuge
liquidpouch
lockpickingkit
LoreNote
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
medcrate
medicalstation
medicalsuture
mediumbattery
medkit
milk
mindwipe
minibarrel
minilaserdrill
morphine
mp3player
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
nondescriptcan
nopopcorn
nutrientbar
oilpipe
opium
overgrowntick
OxygenCrystal
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
pop
popcorn
popfruit
pouch
present
primitivediggingtool
processedcopper
purse
radbarrel
rag
rake
rangefinder
rawcopper
rechargingstation
reinforceddoor
reinforcedrope
ReliefCrystal
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
sandrose
sawblade
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
shuttledoor
shuttleelevator
sickle
sidestabber
sidestabberflip
skullcrusher
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
SoothingCrystal
soothingcrystalshard
soundcannon
soup
spacedrain
spaceheater
defibrack
holidaytree
marbleBackground
mushroomrope
mushroomropeend
sandvinehook
sandvinerope
spentfuel
spikestabber
splint
spraybottle
stalactite
stalagmite
steak
sterilizedbandage
stick
stonefruitclosed
stonefruitopen
stoneplant
streptokinase
striderpelt
string
syringe
tacticalboots
tacticalgloves
terminal
terrainscanner
thornbackelder
thornbackyoung
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
trader1
trader2
trader3
trashbag
traumarig
trowel
TurbulentCrystal
turbulentcrystalshard
turret
tutorialcraftingbutton
tweezers
vasopressin
venomgland
wallbiter
wallflower
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
xaloris
xalorissponge
*/