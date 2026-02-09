using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx;
using KrokoshaCasualtiesMP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    private void Awake()
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
            Client.MessageLog.OnMessageReceived += message => ThreadingHelper.Instance.StartSyncInvoke(() =>
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
            });
        }
    }
    private void PrintPlainJSON(LogMessage message)
    {
        if (LastGotMessage == message) { return; } // avoids spam
        APClientClass.sendServerMessage.Invoke(null, ["Archipelago", message.ToString()]);
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
        APClientClass.sendServerMessage.Invoke(null, ["Archipelago", constructedMessage]);
    }
    private void PrintHintJSON(HintItemSendLogMessage hint)
    {
        if (LastGotHintMessage == hint || hint.IsFound == true) return; // don't show found hints (less clutter)
        LastGotHintMessage = hint;
        APClientClass.sendServerMessage.Invoke(null, ["Archipelago", $"{hint.Receiver}'s {hint.Item.ItemName} is at {hint.Sender}'s {hint.Item.LocationName}."]);
        APCanvas.EnqueueArchipelagoNotification($"{hint.Receiver}'s {hint.Item.ItemName} is at {hint.Sender}'s {hint.Item.LocationName}.",2);
    }
    private void CreateAPCommands()
    {
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apdeathlink", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            if (APClientClass.dlService is null)
            {
                return "DeathLinkService is null! This shouldn't happen, yell at me on Discord or Github if it does!";
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
                return "CUAP: DeathLink Disabled.";
            }
            else
            {
                if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
                {
                    return "No severity was given. Choices are 'kill' or 'limbdamage'";
                }
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
                if (splitted[1] == "kill")
                {
                    DeathlinkManager.DeathlinkSeverity = true;
                }
                else if (splitted[1] == "limbdamage")
                {
                    DeathlinkManager.DeathlinkSeverity = false;
                }
                else
                {
                    DeathlinkManager.DeathlinkSeverity = true;
                    return $"CUAP: Severity of {splitted[1]} is invalid. Defaulted to 'kill'";
                }
                return "CUAP: DeathLink Enabled.";
            }
        }, "Archipelago: Toggles DeathLink. Choices are 'kill' and 'limbdamage'");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apchat", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                return "No chat message was given.";
            }
            string chatMessage = string.Join(" ", splitted.Skip(1));
            Client.Say(chatMessage);
            return "CUAP: Chat message sent.";
        }, "Archipelago: Sends a message to Archipelago chat.");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("aphint", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                Client.Say("!hint");
                return "CUAP: Hint status requested.";
            }
            string itemName = string.Join(" ", splitted.Skip(1));
            Client.Say("!hint " + itemName);
            return "CUAP: Hint sent.";
        }, "Archipealgo: Alias for !hint command");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("aphintlocation", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                return "No location was given to check.";
            }
            string locName = string.Join(" ", splitted.Skip(1));
            Client.Say("!hint_location " + locName);
            return "CUAP: Hint sent.";
        }, "Archipelago: Alias for !hint_location");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("aprelease", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            Client.Say("!release");
            return "CUAP: Release requested.";
        }, "Archipelago: Alias for !release");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apcollect", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            Client.Say("!collect");
            return "CUAP: Collect requested.";
        }, "Archipelago: Alias for !collect command");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apcheat", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                return "No item was given to cheat in.";
            }
            string itemName = string.Join(" ", splitted.Skip(1));
            Client.Say("!getitem " + itemName);
            return "CUAP: Cheat requested.";
        }, "Archipelago: Alias for !getitem");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apalias", delegate (string inputtext, List<string> splitted)
        {
            if (APClientClass.session is null)
            {
                return "Archipelago isn't connected or session was closed. You must be connected to run this command.";
            }
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                return "No name was given.";
            }
            string newName = string.Join(" ", splitted.Skip(1));
            Client.Say("!alias " + newName);
            return "CUAP: Alias change requested.";
        }, "Archipealgo: Alias for !alias");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apreportbug", delegate (string inputtext, List<string> splitted)
        {
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                return "Whether or not to take a screenshot wasn't specified. Please specify 'true' or 'false'";
            }
            if (splitted[1] == "true")
            {
                StartCoroutine(CommandPatch.CaptureScreenshot());
            }
            Application.OpenURL("https://github.com/Nichologeam/CUAP/issues/new?template=issuetemplate.md");
            return "CUAP: Github opened";
        }, "Archipelago: Report a bug. Be sure to specify your version as the Casualties: Together version!");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apresetantispam", delegate (string inputtext, List<string> splitted)
        {
            if (APCanvas.InGame)
            {
                GameObject.Find("World").GetComponent<DepthChecks>().AlreadySentChecks.Clear();
                try { GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<Moodlesanity>().AlreadySentChecks.Clear(); }
                catch { }
                try { CraftingChecks.AlreadySentChecks.Clear(); }
                catch { }
            }
            return "CUAP: Data cleared. Run apreportbug if the issue persists.";
        }, "Archipealgo: Clears local copies of sent locations.");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apsetskill", delegate (string inputtext, List<string> splitted)
        {
            if (splitted.Count < 2 || string.IsNullOrWhiteSpace(splitted[1]))
            {
                return "No skill was given. Choices are 'STR', 'RES', or 'INT'";
            }
            if (splitted[1] == "STR")
            {
                APClientClass.MaxSTR = Convert.ToInt16(splitted[2]);
            }
            if (splitted[1] == "RES")
            {
                APClientClass.MaxRES = Convert.ToInt16(splitted[2]);
            }
            if (splitted[1] == "INT")
            {
                APClientClass.MaxINT = Convert.ToInt16(splitted[2]);
            }
            return "CUAP: Skills applied.";
        }, "Archipelago: Force set a Skillsanity skill to a certain level");
        ConsoleScript_Added_KrokoshaMultiplayerCommands_Patch.AddCommand("apfixquests", delegate (string inputtext, List<string> splitted)
        {
            if (APCanvas.InGame)
            {
                Moodlesanity.RefreshMaxQuests(false);
            }
            return "CUAP: Quests refreshed. Run apreportbug if the issue persists.";
        }, "Archipelago: Force refresh the Moodlesanity Questboard");
    }
    public static IEnumerator CaptureScreenshot()
    {   // this is incredibly dumb, but it works
        string CUAPFolder = Path.Combine(BepInEx.Paths.PluginPath, "CUAP\\"); // Get CUAP folder
        Console.gameObject.GetComponentInChildren<Canvas>().enabled = false; // hide the console
        yield return 0; // wait a frame so it's gone in render
        ScreenCapture.CaptureScreenshot(CUAPFolder + "apreportbug.png"); // Put screenshot in CUAP folder
        yield return 0; // wait ANOTHER frame for the screenshot to actually get taken
        Console.gameObject.GetComponentInChildren<Canvas>().enabled = true; // then finally put the console back
    }
}