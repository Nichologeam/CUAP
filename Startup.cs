using BepInEx;
using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CUAP;

[BepInPlugin("nichologeam.cuap", "Casualties: Unknown Archipelago", "0.6.0.1")]
public class Startup : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static ArchipelagoSession Client;
    public static AssetBundle apassets;
    GameObject Handler;
    GameObject Console;
    GameObject Body;
    GameObject WorldGen;
    GameObject Moodles;
        
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogMessage($"Casualties: Unknown Archipelago Plugin v0.6.0-pre1 loaded!");
        Handler = new GameObject("Archipelago Handler");
        DontDestroyOnLoad(Handler);
        apassets = AssetBundle.LoadFromFile(Path.Combine(BepInEx.Paths.PluginPath, "CUAP", "apassets"));
        var UI = Instantiate(apassets.LoadAsset<GameObject>("APCanvas"));
        DontDestroyOnLoad(UI);
        UI.AddComponent<APCanvas>();
    }
    private void Update()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SceneManager.GetActiveScene().name == "PreGen")
        {
            try
            {
                ScrollableText.ForceClose(); // skip intoductory story
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
                Console = GameObject.Find("Console(Clone)");
                if (!Console.GetComponent<CommandPatch>())
                {
                    Console.AddComponent<CommandPatch>();
                }
                GameObject.Find("Canvas/VersionWarning/Text (TMP) (1)").GetComponent<TextMeshProUGUI>().text =
                """
                <alpha=#11><i>...of both Casualties: Unknown and CUAP.<alpha=#FF></i>

                Bug reports on the Discord servers would be appreciated.
                <size=16><alpha=#11>Either the Orsoniks' Studio #art or AP: After Dark #future-game-design threads.<alpha=#FF><size=20>

                Keep a look-out for:<alpha=#55>
                - Bugs with the crafting system
                - Incorrectly sending checks
                - Errors, both onscreen and in the BepInEx console
                - Nonfuctional .yaml settings
                - Glitches
                <size=16><alpha=#11>You can also directly create issues on the CUAP Github repository, or by using `apreportbug` in the debug console.
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
            Body = null;
            WorldGen = null;
            Moodles = null;
        }
    }
}