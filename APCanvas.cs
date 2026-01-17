using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    public Button ConnectButton;
    public static bool DeathlinkEnabled = false;
    public static TMP_Text Status;
    public static bool DisplayingMessage;
    public static bool InGame
    {
        get
        {
            var camera = GameObject.Find("Main Camera");
            return camera != null && camera.GetComponent<PlayerCamera>() != null;
        }
    }

    private void Start()
    {
        ConnectionBackground = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background"); // containing object for the connection ui
        ConnectedBackground = GameObject.Find("APCanvas(Clone)/Canvas/Connected Background"); // containing object for the connected ui
        Ipporttext = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/IPandPort").GetComponent<TMP_InputField>(); // address and port input
        Slot = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/Slot").GetComponent<TMP_InputField>(); // slot name input
        Password = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/Password").GetComponent<TMP_InputField>(); // password input
        ConnectButton = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/Connect").GetComponent<Button>(); // connect to archipelago button
        Status = GameObject.Find("APCanvas(Clone)/Canvas/Connected Background/Status").GetComponent<TMP_Text>(); // goal status tracker
        ConnectButton.onClick.AddListener(OnConnectPressed); // run connect function when button is pressed
        if (!File.Exists("ApConnection.txt")) return; // Read saved slot information from file
        var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
        Ipporttext.text = fileText[0];
        Password.text = fileText[1];
        Slot.text = fileText[2];
    }

    void OnGUI()
    {
        if (Ipporttext is null) // some race condition nonsense can make Start fail to find input fields. just rerun Start if it happens.
        {
            Start();
            return;
        }
        if (!ShowGUI)
        {
            ConnectedBackground.SetActive(false);
            ConnectedBackground.SetActive(false);
            return;
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
        }
    }

    void OnConnectPressed()
    {
        var ipPortSplit = Ipporttext.text.Split(':');
        if (!int.TryParse(ipPortSplit[1], out var port))
        {
            StartCoroutine(DisplayArchipelagoNotification($"[{ipPortSplit[1]}] is not a valid port",3));
            return;
        }
        var error = TryConnect(port, Slot.text, ipPortSplit[0], Password.text);
        if (error is not null)
        {
            StartCoroutine(DisplayArchipelagoNotification("Connection error: " + string.Join("\n", error), 3));
            return;
        }
        File.WriteAllText("ApConnection.txt", $"{Ipporttext.text}\n{Password.text}\n{Slot.text}");
    }

    private void Update() => APClientClass.Update();

    public static void UpdateGUIDescriptions()
    {
        if (selectedGoal == 1)
        {
            var maxDepth = (300 * DepthExtendersRecieved) + 300;
            Status.text =
                """
                Goal: Reach Depth
                Depth Extenders: <de>
                Max Depth: <md>
                """;
            Status.text = Status.text.Replace("<de>", DepthExtendersRecieved.ToString());
            Status.text = Status.text.Replace("<md>", maxDepth.ToString());
        }
        else if (selectedGoal == 2)
        {
            Status.text =
                """
                Goal: Escape Overgrown
                Layer Unlocks: <lu>
                Deepest Layer: <dl>
                """;
            Status.text = Status.text.Replace("<lu>", DepthExtendersRecieved.ToString());
            Status.text = Status.text.Replace("<dl>", LayerLocker.LayerIDToName[DepthExtendersRecieved]).Replace(" Unlock", "");
        }
        else if (selectedGoal == 3)
        {
            Status.text =
                """
                Goal: Defeat Elder
                Overgrown: <ou>
                """;
            Status.text = Status.text.Replace("<ou>", LayerUnlockDictionary.Contains("Overgrown Depths Unlock") ? "Unlocked" : "Locked");
        }
        else if (selectedGoal == 4)
        {
            Status.text =
                """
                Goal: Craftsanity
                Unlocked: <ru>
                Crafted: <rc>
                """;
            Status.text = Status.text.Replace("<ru>", Recipes.recipes.Count + "/112");
            Status.text = Status.text.Replace("<rc>", CraftingChecks.CraftedRecipes.ToString() + "/112");
        }
    }
    public void DisplayArchipelagoNotificationHelper(string text, int severity) // for places StartCoroutine can't be called
    {
        StartCoroutine(DisplayArchipelagoNotification(text, severity));
    }
    public static IEnumerator DisplayArchipelagoNotification(string text, int severity)
    {
        // display the given text in a popup. higher severity means more interruptive of gameplay.
        // items are lowest severity (1), hints are higher (2), and errors are highest (3).
        if (!ShowGUI)
        {
            yield return 0; // wait until the GUI is allowed
        }
        if (severity == 1)
        {
            var ItemNotif = GameObject.Find("APCanvas(Clone)/Canvas/Item Notification");
            var ItemText = GameObject.Find("APCanvas(Clone)/Canvas/Item Notification/Notification Message").GetComponent<TMP_Text>();
            ItemText.text = text;
            ItemNotif.SetActive(true);
            Sound.Play("warning", Vector2.zero, true, false, null, 1f, 1f, false, false);
            yield return new WaitForSecondsRealtime(3);
            ItemNotif.SetActive(false);
        }
        else if (severity == 2)
        {
            var HintNotif = GameObject.Find("APCanvas(Clone)/Canvas/Hint Notification");
            var HintText = GameObject.Find("APCanvas(Clone)/Canvas/Hint Notification/Notification Message").GetComponent<TMP_Text>();
            HintText.text = text;
            HintNotif.SetActive(true);
            Sound.Play("shuttleNotice", Vector2.zero, true, false, null, 0.6f, 1f, false, false);
            yield return new WaitForSecondsRealtime(5);
            HintNotif.SetActive(false);
        }
        else if (severity == 3)
        {
            var ErrorNotif = GameObject.Find("APCanvas(Clone)/Canvas/Error Notification");
            var ErrorText = GameObject.Find("APCanvas(Clone)/Canvas/Error Notification/Notification Message").GetComponent<TMP_Text>();
            ErrorText.text = text;
            ErrorNotif.SetActive(true);
            yield return new WaitForSecondsRealtime(10);
            ErrorNotif.SetActive(false);
        }
    }
}