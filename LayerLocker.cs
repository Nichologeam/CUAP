using CreepyUtil.Archipelago;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CUAP;

public class LayerLocker : MonoBehaviour
{
    public static ApClient Client;
    private List<string> LayerHandler = new List<string>();
    private string SelectedLayer;
    private int LayerId = -1;
    public static int LayerCount = 0;
    private WorldGeneration worldgen;
    public static Dictionary<string, int> LayerNameToID = new Dictionary<string, int>()
    {
        {"Gravel Lands Unlock",0},
        {"Deeper Gravel Lands Unlock",1},
        {"Dried Desert Unlock",2},
        {"Wasteland Unlock",3},
        {"Overgrown Depths Unlock",4},
    };
    public static Dictionary<int, string> LayerIDToName = new Dictionary<int, string>()
    {
        {0,"Gravel Lands Unlock"},
        {1,"Deeper Gravel Lands Unlock"},
        {2,"Dried Desert Unlock"},
        {3,"Wasteland Unlock"},
        {4,"Overgrown Depths Unlock"},
    };

    private void OnEnable()
    {
        Client = APClientClass.Client;
        worldgen = this.gameObject.GetComponent<WorldGeneration>();
        Startup.Logger.LogMessage("LayerLocker Enabled!");
    }
    private void Update()
    {
        if (APClientClass.selectedGoal is 1 or 3) // goals 1 and 3
        {
            LayerHandler = APClientClass.LayerUnlockDictionary;
            if (worldgen.loadingObject.activeSelf && !worldgen.tutorialOverride) // as funny as being in overgrown depths tutorial is, i had to fix this eventually
            {
                if (LayerHandler.Count <= 1) // Minimum is 1, since you always have your starting location. Placed a less than as a failsafe.
                {
                    LayerNameToID.TryGetValue(LayerHandler.FirstOrDefault(), out int LayerId); // If we for some reason have nothing, this goes to Gravel Lands
                    worldgen.biomeDepth = LayerId; // We don't have any unlocks, don't randomize and instead just go back to what we had.
                }
                else
                {
                    if (LayerId == -1) // If LayerId has already been randomized, don't randomize it again.
                    {
                        SelectedLayer = LayerHandler[UnityEngine.Random.Range(0, LayerHandler.Count)];
                        LayerNameToID.TryGetValue(SelectedLayer, out int SelectedId);
                        LayerId = SelectedId;
                    }
                    worldgen.biomeDepth = LayerId;
                }
            }
            else
            {
                LayerId = -1;
            }
        }
        else if (APClientClass.selectedGoal is 2 or 4) // goals 2 and 4
        {
            if (worldgen.loadingObject.activeSelf)
            {
                LayerCount = APClientClass.DepthExtendersRecieved;
                if (LayerCount == 0) // No progressive layers received
                {
                    worldgen.biomeDepth = 0; // We don't have any new layers. Go back to Gravel Lands.
                }
                else if (LayerCount < worldgen.biomeDepth) // deeper than our max layer?
                {
                    worldgen.biomeDepth = LayerCount; // go back one layer
                }
            }
        }
    }
}