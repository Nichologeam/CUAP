#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using BepInEx;
using CreepyUtil.Archipelago;
using UnityEngine;
using static Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags;

namespace CUAP;

// heavily modified version of https://github.com/SWCreeperKing/PowerwashSimAP/blob/master/src/ApDirtClient.cs
public class APClientClass
{
    private static List<long> ChecksToSend = [];
    public static ConcurrentQueue<long> ChecksToSendQueue = [];
    public static ApClient? Client;
    public static List<string> LayerUnlockDictionary = new List<string>();
    private static double NextSend = 4;

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
        Startup.Logger.LogMessage("Disconnected from Archipelago");
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
                if ((bool)(item!.ItemName.EndsWith(" Trap"))) // Trap item. Send off to the TrapHandler to deal with.
                {
                    try
                    {
                        TrapHandler Traps = GameObject.Find("Experiment/Body").GetComponent<TrapHandler>();
                        Traps.ProcessTraps(item.ItemName);
                    }
                    catch
                    {
                        Startup.Logger.LogWarning("Trap dodged. Probably collected offline.");
                    }
                }
                if (item.ItemName == "Victory") // self explanatory
                {
                    Client.Goal();
                }
                try
                {
                    ExperimentDialog Dialog = GameObject.Find("Experiment/Body").GetComponent<ExperimentDialog>();
                    Dialog.ProcessDialog(item);
                }
                catch
                {
                    Startup.Logger.LogWarning("Client can't find Experiment to say dialog. Player probably in main menu.");
                }
            }
            var newItems = rawNewItems // not entierly sure what this does but i dont want to delete it. it seems important.
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

    private static void SendChecks()
    {
        NextSend = 3;
        Startup.Logger.LogMessage("Running SendLocations.");
        Client?.SendLocations(ChecksToSend.ToArray());
        ChecksToSend.Clear();
    }
}