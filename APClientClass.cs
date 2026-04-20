#nullable enable
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace CUAP;

// heavily modified version of https://github.com/SWCreeperKing/PowerwashSimAP/blob/master/src/ApDirtClient.cs
public class APClientClass
{
    public static List<long> ChecksToSend = [];
    public static List<string> LayerUnlockDictionary = new List<string>();
    public static List<string> RecipeUnlockDictionary = new List<string>();
    public static Dictionary<int, Dictionary<long, string>> playerItemIdToName = new Dictionary<int, Dictionary<long, string>>();
    public static Dictionary<int, Dictionary<long, string>> playerLocIdToName = new Dictionary<int, Dictionary<long, string>>();
    public static Dictionary<string, object> slotdata = [];
    public static int MaxSTR;
    public static int MaxRES;
    public static int MaxINT;
    private static double NextSend = 4;
    public static int DepthExtendersRecieved = 0;
    public static int leftArmUnlocks;
    public static int rightArmUnlocks;
    public static int selectedGoal;
    public static int minigameRandom;
    private static float reconnectCountdown;
    public static ArchipelagoSession? session;
    public static DeathLinkService? dlService;

    public static string[]? TryConnect(string address, string slot, string? password)
    {
        if (ThreadingHelper.Instance == null) // V5.0.2 causes BepInEx's bootstrapper to fail creating this, so we'll do it ourselves.
        {
            Startup.Logger.LogWarning("BepInEx.ThreadingHelper is null. Recreating...");
            var threadingHelperType = typeof(ThreadingHelper);
            var initializeMethod = threadingHelperType.GetMethod("Initialize", BindingFlags.Static | BindingFlags.NonPublic);
            initializeMethod!.Invoke(null, null);
        }
        try
        {
            Startup.Logger.LogMessage($"Attempting to connect to [{address}], with password: [{password}], on slot: [{slot}]");
            if (password.IsNullOrWhiteSpace()) password = null;
            session = ArchipelagoSessionFactory.CreateSession(address);
            session!.Items.ItemReceived += (item) => ThreadingHelper.Instance!.StartSyncInvoke(() => ProcessItem(item)); // this has to be run on main thread or unity will hard crash
            var loginResult = session.TryConnectAndLogin("Casualties: Unknown", slot, ItemsHandlingFlags.AllItems, new Version(0, 6, 7), password: password, requestSlotData: true);
            if (loginResult is LoginFailure failure)
            {
                Disconnect();
                return failure.Errors;
            }
            else if (loginResult is LoginSuccessful)
            {
                HasConnected();
            }
        }
        catch (Exception e)
        {
            Disconnect();
            return [e.Message, e.StackTrace!];
        }

        return null;
    }

    public static void Disconnect()
    {
        session?.Socket.DisconnectAsync();
        session = null;
        APCanvas.versionTag.text = APLocale.Get("versionTag", APLocale.APLanguageType.UI) + Startup.CUAPVersion;
        Time.timeScale = 1;
    }

    public static void HasConnected()
    {
        slotdata = session!.DataStorage.GetSlotData(session.Players.ActivePlayer.Slot);
        Startup.Logger.LogMessage("Connnected to Archipelago!");
        dlService = session.CreateDeathLinkService();
        CommandPatch.Console.GetComponent<CommandPatch>().Subscribe();
        if (slotdata != null && session != null)
        {
            if (slotdata.TryGetValue("Goal", out var goal))
            {
                selectedGoal = Convert.ToInt32(goal);
            }
            if (slotdata.TryGetValue("APWorldVersion", out object serverVersion))
            {
                serverVersion = Convert.ToString(serverVersion);
                APCanvas.versionTag.text =
                    $"""
                    {APLocale.Get("versionTag", APLocale.APLanguageType.UI)}{Startup.CUAPVersion}
                    {APLocale.Get("serverVersion", APLocale.APLanguageType.UI)}{serverVersion}
                    """;
                if (!serverVersion.Equals(Startup.CUAPVersion))
                {
                    string errorMsg = APLocale.Get("verMismatch", APLocale.APLanguageType.Errors);
                    errorMsg = errorMsg.Replace("<cli>", Startup.CUAPVersion);
                    errorMsg = errorMsg.Replace("<ser>", $"{serverVersion}");
                    Startup.Logger.LogWarning($"Server/Client Version Mismatch! Client: {Startup.CUAPVersion}, Server: {serverVersion}!");
                    APCanvas.EnqueueArchipelagoNotification(errorMsg, 3);
                }
            }
            if (slotdata.TryGetValue("Minigames", out var minigames))
            {
                minigameRandom = Convert.ToInt32(minigames);
            }
        }
        APCanvas.instance.StartCoroutine(NewConnectionCountdown()); // i need StartCoroutine, so i'll use APCanvas just because it is guarenteed to exist
    }

    private static IEnumerator NewConnectionCountdown()
    {
        reconnectCountdown = 5; // don't display item notifications for the first 5 seconds upon connecting (hides re-receiving every item)
        // this is set to 5 seconds because the client will timeout after 4, so there's no need to wait longer than that, even if it's a 500 item re-recieve
        while (reconnectCountdown > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            reconnectCountdown--;
        }
    }

    private static void ProcessItem(ReceivedItemsHelper helper)
    {
        try
        {
            var item = helper.DequeueItem();
            if (item is null)
            {
                Startup.Logger.LogWarning("ProcessItem called without any items in the queue!");
                return;
            }
            Startup.Logger.LogMessage($"Received {item.ItemName}");
            if (reconnectCountdown <= 0) // countdown has passed
            {
                string itemColor = CommandPatch.ItemDataToPriorityColor(item.Flags);
                if (item.Player == session!.Players.ActivePlayer)
                {
                    APCanvas.EnqueueArchipelagoNotification($"<color=#EE00EE>{APLocale.Get("you", APLocale.APLanguageType.Messages)}</color> {APLocale.Get("foundLocal", APLocale.APLanguageType.Messages)} <color={itemColor}>{item.ItemName}</color>!", 1);
                }
                else
                {
                    APCanvas.EnqueueArchipelagoNotification($"{APLocale.Get("received", APLocale.APLanguageType.Messages)} <color={itemColor}>{item.ItemName}</color> {APLocale.Get("from", APLocale.APLanguageType.Messages)} <color=#FAFAD2>{item.Player.Name}</color>!", 1);
                }
            }
            bool processed = false;
            // Start with item groups
            if (item.ItemName.EndsWith(" Trap") || item.ItemName == "Fellow Experiment") // Trap item. Send off to the TrapHandler to deal with.
            {
                if (APCanvas.InGame)
                {
                    TrapHandler Traps = GameObject.Find("Experiment/Body").GetComponent<TrapHandler>();
                    Traps.ProcessTraps(item.ItemName, item.Player.Name);
                }
                return;
            }
            if (item.ItemName.EndsWith(" Recipe")) // Recipe item. Add it to the list of unlocked recipes.
            {
                RecipeUnlockDictionary.Add(item.ItemName);
                if (APCanvas.InGame && CraftingChecks.freesamples)
                {
                    var spawnID = CraftingChecks.CheckNameToItem.Get(item.ItemName);
                    if (spawnID != null)
                    {
                        UnityEngine.Object.Instantiate(Resources.Load<GameObject>(spawnID), GameObject.Find("Experiment/Body").transform.position, Quaternion.identity);
                    }
                }
                processed = true;
            }
            if (item.ItemName.EndsWith(" Crystal Shard"))
            {
                string objectName = item.ItemName.ToLower().Replace(" ",""); // lowercase. remove spaces
                if (APCanvas.InGame)
                {
                    UnityEngine.Object.Instantiate(Resources.Load<GameObject>(objectName),
                    GameObject.Find("Experiment/Body").transform.position, Quaternion.identity);
                }
                processed = true;
            }
            if (item.ItemName.EndsWith(" Minigame"))
            {
                MinigameLocker.minigamesUnlocked.Add(item.ItemName);
                processed = true;
            }
            if (!processed)
            {
                switch (item.ItemName) // then put everything else in a switch statement to make it cleaner
                {
                    case "Hope":
                        if (APCanvas.InGame)
                        {
                            var plr = GameObject.Find("Experiment/Body");
                            plr.GetComponent<Body>().happiness += 3;
                            Sound.Play("moodup", plr.transform.position, true);
                        }
                        break;
                    case "Despair":
                        if (APCanvas.InGame)
                        {
                            var plr = GameObject.Find("Experiment/Body");
                            plr.GetComponent<Body>().happiness -= 1;
                            Sound.Play("mooddown", plr.transform.position, true);
                        }
                        break;
                    case "Gravel Lands Unlock":
                        LayerUnlockDictionary.Add(item.ItemName);
                        break;
                    case "Deeper Gravel Lands Unlock":
                        LayerUnlockDictionary.Add(item.ItemName);
                        break;
                    case "Dried Desert Unlock":
                        LayerUnlockDictionary.Add(item.ItemName);
                        break;
                    case "Wasteland Unlock":
                        LayerUnlockDictionary.Add(item.ItemName);
                        break;
                    case "Overgrown Depths Unlock":
                        LayerUnlockDictionary.Add(item.ItemName);
                        break;
                    case "Progressive Layer":
                        DepthExtendersRecieved++; // reusing this since it would be unused in Overgrown Depths goal
                        break;
                    case "Depth Extender":
                        DepthExtendersRecieved++;
                        break;
                    case "Progressive Quests":
                        Moodlesanity.RefreshMaxQuests(true);
                        break;
                    case "Questboard Slot":
                        APCanvas.UpdateQuestboard(true);
                        break;
                    case "Progressive Left Arm":
                        leftArmUnlocks++;
                        if (APCanvas.InGame) LimbUnlocks.instance.RestoreLimbs();
                        break;
                    case "Progressive Right Arm":
                        rightArmUnlocks++;
                        if (APCanvas.InGame) LimbUnlocks.instance.RestoreLimbs();
                        break;
                    case "Progressive STR":
                        MaxSTR++;
                        if (APCanvas.InGame) SkillReceiving.playerSkills.UpdateExpBoundaries();
                        break;
                    case "Progressive RES":
                        MaxRES++;
                        if (APCanvas.InGame) SkillReceiving.playerSkills.UpdateExpBoundaries();
                        break;
                    case "Progressive INT":
                        MaxINT++;
                        if (APCanvas.InGame) SkillReceiving.playerSkills.UpdateExpBoundaries();
                        break;
                    case "Minigames":
                        MinigameLocker.minigameItemObtained = true;
                        break;
                    default:
                        Startup.Logger.LogWarning($"{item.ItemName} is unhandled!");
                        APCanvas.EnqueueArchipelagoNotification(item.ItemName + APLocale.Get("unhandledItem", APLocale.APLanguageType.Errors), 3);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Startup.Logger.LogError("ProcessItem Error: " + ex.Message + ex.StackTrace);
            APCanvas.EnqueueArchipelagoNotification(APLocale.Get("processitem", APLocale.APLanguageType.Errors) + ex.Message + ex.StackTrace,3);
            return;
        }
    }

    public static bool IsConnected()
    {
        return session is not null && session.Socket.Connected;
    }

    public static void Update()
    {
        Startup.instance.Update(); // after V5.0.2, Startup gets force disabled at game startup. Reennabling it doesn't work, so I'll just call update manually!
        try
        {
            if (WoundView.view.gameObject.activeInHierarchy) // woundview is open (client covers it)
            {
                APCanvas.ShowMainGUI = false;
                APCanvas.ShowSkillTracker = true;
            }
            else if (GameObject.Find("Main Camera/Canvas").transform.Find("GammaPanel").gameObject.activeSelf) // gamma panel is open
            {
                APCanvas.ShowMainGUI = true;
                APCanvas.ShowSkillTracker = false;
            }
            else
            {
                APCanvas.ShowMainGUI = false;
                APCanvas.ShowSkillTracker = false;
            }
            if (WorldGeneration.world.unchippedMode) // player is unchipped
            {
                APCanvas.ShowSkillTracker = false;
            }
        }
        catch
        {
            APCanvas.ShowMainGUI = false; // default false
            APCanvas.ShowSkillTracker = false;
        }
        APCanvas.UpdateGUIDescriptions();
        if (session is null) return;
        if (session?.Socket is null || !session.Socket.Connected) return;
        session.Socket.PacketReceived += Socket_PacketReceived;
        NextSend -= Time.deltaTime;
        if (ChecksToSend.Any() && NextSend <= 0)
        {
            SendChecks();
        }
    }

    private static void Socket_PacketReceived(ArchipelagoPacketBase packet)
    {
        if (packet is LocationInfoPacket)
        {
            var items = packet.ToJObject()["locations"]?.ToObject<List<NetworkItem>>();
            if (items != null && session != null)
            {
                foreach (var item in items)
                {
                    var itemname = session.Items.GetItemName(item.Item, session.Players?.GetPlayerInfo(item.Player).Game);
                    CraftingChecks.BlueprintToItemName.Add(item.Location - 22318500, itemname);
                    CraftingChecks.BlueprintToPlayerName.Add(item.Location - 22318500, 
                        session.Players != null && item.Player >= 0 ? session.Players.GetPlayerName(item.Player) : $"Unknown Player (id:{item.Player})");
                }
            }
        }
    }

    private static async void SendChecks()
    {
        NextSend = 3;
        try
        {
            await session!.Locations.CompleteLocationChecksAsync(ChecksToSend.ToArray());
            ChecksToSend.Clear();
        }
        catch (Exception ex)
        {
            APCanvas.EnqueueArchipelagoNotification(APLocale.Get("sendchecks", APLocale.APLanguageType.Errors) + ex,3);
            Startup.Logger.LogError($"SendChecks failed! {ex}");
            Disconnect();
        }
    }
}