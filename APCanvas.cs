using KrokoshaCasualtiesMP;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CUAP.APClientClass;

namespace CUAP;

public class APCanvas : MonoBehaviour
{
    public static APCanvas instance;
    public static bool ShowMainGUI = true;
    public static bool ShowSkillTracker = false;
    public static bool skillsanityEnabled = true;
    public GameObject ConnectionBackground;
    public GameObject ConnectedBackground;
    public TMP_InputField Ipporttext;
    public TMP_InputField Password;
    public TMP_InputField Slot;
    public Button ConnectButton;
    private GameObject SkillsanityTracker;
    private static TMP_Text SkillsanitySTR;
    private static TMP_Text SkillsanityRES;
    private static TMP_Text SkillsanityINT;
    private GameObject MoodlesanityQuestboard;
    private static Image Moodle1Image;
    private static TMP_Text Moodle1Text;
    private static Image Moodle2Image;
    private static TMP_Text Moodle2Text;
    private static Image Moodle3Image;
    private static TMP_Text Moodle3Text;
    private static Image Moodle4Image;
    private static TMP_Text Moodle4Text;
    private static int UnlockedSlots = 1;
    private static GameObject ItemNotif;
    private static TMP_Text ItemText;
    private static bool ItemProcessing;
    private static GameObject HintNotif;
    private static TMP_Text HintText;
    private static bool HintProcessing;
    private static GameObject ErrorNotif;
    private static TMP_Text ErrorText;
    public static TMP_Text versionTag;
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
        togetherAssembly = typeof(KrokoshaScavMultiplayer).Assembly; // run this here to guarentee that both mods are loaded (since we're loading a scene)
        var serverChat = togetherAssembly.GetType("KrokoshaCasualtiesMP.Chat");
        sendServerMessage = serverChat.GetMethod("Server_ChatAnnouncement",
            bindingAttr: BindingFlags.Instance | BindingFlags.Static,
            binder: null,
            types: [typeof(string), typeof(string)],
            modifiers: null);
        ConnectionBackground = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background"); // containing object for the connection ui
        ConnectedBackground = GameObject.Find("APCanvas(Clone)/APCanvas/Connected Background"); // containing object for the connected ui
        SkillsanityTracker = GameObject.Find("APCanvas(Clone)/APCanvas/Skillsanity"); // containing object for the skillsanity tracker
        SkillsanitySTR = GameObject.Find("APCanvas(Clone)/APCanvas/Skillsanity/STR").GetComponent<TMP_Text>();
        SkillsanityRES = GameObject.Find("APCanvas(Clone)/APCanvas/Skillsanity/RES").GetComponent<TMP_Text>();
        SkillsanityINT = GameObject.Find("APCanvas(Clone)/APCanvas/Skillsanity/INT").GetComponent<TMP_Text>();
        MoodlesanityQuestboard = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard"); // containing object for the moodlesanity quests
        Moodle1Image = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Image 1").GetComponent<Image>();
        Moodle1Text = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Name 1").GetComponent<TMP_Text>();
        Moodle2Image = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Image 2").GetComponent<Image>();
        Moodle2Text = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Name 2").GetComponent<TMP_Text>();
        Moodle3Image = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Image 3").GetComponent<Image>();
        Moodle3Text = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Name 3").GetComponent<TMP_Text>();
        Moodle4Image = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Image 4").GetComponent<Image>();
        Moodle4Text = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard/Moodle Name 4").GetComponent<TMP_Text>();
        Ipporttext = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/IPandPort").GetComponent<TMP_InputField>(); // address and port input
        Slot = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/Slot").GetComponent<TMP_InputField>(); // slot name input
        Password = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/Password").GetComponent<TMP_InputField>(); // password input
        ConnectButton = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/Connect").GetComponent<Button>(); // connect to archipelago button
        Status = GameObject.Find("APCanvas(Clone)/APCanvas/Connected Background/Status").GetComponent<TMP_Text>(); // goal status tracker
        versionTag = GameObject.Find("APCanvas(Clone)/APCanvas/Version Tag").GetComponent<TMP_Text>();
        ConnectButton.onClick.AddListener(OnConnectPressed); // run connect function when button is pressed
        ItemNotif = GameObject.Find("APCanvas(Clone)/APCanvas/Item Notification");
        ItemText = GameObject.Find("APCanvas(Clone)/APCanvas/Item Notification/Notification Message").GetComponent<TMP_Text>();
        HintNotif = GameObject.Find("APCanvas(Clone)/APCanvas/Hint Notification");
        HintText = GameObject.Find("APCanvas(Clone)/APCanvas/Hint Notification/Notification Message").GetComponent<TMP_Text>();
        ErrorNotif = GameObject.Find("APCanvas(Clone)/APCanvas/Error Notification");
        ErrorText = GameObject.Find("APCanvas(Clone)/APCanvas/Error Notification/Notification Message").GetComponent<TMP_Text>();
        UpdateSkillsanityValues(0, 60);
        UpdateSkillsanityValues(1, 60);
        UpdateSkillsanityValues(2, 60);
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
        if (!ShowSkillTracker || !skillsanityEnabled)
        {
            SkillsanityTracker.SetActive(false);
        }
        if (ShowSkillTracker)
        {
            SkillsanityTracker.SetActive(true);
        }
        MoodlesanityQuestboard.SetActive(InGame);
        if (!ShowMainGUI)
        {
            ConnectedBackground.SetActive(false);
            ConnectedBackground.SetActive(false);
            MoodlesanityQuestboard.SetActive(false);
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
        if (!Ipporttext.text.Equals("localhost"))
        {
            if (!Ipporttext.text.Contains(":"))
            {
                EnqueueArchipelagoNotification($"Connection error: No server port was given.", 3);
                Startup.Logger.LogError($"Connection error: No server port was given.");
            }
            var ipPortSplit = Ipporttext.text.Split(':');
            if (!int.TryParse(ipPortSplit[1], out var port))
            {
                EnqueueArchipelagoNotification($"Connection error: [{ipPortSplit[1]}] is not a valid port.", 3);
                Startup.Logger.LogError($"Connection error: [{ipPortSplit[1]}] is not a valid port");
                return;
            }
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
    public static void UpdateSkillsanityValues(int skill, float newExp)
    {
        switch (skill)
        {
            case 0: // STR
                if (newExp == -1)
                {
                    SkillsanitySTR.text = $"STR: All checks sent!";
                }
                else
                {
                    SkillsanitySTR.text = $"STR: {newExp} exp to next check";
                }
                break;
            case 1: // RES
                if (newExp == -1)
                {
                    SkillsanityRES.text = $"RES: All checks sent!";
                }
                else
                {
                    SkillsanityRES.text = $"RES: {newExp} exp to next check";
                }
                break;
            case 2: // INT
                if (newExp == -1)
                {
                    SkillsanityINT.text = $"INT: All checks sent!";
                }
                else
                {
                    SkillsanityINT.text = $"INT: {newExp} exp to next check";
                }
                break;
            default: // none of the above?
                Startup.Logger.LogError($"Skillsanity Error: UpdateSkillsanityValues was called with an invalid skill ({skill})");
                EnqueueArchipelagoNotification($"Skillsanity Error: UpdateSkillsanityValues was called with invald skill ({skill})",3);
                break;
        }
    }
    public static void UpdateQuestboard(bool unlockingSlot = false)
    {// if you think this looks bad, the basegame moodle code is worse
        if (unlockingSlot)
        {
            UnlockedSlots++;
        }
        if (!InGame) return;
        CheckIfOutOfSlots();
        Moodle1Text.text = Moodlesanity.questsAvailable[0].Replace("Moodlesanity - ","");
        Moodle1Image.sprite = Resources.Load<Sprite>($"moodles/{Moodlesanity.CheckToInternalMoodID.GetValueOrDefault(Moodlesanity.questsAvailable[0])}");
        Debug.Log($"Moodle/{Moodlesanity.CheckToInternalMoodID.GetValueOrDefault(Moodlesanity.questsAvailable[0])}");
        if (UnlockedSlots < 2 || !(Moodlesanity.questsAvailable.Count >= 2)) return;
        Moodle2Text.text = Moodlesanity.questsAvailable[1].Replace("Moodlesanity - ", "");
        Moodle2Image.sprite = Resources.Load<Sprite>($"moodles/{Moodlesanity.CheckToInternalMoodID.GetValueOrDefault(Moodlesanity.questsAvailable[1])}");
        if (UnlockedSlots < 3 || !(Moodlesanity.questsAvailable.Count >= 3)) return;
        Moodle3Text.text = Moodlesanity.questsAvailable[2].Replace("Moodlesanity - ", "");
        Moodle3Image.sprite = Resources.Load<Sprite>($"moodles/{Moodlesanity.CheckToInternalMoodID.GetValueOrDefault(Moodlesanity.questsAvailable[2])}");
        if (UnlockedSlots < 4 || !(Moodlesanity.questsAvailable.Count >= 4)) return;
        Moodle4Text.text = Moodlesanity.questsAvailable[3].Replace("Moodlesanity - ", "");
        Moodle4Image.sprite = Resources.Load<Sprite>($"moodles/{Moodlesanity.CheckToInternalMoodID.GetValueOrDefault(Moodlesanity.questsAvailable[3])}");
    }
    private static void CheckIfOutOfSlots()
    {
        if (UnlockedSlots > Moodlesanity.questsAvailable.Count)
        {
            Moodle1Text.text = "Out of quests!";
            Moodle1Image.sprite = Startup.apassets.LoadAsset<Sprite>("aplogo200");
            if (UnlockedSlots < 2) return;
            Moodle2Text.text = "Out of quests!";
            Moodle2Image.sprite = Startup.apassets.LoadAsset<Sprite>("aplogo200");
            if (UnlockedSlots < 3) return;
            Moodle3Text.text = "Out of quests!";
            Moodle3Image.sprite = Startup.apassets.LoadAsset<Sprite>("aplogo200");
            if (UnlockedSlots < 4) return;
            Moodle4Text.text = "Out of quests!";
            Moodle4Image.sprite = Startup.apassets.LoadAsset<Sprite>("aplogo200");
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
            yield return 0; // one frame of downtime to make it clearer that the next item is a new one
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