using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using CreepyUtil.Archipelago;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ApClient Client;
    public static ConsoleScript Console;
    private GameObject body;
    private string LastGotMessage;
    private JsonMessagePart[] LastGotItemMessage;
    private HintPrintJsonPacket LastGotHintMessage;

    private void OnEnable()
    {
        Console = gameObject.GetComponent<ConsoleScript>();
        Startup.Logger.LogMessage("Console has been patched!");
        CreateAPCommands();
    }
    private void Update()
    {
        Client = APClientClass.Client;
        if (Client is not null)
        {
            Client.OnServerMessagePacketReceived += PrintPlainJSON;
            Client.OnItemLogPacketReceived += PrintItemJSON;
            Client.OnHintPrintJsonPacketReceived += PrintHintJSON;
        }
    }
    private void PrintPlainJSON(object sender, PrintJsonPacket message)
    {
        if (LastGotMessage == message.Data[0].Text) { return; } // avoids spam
        Console.LogToConsole(message.Data[0].Text);
        LastGotMessage = message.Data[0].Text;
    }
    private void PrintItemJSON(object sender, PrintJsonPacket message)
    {
        var combinedText = message.Data;
        if (LastGotItemMessage == combinedText) return;
        LastGotItemMessage = combinedText;
        string constructedMessage = "";
        if (message.Data[1].Text == " sent ") // multiworld item (8 properties)
        { // this is probably needlessly overcomplicated, but it works
            var sendPlrName = (Client.PlayerNames[Convert.ToInt32(message.Data[0].Text)]);
            var recPlrName = (Client.PlayerNames[Convert.ToInt32(message.Data[4].Text)]);
            APClientClass.playerItemIdToName.TryGetValue(Convert.ToInt32(message.Data[4].Text), out Dictionary<long, string> recPlrItemIds);
            recPlrItemIds.TryGetValue(Convert.ToInt64(message.Data[2].Text), out string sentItemName);
            APClientClass.playerLocIdToName.TryGetValue(Convert.ToInt32(message.Data[0].Text), out Dictionary<long, string> sendPlrLocIds);
            sendPlrLocIds.TryGetValue(Convert.ToInt64(message.Data[6].Text), out string foundLocName);
            constructedMessage = sendPlrName + message.Data[1].Text + sentItemName + message.Data[3].Text + recPlrName + message.Data[5].Text + foundLocName + message.Data[7].Text;
        }
        else if (message.Data[1].Text == " found their ") // local item (6 properties)
        {
            var plrName = (Client.PlayerNames[Convert.ToInt32(message.Data[0].Text)]);
            APClientClass.playerItemIdToName.TryGetValue(Convert.ToInt32(message.Data[0].Text), out Dictionary<long, string> plrItemIds);
            plrItemIds.TryGetValue(Convert.ToInt64(message.Data[2].Text), out string itemName);
            APClientClass.playerLocIdToName.TryGetValue(Convert.ToInt32(message.Data[0].Text), out Dictionary<long, string> plrLocIds);
            plrLocIds.TryGetValue(Convert.ToInt64(message.Data[4].Text), out string locName);
            constructedMessage = plrName + message.Data[1].Text + itemName + message.Data[3].Text + locName + message.Data[5].Text;
        }
        Console.LogToConsole(constructedMessage);
        // message.Data[local]/[multiworld]
        // message.Data[0]/[0] is finding player ID
        // message.Data[1]/[1] is either " sent " or " found their "
        // message.Data[2]/[2] is receiving item ID
        // message.Data[n]/[3] is " to "
        // message.Data[n]/[4] is receiving player ID
        // message.Data[3]/[5] is " ("
        // message.Data[4]/[6] is the finder's location ID
        // message.Data[5]/[7] is ")"
    }
    private void PrintHintJSON(object sender, HintPrintJsonPacket message)
    {
        var combinedText = message;
        if (LastGotHintMessage == combinedText || message.Found == true) return; // don't show found hints (less clutter)
        LastGotHintMessage = combinedText;
        var recPlrName = (Client.PlayerNames[message.ReceivingPlayer]);
        NetworkItem item = message.Item;
        var fndPlrName = (Client.PlayerNames[item.Player]);
        APClientClass.playerItemIdToName.TryGetValue(message.ReceivingPlayer, out Dictionary<long, string> plrItemIds);
        plrItemIds.TryGetValue(item.Item, out string itemName);
        APClientClass.playerLocIdToName.TryGetValue(item.Player, out Dictionary<long, string> plrLocIds);
        plrLocIds.TryGetValue(item.Location, out string locName);
        string constructedMessage = recPlrName + "'s " + itemName + " is at " + locName + " in " + fndPlrName + "'s world.";
        Console.LogToConsole(constructedMessage);
        // message.Data[0] is receiving player ID
        // message.Data[1] is the NetworkItem
        // message.Data[2] is if the hint has been found
    }
    private void CreateAPCommands()
    {
        ConsoleScript.Commands.Add(new Command("aptoggledeathlink", "Toggles DeathLink for the current game session.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            if (APGui.DeathlinkEnabled)
            {
                APClientClass.Client.Session.Socket.SendPacket(new ConnectUpdatePacket()
                {
                    Tags = []
                });
                APGui.DeathlinkEnabled = false;
                try
                {
                    body = GameObject.Find("Experiment/Body");
                    Destroy(body.GetComponent<DeathlinkManager>());
                }
                catch
                {
                    // we're on the main menu. perfectly fine for a command like this
                }
                Console.LogToConsole("CUAP: DeathLink Disabled.");
            }
            else
            {
                APClientClass.Client.Session.Socket.SendPacket(new ConnectUpdatePacket()
                {
                    Tags = ["DeathLink"]
                });
                APGui.DeathlinkEnabled = true;
                try
                {
                    body = GameObject.Find("Experiment/Body");
                    Destroy(body.AddComponent<DeathlinkManager>());
                }
                catch
                {
                    // we're on the main menu. perfectly fine for a command like this
                }
                if (args[1] == "kill")
                {
                    DeathlinkManager.DeathlinkSeverity = true;
                }
                else if (args[1] == "limbdamage")
                {
                    DeathlinkManager.DeathlinkSeverity = false;
                }
                else
                {
                    DeathlinkManager.DeathlinkSeverity = true;
                    Console.LogToConsole("CUAP: Severity of " + args[1] + " is invalid. Defaulted to 'kill'");
                }
                Console.LogToConsole("CUAP: DeathLink Enabled.");
            }
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("severity", "How punishing DeathLink should be. Choices are 'kill' and 'limbdamage'")
        }));
        ConsoleScript.Commands.Add(new Command("apchat", "Sends a message to Archipelago chat.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception("No chat message was given.");
            }
            string chatMessage = string.Join(" ", args.Skip(1));
            APClientClass.Client.Say(chatMessage);
            Console.LogToConsole("CUAP: Chat message sent.");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("text", "Chat message to send.")
        }));
        ConsoleScript.Commands.Add(new Command("aphint", "Alias for !hint command.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                Client.Say("!hint");
                Console.LogToConsole("CUAP: Hint status requested.");
                return;
            }
            string itemName = string.Join(" ", args.Skip(1));
            Client.Say("!hint " + itemName);
            Console.LogToConsole("CUAP: Hint sent.");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("item", "Item to hint for. Leave blank to request hint status.")
        }));
        ConsoleScript.Commands.Add(new Command("aphintlocation", "Alias for !hint_location command.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception("No location was given to check.");
            }
            string locName = string.Join(" ", args.Skip(1));
            Client.Say("!hint_location " + locName);
            Console.LogToConsole("CUAP: Hint sent.");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("location", "Location to hint.")
        }));
        ConsoleScript.Commands.Add(new Command("aprelease", "Alias for !release command.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            Client.Say("!release");
            Console.LogToConsole("CUAP: Release requested.");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apcollect", "Alias for !collect command.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            Client.Say("!collect");
            Console.LogToConsole("CUAP: Collect requested.");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apcheat", "Alias for !getitem command.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception("No item was given to cheat in.");
            }
            string itemName = string.Join(" ", args.Skip(1));
            Client.Say("!getitem " + itemName);
            Console.LogToConsole("CUAP: Cheat requested.");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("item", "Item to request be cheated in.")
        }));
        ConsoleScript.Commands.Add(new Command("apalias", "Alias for !alias command.", delegate (string[] args)
        {
            if (APClientClass.Client is null)
            {
                throw new Exception("Archipelago isn't connected.");
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception("No name was given.");
            }
            string newName = string.Join(" ", args.Skip(1));
            Client.Say("!alias " + newName);
            Console.LogToConsole("CUAP: Alias change requested.");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("name", "Alias to change your slot name to.")
        }));
    }
}