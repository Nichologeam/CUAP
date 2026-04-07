using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ArchipelagoSession Client;
    public static ConsoleScript Console;
    private GameObject body;
    private LogMessage LastGotMessage;
    private ItemSendLogMessage LastGotItemMessage;
    private HintItemSendLogMessage LastGotHintMessage;
    private MethodInfo CheckArgumentCount;
    private bool raceMode;

    private void OnEnable()
    {
        Console = gameObject.GetComponent<ConsoleScript>();
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

                    case CountdownLogMessage countdown:
                        var textbox = DepthChecks.instance.DisplayText;
                        if (countdown.RemainingSeconds > 0)
                        {
                            if (APCanvas.InGame)
                            {
                                textbox.text = APCanvas.coloredAPText + APLocale.Get("countdown", APLocale.APLanguageType.Messages) + countdown.RemainingSeconds;
                            }
                            LogToConsole(APLocale.Get("countdown", APLocale.APLanguageType.Messages) + countdown.RemainingSeconds);
                        }
                        else // countdown is 0, which means server just said GO (but the server just sends '0')
                        {
                            if (APCanvas.InGame)
                            {
                                textbox.text = APCanvas.coloredAPText + APLocale.Get("countdownOver", APLocale.APLanguageType.Messages);
                            }
                            LogToConsole(APLocale.Get("countdownOver", APLocale.APLanguageType.Messages));
                            StartCoroutine(ClearText());
                        }
                        break;

                    case JoinLogMessage join:
                        if (join.Player.Game == "Archipelago") // unsure if this will actually trigger at runtime...
                        {
                            APCanvas.EnqueueArchipelagoNotification($"<color=#00FF00>{join.Player.Alias}{APLocale.Get("joinSpectator", APLocale.APLanguageType.Messages)}</color>",0);
                        }
                        string verb;
                        if (join.Tags.Contains("TextOnly"))
                        {
                            verb = APLocale.Get("textClient", APLocale.APLanguageType.Messages);
                        }
                        else if (join.Tags.Contains("Tracker") || join.Tags.Contains("PopTracker"))
                        {
                            verb = APLocale.Get("tracker", APLocale.APLanguageType.Messages);
                        }
                        else if (join.Tags.Contains("HintGame"))
                        {
                            verb = APLocale.Get("hintGame", APLocale.APLanguageType.Messages);
                        }
                        else
                        {
                            verb = APLocale.Get("playing", APLocale.APLanguageType.Messages);
                        }
                        APCanvas.EnqueueArchipelagoNotification($"<color=#00FF00>{join.Player.Alias}{APLocale.Get("joinGeneric", APLocale.APLanguageType.Messages)}</color> {verb} {join.Player.Game}.<br>[{string.Join(", ", join.Tags)}]",0);
                        break;

                    case LeaveLogMessage leave:
                        APCanvas.EnqueueArchipelagoNotification($"<color=#FF0000>{leave.Player.Alias}{APLocale.Get("leave", APLocale.APLanguageType.Messages)}</color>",0);
                        break;

                    case GoalLogMessage goal:
                        APCanvas.EnqueueArchipelagoNotification($"{goal.Player.Alias}{APLocale.Get("goal", APLocale.APLanguageType.Messages)}",0);
                        break;

                    default:
                        PrintPlainJSON(message);
                        break;
                }
            };
            CheckRaceMode();
        }
    }
    private void PrintPlainJSON(LogMessage message)
    {
        if (LastGotMessage == message) { return; } // avoids spam
        LogToConsole(message.ToString());
        APCanvas.EnqueueArchipelagoNotification(message.ToString(),0);
        LastGotMessage = message;
    }
    private void PrintItemJSON(ItemSendLogMessage message)
    {
        if (LastGotItemMessage == message) return;
        LastGotItemMessage = message;
        if (message.Item.LocationName == "Cheat Console") return; // cheated in item.
        string constructedMessage = "";
        var itemColor = ItemDataToPriorityColor(message.Item.Flags);
        if (message.Receiver != message.Sender) // not a local item
        {
            if (message.IsSenderTheActivePlayer)
            {
                constructedMessage = $"<color=#EE00EE>{APLocale.Get("you", APLocale.APLanguageType.Messages)}</color> {APLocale.Get("sent", APLocale.APLanguageType.Messages)} <color={itemColor}>{message.Item.ItemName}</color> {APLocale.Get("to", APLocale.APLanguageType.Messages)} <color=#FAFAD2>{message.Receiver}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)";
            }
            else if (message.IsReceiverTheActivePlayer)
            {
                constructedMessage = $"<color=#FAFAD2>{message.Item.Player}</color> {APLocale.Get("sent", APLocale.APLanguageType.Messages)} <color={itemColor}>{message.Item.ItemName}</color> {APLocale.Get("to", APLocale.APLanguageType.Messages)} <color=#EE00EE>{APLocale.Get("you", APLocale.APLanguageType.Messages)}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)";
            }
            else // two unrelated players, neither casualites
            {
                constructedMessage = $"<color=#FAFAD2>{message.Item.Player}</color> {APLocale.Get("sent", APLocale.APLanguageType.Messages)} <color={itemColor}>{message.Item.ItemName}</color> {APLocale.Get("to", APLocale.APLanguageType.Messages)} <color=#FAFAD2>{message.Receiver}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)";
            }
        }
        else if (message.Sender == message.Receiver) // player found their own item
        {
            constructedMessage = message.IsReceiverTheActivePlayer 
                ? $"<color=#EE00EE>{APLocale.Get("you", APLocale.APLanguageType.Messages)}</color> {APLocale.Get("foundLocal", APLocale.APLanguageType.Messages)} <color={itemColor}>{message.Item.ItemName}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)" // true (it is the casualties player)
                : $"<color=#FAFAD2>{message.Receiver}</color> {APLocale.Get("foundRemote", APLocale.APLanguageType.Messages)} <color={itemColor}>{message.Item.ItemName}</color> (<color=#00FF7F>{message.Item.LocationName}</color>)"; // false (it's someone else)
        }
        LogToConsole(constructedMessage);
    }
    private void PrintHintJSON(HintItemSendLogMessage hint)
    {
        if (LastGotHintMessage == hint || hint.IsFound == true) return; // don't show found hints (less clutter)
        LastGotHintMessage = hint;
        var itemColor = ItemDataToPriorityColor(hint.Item.Flags);
        string constructedMessage = "";
        if (hint.IsReceiverTheActivePlayer)
        {
            if (hint.IsSenderTheActivePlayer) // casualties item in casualties world (not barbie)
            {
                constructedMessage = $"<color=#EE00EE>{APLocale.Get("your", APLocale.APLanguageType.Messages)}</color> <color={itemColor}>{hint.Item.ItemName}</color> {APLocale.Get("at", APLocale.APLanguageType.Messages)} <color=#00FF7F>{hint.Item.LocationName}</color>.";
            }
            else // casualties item in other world
            {
                constructedMessage = $"<color=#EE00EE>{APLocale.Get("your", APLocale.APLanguageType.Messages)}</color> <color={itemColor}>{hint.Item.ItemName}</color> {APLocale.Get("at", APLocale.APLanguageType.Messages)} <color=#FAFAD2>{hint.Sender}</color>'s <color=#00FF7F>{hint.Item.LocationName}</color>.";
            }
        }
        else if (hint.IsSenderTheActivePlayer) // other item in casualties world
        {
            constructedMessage = $"<color=#FAFAD2>{hint.Receiver}</color>'s <color={itemColor}>{hint.Item.ItemName}</color> {APLocale.Get("at", APLocale.APLanguageType.Messages)} <color=#00FF7F>{hint.Item.LocationName}</color>.";
        }
        LogToConsole(constructedMessage);
        APCanvas.EnqueueArchipelagoNotification(constructedMessage,2);
    }
    private void CreateAPCommands()
    {
        ConsoleScript.Commands.Add(new Command("apdeathlink", APLocale.Get("dlDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            if (APClientClass.dlService is null)
            {
                throw new Exception(APLocale.Get("dlsNull", APLocale.APLanguageType.Errors));
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
                    string msg = APLocale.Get("dlInvalid", APLocale.APLanguageType.Commands);
                    msg = msg.Replace("<sev>", args[1]);
                    LogToConsole($"<color=#FF0000>{msg}</color>");
                }
            }
        }, new Dictionary<int, List<string>> {
        {
            0,
            new List<string> {"kill", "limbdamage"}
        } }, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("dlSeverity", APLocale.APLanguageType.Commands), APLocale.Get("dlSeverityDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("apchat", APLocale.Get("chatDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception(APLocale.Get("chatEmpty", APLocale.APLanguageType.Commands));
            }
            string chatMessage = string.Join(" ", args.Skip(1));
            Client.Say(chatMessage);
            if (APCanvas.InGame)
            {
                GameObject.Find("Experiment/Body").GetComponent<Talker>().Talk(chatMessage, force: true);
            }
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("chatText", APLocale.APLanguageType.Commands), APLocale.Get("chatTextDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("aphint", APLocale.Get("hintDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                Client.Say("!hint");
                return;
            }
            string itemName = string.Join(" ", args.Skip(1));
            Client.Say($"!hint {itemName}");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("hintItem", APLocale.APLanguageType.Commands), APLocale.Get("hintItemDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("aphintlocation", APLocale.Get("hintLocDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception(APLocale.Get("hintLocEmpty", APLocale.APLanguageType.Commands));
            }
            string locName = string.Join(" ", args.Skip(1));
            Client.Say($"!hint_location {locName}");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("hintLocLocation", APLocale.APLanguageType.Commands), APLocale.Get("hintLocLocationDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("aprelease", APLocale.Get("releaseDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            Client.Say("!release");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apcollect", APLocale.Get("collectDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            Client.Say("!collect");
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apalias", APLocale.Get("aliasDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception(APLocale.Get("aliasEmpty", APLocale.APLanguageType.Commands));
            }
            string newName = string.Join(" ", args.Skip(1));
            Client.Say($"!alias {newName}");
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("aliasName", APLocale.APLanguageType.Commands), APLocale.Get("aliasNameDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("apreportbug", APLocale.Get("reportBugDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
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
            new ValueTuple<string, string>(APLocale.Get("reportBugScreenshot", APLocale.APLanguageType.Commands), APLocale.Get("reportBugScreenshotDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("apresetantispam", APLocale.Get("resetSpamDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (APCanvas.InGame)
            {
                GameObject.Find("World").GetComponent<DepthChecks>().AlreadySentChecks.Clear();
                try { GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<Moodlesanity>().AlreadySentChecks.Clear(); }
                catch { }
                try { CraftingChecks.AlreadySentChecks.Clear(); CraftsanitySender.alreadySentChecks.Clear(); }
                catch { }
            }
            LogToConsole(APLocale.Get("resetSpamConfirm", APLocale.APLanguageType.Commands));
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("apfixquests", APLocale.Get("questsDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (APCanvas.InGame)
            {
                Moodlesanity.RefreshMaxQuests(false);
            }
            LogToConsole(APLocale.Get("questsConfirm", APLocale.APLanguageType.Commands));
        }, null, Array.Empty<ValueTuple<string, string>>()));
        ConsoleScript.Commands.Add(new Command("aplang", APLocale.Get("langDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            CheckArgumentCount.Invoke(Console, [args, 1]);
            APLocale.LoadLang(args[1]);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // reload current scene
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("langFile", APLocale.APLanguageType.Commands), APLocale.Get("langFileDesc", APLocale.APLanguageType.Commands))
        }));
        if (raceMode) return; // commands after this can be used to cheat. disable them in race mode
        ConsoleScript.Commands.Add(new Command("apcheat", APLocale.Get("cheatDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
        {
            if (!APClientClass.IsConnected())
            {
                throw new Exception(APLocale.Get("cmdNotConnected", APLocale.APLanguageType.Errors));
            }
            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
            {
                throw new Exception(APLocale.Get("cheatEmpty", APLocale.APLanguageType.Commands));
            }
            string itemName = string.Join(" ", args.Skip(1));
            Client.Say("!getitem " + itemName);
        }, null, new ValueTuple<string, string>[]
        {
            new ValueTuple<string, string>(APLocale.Get("cheatItem", APLocale.APLanguageType.Commands), APLocale.Get("cheatItemDesc", APLocale.APLanguageType.Commands))
        }));
        ConsoleScript.Commands.Add(new Command("apsetskill", APLocale.Get("setSkillDesc", APLocale.APLanguageType.Commands), delegate (string[] args)
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
            new ValueTuple<string, string>(APLocale.Get("setSkillSkill", APLocale.APLanguageType.Commands), APLocale.Get("setSkillSkillDesc", APLocale.APLanguageType.Commands)),
            new ValueTuple<string, string>(APLocale.Get("setSkillLevel", APLocale.APLanguageType.Commands), APLocale.Get("setSkillLevelDesc", APLocale.APLanguageType.Commands))
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
        LogToConsole(APLocale.Get("screenshotNotif", APLocale.APLanguageType.Commands) + CUAPFolder);
    }

    public static void LogToConsole(string text) // copying the basegame LogToConsole because V5 made it a private method that sometimes breaks when called
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
        var logs = ConsoleScript.instance.logs;
        logs.Add($"<color=#FFFFFF>[<alpha=#55>{timeSpan.ToString("mm\\:ss")}<alpha=#FF>] [{APCanvas.coloredAPText}]<color=#FFFFFF> {text}");
        if (logs.Count > 100)
        {
            logs.RemoveAt(0);
        }
        if (ConsoleScript.instance.active)
        {
            ConsoleScript.instance.logText.text = string.Join("\n", logs);
        }
    }

    public static string ItemDataToPriorityColor(ItemFlags flags)
    {
        var itemColor = "#00EEEE"; // default to filler color (cyan)
        // a switch statement would be better here, but it would't support items with more than one tag (rare, but they exist)
        if (flags.HasFlag(ItemFlags.Trap))
        {
            itemColor = "#FA8072"; // salmon (trap)
        }
        else if (flags.HasFlag(ItemFlags.Advancement))
        {
            itemColor = "#AF99EF"; // plum (progression)
        }
        else if (flags.HasFlag(ItemFlags.NeverExclude))
        {
            itemColor = "#6D8BE8"; // slateblue (useful)
        }
        return itemColor;
    }

    private IEnumerator ClearText()
    {
        yield return new WaitForSecondsRealtime(5);
        DepthChecks.instance.DisplayText.text = "";
    }

    private async void CheckRaceMode()
    {
        raceMode = await Client.DataStorage.GetRaceModeAsync();
        if (raceMode)
        {
            Startup.Logger.LogWarning("Archipelago server is in Race Mode! Disabling basegame console commands.");
            ConsoleScript.Commands.Clear(); // remove all basegame commands
            CreateAPCommands(); // re-add ap commands
            var playerDetails = typeof(ConsoleScript).GetField("playerDetailsRegistered", BindingFlags.NonPublic | BindingFlags.Instance);
            playerDetails.SetValue(ConsoleScript.instance, true); // This is something that runs once a session that gets information about the player for command autofill.
            // Since we disable all of the commands in race mode, this breaks and causes NREs trying to update commands that don't exist anymore.
            // So we force it to say it has already happened (a lie, but it doesn't matter)
            var spawnEntities = typeof(ConsoleScript).GetField("registeredSpawnEntities", BindingFlags.NonPublic | BindingFlags.Instance);
            spawnEntities.SetValue(ConsoleScript.instance, true); // Same thing, but for autofilling the `spawn` command.
        }
    }
}