using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using CreepyUtil.Archipelago;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ApClient Client;
    public static ConsoleScript Console;
    private GameObject body;
    private string LastFrameText;
    private string ChatMessage;
    private string LastGotMessage;
    private JsonMessagePart[] LastGotItemMessage;

    private void OnEnable()
    {
        Console = gameObject.GetComponent<ConsoleScript>();
        Startup.Logger.LogMessage("Console has been patched!");
        CreateAPCommands();
    }
    private void Update()
    {
        if (Client is null)
        {
            try
            {
                Client = APClientClass.Client;
            }
            catch
            {
                // not connected
            }
        }
        if (Input.GetKeyDown(KeyCode.Return)) // Console message may have been sent
        {
            try
            {
                if (LastFrameText.Substring(0, 4) != "talk")
                {
                    // Was the command 'talk'? If not, don't continue
                    return;
                }
                else
                {
                    // Split off the 'talk' command, then send the rest to the server
                    ChatMessage = LastFrameText.Substring(5);
                    Client.Say(ChatMessage);
                }
            }
            catch
            {
                return; // Message was less than 4 characters. Doing this to prevent errors when sending blank or short messages.
            }
        }
        LastFrameText = Console.input.text; // enter was not pressed on this frame
        if (Client is not null)
        {
            Client.OnServerMessagePacketReceived += PrintPlainJSON;
            Client.OnItemLogPacketReceived += PrintItemJSON;
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
        if (LastGotItemMessage == combinedText) { return; }
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
    }
}