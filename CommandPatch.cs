using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ArchipelagoSession Client;
    public static ConsoleScript Console;
    private GameObject body;
    private LogMessage LastGotMessage;
    private ItemSendLogMessage LastGotItemMessage;
    private HintItemSendLogMessage LastGotHintMessage;

    private void OnEnable()
    {
        Console = gameObject.GetComponent<ConsoleScript>();
        Startup.Logger.LogMessage("Console has been patched!");
        CreateAPCommands();
    }
    public void Subscribe()
    {
        Client = APClientClass.session;
        if (Client is not null)
        {
            Client.MessageLog.OnMessageReceived += message =>
            {
                switch (message)
                {
                    case HintItemSendLogMessage hint:
                        PrintHintJSON(hint);
                        break;

                    case ItemSendLogMessage item:
                        PrintItemJSON(item);
                        break;

                    default:
                        PrintPlainJSON(message);
                        break;
                }
            };
        }
    }
    private void PrintPlainJSON(LogMessage message)
    {
        if (LastGotMessage == message) { return; } // avoids spam
        Console.LogToConsole(message.ToString());
        LastGotMessage = message;
    }
    private void PrintItemJSON(ItemSendLogMessage message)
    {
        if (LastGotItemMessage == message) return;
        LastGotItemMessage = message;
        string constructedMessage = "";
        if (message.Receiver != message.Sender) // not a local item
        {
            constructedMessage = $"{message.Sender} sent {message.Item.ItemName} to {message.Receiver} ({message.Item.LocationName})";
        }
        else if (message.Sender == message.Receiver) // player found their own item
        {
            constructedMessage = message.IsReceiverTheActivePlayer 
                ? $"You found your {message.Item.ItemName} ({message.Item.LocationName})" // true (it is the casualties player)
                : $"{message.Receiver} found their {message.Item.ItemName} ({message.Item.LocationName})"; // false (it's someone else)
        }
        Console.LogToConsole(constructedMessage);
    }
    private void PrintHintJSON(HintItemSendLogMessage hint)
    {
        if (LastGotHintMessage == hint || hint.IsFound == true) return; // don't show found hints (less clutter)
        LastGotHintMessage = hint;
        Console.LogToConsole($"{hint.Receiver}'s {hint.Item.ItemName} is at {hint.Sender}'s {hint.Item.LocationName}.");
        APCanvas.EnqueueArchipelagoNotification($"{hint.Receiver}'s {hint.Item.ItemName} is at {hint.Sender}'s {hint.Item.LocationName}.",2);
    }
    private void CreateAPCommands()
    {
        ConsoleScript.Commands.Add(new Command("apdeathlink", "Toggles DeathLink for the current game session.", delegate (string[] args)
        {
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
            }
            if (APClientClass.dlService is null)
            {
                throw new Exception("DeathLinkService is null! This shouldn't happen, yell at me on Discord or Github if it does!");
            }
            if (APCanvas.DeathlinkEnabled)
            {
                APClientClass.dlService.DisableDeathLink();
                APCanvas.DeathlinkEnabled = false;
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
                APClientClass.dlService.EnableDeathLink();
                APCanvas.DeathlinkEnabled = true;
                try
                {
                    body = GameObject.Find("Experiment/Body");
                    body.AddComponent<DeathlinkManager>();
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
        }, new Dictionary<int, List<string>> {
        {
            0,
            new List<string> {"kill","limbdamage"}
        } }, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("severity", "How punishing DeathLink should be. Choices are 'kill' and 'limbdamage'")
        }));
        ConsoleScript.Commands.Add(new Command("apchat", "Sends a message to Archipelago chat.", delegate (string[] args)
        {
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception("No chat message was given.");
            }
            string chatMessage = string.Join(" ", args.Skip(1));
            Client.Say(chatMessage);
            Console.LogToConsole("CUAP: Chat message sent.");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("text", "Chat message to send.")
        }));
        ConsoleScript.Commands.Add(new Command("aphint", "Alias for !hint command.", delegate (string[] args)
        {
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
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
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
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
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
            }
            Client.Say("!release");
            Console.LogToConsole("CUAP: Release requested.");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apcollect", "Alias for !collect command.", delegate (string[] args)
        {
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
            }
            Client.Say("!collect");
            Console.LogToConsole("CUAP: Collect requested.");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apcheat", "Alias for !getitem command.", delegate (string[] args)
        {
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
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
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
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
        ConsoleScript.Commands.Add(new Command("apreportbug", "Opens CUAP's Github to report a bug.", delegate (string[] args)
        {
            Console.CheckArgumentCount(args, 1);
            if (args[1] == "true")
            {
                StartCoroutine(CommandPatch.CaptureScreenshot());
            }
            Application.OpenURL("https://github.com/Nichologeam/CUAP/issues/new?template=issuetemplate.md");
        }, new Dictionary<int, List<string>> {
        {
            0,
            new List<string> {"true","false"}
        } }, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("screenshot", "Whether to capture a screenshot to the CUAP folder.")
        }));
        ConsoleScript.Commands.Add(new Command("apresetantispam", "Clears CUAP's copies of sent locations. Use this to resend broken checks.", delegate (string[] args)
        {
            if (APCanvas.InGame)
            {
                GameObject.Find("World").GetComponent<DepthChecks>().AlreadySentChecks.Clear();
                // SkillChecks.alreadySentChecks.Clear();
                try { GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<Moodlesanity>().AlreadySentChecks.Clear(); }
                catch { }
                try { CraftingChecks.AlreadySentChecks.Clear(); }
                catch { }
            }
            Console.LogToConsole("CUAP: Data cleared.");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apsetskill", "Force set a skill to a certain level. Only works if Skillsanity is enabled.", delegate (string[] args)
        {
            Console.CheckArgumentCount(args, 2);
            if (args[1] == "STR")
            {
                APClientClass.MaxSTR = Convert.ToInt16(args[2]);
            }
            if (args[1] == "RES")
            {
                APClientClass.MaxRES = Convert.ToInt16(args[2]);
            }
            if (args[1] == "INT")
            {
                APClientClass.MaxINT = Convert.ToInt16(args[2]);
            }
        }, new Dictionary<int, List<string>> {
        {
            0,
            new List<string> {"STR","RES","INT"}
        } }, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("skill", "Which skill to change the level of."),
            new ValueTuple<string, string>("level", "What level to set the skill to.")
        }));
    }
    public static IEnumerator CaptureScreenshot()
    {   // this is incredibly dumb, but it works
        string CUAPFolder = Path.Combine(BepInEx.Paths.PluginPath, "CUAP\\"); // Get CUAP folder
        Console.gameObject.GetComponentInChildren<Canvas>().enabled = false; // hide the console
        yield return 0; // wait a frame so it's gone in render
        ScreenCapture.CaptureScreenshot(CUAPFolder + "apreportbug.png"); // Put screenshot in CUAP folder
        yield return 0; // wait ANOTHER frame for the screenshot to actually get taken
        Console.gameObject.GetComponentInChildren<Canvas>().enabled = true; // then finally put the console back
        Console.LogToConsole("CUAP: Screenshot saved to " + CUAPFolder);
    }
}