using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
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
        {78,-966812869},
        {79,-966812868},
        {80,-966812867},
        {81,-966812866},
        {82,-966812865},
        {83,-966812864},
        {84,-966812863},
        {85,-966812862},
        {86,-966812861},
        {87,-966812860},
        {88,-966812859},
        {89,-966812858},
        {90,-966812857},
        {91,-966812856},
        {92,-966812855},
        {93,-966812854},
        {94,-966812853},
        {95,-966812852},
        {96,-966812851},
        {97,-966812850},
        {98,-966812849},
        {99,-966812848},
        {100,-966812847},
        {101,-966812846},
        {102,-966812845},
        {103,-966812844},
    };
    private int lastFrameRecipeCount;
    private static Dictionary<string, Recipe> CheckNameToRecipe = new Dictionary<string, Recipe>()
{   // Same order as items.py, and the interal recipe order in-game
    {"Foliage rope Recipe",new Recipe
        {
            INT = 1,
            result = new RecipeResult
            {
                id = "rope"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"String Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "string",
                amount = 3
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting"),
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Canvas Recipe",new Recipe
        {
            INT = 2,
            result = new RecipeResult
            {
                id = "canvas"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Wood scraps Recipe",new Recipe
        {
            INT = 3,
            result = new RecipeResult
            {
                id = "woodscraps"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("foliage"),
                    minimumCondition = 0f
                }
            }
        }
    },
    {"Ripped dressing Recipe",new Recipe
        {
            INT = 0,
            result = new RecipeResult
            {
                id = "rippeddressing"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("rippable"),
                    destroyItem = false,
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Sterilized dressing Recipe",new Recipe
        {
            INT = 1,
            result = new RecipeResult
            {
                id = "sterilizedbandage"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("dressing")
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("disinfectant", 10f)
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Bio-chem fluid Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "biochem",
                isLiquid = true,
                resultCondition = 10f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "glowplantfruit",
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Opium Recipe",new Recipe
        {
            INT = 7,
            result = new RecipeResult
            {
                id = "opium",
                isLiquid = true,
                resultCondition = 50f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "mushpear",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "mushpear",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    specificId = "biochem",
                    minimumCondition = 10f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("water", 50f)
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Morphine Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "morphine",
                isLiquid = true,
                resultCondition = 50f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(75f)
                {
                    specificId = "opium",
                    isLiquid = true
                },
                new RecipeItem(10f)
                {
                    isLiquid = true,
                    specificId = "biochem"
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("water", 15f)
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Fentanyl Recipe",new Recipe
        {
            INT = 11,
            result = new RecipeResult
            {
                id = "fentanyl",
                isLiquid = true,
                resultCondition = 10f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(50f)
                {
                    specificId = "morphine",
                    isLiquid = true
                },
                new RecipeItem(25f)
                {
                    isLiquid = true,
                    specificId = "biochem"
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("water", 25f)
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Painkillers Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "painkillers",
                isLiquid = true,
                resultCondition = 200f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "reliefcrystalshard"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(20f)
                {
                    isLiquid = true,
                    specificId = "biochem"
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("water", 50f)
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Neural booster Recipe",new Recipe
        {
            INT = 18,
            result = new RecipeResult
            {
                id = "neuralbooster"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(50f)
                {
                    isLiquid = true,
                    specificId = "biochem"
                },
                new RecipeItem(80f)
                {
                    isLiquid = true,
                    specificId = "braingrow"
                },
                new RecipeItem(30f)
                {
                    isLiquid = true,
                    specificId = "mindwipe"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Canteen Recipe",new Recipe
        {
            INT = 6,
            result = new RecipeResult
            {
                id = "canteen"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "plasticchunk",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "plasticchunk",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    specificId = "biochem",
                    minimumCondition = 10f
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Foliage bag Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "foliagebag"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("canvas"),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("canvas"),
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Sling bag Recipe",new Recipe
        {
            INT = 7,
            result = new RecipeResult
            {
                id = "slingbag"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "foliagebag",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "string",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Scrap cube Recipe",new Recipe
        {
            INT = 6,
            result = new RecipeResult
            {
                id = "scrapcube"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "scrapmetal",
                    minimumCondition = 0.9f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "biochem",
                    isLiquid = true,
                    minimumCondition = 10f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Scrap panel Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "scrappanel",
                amount = 5
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "scrapcube",
                    minimumCondition = 0.9f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting"),
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Scrap tube Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "scraptube",
                amount = 2
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "scrapcube",
                    minimumCondition = 0.9f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting"),
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Nails Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "nails",
                amount = 2
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cube", 1f)
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting", 2.5f),
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Wood cube Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "woodcube"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "woodscraps"
                },
                new RecipeItem(0f)
                {
                    specificId = "woodscraps"
                },
                new RecipeItem(0f)
                {
                    specificId = "woodscraps"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "biochem",
                    isLiquid = true,
                    minimumCondition = 10f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Wood panel Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "woodpanel",
                amount = 5
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "woodcube",
                    minimumCondition = 0.9f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting"),
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Stick Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "stick",
                amount = 2
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "woodcube"
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting"),
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Flimsy knife Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "flimsyknife"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "scrapmetal",
                    minimumCondition = 0.9f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "rope",
                    minimumCondition = 0f
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cube", 0.5f)
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Processed copper Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "processedcopper"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "rawcopper",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    specificId = "biochem",
                    minimumCondition = 40f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Bundle of wires Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "bundleofwires",
                amount = 6
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "processedcopper",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "plasticchunk",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "plasticchunk",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "plasticchunk",
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Titanium slab Recipe",new Recipe
        {
            INT = 15,
            result = new RecipeResult
            {
                id = "titaniumslab"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "ilmenitechunk",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    specificId = "biochem",
                    minimumCondition = 50f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("water", 100f)
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("hammering", 4f),
                    destroyItem = false,
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting", 4f),
                    destroyItem = false,
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Titanium sheet Recipe",new Recipe
        {
            INT = 15,
            result = new RecipeResult
            {
                id = "titaniumsheet",
                amount = 5
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "titaniumslab",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    specificId = "biochem",
                    minimumCondition = 10f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting", 4f),
                    destroyItem = false,
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Titanium rod Recipe",new Recipe
        {
            INT = 15,
            result = new RecipeResult
            {
                id = "titaniumrod",
                amount = 2
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "titaniumslab",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    isLiquid = true,
                    specificId = "biochem",
                    minimumCondition = 10f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("cutting", 4f),
                    destroyItem = false,
                    minimumCondition = 0f
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Alien blood Recipe",new Recipe
        {
            INT = 2,
            result = new RecipeResult
            {
                id = "alienblood",
                isLiquid = true,
                resultCondition = 50f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    minimumCondition = 0f,
                    specificId = "bloodsac"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Blood Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "blood",
                isLiquid = true,
                resultCondition = 100f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(99.99f)
                {
                    specificId = "alienblood",
                    isLiquid = true
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 40f),
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Makeshift wrench Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "makeshiftwrench",
                resultCondition = 1f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = "rod",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = "rod",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "biochem",
                    isLiquid = true,
                    minimumCondition = 10f
                },
                new RecipeItem(0.9f)
                {
                    quality = "hammering",
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Wrench Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "wrench",
                resultCondition = 1f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "makeshiftwrench",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("rod", 4f),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    specificId = "biochem",
                    isLiquid = true,
                    minimumCondition = 10f
                },
                new RecipeItem(0.9f)
                {
                    quality = "hammering",
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Crude cleaver Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "crudecleaver",
                resultCondition = 1f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("rod", 0.5f),
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = "panel",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = "panel",
                    minimumCondition = 0f
                },
                new RecipeItem(0.9f)
                {
                    quality = "nails",
                    minimumCondition = 0f,
                    destroyItem = false
                },
                new RecipeItem(0.9f)
                {
                    quality = "hammering",
                    minimumCondition = 0f,
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Flammable powder Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "flammablepowder"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "stonefruitopen"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                },
                new RecipeItem(5f)
                {
                    isLiquid = true,
                    specificId = "biochem"
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Casing Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "casing",
                amount = 5
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"9mm round Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "9mmround",
                amount = 5
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("hammering", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"5.56 round Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "556round",
                amount = 4
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("hammering", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"12-Gauge buckshot Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "12gauge",
                amount = 3
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "casing"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("hammering", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Magazine base Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "magazinebase"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Small magazine Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "smallmagazine"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "magazinebase"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Rifle magazine Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "riflemagazine"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "magazinebase"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Box of 12-Guage Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "boxof12gauge"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Dynamite Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "dynamite"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Large carcass Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "largecarcass"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "carcass"
                },
                new RecipeItem(0f)
                {
                    specificId = "carcass"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Circuit board Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "circuitboard"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Small battery Recipe",new Recipe
        {
            INT = 11,
            result = new RecipeResult
            {
                id = "smallbattery",
                amount = 2
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "processedcopper"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Medium battery Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "mediumbattery"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "smallbattery"
                },
                new RecipeItem(0f)
                {
                    specificId = "smallbattery"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Large battery Recipe",new Recipe
        {
            INT = 13,
            result = new RecipeResult
            {
                id = "largebattery"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "mediumbattery"
                },
                new RecipeItem(0f)
                {
                    specificId = "processedcopper"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Flashlight Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "flashlight"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "lightbulb"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Headlamp Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "headlamp"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "flashlight"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"LCD screen Recipe",new Recipe
        {
            INT = 11,
            result = new RecipeResult
            {
                id = "lcdscreen"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Flexiglass Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "flexiglass"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Lightbulb Recipe",new Recipe
        {
            INT = 11,
            result = new RecipeResult
            {
                id = "lightbulb"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    quality = "cube"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Limb wraps Recipe",new Recipe
        {
            INT = 6,
            result = new RecipeResult
            {
                id = "limbwraps"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = "dressing"
                },
                new RecipeItem(0.9f)
                {
                    quality = "dressing"
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Makeshift lamp Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "makeshiftheadlamp"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "carcass"
                },
                new RecipeItem(0f)
                {
                    specificId = "glowplantfruit"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Bicycle helmet Recipe",new Recipe
        {
            INT = 13,
            result = new RecipeResult
            {
                id = "bikehelmet"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "makeshifthelmet"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Makeshift helmet Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "makeshifthelmet"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "largecarcass"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Makeshift digging tool Recipe",new Recipe
        {
            INT = 7,
            result = new RecipeResult
            {
                id = "makeshiftdiggingtool"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "flimsyknife"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("rod", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("rod", 0.5f)
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Makeshift rifle Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "makeshiftrifle"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "magazinebase"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 1f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 1f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("rod", 1f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("rod", 1f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cube", 0.5f)
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Mini laser drill Recipe",new Recipe
        {
            INT = 16,
            result = new RecipeResult
            {
                id = "minilaserdrill"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 4f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 4f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 4f)
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    specificId = "flashlight"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Dressing Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "bandage"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    specificId = "rippeddressing"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "rippeddressing"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Lantern Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "lantern"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "emissivecrystalshard"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Terrain scanner Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "terrainscanner"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    specificId = "lcdscreen"
                },
                new RecipeItem(0f)
                {
                    quality = "nails"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Advanced scuba diving gear Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "scubadivinggear"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "oxygencrystalshard"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Pickaxe Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "pickaxe"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "makeshiftdiggingtool"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "nails"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Scaffolding pack Recipe",new Recipe
        {
            INT = 14,
            result = new RecipeResult
            {
                id = "scaffoldingpack"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cube", 4f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cube", 4f)
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Backpack Recipe",new Recipe
        {
            INT = 13,
            result = new RecipeResult
            {
                id = "bigpack"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = "canvas"
                },
                new RecipeItem(0f)
                {
                    quality = "canvas"
                },
                new RecipeItem(0f)
                {
                    quality = "canvas"
                },
                new RecipeItem(0f)
                {
                    quality = "canvas"
                },
                new RecipeItem(0f)
                {
                    quality = "canvas"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Bowl of cereal Recipe",new Recipe
        {
            INT = 2,
            result = new RecipeResult
            {
                id = "bowlofcereal",
                dontDrainResultLiquid = true,
                amount = 3
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "cereal"
                },
                new RecipeItem(500f)
                {
                    specificId = "milk",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Fat Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "fat",
                isLiquid = true,
                resultCondition = 100f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.2f)
                {
                    quality = "meat"
                },
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cutting", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Soap Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "soap",
                isLiquid = true,
                resultCondition = 100f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(50f)
                {
                    specificId = "fat",
                    isLiquid = true
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 40f),
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Clotting mush Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "clottingmush"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("blood", 50f),
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Naltrexone Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "naltrexone",
                isLiquid = true,
                resultCondition = 200f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "digestioncrystalshard"
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    isLiquid = true,
                    quality = new CraftingQuality("opiate", 50f)
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Antidepressants Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "antidepressants",
                isLiquid = true,
                resultCondition = 200f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "soothingcrystalshard"
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Auto-injector Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "autoinjector"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Materials
        }
    },
    {"Auto-auto-pump Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "autoautopump"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "autopump"
                },
                new RecipeItem(0f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    specificId = "autoinjector"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Antiseptic mush Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "antisepticmush"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "bulbskin"
                },
                new RecipeItem(0f)
                {
                    specificId = "roselight"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Plastic dressing Recipe",new Recipe
        {
            INT = 14,
            result = new RecipeResult
            {
                id = "plasticbandage"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.5f)
                {
                    quality = "dressing"
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 4f)
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Tourniquet Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "tourniquet",
                resultCondition = 0.5f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    specificId = "string"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Bone welding tool Recipe",new Recipe
        {
            INT = 14,
            result = new RecipeResult
            {
                id = "boneweldingtool"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(10f)
                {
                    specificId = "lrdserum",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Tweezers Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "tweezers",
                resultCondition = 0.1f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Blood bag Recipe",new Recipe
        {
            INT = 12,
            result = new RecipeResult
            {
                id = "bloodbag"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "flexiglass"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "autoinjector"
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Antiseptic bottle Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "disinfectant",
                isLiquid = true,
                resultCondition = 100f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(25f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(50f)
                {
                    specificId = "soap",
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("blood", 25f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 50f),
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Relief cream Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "reliefcream",
                isLiquid = true,
                resultCondition = 100f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "flammablepowder"
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 50f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("blood", 25f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("opiate", 50f),
                    isLiquid = true
                },
                new RecipeItem(25f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Splint Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "splint",
                resultCondition = 0.8f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 0.5f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("rod")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("rod")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("nails"),
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("hammering"),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Bruise kit Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "bruisekit"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.5f)
                {
                    quality = "dressing"
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("opiate", 25f),
                    isLiquid = true
                },
                new RecipeItem(20f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Carcass splint Recipe",new Recipe
        {
            INT = 6,
            result = new RecipeResult
            {
                id = "carcasssplint"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "largecarcass"
                },
                new RecipeItem(0f)
                {
                    specificId = "carcass"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Makeshift L.R.D. Recipe",new Recipe
        {
            INT = 10,
            result = new RecipeResult
            {
                id = "makeshiftlrd"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "autoinjector"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"L.R.D. Recipe",new Recipe
        {
            INT = 15,
            result = new RecipeResult
            {
                id = "lrd"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "makeshiftlrd"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 4f)
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("panel", 4f)
                },
                new RecipeItem(25f)
                {
                    specificId = "biochem",
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 100f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"L.R.D. Serum Recipe",new Recipe
        {
            INT = 11,
            result = new RecipeResult
            {
                id = "lrdserum",
                isLiquid = true,
                resultCondition = 25f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("disinfectant", 25f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 50f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("opiate", 25f),
                    isLiquid = true
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("blood", 25f),
                    isLiquid = true
                },
                new RecipeItem(25f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Produce juice Recipe",new Recipe
        {
            INT = 2,
            result = new RecipeResult
            {
                id = "producejuice",
                resultCondition = 250f,
                isLiquid = true
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0.5f)
                {
                    quality = "produce"
                },
                new RecipeItem(0.5f)
                {
                    quality = "produce"
                },
                new RecipeItem(0.5f)
                {
                    quality = "produce"
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 75f),
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("hammering", 0.1f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Refined juice Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "refinedjuice",
                resultCondition = 200f,
                isLiquid = true
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(250f)
                {
                    specificId = "producejuice",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("heatsource"),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Drill repair kit Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "drillrepairkit"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "panel"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    quality = "rod"
                },
                new RecipeItem(0f)
                {
                    quality = "cube"
                },
                new RecipeItem(0f)
                {
                    quality = "nails",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "bundleofwires"
                },
                new RecipeItem(0.9f)
                {
                    specificId = "circuitboard"
                },
                new RecipeItem(0f)
                {
                    quality = "hammering",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Tools
        }
    },
    {"Procoagulant Recipe",new Recipe
        {
            INT = 14,
            result = new RecipeResult
            {
                id = "procoagulant",
                isLiquid = true,
                resultCondition = 100f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "clottingmush"
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("blood", 50f)
                },
                new RecipeItem(0f)
                {
                    specificId = "bunchunk"
                },
                new RecipeItem(0f)
                {
                    specificId = "bunchunk"
                },
                new RecipeItem(25f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Antiseptic bottle Recipe",new Recipe
        {
            INT = 7,
            result = new RecipeResult
            {
                id = "disinfectant"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "canteen"
                },
                new RecipeItem(0f)
                {
                    specificId = "plasticchunk"
                },
                new RecipeItem(0f)
                {
                    quality = "cutting",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Medicine
        }
    },
    {"Firestarter Recipe",new Recipe
        {
            INT = 8,
            result = new RecipeResult
            {
                id = "firestarter"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "stick"
                },
                new RecipeItem(0f)
                {
                    specificId = "stick"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("foliage")
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Campfire Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "campfire"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("firestarter"),
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("flammable")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("flammable")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("flammable")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("flammable")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("flammable")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("foliage")
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("foliage")
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Water Recipe",new Recipe
        {
            INT = 4,
            result = new RecipeResult
            {
                id = "water",
                isLiquid = true,
                resultCondition = 200f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                },
                new RecipeItem(200f)
                {
                    specificId = "groundwater",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Charcoal Recipe",new Recipe
        {
            INT = 7,
            result = new RecipeResult
            {
                id = "charcoal",
                amount = 3
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    specificId = "droppings"
                },
                new RecipeItem(10f)
                {
                    specificId = "biochem",
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Bread Recipe",new Recipe
        {
            INT = 7,
            result = new RecipeResult
            {
                id = "bread"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = "flour"
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("water", 150f),
                    isLiquid = true
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Rye flour Recipe",new Recipe
        {
            INT = 6,
            result = new RecipeResult
            {
                id = "ryeflour"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "ryebulb"
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("hammering", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Torch Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "torch",
                resultCondition = 0f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "stick"
                },
                new RecipeItem(0f)
                {
                    specificId = "stick"
                },
                new RecipeItem(0f)
                {
                    specificId = "rope"
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Torch (relight) Recipe",new Recipe
        {
            INT = 5,
            result = new RecipeResult
            {
                id = "torch",
                resultCondition = 1f
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    specificId = "torch"
                },
                new RecipeItem(25f)
                {
                    specificId = "fat",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Utilities
        }
    },
    {"Nutrient bar Recipe",new Recipe
        {
            INT = 9,
            result = new RecipeResult
            {
                id = "nutrientbar"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("flour", 1f)
                },
                new RecipeItem(0.5f)
                {
                    quality = new CraftingQuality("produce")
                },
                new RecipeItem(0.9f)
                {
                    quality = new CraftingQuality("fat", 25f),
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cutting", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Food
        }
    },
    {"Pemmican Recipe",new Recipe
        {
            INT = 6,
            result = new RecipeResult
            {
                id = "pemmican"
            },
            items = new List<RecipeItem>
            {
                new RecipeItem(0f)
                {
                    quality = "heatsource",
                    destroyItem = false
                },
                new RecipeItem(0.2f)
                {
                    quality = "meat"
                },
                new RecipeItem(0.5f)
                {
                    quality = "produce"
                },
                new RecipeItem(75f)
                {
                    specificId = "fat",
                    isLiquid = true
                },
                new RecipeItem(0f)
                {
                    quality = new CraftingQuality("cutting", 0.5f),
                    destroyItem = false
                }
            },
            category = Recipes.RecipeCategory.Food
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
            Recipes.recipes.Clear();
            foreach (string gotrecipe in RecievedRecipes)
            {
                CheckNameToRecipe.TryGetValue(gotrecipe, out Recipe recipeToLearn);
                Recipes.recipes.Add(recipeToLearn); // Add the recipe to the list
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