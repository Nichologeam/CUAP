#nullable enable
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using CreepyUtil.Archipelago;
using CreepyUtil.Archipelago.ApClient;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements.Collections;
using static Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags;

namespace CUAP;

// heavily modified version of https://github.com/SWCreeperKing/PowerwashSimAP/blob/master/src/ApDirtClient.cs
public class APClientClass
{
    private static List<string> ChecksToSend = [];
    public static ConcurrentQueue<string> ChecksToSendQueue = [];
    public static ApClient? Client;
    public static List<string> LayerUnlockDictionary = new List<string>();
    public static List<string> RecipeUnlockDictionary = new List<string>();
    public static Dictionary<int, Dictionary<long, string>> playerItemIdToName = new Dictionary<int, Dictionary<long, string>>();
    public static Dictionary<int, Dictionary<long, string>> playerLocIdToName = new Dictionary<int, Dictionary<long, string>>();
    private static double NextSend = 4;
    public static int DepthExtendersRecieved = 0;
    private static bool datapackageprocessed = false;
    public static int selectedGoal;
    static readonly FieldInfo SessionField = AccessTools.Field(typeof(ApClient), "Session"); // this got privated during a client update
    public static ArchipelagoSession? session;

    public static string[]? TryConnect(int port, string slot, string address, string password)
    {
        try
        {
            Client = new ApClient();
            Startup.Logger.LogMessage($"Attempting to connect to [{address}]:[{port}], with password: [{password}], on slot: [{slot}]");
            var connectError = Client.TryConnect(new LoginInfo(port, slot, address, password),
                "Casualties: Unknown", AllItems, (new Version(0, 6, 5)), requestSlotData: true);
            if (connectError is not null && connectError.Length > 0)
            {
                Disconnect();
                return connectError;
            }

            HasConnected();
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
        Client?.TryDisconnect();
        Client = null;
    }

    public static void HasConnected()
    {
        var slotdata = Client?.SlotData!;
        Startup.Logger.LogMessage("Connnected to Archipelago!");
        session = (ArchipelagoSession)SessionField.GetValue(Client);
        session.Socket.SendPacket(new GetFullDataPackagePacket());
        if (slotdata != null && Client != null)
        {
            var options = Client.SlotData["options"] as JObject;
            if (options != null)
            {
                if (options.TryGetValue("Goal", out var goal))
                {
                    selectedGoal = Convert.ToInt32(goal);
                }
            }
        }
    }

    public static bool IsConnected()
    {
        return Client is not null && Client.IsConnected;
    }

    public static void Update()
    {
        try
        {
            if (GameObject.Find("Console(Clone)").GetComponent<ConsoleScript>().active) // console is pulled down (client makes it difficult to read)
            {
                APCanvas.ShowGUI = false;
            }
            else if (GameObject.Find("Main Camera/Canvas/WoundView").activeSelf) // woundview is open (client covers it)
            {
                APCanvas.ShowGUI = false;
            }
            else
            {
                APCanvas.ShowGUI = true;
            }
        }
        catch
        {
            APCanvas.ShowGUI = true; // default true
        }
        APCanvas.UpdateGUIDescriptions();
        if (Client is null) return;
        Client.UpdateConnection();
        if (session?.Socket is null || !Client.IsConnected) return;
        session.Socket.PacketReceived += Socket_PacketReceived;
        NextSend -= Time.deltaTime;
        if (ChecksToSend.Any() && NextSend <= 0)
        {
            SendChecks();
        }

        var rawNewItems = Client.GetOutstandingItems().ToArray();
        if (rawNewItems.Any())
        {
            foreach (var item in rawNewItems)
            {
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
                        continue;
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
                        continue; // we're on the main menu
                    }
                }
                if (item.ItemName == "Depth Extender")
                {
                    DepthExtendersRecieved++;
                }
                if (item.ItemName.StartsWith("Progressive ") && item.ItemName != "Progressive Layer")
                {
                    string Skill = item.ItemName.Substring(12);
                    if (Skill == "STR")
                    {
                        SkillChecks.apMaxStr++;
                        SkillChecks.playerSkills.UpdateExpBoundaries();
                    }
                    else if(Skill == "RES")
                    {
                        SkillChecks.apMaxRes++;
                        SkillChecks.playerSkills.UpdateExpBoundaries();
                    }
                    else if(Skill == "INT")
                    {
                        SkillChecks.apMaxInt++;
                        SkillChecks.playerSkills.UpdateExpBoundaries();
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
                        continue;
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
                        continue;
                    }
                }
                try
                {
                    if ((bool)(item!.ItemName.EndsWith(" Trap")) || item!.ItemName == "Fellow Experiment")
                    {
                        continue;
                    }
                    ExperimentDialog.ProcessDialog(item);
                }
                catch
                {
                    continue;
                }
            }
            var newItems = rawNewItems
                          .Where(item => item?.Flags != 0)
                          .Select(item => item?.ItemName!)
                          .ToArray();
        }

        while (!ChecksToSendQueue.IsEmpty)
        {
            ChecksToSendQueue.TryDequeue(out var location);
            ChecksToSend.Add(location);
        }
    }

    private static void Socket_PacketReceived(ArchipelagoPacketBase packet)
    {
        if (packet is LocationInfoPacket)
        {
            var items = packet.ToJObject()["locations"]?.ToObject<List<NetworkItem>>();
            if (items != null && Client != null)
            {
                foreach (var item in items)
                {
                    playerItemIdToName.TryGetValue(item.Player, out Dictionary<long, string> blueprintitemidtoname);
                    blueprintitemidtoname.TryGetValue(item.Item, out string itemname);
                    CraftingChecks.BlueprintToItemName.Add(item.Location - 22318500, itemname);
                    CraftingChecks.BlueprintToPlayerName.Add(item.Location - 22318500, Client.PlayerNames != null && item.Player >= 0 ? Client.PlayerNames[item.Player] : $"Unknown Player (id:{item.Player})");
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
                if (games == null || Client == null) {Startup.Logger.LogError("'Games' is null!"); return;}
                var allPlayers = Client.AllPlayers.ToList();
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
                GameObject.Find("APCanvas(Clone)").GetComponent<APCanvas>().DisplayArchipelagoNotificationHelper("Datapackage Error: " + ex.ToString(), 3);
                Startup.Logger.LogError("Datapackage Error: " + ex.ToString());
            }
        }
        else if (packet is RetrievedPacket)
        {
            var data = packet.ToJObject()["data"];
            if (data == null) { Startup.Logger.LogError("'data' is null!"); return; }
            var keylist = data["keys"];
            JObject? keys = keylist as JObject;
            if (keys == null || Client == null) { Startup.Logger.LogError("'keys' is null!"); return; }
            var token = keys["crafted_blueprints"];
            if (token != null && token.Type != JTokenType.Null)
            {
                CraftingChecks.RecipeCraftedBefore = token.ToObject<Dictionary<int, bool>>() ?? [];
                CraftingChecks.CraftedRecipes = CraftingChecks.RecipeCraftedBefore.Count(kvp => kvp.Value);
            }
        }
    }

    private static void SendChecks()
    {
        NextSend = 3;
        try
        {
            Client?.SendLocations(ChecksToSend.ToArray());
            ChecksToSend.Clear();
        }
        catch (Exception ex)
        {
            GameObject.Find("APCanvas(Clone)").GetComponent<APCanvas>().DisplayArchipelagoNotificationHelper("SendChecks failed! " + ex.ToString(),3);
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
