using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CUAP.APClientClass;

namespace CUAP;

public class APCanvas : MonoBehaviour
{
    public static APCanvas instance;
    public static bool ShowGUI = true;
    public GameObject ConnectionBackground;
    public GameObject ConnectedBackground;
    public TMP_InputField Ipporttext;
    public TMP_InputField Password;
    public TMP_InputField Slot;
    public Button ConnectButton;
    private static GameObject ItemNotif;
    private static TMP_Text ItemText;
    private static bool ItemProcessing;
    private static GameObject HintNotif;
    private static TMP_Text HintText;
    private static bool HintProcessing;
    private static GameObject ErrorNotif;
    private static TMP_Text ErrorText;
    private static bool ErrorProcessing;
    private static Queue<string> ItemQueue = new Queue<string>();
    private static Queue<string> HintQueue = new Queue<string>();
    private static Queue<string> ErrorQueue = new Queue<string>();
    public static bool DeathlinkEnabled = false;
    public static TMP_Text Status;
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
        instance = this;
        ConnectionBackground = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background"); // containing object for the connection ui
        ConnectedBackground = GameObject.Find("APCanvas(Clone)/Canvas/Connected Background"); // containing object for the connected ui
        Ipporttext = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/IPandPort").GetComponent<TMP_InputField>(); // address and port input
        Slot = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/Slot").GetComponent<TMP_InputField>(); // slot name input
        Password = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/Password").GetComponent<TMP_InputField>(); // password input
        ConnectButton = GameObject.Find("APCanvas(Clone)/Canvas/Connection Background/Connect").GetComponent<Button>(); // connect to archipelago button
        Status = GameObject.Find("APCanvas(Clone)/Canvas/Connected Background/Status").GetComponent<TMP_Text>(); // goal status tracker
        ConnectButton.onClick.AddListener(OnConnectPressed); // run connect function when button is pressed
        ItemNotif = GameObject.Find("APCanvas(Clone)/Canvas/Item Notification");
        ItemText = GameObject.Find("APCanvas(Clone)/Canvas/Item Notification/Notification Message").GetComponent<TMP_Text>();
        HintNotif = GameObject.Find("APCanvas(Clone)/Canvas/Hint Notification");
        HintText = GameObject.Find("APCanvas(Clone)/Canvas/Hint Notification/Notification Message").GetComponent<TMP_Text>();
        ErrorNotif = GameObject.Find("APCanvas(Clone)/Canvas/Error Notification");
        ErrorText = GameObject.Find("APCanvas(Clone)/Canvas/Error Notification/Notification Message").GetComponent<TMP_Text>();
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
        if (!Ipporttext.text.Contains(":") && !Ipporttext.text.Equals("localhost"))
        {
            EnqueueArchipelagoNotification($"No server port was given.",3);
            Startup.Logger.LogError($"No server port was given.");
        }
        var ipPortSplit = Ipporttext.text.Split(':');
        if (!int.TryParse(ipPortSplit[1], out var port))
        {
            EnqueueArchipelagoNotification($"[{ipPortSplit[1]}] is not a valid port.",3);
            Startup.Logger.LogError($"[{ipPortSplit[1]}] is not a valid port");
            return;
        }
        var error = TryConnect(Ipporttext.text, Slot.text, Password.text);
        if (error is not null)
        {
            EnqueueArchipelagoNotification("Connection error: " + string.Join("\n", error),3);
            Startup.Logger.LogError("Connection error: " + string.Join("\n", error));
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
            Status.text = Status.text.Replace("<ru>", Recipes.recipes.Count + "/120");
            Status.text = Status.text.Replace("<rc>", CraftingChecks.CraftedRecipes.ToString() + "/120");
        }
    }
    public static void EnqueueArchipelagoNotification(string text, int severity)
    {
        switch(severity)
        {
            case 1:
                ItemQueue.Enqueue(text);
                if (!ItemProcessing)
                {
                    ItemProcessing = true;
                    instance.StartCoroutine(ProcessItemQueue());
                }
                break;
            case 2:
                HintQueue.Enqueue(text);
                if (!HintProcessing)
                {
                    HintProcessing = true;
                    instance.StartCoroutine(ProcessHintQueue());
                }
                break;
            case 3:
                ErrorQueue.Enqueue(text);
                if (!ErrorProcessing)
                {
                    ErrorProcessing = true;
                    instance.StartCoroutine(ProcessErrorQueue());
                }
                break;
        }
    }
    private static IEnumerator ProcessItemQueue()
    {
        while (ItemQueue.Count > 0)
        {
            ItemText.text = ItemQueue.Dequeue();
            ItemNotif.SetActive(true);
            Sound.Play("warning", Vector2.zero, true, false, null, 1.2f, 1f, false, false);
            yield return new WaitForSeconds(3);
            ItemNotif.SetActive(false);
        }
        ItemProcessing = false;
    }
    private static IEnumerator ProcessHintQueue()
    {
        while (HintQueue.Count > 0)
        {
            HintText.text = HintQueue.Dequeue();
            HintNotif.SetActive(true);
            Sound.Play("shuttleNotice", Vector2.zero, true, false, null, 0.6f, 1f, false, false);
            yield return new WaitForSeconds(5);
            HintNotif.SetActive(false);
        }
        HintProcessing = false;
    }
    private static IEnumerator ProcessErrorQueue()
    {
        while (ErrorQueue.Count > 0)
        {
            ErrorText.text = ErrorQueue.Dequeue();
            ErrorNotif.SetActive(true);
            yield return new WaitForSeconds(10);
            ErrorNotif.SetActive(false);
        }
        ErrorProcessing = false;
    }
}