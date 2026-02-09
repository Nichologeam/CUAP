using BepInEx;
using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HarmonyLib;

namespace CUAP;

[BepInPlugin("nichologeam.cuap", "Casualties: Unknown Archipelago", "0.0.1.0")]
public class Startup : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static ArchipelagoSession Client;
    public static string CUAPVersion = "CT v0.0.1";
    public static AssetBundle apassets;
    private static Harmony apHarmony;
    GameObject Handler;
    GameObject Console;
    GameObject Body;
    GameObject WorldGen;
    GameObject Moodles;
    public static GameObject pauseMenu;
        
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogMessage($"Casualties: Together Archipelago Plugin {CUAPVersion} loaded!");
        Handler = new GameObject("Archipelago Handler");
        DontDestroyOnLoad(Handler);
        apassets = AssetBundle.LoadFromFile(Path.Combine(BepInEx.Paths.PluginPath, "CUAP", "apctassets"));
        var UI = Instantiate(apassets.LoadAsset<GameObject>("APCTCanvas"));
        DontDestroyOnLoad(UI);
        UI.AddComponent<APCanvas>();
        apHarmony = new Harmony("nichologeam.cuap.harmony");
        apHarmony.PatchAll();
        Logger.LogMessage($"Harmony patches applied!");
    }
    private void Update()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().name == "PreGen")
        {
            try
            {
                Client = APClientClass.session;
                if (Client is null || !Client.Socket.Connected) // we aren't connected. disable the start run buttons.
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
                GameObject.Find("Canvas/VersionWarning/Text (TMP) (1)").GetComponent<TextMeshProUGUI>().text =
                """
                <alpha=#11><i>...a mod for a mod, actually.<alpha=#FF></i>

                If you find a bug, assume it's with CUAP and not Casualties: Together.
                Bugs can be reported in either the Target Planet #art or AP: After Dark #future-game-design threads.
                <alpha=#11><i>Please make sure to note that you are playing the CT version of CUAP.<alpha=#FF></i>
                """;
            }
            catch
            {
                return; // Scene isn't fully loaded yet. Wait a bit.
            }
        }
        else if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (Client is null || !Client.Socket.Connected)
            {
                GameObject.Find("World").GetComponent<WorldGeneration>().SaveAndExit();
                PlayerCamera.main.ToMainMenu();
            }
            Console = GameObject.Find("Console/Canvas").transform.Find("Console").gameObject;
            if (!Console.GetComponent<CommandPatch>())
            {
                Console.AddComponent<CommandPatch>();
                Console.GetComponent<CommandPatch>().Subscribe();
            }
            pauseMenu = GameObject.Find("Main Camera/Canvas").transform.Find("GammaPanel").gameObject;
            if (APClientClass.selectedGoal == 3) // elder thornback goal
            {
                try
                {
                    var elder = GameObject.Find("thornbackelder(Clone)");
                    if (elder.GetComponent<ElderThornback>()) return;
                    elder.AddComponent<ElderThornback>();
                }
                catch
                {
                    return;
                }
            }
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene") // Loading into the game, let's apply Archipelago patches.
        {
            Body = GameObject.Find("Experiment/Body");
            if (Body.GetComponent<TrapHandler>())
            {
                return; // Patches have already been applied, no need to apply them again.
            }
            else
            {
                Body.AddComponent<DeathlinkManager>();
                Body.AddComponent<TrapHandler>();
                Body.AddComponent<ExperimentDialog>();
                Body.AddComponent<SkillReceiving>();
                Body.AddComponent<LimbUnlocks>();
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
            Body = null;
            WorldGen = null;
            Moodles = null;
        }
    }
}