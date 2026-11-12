using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CUAP;

[BepInPlugin("nichologeam.cuap", "Casualties: Archipelago", "0.13.0")]
[BepInDependency("CasualtiesMP", BepInDependency.DependencyFlags.SoftDependency)] // ensure the MP mod is loaded first if it exists. do nothing if it doesn't
public class Startup : BaseUnityPlugin
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);
    public static new ManualLogSource Logger;
    public static ArchipelagoSession Client;
    public static string CUAPVersion = "v0.13.0-pre2";
    public static AssetBundle apassets;
    private static Harmony apHarmony;
    public static Startup instance;
    GameObject Handler;
    GameObject Console;
    GameObject Body;
    GameObject WorldGen;
    GameObject Moodles;
        
    private void Awake()
    {
        if (Chainloader.PluginInfos.TryGetValue("CasualtiesMP", out PluginInfo MPMod)) // fails silently if Multiplayer isn't installed
        {
            bool isDisabled = Convert.ToBoolean(PlayerPrefs.GetInt("CasualtiesMP_FORCE_DISABLE_MP_MOD"));
            if (!isDisabled)
            {
                throw new System.Exception("[Casualties: Archipelago] This mod is not compatable with multiplayer! Disable or remove the multiplayer mod to use Casualties: Archipelago!");
            }
        }
        instance = this;
        Logger = base.Logger;
        Logger.LogMessage($"Casualties: Archipelago {CUAPVersion} loaded!");
        Logger.LogWarning("If these lines appear in the console, but the mod does not initalize, do the following:");
        Logger.LogMessage("Go to 'BepInEx/config' and open the BepInEx.cfg file in a text editor,");
        Logger.LogMessage("Scroll down to the bottom, to the 'Preloader.Entrypoint' section,");
        Logger.LogMessage("Change the 'Type' option (second one) from 'Application' to 'MonoBehaviour',");
        Logger.LogMessage("Save the file and restart Casualties: Unknown.");
        Logger.LogMessage("If the mod has no issues booting, ignore these instructions.");
        StartCoroutine(InitWhenSceneExists());
    }
    private void Init()
    {
        Logger.LogMessage("Init");
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
        Console = GameObject.Find("Console(Clone)");
        Console.AddComponent<CommandPatch>();
        SetWindowTitle($"Casualties: Archipelago {CUAPVersion} | Disconnected");
        SteamManager.disableAchievements = true;
        ReplaceLogos();
    }

    // due to a change in Cas: Unk version 5.0.2, the mod stopped loading properly due to it initalizing before Unity had actually made the scene
    // because of this nonsense, I have to move a majority of the startup code from Awake to a custom Init function.
    private IEnumerator InitWhenSceneExists()
    {
        yield return new WaitUntil(() => SceneManager.sceneCount > 0 && SceneManager.GetActiveScene().IsValid() &&  SceneManager.GetActiveScene().isLoaded);
        Init();
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
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene") // Loading into the game, let's apply Archipelago patches.
        {
            if (Gifting.shouldGiftBoxExist)
            {
                Gifting.giftService.OpenGiftBox(true, []); // all gifts accepted, no traits desired
                Logger.LogMessage("Giftbox open");
            }
            Body = GetPlayerBodyObject();
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
            if (WorldGeneration.world.biomeOverride != WorldGeneration.OverrideSceneType.Tutorial)
            {
                Client.SetClientState(ArchipelagoClientState.ClientPlaying);
            }
        }
        else if (scene.name == "PreGen") // Scene loaded was PreGen, let's clear these objects to avoid errors.
        {
            if (Gifting.shouldGiftBoxExist)
            {
                Gifting.giftService.CloseGiftBox(); // close the gift box if it's open
                Logger.LogMessage("Giftbox closed");
            }
            Body = null;
            WorldGen = null;
            Moodles = null;
            ReplaceLogos();
            Client.SetClientState(ArchipelagoClientState.ClientConnected);
        }
    }

    private static void ReplaceLogos()
    {
        Sprite replacement = apassets.LoadAsset<Sprite>("logotext");
        GameObject.Find("Canvas/Logo").GetComponent<Image>().sprite = replacement;
        Image warning = GameObject.Find("Canvas/Warning/Cas").GetComponent<Image>();
        warning.sprite = replacement;
        warning.preserveAspect = true;
    }

    public static void SetWindowTitle(string title)
    {
        if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows)
        {
            Logger.LogWarning("Changing the window title is disabled on systems other than Windows.");
            return;
        }
        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
        if (hWnd != IntPtr.Zero)
        {
            SetWindowText(hWnd, title);
        }
        else
        {
            Logger.LogError("hWnd returned IntPtr.Zero when changing window title!"); // no translation, as this doesn't need to be user facing
        }
    }

    // Helper functions that I don't know where else to put
    public static GameObject GetPlayerBodyObject()
    {
        GameObject camera = GameObject.Find("Main Camera");
        if (camera == null)
        {
            Logger.LogError("Can't get the player's Body object when they aren't in a run!");
            return null;
        }
        Body body = camera.GetComponent<PlayerCamera>().body;
        return body.gameObject;
    }

    public static Body GetPlayerBodyScript()
    {
        GameObject camera = GameObject.Find("Main Camera");
        if (camera == null)
        {
            Logger.LogError("Can't get the player's Body script when they aren't in a run!");
            return null;
        }
        return camera.GetComponent<PlayerCamera>().body;
    }

    public static int GetPlayerSpecies()
    {
        GameObject camera = GameObject.Find("Main Camera");
        if (camera == null)
        {
            Logger.LogError("Can't get the player's species when they aren't in a run!");
            return -1;
        }
        Body body = camera.GetComponent<PlayerCamera>().body;
        return body.charType; // Expie = 0, Milky = 1, Dune = 2
    }

    public static async void CheckHasAllSawians(bool report = false)
    {
        ArchipelagoSession Client = APClientClass.session;
        if (!APClientClass.goalNeedsAllSawians && !report)
        {
            Client.SetGoalAchieved(); // there's no need to check this, simply goal
            DepthChecks.instance.DisplayText.text = APLocale.Get("goal", APLocale.APLanguageType.UI);
        }
        Client.DataStorage[Scope.Slot, "SawianGoals"].Initialize(new[] { false, false, false }); // creates the key and sets all values to false if the key doesn't already exist
        bool[] goals = await Client.DataStorage[Scope.Slot, "SawianGoals"].GetAsync<bool[]>();
        if (report)
        {
            CommandPatch.LogToConsole($"Expieriment: {goals[0]}, Milky: {goals[1]}, Dune: {goals[2]}");
            return;
        }
        int currentSawian = GetPlayerSpecies();
        goals[currentSawian] = true;
        if (goals.All(x => x)) // all sawians have goaled
        {
            Client.SetGoalAchieved();
            DepthChecks.instance.DisplayText.text = APLocale.Get("goal", APLocale.APLanguageType.UI);
        }
        Client.DataStorage[Scope.Slot, "SawianGoals"] = goals;
    }
}