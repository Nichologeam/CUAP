#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using CreepyUtil.Archipelago;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements.Collections;
using static System.Collections.Specialized.BitVector32;
using static Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags;

namespace CUAP;

// heavily modified version of https://github.com/SWCreeperKing/PowerwashSimAP/blob/master/src/ApDirtClient.cs
public class APClientClass
{
    private static List<long> ChecksToSend = [];
    public static ConcurrentQueue<long> ChecksToSendQueue = [];
    public static ApClient? Client;
    public static List<string> LayerUnlockDictionary = new List<string>();
    public static List<string> RecipeUnlockDictionary = new List<string>();
    private static double NextSend = 4;
    public static int DepthExtendersRecieved = 0;

    public static string[]? TryConnect(int port, string slot, string address, string password, bool deathlink)
    {
        try
        {
            Client = new ApClient();
            if (!deathlink)
            {
                Startup.Logger.LogMessage($"Attempting to connect to [{address}]:[{port}], with password: [{password}], on slot: [{slot}]");
                var connectError = Client.TryConnect(new LoginInfo(port, slot, address, password), 0x3AF4F1BC,
                    "Casualties: Unknown", AllItems, (new Version(0, 6, 3)), requestSlotData: true);
                if (connectError is not null && connectError.Length > 0)
                {
                    Startup.Logger.LogError("There was an Error connecting!" + connectError);
                    Disconnect();
                    return connectError;
                }
            }
            else
            {
                Startup.Logger.LogMessage($"Attempting to connect with DeathLink to [{address}]:[{port}], with password: [{password}], on slot: [{slot}]");
                var connectError = Client.TryConnect(new LoginInfo(port, slot, address, password), 0x3AF4F1BC,
                    "Casualties: Unknown", AllItems, (new Version(0, 6, 3)), ["DeathLink"], requestSlotData: true);
                if (connectError is not null && connectError.Length > 0)
                {
                    Startup.Logger.LogError("There was an Error connecting with DeathLink!" + connectError);
                    Disconnect();
                    return connectError;
                }
            }

            HasConnected();
        }
        catch (Exception e)
        {
            Startup.Logger.LogError("There was an Error with Archipelago!");
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
    }

    public static bool IsConnected()
    {
        return Client is not null && Client.IsConnected && Client.Session.Socket.Connected;
    }

    public static void Update()
    {
        if (Client is null) return;
        Client.UpdateConnection();
        if (Client?.Session?.Socket is null || !Client.IsConnected) return;
        Client.Session.Socket.PacketReceived += Socket_PacketReceived;
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
                if ((bool)(item!.ItemName.EndsWith(" Trap")) || item!.ItemName == "Fellow Experiment") // Trap item. Send off to the TrapHandler to deal with.
                {
                    try
                    {
                        TrapHandler Traps = GameObject.Find("Experiment/Body").GetComponent<TrapHandler>();
                        Traps.ProcessTraps(item.ItemName);
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
                            GameObject.Find("Experiment").transform.position, Quaternion.identity);
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
                if (item.ItemName.StartsWith("Progressive "))
                {
                    string Skill = item.ItemName.Substring(12);
                    if (Skill == "STR")
                    {
                        SkillChecks.apMaxStr++;
                    }
                    else if(Skill == "RES")
                    {
                        SkillChecks.apMaxRes++;
                    }
                    else if(Skill == "INT")
                    {
                        SkillChecks.apMaxInt++;
                    }
                }
                try
                {
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

    private static void Socket_PacketReceived(Archipelago.MultiClient.Net.ArchipelagoPacketBase packet)
    {
        if (packet is LocationInfoPacket)
        {
            var items = packet.ToJObject()["locations"]?.ToObject<List<NetworkItem>>();
            if (items != null && Client != null)
            {
                foreach (var item in items)
                {
                    CraftingChecks.BlueprintToItemName.Add(item.Location - 22318500, Client.ItemIdToItemName(item.Item, Client.PlayerSlot) ?? $"Unknown Item (id:{item.Item})");
                    CraftingChecks.BlueprintToPlayerName.Add(item.Location - 22318500, Client.PlayerNames != null && item.Player >= 0 ? Client.PlayerNames[item.Player] : $"Unknown Player (id:{item.Player})");
                }
            }
        }
    }

    private static void SendChecks()
    {
        NextSend = 3;
        Client?.SendLocations(ChecksToSend.ToArray());
        ChecksToSend.Clear();
    }
}