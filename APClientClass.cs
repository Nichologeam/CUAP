#nullable enable
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Collections;
using BepInEx;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;

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
    private static bool datapackageprocessed = false;
    public static int selectedGoal;
    public static ArchipelagoSession? session;
    public static DeathLinkService? dlService;

    public static string[]? TryConnect(string address, string slot, string? password)
    {
        try
        {
            Startup.Logger.LogMessage($"Attempting to connect to [{address}], with password: [{password}], on slot: [{slot}]");
            if (password.IsNullOrWhiteSpace())
            {
                password = null;
            }
            session = ArchipelagoSessionFactory.CreateSession(address);
            session!.Items.ItemReceived += (item) => ThreadingHelper.Instance.StartSyncInvoke(() => ProcessItem(item)); // this has to be run on main thread or unity will hard crash
            var loginResult = session.TryConnectAndLogin("Casualties: Unknown", slot, ItemsHandlingFlags.AllItems, (new Version(0, 6, 5)), password: password, requestSlotData: true);
            
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
    }

    public static void HasConnected()
    {
        slotdata = session!.DataStorage.GetSlotData(session.Players.ActivePlayer.Slot);
        Startup.Logger.LogMessage("Connnected to Archipelago!");
        dlService = session.CreateDeathLinkService();
        session.Socket.SendPacket(new GetFullDataPackagePacket());
        GameObject.Find("Console(Clone)").GetComponent<CommandPatch>().Subscribe();
        if (slotdata != null && session != null)
        {
            if (slotdata.TryGetValue("Goal", out var goal))
            {
                selectedGoal = Convert.ToInt32(goal);
            }
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
            Startup.Logger.LogMessage("Received " + item.ItemName);
            if ((bool)(item!.ItemName.EndsWith(" Unlock"))) // <layername> Unlock item. Add it to the list of unlocked layers.
            {
                LayerUnlockDictionary.Add(item.ItemName);
            }
            if (item.ItemName == ("Progressive Layer"))
            {
                DepthExtendersRecieved++; // reusing this since it would be unused in Overgrown Depths goal
            }
            if ((bool)(item!.ItemName.EndsWith(" Trap")) || item!.ItemName == "Fellow Experiment") // Trap item. Send off to the TrapHandler to deal with.
            {
                try
                {
                    TrapHandler Traps = GameObject.Find("Experiment/Body").GetComponent<TrapHandler>();
                    Traps.ProcessTraps(item.ItemName, item.Player.Name);
                }
                catch
                {
                    return; // we're on the main menu
                }
            }
            if ((bool)(item!.ItemName.EndsWith(" Recipe"))) // Recipe item. Add it to the list of unlocked recipes.
            {
                RecipeUnlockDictionary.Add(item.ItemName);
                try
                {
                    if (CraftingChecks.freesamples)
                    {
                        UnityEngine.Object.Instantiate(Resources.Load<GameObject>(CraftingChecks.CheckNameToItem.Get(item.ItemName)),
                        GameObject.Find("Experiment/Body").transform.position, Quaternion.identity);
                    }
                }
                catch
                {
                    return; // we're on the main menu
                }
            }
            if (item.ItemName == "Depth Extender")
            {
                DepthExtendersRecieved++;
            }
            if (item.ItemName == "Progressive Left Arm")
            {
                LimbUnlocks.instance.leftArmUnlocks++;
                LimbUnlocks.instance.RestoreLimbs();
            }
            if (item.ItemName == "Progressive Right Arm")
            {
                LimbUnlocks.instance.rightArmUnlocks++;
                LimbUnlocks.instance.RestoreLimbs();
            }
            if (item.ItemName.StartsWith("Progressive ") && item.ItemName != "Progressive Layer")
            {
                string Skill = item.ItemName.Substring(12);
                if (Skill == "STR")
                {
                    MaxSTR++;
                    if (!APCanvas.InGame) // we're on the main menu
                    {
                        return;
                    }
                    SkillReceiving.playerSkills.UpdateExpBoundaries();
                }
                else if (Skill == "RES")
                {
                    MaxRES++;
                    if (!APCanvas.InGame) // we're on the main menu
                    {
                        return;
                    }
                    SkillReceiving.playerSkills.UpdateExpBoundaries();
                }
                else if (Skill == "INT")
                {
                    MaxINT++;
                    if (!APCanvas.InGame) // we're on the main menu
                    {
                        return;
                    }
                    SkillReceiving.playerSkills.UpdateExpBoundaries();
                }
            }
            if (item.ItemName == "Hope")
            {
                try
                {
                    var plr = GameObject.Find("Experiment/Body");
                    plr.GetComponent<Body>().happiness += 3;
                    Sound.Play("moodup", plr.transform.position, true);
                }
                catch
                {
                    return;
                }
            }
            if (item.ItemName == "Despair")
            {
                try
                {
                    var plr = GameObject.Find("Experiment/Body");
                    plr.GetComponent<Body>().happiness -= 1;
                    Sound.Play("mooddown", plr.transform.position, true);
                }
                catch
                {
                    return;
                }
            }
            try
            {
                ExperimentDialog.ProcessDialog(item);
            }
            catch
            {
                return;
            }
        }
        catch (Exception ex)
        {
            Startup.Logger.LogError("ProcessItem Error: " + ex.Message + ex.StackTrace);
            APCanvas.EnqueueArchipelagoNotification("ProcessItem Error: " + ex.Message + ex.StackTrace,3);
            return;
        }
    }

    public static bool IsConnected()
    {
        return session is not null && session.Socket.Connected;
    }

    public static void Update()
    {
        try
        {
            if (GameObject.Find("Console(Clone)").GetComponent<ConsoleScript>().active) // console is pulled down (client makes it difficult to read)
            {
                APCanvas.ShowMainGUI = false;
                APCanvas.ShowSkillTracker = false;
            }
            else if (GameObject.Find("Main Camera/Canvas/WoundView").activeSelf) // woundview is open (client covers it)
            {
                APCanvas.ShowMainGUI = false;
                APCanvas.ShowSkillTracker = true;
            }
            else
            {
                APCanvas.ShowMainGUI = true;
                APCanvas.ShowSkillTracker = false;
            }
        }
        catch
        {
            APCanvas.ShowMainGUI = true; // default true
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
                    playerItemIdToName.TryGetValue(item.Player, out Dictionary<long, string> blueprintitemidtoname);
                    blueprintitemidtoname.TryGetValue(item.Item, out string itemname);
                    CraftingChecks.BlueprintToItemName.Add(item.Location - 22318500, itemname);
                    CraftingChecks.BlueprintToPlayerName.Add(item.Location - 22318500, 
                        session.Players != null && item.Player >= 0 ? session.Players.GetPlayerName(item.Player) : $"Unknown Player (id:{item.Player})");
                }
            }
        }
        else if (packet is DataPackagePacket && !datapackageprocessed)
        {
            try
            {
                Startup.Logger.LogMessage("Received DataPackage");
                datapackageprocessed = true;
                var data = packet.ToJObject()["data"];
                if (data == null) {Startup.Logger.LogError("'data' is null!"); return;}
                var gamelist = data["games"];
                JObject? games = gamelist as JObject;
                if (games == null || session == null) {Startup.Logger.LogError("'Games' is null!"); return;}
                var allPlayers = session.Players.AllPlayers;
                foreach (var player in allPlayers)
                {
                    Startup.Logger.LogMessage($"Processing {player.Name}");
                    int playerID = player.Slot;
                    string gameName = player.Game;
                    if (!games.TryGetValue(gameName, out JToken? gameDataToken))
                    {
                        continue;
                    }
                    JObject? gameData = gameDataToken as JObject;
                    if (gameData == null)
                    {
                        continue;
                    }
                    JObject? itemNameToIdJson = gameData["item_name_to_id"] as JObject;
                    if (itemNameToIdJson == null)
                    {
                        continue;
                    }
                    Dictionary<long, string> itemNameToId = new Dictionary<long, string>();
                    foreach (var prop in itemNameToIdJson.Properties())
                    {
                        long id = prop.Value.Value<long>();
                        string name = prop.Name;

                        if (!itemNameToId.ContainsKey(id))
                            itemNameToId[id] = name;
                    }
                    playerItemIdToName[playerID] = itemNameToId;
                    JObject? locNameToIdJson = gameData["location_name_to_id"] as JObject;
                    if (locNameToIdJson == null)
                    {
                        continue;
                    }
                    Dictionary<long, string> locNameToId = new Dictionary<long, string>();
                    foreach (var prop in locNameToIdJson.Properties())
                    {
                        long id = prop.Value.Value<long>();
                        string name = prop.Name;

                        if (!locNameToId.ContainsKey(id))
                            locNameToId[id] = name;
                    }
                    playerLocIdToName[playerID] = locNameToId;
                }
            }
            catch (Exception ex)
            {
                APCanvas.EnqueueArchipelagoNotification("Datapackage Error: " + ex.ToString(),3);
                Startup.Logger.LogError("Datapackage Error: " + ex.ToString());
            }
        }
        else if (packet is RetrievedPacket)
        {
            var data = packet.ToJObject()["data"];
            if (data == null) { Startup.Logger.LogError("'data' is null!"); return; }
            var keylist = data["keys"];
            JObject? keys = keylist as JObject;
            if (keys == null || session == null) { Startup.Logger.LogError("'keys' is null!"); return; }
            var token = keys["crafted_blueprints"];
            if (token != null && token.Type != JTokenType.Null)
            {
                CraftingChecks.RecipeCraftedBefore = token.ToObject<Dictionary<int, bool>>() ?? [];
                CraftingChecks.CraftedRecipes = CraftingChecks.RecipeCraftedBefore.Count(kvp => kvp.Value);
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
            APCanvas.EnqueueArchipelagoNotification("SendChecks failed! " + ex.ToString(),3);
            Startup.Logger.LogError("SendChecks failed! " + ex.ToString());
            Disconnect();
        }
    }
}

public class GetFullDataPackagePacket : ArchipelagoPacketBase
{// Because 'Games' is required to have a value in the real datapackage, we can't set it to null for all games.
    // So I'll just make my own without a games value! Problem solved!
    public override ArchipelagoPacketType PacketType => ArchipelagoPacketType.GetDataPackage;
}
