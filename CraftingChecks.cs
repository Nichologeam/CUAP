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
using System.Linq;

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
    private int lastFrameRecipeCount;
    private static Dictionary<string, Recipe> CheckNameToRecipe = new Dictionary<string, Recipe>()
{   // Same order as items.py, and the interal recipe order in-game
    {"Neural booster Recipe",new Recipe // This is the exact way the game makes recipes (see Recipes.SetUpRecipes in the game's code)
        {
            result = "neuralbooster",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "braingrow"
                },
                new RecipeItem
                {
                    id = "mindwipe"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true // set every recipe to alwaysShow to prevent confusion with newly unlocked recipes being hidden
        }
    },
    {"Scaffolding pack Recipe",new Recipe
        {
            result = "scaffoldingpack",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                }
            },
            alwaysShow = true
        }
    },
    {"Foliage rope (from foliage) Recipe",new Recipe
        {
            result = "rope",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "foliage"
                },
                new RecipeItem
                {
                    id = "foliage"
                }
            },
            alwaysShow = true
        }
    },
    {"Foliage rope (from dry foliage) Recipe",new Recipe
        {
            result = "rope",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dryfoliage"
                },
                new RecipeItem
                {
                    id = "dryfoliage"
                }
            },
            alwaysShow = true
        }
    },
    {"Foliage (from dry foliage) Recipe",new Recipe
        {
            result = "foliage",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dryfoliage"
                },
                new RecipeItem
                {
                    id = "water",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0.05f,
                    minCondition = 0.05f
                }
            },
            alwaysShow = true
        }
    },
    {"Foliage rope (from musharm) Recipe",new Recipe
        {
            result = "rope",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "musharm"
                },
                new RecipeItem
                {
                    id = "musharm"
                }
            },
            alwaysShow = true
        }
    },
    {"Serilized dressing Recipe",new Recipe
        {
            result = "sterilizedbandage",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "antiseptic",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0.2f,
                    minCondition = 0.2f
                }
            },
            scaleByFirstItemCondition = true,
            alwaysShow = true
        }
    },
    {"Dressing (from ripped dressing) Recipe",new Recipe
        {
            result = "bandage",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "rippeddressing"
                },
                new RecipeItem
                {
                    id = "rippeddressing"
                }
            },
            scaleByFirstItemCondition = true,
            alwaysShow = true
        }
    },
    {"Burger Recipe",new Recipe
        {
            result = "burger",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "meat",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "ketchup",
                    destroyItem = false,
                    minCondition = 0.2f,
                    reduceCondition = 0.2f
                },
                new RecipeItem
                {
                    id = "foliage"
                },
                new RecipeItem
                {
                    id = "foliage"
                }
            },
            alwaysShow = true
        }
    },
    {"Ripped dressing (from foliage) Recipe",new Recipe
        {
            result = "rippeddressing",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "foliage"
                },
                new RecipeItem
                {
                    id = "foliage"
                },
                new RecipeItem
                {
                    id = "foliage"
                }
            },
            alwaysShow = true
        }
    },
    {"Ripped dressing (from dried foliage) Recipe",new Recipe
        {
            result = "rippeddressing",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dryfoliage"
                },
                new RecipeItem
                {
                    id = "dryfoliage"
                },
                new RecipeItem
                {
                    id = "dryfoliage"
                }
            },
            alwaysShow = true
        }
    },
    {"Canteen Recipe",new Recipe
        {
            result = "canteen",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "water",
                    minCondition = 0f,
                    isTag = true
                },
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
            resultCondition = 0f
        }
    },
    {"Filtered canteen Recipe",new Recipe
        {
            result = "filtercanteen",
            scaleByFirstItemCondition = true,
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "canteen",
                    minCondition = 0f
                },
                new RecipeItem
                {
                    id = "filterstraw"
                }
            },
            alwaysShow = true,
        }
    },
    {"Large carcass Recipe",new Recipe
        {
            result = "largecarcass",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "carcass"
                },
                new RecipeItem
                {
                    id = "carcass"
                }
            },
            alwaysShow = true
        }
    },
    {"Carcass splint Recipe",new Recipe
        {
            result = "carcasssplint",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "largecarcass"
                },
                new RecipeItem
                {
                    id = "carcass"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true
        }
    },
    {"Metal digging tool Recipe",new Recipe
        {
            result = "primitivediggingtool",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "stick"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true,
            resultCondition = 0.75f
        }
    },
    {"Pickaxe Recipe",new Recipe
        {
            result = "pickaxe",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "ironstick"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true,
            resultCondition = 1f
        }
    },
    {"Reinforced rope Recipe",new Recipe
        {
            result = "reinforcedrope",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    reduceCondition = 0.25f,
                    minCondition = 0.25f,
                    destroyItem = false
                }
            },
            alwaysShow = true
        }
    },
    {"Makeshift L.R.D. Recipe",new Recipe
        {
            result = "makeshiftlrd",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "rope",
                    minCondition = 0.5f,
                    reduceCondition = 0.5f,
                    destroyItem = true
                },
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true,
                    minCondition = 0.5f,
                    reduceCondition = 0.5f,
                    destroyItem = true
                }
            },
            alwaysShow = true
        }
    },
    {"L.R.D. Recipe",new Recipe
        {
            result = "lrd",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "makeshiftlrd"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "opiate",
                    isTag = true,
                    minCondition = 0.5f,
                    reduceCondition = 0.5f,
                    destroyItem = true
                }
            },
            alwaysShow = true
        }
    },
    {"Bruise kit Recipe",new Recipe
        {
            result = "bruisekit",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "opiate",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0.35f
                }
            },
            alwaysShow = true
        }
    },
    {"Splint Recipe",new Recipe
        {
            result = "splint",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
            resultCondition = 0.6f
        }
    },
    {"Foliage bag Recipe",new Recipe
        {
            result = "foliagebag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "foliage"
                },
                new RecipeItem
                {
                    id = "foliage"
                }
            },
            alwaysShow = true
        }
    },
    {"Sling bag Recipe",new Recipe
        {
            result = "slingbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "reinforcedrope"
                },
                new RecipeItem
                {
                    id = "foliagebag"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Relief cream Recipe",new Recipe
        {
            result = "paincream",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "opiate",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "bloodsac"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true,
            resultCondition = 1f
        }
    },
    {"Antiseptic Recipe",new Recipe
        {
            result = "disinfectant",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "water",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "bloodsac"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true,
            resultCondition = 0.5f
        }
    },
    {"Blood bag (from alloy) Recipe",new Recipe
        {
            result = "bloodbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
        }
    },
    {"Blood bag (from empty bag) Recipe",new Recipe
        {
            result = "bloodbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "bloodbagempty"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
        }
    },
    {"Blood bag (from filled bag) Recipe",new Recipe
        {
            result = "bloodbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "bloodbag"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
        }
    },
    {"Blood bag (from yellow flesh) Recip",new Recipe
        {
            result = "bloodbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bloodbaghuman"
                },
                new RecipeItem
                {
                    id = "experimentflesh"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Dressing (from musharm) Recipe",new Recipe
        {
            result = "bandage",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "musharm"
                },
                new RecipeItem
                {
                    id = "musharm"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Opium Recipe",new Recipe
        {
            result = "opium",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "mushpear"
                },
                new RecipeItem
                {
                    id = "mushpear"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                }
            },
            alwaysShow = true,
            resultCondition = 0.5f
        }
    },
    {"Morphine (from opium) Recipe",new Recipe
        {
            result = "morphine",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "opium"
                },
                new RecipeItem
                {
                    id = "opium"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
        }
    },
    {"Fentanyl Recipe",new Recipe
        {
            result = "fentanyl",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "morphine"
                },
                new RecipeItem
                {
                    id = "morphine"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
        }
    },
    {"Lantern Recipe",new Recipe
        {
            result = "lantern",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "emissivecrystalshard"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            scaleByFirstItemCondition = true,
            alwaysShow = true
        }
    },
    {"Antidepressants Recipe",new Recipe
        {
            result = "antidepressants",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "soothingcrystalshard"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Turbulent crystal shard Recipe",new Recipe
        {
            result = "turbulentcrystalshard",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "gravbag"
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
        }
    },
    {"Flammable powder (from stuck fruit) Recipe",new Recipe
        {
            result = "flammablepowder",
            resultCondition = 0.75f,
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "stonefruitopen"
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
        }
    },
    {"Flammable powder (from freed fruit) Recipe",new Recipe
        {
            result = "flammablepowder",
            resultCondition = 0.75f,
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "stonefruitclosed"
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
        }
    },
    {"Dynamite Recipe",new Recipe
        {
            result = "dynamite",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "flammablepowder"
                },
                new RecipeItem
                {
                    id = "flammablepowder"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Magazine base Recipe",new Recipe
        {
            result = "magazinebase",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "metalalloy",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                }
            },
            alwaysShow = true,
        }
    },
    {"Small magazine Recipe",new Recipe
        {
            result = "smallmagazine",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "magazinebase"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Rifle magazine Recipe",new Recipe
        {
            result = "riflemagazine",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "magazinebase"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Box of 12-Gauge Recipe",new Recipe
        {
            result = "boxof12gauge",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "magazinebase"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 0.1f,
                    minCondition = 0.1f
                }
            },
            alwaysShow = true,
        }
    },
    {"Makeshift rifle Recipe",new Recipe
        {
            result = "makeshiftrifle",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "magazinebase"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
        }
    },
    {"Scrap metal Recipe",new Recipe
        {
            result = "scrapmetal",
            resultCondition = 0.15f,
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "casing"
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
        }
    },
    {"9mm round Recipe",new Recipe
        {
            result = "9mmround",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 1f,
                    minCondition = 0.9f
                },
                new RecipeItem
                {
                    id = "flammablepowder",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
            amount = 8
        }
    },
    {"12-Gauge buckshot Recipe",new Recipe
        {
            result = "12gauge",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    reduceCondition = 1f,
                    minCondition = 0.9f
                },
                new RecipeItem
                {
                    id = "flammablepowder",
                    destroyItem = false,
                    reduceCondition = 0.7f,
                    minCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "glowplantfruit",
                    destroyItem = false,
                    reduceCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
            amount = 6
        }
    },
    {"5.56 round Recipe",new Recipe
        {
            result = "556round",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "metalalloy",
                    destroyItem = false,
                    reduceCondition = 0.5f,
                    minCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "flammablepowder",
                    destroyItem = false,
                    reduceCondition = 0.4f,
                    minCondition = 0.4f
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
            amount = 6
        }
    },
    {"Tweezers Recipe",new Recipe
        {
            result = "tweezers",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "tool",
                    isTag = true,
                    destroyItem = false,
                    reduceCondition = 0f
                }
            },
            alwaysShow = true,
            resultCondition = 0.1f
        }
    },
    {"Metal alloy Recipe",new Recipe
        {
            result = "metalalloy",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f,
                    reduceCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f,
                    reduceCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true
        }
    },
    {"Advanced scuba diving gear Recipe",new Recipe
        {
            result = "scubadivinggear",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "oxygencrystalshard"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Makeshift digging tool Recipe",new Recipe
        {
            result = "makeshiftdiggingtool",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Bone welding tool Recipe",new Recipe
        {
            result = "boneweldingtool",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "flammablepowder"
                }
            },
            alwaysShow = true,
        }
    },
    {"Clotting mush Recipe",new Recipe
        {
            result = "clottingmush",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "flammablepowder"
                },
                new RecipeItem
                {
                    id = "scrapmetal",
                    destroyItem = false,
                    minCondition = 0.5f,
                    reduceCondition = 0.5f
                }
            },
            alwaysShow = true,
        }
    },
    {"Procoagulant Recipe",new Recipe
        {
            result = "bloodcoagulant",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "clottingmush"
                },
                new RecipeItem
                {
                    id = "bloodsac"
                },
                new RecipeItem
                {
                    id = "bunchunk"
                },
                new RecipeItem
                {
                    id = "bunchunk"
                }
            },
            alwaysShow = true,
        }
    },
    {"Mini laser drill Recipe",new Recipe
        {
            result = "minilaserdrill",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "flashlight"
                }
            },
            alwaysShow = true,
        }
    },
    {"Makeshift helmet Recipe",new Recipe
        {
            result = "makeshifthelmet",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "largecarcass"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Bicycle helmet Recipe",new Recipe
        {
            result = "bikehelmet",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "makeshifthelmet"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                }
            },
            alwaysShow = true,
        }
    },
    {"Makeshift headlamp Recipe",new Recipe
        {
            result = "makeshiftheadlamp",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "carcass"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                },
                new RecipeItem
                {
                    id = "rope"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Duffel bag Recipe",new Recipe
        {
            result = "duffelbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "brokenbag"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true,
            resultCondition = 0.75f
        }
    },
    {"Tourniquet Recipe",new Recipe
        {
            result = "tourniquet",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "scrapmetal",
                    minCondition = 0.7f
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
            resultCondition = 0.2f
        }
    },
    {"Plastic dressing Recipe",new Recipe
        {
            result = "plasticbandage",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Limb wraps Recipe",new Recipe
        {
            result = "limbwraps",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Headlamp Recipe",new Recipe
        {
            result = "headlamp",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "flashlight"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Bowl of cereal Recipe",new Recipe
        {
            result = "bowlofcereal",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "milk",
                    destroyItem = false,
                    reduceCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "cereal",
                    destroyItem = false,
                    reduceCondition = 0.5f
                },
                new RecipeItem
                {
                    id = "foliage"
                }
            },
            alwaysShow = true,
        }
    },
    {"Naltrexone Recipe",new Recipe
        {
            result = "naltrexone",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "digestioncrystalshard"
                },
                new RecipeItem
                {
                    id = "opiate",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Blood sac Recipe",new Recipe
        {
            result = "bloodsac",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "experimentflesh"
                },
                new RecipeItem
                {
                    id = "experimentflesh"
                }
            },
            alwaysShow = true,
        }
    },
    {"Antiseptic mush Recipe",new Recipe
        {
            result = "antisepticmush",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bulbskin"
                },
                new RecipeItem
                {
                    id = "roselight"
                }
            },
            alwaysShow = true,
        }
    },
    {"Morphine (from relief crystal) Recipe",new Recipe
        {
            result = "morphine",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "reliefcrystalshard"
                },
                new RecipeItem
                {
                    id = "water",
                    isTag = true
                }
            },
            alwaysShow = true,
        }
    },
    {"Grav-Bag Recipe",new Recipe
        {
            result = "gravbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "turbulentcrystalshard"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Terrain scanner Recipe",new Recipe
        {
            result = "terrainscanner",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "glowplantfruit"
                }
            },
            alwaysShow = true,
        }
    },
    {"Auto-auto-pump Recipe",new Recipe
        {
            result = "autoautopump",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "autopump"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                },
                new RecipeItem
                {
                    id = "reinforcedrope"
                }
            },
            alwaysShow = true,
        }
    },
    {"Climbing claws Recipe",new Recipe
        {
            result = "climbingclaws",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "claws"
                },
                new RecipeItem
                {
                    id = "rope"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Blood bag (from crystal) Recipe",new Recipe
        {
            result = "bloodbag",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "bloodcrystalshard"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Ice pack Recipe",new Recipe
        {
            result = "icepack",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "frigiantfruit"
                },
                new RecipeItem
                {
                    id = "frigiantfruit"
                },
                new RecipeItem
                {
                    id = "dressing",
                    isTag = true
                },
                new RecipeItem
                {
                    id = "metalalloy"
                }
            },
            alwaysShow = true,
        }
    },
    {"Flashlight Recipe",new Recipe
        {
            result = "flashlight",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "lightbulb"
                },
                new RecipeItem
                {
                    id = "metalalloy"
                },
                new RecipeItem
                {
                    id = "circuitboard"
                }
            },
            alwaysShow = true,
            scaleByFirstItemCondition = true
        }
    },
    {"Foliage (from fungus chunk) Recipe",new Recipe
        {
            result = "foliage",
            items = new List<RecipeItem>
            {
                new RecipeItem
                {
                    id = "funguschunk"
                },
                new RecipeItem
                {
                    id = "funguschunk"
                }
            },
            alwaysShow = true,
        }
    }
};

    private void OnEnable()
    {
        Client = APClientClass.Client;
        RecievedRecipes = APClientClass.RecipeUnlockDictionary;
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("RandomizeRecipes", out var recipesoption)) // check if recipe randomization is enabled.
        {
            if (!Convert.ToBoolean(recipesoption))
            {
                Startup.Logger.LogWarning("Recipe Randomization is disabled, destroying script.");
                Destroy(this);
            }
        }
        Recipes.recipes.Clear(); // Unlearn every recipe upon first connecting. We will recieve them with items later.
    }
    private void Update()
    {
        if (RecievedRecipes.Count > lastFrameRecipeCount)
        {
            foreach (string gotrecipe in RecievedRecipes)
            {
                Recipes.recipes.Clear();
                CheckNameToRecipe.TryGetValue(gotrecipe, out Recipe recipeToLearn);
                Recipes.recipes.Add(recipeToLearn); // Add the recipe to the list
                Debug.Log("Learned Recipe " + gotrecipe + " from Archipelago!");
                foreach (Recipe learnedRecipe in Recipes.recipes)
                {
                    foreach (RecipeItem item in learnedRecipe.items)
                    {
                        if (item.isTag)
                        {
                            foreach (KeyValuePair<string, ItemInfo> item2 in Item.GlobalItems.Where((KeyValuePair<string, ItemInfo> x) => x.Value.HasTag(item.id)))
                            {
                                item2.Value.craftable = true;
                            }
                        }
                        else
                        {
                            Item.GetItem(item.id).craftable = true;
                        }
                    }
                }
            }
        }
        lastFrameRecipeCount = RecievedRecipes.Count;
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