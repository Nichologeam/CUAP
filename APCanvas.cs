using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using static CUAP.APClientClass;

namespace CUAP;

public class APCanvas : MonoBehaviour
{
    public static bool ShowGUI = true;
    public GameObject ConnectionBackground;
    public GameObject ConnectedBackground;
    public TMP_InputField Ipporttext;
    public TMP_InputField Password;
    public TMP_InputField Slot;
    public static bool DeathlinkEnabled = false;
    public static TMP_Text GUIDescription;
    public static bool DisplayingMessage;
    public static bool InGame
    {
        get
        {
            var camera = GameObject.Find("Main Camera");
            return camera != null && camera.GetComponent<PlayerCamera>() != null;
        }
    }

    private void Awake()
    {
        GUIDescription = GameObject.Find("APCanvas/Canvas/Connected Background/Status").GetComponent<TMP_Text>();
        if (!File.Exists("ApConnection.txt")) return; // Read saved slot information from file
        var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
        Ipporttext.text = fileText[0];
        Password.text = fileText[1];
        Slot.text = fileText[2];
    }

    void OnGUI()
    {
        if (!ShowGUI)
        {
            ConnectedBackground.SetActive(false);
            ConnectedBackground.SetActive(false);
        }
        if (!IsConnected())
        {
            ConnectionBackground.SetActive(true);
            ConnectedBackground.SetActive(false);
        }
        else
        {
            ConnectedBackground.SetActive(true);
            ConnectionBackground.SetActive(false);
            try
            {
                if (InGame)
                {
                    ConnectedBackground.transform.position = new(0,0);
                }
                else
                {
                    ConnectedBackground.transform.position = new(0,-100);
                }
            }
            catch
            {
                ConnectedBackground.transform.position = new(0, -100);
            }
        }
    }

    void OnConnectPressed()
    {
        var ipPortSplit = Ipporttext.text.Split(':');
        if (!int.TryParse(ipPortSplit[1], out var port))
        {
            DisplayArchipelagoNotification($"[{ipPortSplit[1]}] is not a valid port",3);
            return;
        }
        var error = TryConnect(port, Slot.text, ipPortSplit[0], Password.text);
        if (error is not null)
        {
            DisplayArchipelagoNotification("Connection error: " + string.Join("\n", error),3);
            return;
        }
        File.WriteAllText("ApConnection.txt", $"{Ipporttext}\n{Password}\n{Slot}");
    }

    private void Update() => APClientClass.Update();

    public static void UpdateGUIDescriptions()
    {
        if (selectedGoal == 1)
        {
            var maxDepth = (300 * DepthExtendersRecieved) + 300;
            GUIDescription.text =
                """
                Goal: Reach Depth
                Depth Extenders: <de>
                Max Depth: <md>
                """;
            GUIDescription.text = GUIDescription.text.Replace("<de>", DepthExtendersRecieved.ToString());
            GUIDescription.text = GUIDescription.text.Replace("<md>", maxDepth.ToString());
        }
        else if (selectedGoal == 2)
        {
            GUIDescription.text =
                """
                Goal: Escape Overgrown
                Layer Unlocks: <lu>
                Deepest Layer: <dl>
                """;
            GUIDescription.text = GUIDescription.text.Replace("<lu>", DepthExtendersRecieved.ToString());
            GUIDescription.text = GUIDescription.text.Replace("<dl>", LayerLocker.LayerIDToName[DepthExtendersRecieved]).Replace(" Unlock", "");
        }
        else if (selectedGoal == 3)
        {
            GUIDescription.text =
                """
                Goal: Defeat Elder
                Overgrown: <ou>
                """;
            GUIDescription.text = GUIDescription.text.Replace("<ou>", LayerUnlockDictionary.Contains("Overgrown Depths Unlock") ? "Unlocked" : "Locked");
        }
        else if (selectedGoal == 4)
        {
            GUIDescription.text =
                """
                Goal: Craftsanity
                Unlocked: <ru>
                Crafted: <rc>
                """;
            GUIDescription.text = GUIDescription.text.Replace("<ru>", Recipes.recipes.Count + "/112");
            GUIDescription.text = GUIDescription.text.Replace("<rc>", CraftingChecks.CraftedRecipes.ToString() + "/112");
        }
    }
    public static IEnumerator DisplayArchipelagoNotification(string text, int severity)
    {
        // display the given text in a popup. higher severity means more interruptive of gameplay.
        // items are lowest severity (1), hints are higher (2), and errors are highest (3).
        if (severity == 1)
        {
            var ItemNotif = GameObject.Find("APCanvas/Canvas/Item Notification");
            var ItemText = GameObject.Find("APCanvas/Canvas/Item Notification/Notification Message").GetComponent<TMP_Text>();
            ItemText.text = text;
            ItemNotif.SetActive(true);
            Sound.Play("laser", Vector2.zero, true, false, null, 1, 1, false, false);
            yield return new WaitForSecondsRealtime(3);
            ItemNotif.SetActive(false);
        }
        else if (severity == 2)
        {
            var HintNotif = GameObject.Find("APCanvas/Canvas/Hint Notification");
            var HintText = GameObject.Find("APCanvas/Canvas/Hint Notification/Notification Message").GetComponent<TMP_Text>();
            HintText.text = text;
            HintNotif.SetActive(true);
            Sound.Play("shuttleNotice", Vector2.zero, true, false, null, 1, 1, false, false);
            yield return new WaitForSecondsRealtime(5);
            HintNotif.SetActive(false);
        }
        else if (severity == 3)
        {
            var ErrorNotif = GameObject.Find("APCanvas/Canvas/Error Notification");
            var ErrorText = GameObject.Find("APCanvas/Canvas/Error Notification/Notification Message").GetComponent<TMP_Text>();
            ErrorText.text = text;
            ErrorNotif.SetActive(true);
            yield return new WaitForSecondsRealtime(5);
            ErrorNotif.SetActive(false);
        }
    }
}