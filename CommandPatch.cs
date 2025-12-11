using Archipelago.MultiClient.Net.Packets;
using CreepyUtil.Archipelago;
using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ApClient Client;
    private ConsoleScript Console;
    private GameObject body;
    private string LastFrameText;
    private string ChatMessage;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Console = gameObject.GetComponent<ConsoleScript>();
        Startup.Logger.LogMessage("Console has been patched!");
        CreateAPCommands();
    }
    private void Update()
    {
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