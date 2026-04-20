using BepInEx;
using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.Threading.Tasks;

namespace CUAP;

[BepInPlugin("nichologeam.cuap", "Casualties: Unknown Archipelago", "0.8.0.0")]
public class Startup : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
    public static ArchipelagoSession Client;
    public static string CUAPVersion = "v0.8.0";
    public static AssetBundle apassets;
    private static Harmony apHarmony;
    public static Startup instance;
    GameObject Handler;
    GameObject Console;
    GameObject Body;
    GameObject WorldGen;
    GameObject Moodles;
        
    private async void Awake()
    {
        await Task.Delay(1000);
        instance = this;
        Logger = base.Logger;
        Logger.LogMessage($"Casualties: Unknown Archipelago Plugin {CUAPVersion} loaded!");
        Handler = new GameObject("Archipelago Handler");
        DontDestroyOnLoad(Handler);
        APLocale.LoadLang("EN.json");
        apassets = AssetBundle.LoadFromFile(Path.Combine(BepInEx.Paths.PluginPath, "CUAP", "apassets"));
        var UI = Instantiate(apassets.LoadAsset<GameObject>("APCanvas"));
        DontDestroyOnLoad(UI);
        UI.AddComponent<APCanvas>();
        apHarmony = new Harmony("nichologeam.cuap.harmony");
        apHarmony.PatchAll();
        Logger.LogMessage($"Harmony patches applied!");
        SceneManager.sceneLoaded += OnSceneLoaded;
        MainMenuPatches();
        Console = GameObject.Find("Console(Clone)");
        Console.AddComponent<CommandPatch>();
        var betaNotif = GameObject.Find("GlobalDark(Clone)/betabuild").GetComponent<TextMeshProUGUI>();
        betaNotif.text = $"{betaNotif.text}\n{APLocale.Get("mainMenuMessage", APLocale.APLanguageType.UI)}";
    }
    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "PreGen")
        {
            try
            {
                ScrollableText.ForceClose(); // skip intoductory story
                Client = APClientClass.session;
                GameObject.Find("Canvas/MenuBackground/Experiment/Clickable").SetActive(!(Client is null || !Client.Socket.Connected)); // Experiment on cave floor
                GameObject.Find("Canvas/MenuBackground/TutorialRadio/Clickable").SetActive(!(Client is null || !Client.Socket.Connected)); // Tutorial Radio on cave floor
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
                APClientClass.Disconnect();
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
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
            MainMenuPatches();
        }
    }
    // due to a change in Cas: Unk version 5.0.2, the mod stopped loading properly without some odd workarounds (hence the `await Task.Delay` in Awake)
    // because of this nonsense, I can't actually hook SceneManager.sceneLoaded quick enough to catch the main menu loading on game start
    // so I have to put this both in Awake, and in OnSceneLoaded. why duplicate code when I can just make it a function instead?
    // that's why this random function with two lines is here.
    private void MainMenuPatches()
    {
        // GameObject.Find("Canvas/Logo").GetComponent<Image>().sprite = apassets.LoadAsset<Sprite>("logotext"); <<< Uses outdated logo. Needs to be remade
    }
}