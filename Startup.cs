using BepInEx;
using BepInEx.Logging;
using CreepyUtil.Archipelago;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CUAP;

[BepInPlugin("nichologeam.cuap", "Casualties: Unknown Archipelago", "0.4.2.0")]
public class Startup : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static ApClient Client;
    GameObject Handler;
    GameObject Console;
    GameObject Body;
    GameObject WorldGen;
    GameObject Moodles;
        
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogMessage($"Casualties: Unknown/Scav Prototype Archipelago Plugin v0.4.2 loaded!");
        Handler = new GameObject("Archipelago GUI Hander");
        Handler.AddComponent<APGui>();
        DontDestroyOnLoad(Handler);
    }
    private void Update()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().name == "PreGen")
        {
            try
            {
                ScrollableText.ForceClose(); // skip intoductory story
                Client = APClientClass.Client;
                if (Client is null || !(Client?.IsConnected ?? false)) // we aren't connected. disable the start run buttons.
                {
                    GameObject.Find("Canvas/Button").GetComponent<Button>().interactable = false; // Start Run button
                    GameObject.Find("Canvas/Button (7)").GetComponent<Button>().interactable = false; // Continue button
                    GameObject.Find("Canvas/Button (2)").GetComponent<Button>().interactable = false; // Tutorial button
                }
                else
                {
                    GameObject.Find("Canvas/Button").GetComponent<Button>().interactable = true;
                    GameObject.Find("Canvas/Button (7)").GetComponent<Button>().interactable = true;
                    GameObject.Find("Canvas/Button (2)").GetComponent<Button>().interactable = true;
                }
            }
            catch
            {
                return; // Scene isn't fully loaded yet. Wait a bit.
            }
        }
        else if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (Client is null || !(Client?.IsConnected ?? false))
            {
                Startup.Logger.LogError("Archipelago disconnected mid run! Quitting to main menu to prevent breaking the client!");
                GameObject.Find("World").GetComponent<WorldGeneration>().SaveAndExit();
                PlayerCamera.main.ToMainMenu();
            }
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene") // Loading into the game, let's apply Archipelago patches.
        {
            Console = GameObject.Find("Console(Clone)");
            if (Console.GetComponent<CommandPatch>())
            {
                return; // Patches have already been applied, no need to apply them again.
            }
            else
            {
                Console.AddComponent<CommandPatch>();
                Body = GameObject.Find("Experiment/Body");
                Body.AddComponent<DeathlinkManager>();
                Body.AddComponent<TrapHandler>();
                Body.AddComponent<ExperimentDialog>();
                Body.AddComponent<SkillChecks>();
                WorldGen = GameObject.Find("World");
                WorldGen.AddComponent<DepthChecks>();
                WorldGen.AddComponent<LayerLocker>();
                Moodles = GameObject.Find("Main Camera/Canvas/Moodles");
                Moodles.AddComponent<Moodlesanity>();
                Handler.AddComponent<CraftingChecks>();
            }
        }
        else if (scene.name == "PreGen") // Scene loaded was PreGen, let's clear these objects to avoid errors.
        {
            Console = null;
            Body = null;
            WorldGen = null;
            Moodles = null;
        }
    }
}