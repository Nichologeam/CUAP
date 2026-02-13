using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ArchipelagoSession Client;
    public static ConsoleScript Console;
    private GameObject body;
    private LogMessage LastGotMessage;
    private ItemSendLogMessage LastGotItemMessage;
    private HintItemSendLogMessage LastGotHintMessage;
    private static MethodInfo LogToConsole;
    private MethodInfo CheckArgumentCount;

    private void OnEnable()
    {
        Console = gameObject.GetComponent<ConsoleScript>();
        LogToConsole = typeof(ConsoleScript).GetMethod("LogToConsole", BindingFlags.NonPublic | BindingFlags.Instance);
        CheckArgumentCount = typeof(ConsoleScript).GetMethod("CheckArgumentCount", BindingFlags.NonPublic | BindingFlags.Instance);
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
        LogToConsole.Invoke(Console, [message.ToString()]);
        LastGotMessage = message;
    }
    private void PrintItemJSON(ItemSendLogMessage message)
    {
        if (LastGotItemMessage == message) return;
        LastGotItemMessage = message;
        string constructedMessage = "";
        var itemColor = "";
        switch (message.Item.Flags)
        {
            case Archipelago.MultiClient.Net.Enums.ItemFlags.None: // filler/unspecified item
                itemColor = "#00EEEE"; // cyan
                break;
            case Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement: // progression
                itemColor = "#AF99EF"; // plum
                break;
            case Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude: // useful
                itemColor = "#6D8BE8"; // slateblue
                break;
            case Archipelago.MultiClient.Net.Enums.ItemFlags.Trap: // trap
                itemColor = "#FA8072"; // salmon
                break;

        }
        if (message.Receiver != message.Sender) // not a local item
        {
            if (message.IsSenderTheActivePlayer)
            {
                constructedMessage = $"<color=#EE00EE>You</color> sent <color={itemColor}>{message.Item.ItemName}</color> to <color=#FAFAD2>{message.Receiver}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)";
            }
            else if (message.IsReceiverTheActivePlayer)
            {
                constructedMessage = $"<color=#EE00EE>{message.Item.Player}</color> sent <color={itemColor}>{message.Item.ItemName}</color> to <color=#EE00EE>You</color> (<color=#00FF7F>{message.Item.LocationName}</color>)";
            }
            
        }
        else if (message.Sender == message.Receiver) // player found their own item
        {
            constructedMessage = message.IsReceiverTheActivePlayer 
                ? $"<color=#EE00EE>You</color> found your <color={itemColor}>{message.Item.ItemName}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)" // true (it is the casualties player)
                : $"<color=#FAFAD2>{message.Receiver}</color> found their <color={itemColor}>{message.Item.ItemName}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)"; // false (it's someone else)
        }
        LogToConsole.Invoke(Console, [constructedMessage]);
    }
    private void PrintHintJSON(HintItemSendLogMessage hint)
    {
        if (LastGotHintMessage == hint || hint.IsFound == true) return; // don't show found hints (less clutter)
        LastGotHintMessage = hint;
        LogToConsole.Invoke(Console, [$"{hint.Receiver}'s {hint.Item.ItemName} is at {hint.Sender}'s {hint.Item.LocationName}."]);
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
                LogToConsole.Invoke(Console, ["CUAP: Deathlink Disabled"]);
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
                    LogToConsole.Invoke(Console, [$"CUAP: Severity of {args[1]} is invalid. Defaulted to 'kill'"]);
                }
                LogToConsole.Invoke(Console, ["CUAP: DeathLink Enabled"]);
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
            LogToConsole.Invoke(Console, ["CUAP: Chat message sent"]);
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
                LogToConsole.Invoke(Console, ["CUAP: Hint status requested"]);
                return;
            }
            string itemName = string.Join(" ", args.Skip(1));
            Client.Say("!hint " + itemName);
            LogToConsole.Invoke(Console, ["CUAP: Hint sent"]);
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
            LogToConsole.Invoke(Console, ["CUAP: Hint sent"]);
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
            LogToConsole.Invoke(Console, ["CUAP: Release requested"]);
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apcollect", "Alias for !collect command.", delegate (string[] args)
        {
            if (APClientClass.session is null)
            {
                throw new Exception("Archipelago isn't connected or session was closed. You must be connected to run this command.");
            }
            Client.Say("!collect");
            LogToConsole.Invoke(Console, ["CUAP: Collect requested"]);
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
            LogToConsole.Invoke(Console, ["CUAP: Cheat requested"]);
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
            LogToConsole.Invoke(Console, ["CUAP: Alias change requested"]);
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>("name", "Alias to change your slot name to.")
        }));
        ConsoleScript.Commands.Add(new Command("apreportbug", "Opens CUAP's Github to report a bug.", delegate (string[] args)
        {
            CheckArgumentCount.Invoke(Console, [args, 1]);
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
                try { GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<Moodlesanity>().AlreadySentChecks.Clear(); }
                catch { }
                try { CraftingChecks.AlreadySentChecks.Clear(); }
                catch { }
            }
            LogToConsole.Invoke(Console, ["CUAP: Data cleared. Run apreportbug if the issue persists."]);
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apsetskill", "Force set a skill to a certain level. Only works if Skillsanity is enabled.", delegate (string[] args)
        {
            CheckArgumentCount.Invoke(Console, [args, 2]);
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
        ConsoleScript.Commands.Add(new Command("apfixquests", "Forces the questboard to refresh sent checks. Use this if a quest didn't send properly.", delegate (string[] args)
        {
            if (APCanvas.InGame)
            {
                Moodlesanity.RefreshMaxQuests(false);
            }
            LogToConsole.Invoke(Console, ["CUAP: Quests refreshed. Run apreportbug if the issue persists."]);
        }, null, Array.Empty<ValueTuple<string, string>>()));
    }
    public static IEnumerator CaptureScreenshot()
    {   // this is incredibly dumb, but it works
        string CUAPFolder = Path.Combine(BepInEx.Paths.PluginPath, "CUAP\\"); // Get CUAP folder
        Console.gameObject.GetComponentInChildren<Canvas>().enabled = false; // hide the console
        yield return 0; // wait a frame so it's gone in render
        ScreenCapture.CaptureScreenshot(CUAPFolder + "apreportbug.png"); // Put screenshot in CUAP folder
        yield return 0; // wait ANOTHER frame for the screenshot to actually get taken
        Console.gameObject.GetComponentInChildren<Canvas>().enabled = true; // then finally put the console back
        LogToConsole.Invoke(Console, [$"CUAP: Screenshot saved to {CUAPFolder}"]);
    }
}