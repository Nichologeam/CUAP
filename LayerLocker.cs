using Archipelago.MultiClient.Net;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static CUAP.APCanvas;

namespace CUAP;

public class LayerLocker : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private List<string> LayerHandler = new();
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
        Client = APClientClass.session;
        worldgen = this.gameObject.GetComponent<WorldGeneration>();
        Startup.Logger.LogMessage("LayerLocker Enabled!");
    }
    private void Update()
    {
        if (APClientClass.selectedGoal is 2 or 4) // goals 2 and 4
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

[HarmonyPatch(typeof(WorldGeneration), "GenerateWorld")]
// Allows the player to select what the next layer will be from the ones they have unlocked
class PickLayerBeforeGeneration
{
    static bool resuming = false;
    static bool Prefix(WorldGeneration __instance)
    {
        if (resuming)
        {
            resuming = false;
            return true;
        }
        if (__instance.biomeOverride == WorldGeneration.OverrideSceneType.Tutorial)
        {
            return true; // tutorial level. don't pick a layer
        }
        if (APClientClass.selectedGoal is 2 or 4)
        {
            return true; // layers are progressive. don't pick a layer
        }
        __instance.loadingObject.SetActive(true);
        __instance.generatingWorld = true;
        ShowLayerSelector(__instance);
        return false;
    }
    static void ShowLayerSelector(WorldGeneration instance)
    {
        layerSelector.SetActive(true);
        SetupLayerButton(glButton, instance, 0);
        SetupLayerButton(dglButton, instance, 1);
        SetupLayerButton(ddButton, instance, 2);
        SetupLayerButton(wlButton, instance, 3);
        SetupLayerButton(odButton, instance, 4);
    }
    static void SetupLayerButton(Button button, WorldGeneration instance, int layerID)
    {
        button.onClick.RemoveAllListeners();
        LayerLocker.LayerIDToName.TryGetValue(layerID, out var name);
        bool unlocked = APClientClass.LayerUnlockDictionary.Contains(name);
        var text = button.GetComponentInChildren<TMPro.TMP_Text>();
        if (unlocked) // we have this layer
        {
            text.text = name.Replace(" Unlock",""); // strip the unlock
            text.fontSize = 13;
            button.interactable = true;
            button.onClick.AddListener(() => LayerButtonPressed(instance, layerID));
        }
        else
        {
            text.text = "Layer Locked!";
            text.fontSize = 17;
            button.interactable = false;
            // don't even bother adding the listener
        }
    }
    public static void LayerButtonPressed(WorldGeneration instance, int layerID)
    {
        layerSelector.SetActive(false);
        resuming = true;
        instance.biomeDepth = layerID;
        instance.StartCoroutine("GenerateWorld");
    }
}