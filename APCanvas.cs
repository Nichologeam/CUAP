using BepInEx;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CUAP.APClientClass;

namespace CUAP;

public class APCanvas : MonoBehaviour
{
    public static APCanvas instance;
    public static bool ShowMainGUI = true;
    public static bool ShowSkillTracker = false;
    public static bool skillsanityEnabled = false;
    public static string coloredAPText = "Archipelago";
    private readonly Color[] APTextColors = new Color[] // stores the Archipelago logo colors as Color32
    {// (too lazy to do proper conversion, so it's just the raw hex from the TMP <color> tags)
        new Color32(0xc9, 0x76, 0x82, 255), // top circle color
        new Color32(0x75, 0xc2, 0x75, 255), // top right
        new Color32(0xca, 0x94, 0xc2, 255), // bottom right
        new Color32(0xd9, 0xa0, 0x7d, 255), // bottom
        new Color32(0x76, 0x7e, 0xbd, 255), // bottom left
        new Color32(0xee, 0xe3, 0x91, 255), // top left
    };
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
    public static List<string> ShuffledQuests = [];
    private GameObject MoodlesanityQuestboard;
    private static TMP_Text[] MoodleTexts;
    private static Image[] MoodleImages;
    private static Button RerollButton;
    private static TMP_Text RerollText;
    private static int rerollCooldown;
    public static int rerollCooldownMax;
    private static TMP_Text QuestsRemaining;
    private static Image Moodle1Image;
    private static TMP_Text Moodle1Text;
    private static Image Moodle2Image;
    private static TMP_Text Moodle2Text;
    private static Image Moodle3Image;
    private static TMP_Text Moodle3Text;
    private static Image Moodle4Image;
    private static TMP_Text Moodle4Text;
    private static Image Moodle5Image;
    private static TMP_Text Moodle5Text;
    private static Image Moodle6Image;
    private static TMP_Text Moodle6Text;
    private static Image Moodle7Image;
    private static TMP_Text Moodle7Text;
    private static Image Moodle8Image;
    private static TMP_Text Moodle8Text;
    public static int UnlockedSlots = 2;
    private static GameObject TextNotif1;
    private static TMP_Text Text1;
    private static GameObject TextNotif2;
    private static TMP_Text Text2;
    private static GameObject TextNotif3;
    private static TMP_Text Text3;
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
    public static GameObject layerSelector;
    public static Button glButton;
    public static Button dglButton;
    public static Button ddButton;
    public static Button wlButton;
    public static Button odButton;
    private static GameObject apItemsTracker;
    public static TMP_Text apItemsCounter;
    private static Queue<string> TextQueue = new Queue<string>();
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
        ConnectionBackground = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background"); // containing object for the connection ui
        ConnectedBackground = GameObject.Find("APCanvas(Clone)/APCanvas/Connected Background"); // containing object for the connected ui
        SkillsanityTracker = GameObject.Find("APCanvas(Clone)/APCanvas/Skillsanity"); // containing object for the skillsanity tracker
        SkillsanitySTR = SkillsanityTracker.transform.Find("STR").gameObject.GetComponent<TMP_Text>();
        SkillsanityRES = SkillsanityTracker.transform.Find("RES").gameObject.GetComponent<TMP_Text>();
        SkillsanityINT = SkillsanityTracker.transform.Find("INT").gameObject.GetComponent<TMP_Text>();
        MoodlesanityQuestboard = GameObject.Find("APCanvas(Clone)/APCanvas/Questboard"); // containing object for the moodlesanity quests
        QuestsRemaining = MoodlesanityQuestboard.transform.Find("Quests Remaining").gameObject.GetComponent<TMP_Text>();
        RerollButton = MoodlesanityQuestboard.transform.Find("Reroll Button").gameObject.GetComponent<Button>();
        RerollText = RerollButton.transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>();
        Moodle1Image = MoodlesanityQuestboard.transform.Find("Moodle Image 1").gameObject.GetComponent<Image>();
        Moodle1Text = MoodlesanityQuestboard.transform.Find("Moodle Name 1").gameObject.GetComponent<TMP_Text>();
        Moodle2Image = MoodlesanityQuestboard.transform.Find("Moodle Image 2").gameObject.GetComponent<Image>();
        Moodle2Text = MoodlesanityQuestboard.transform.Find("Moodle Name 2").gameObject.GetComponent<TMP_Text>();
        Moodle3Image = MoodlesanityQuestboard.transform.Find("Moodle Image 3").gameObject.GetComponent<Image>();
        Moodle3Text = MoodlesanityQuestboard.transform.Find("Moodle Name 3").gameObject.GetComponent<TMP_Text>();
        Moodle4Image = MoodlesanityQuestboard.transform.Find("Moodle Image 4").gameObject.GetComponent<Image>();
        Moodle4Text = MoodlesanityQuestboard.transform.Find("Moodle Name 4").gameObject.GetComponent<TMP_Text>();
        Moodle5Image = MoodlesanityQuestboard.transform.Find("Moodle Image 5").gameObject.GetComponent<Image>();
        Moodle5Text = MoodlesanityQuestboard.transform.Find("Moodle Name 5").gameObject.GetComponent<TMP_Text>();
        Moodle6Image = MoodlesanityQuestboard.transform.Find("Moodle Image 6").gameObject.GetComponent<Image>();
        Moodle6Text = MoodlesanityQuestboard.transform.Find("Moodle Name 6").gameObject.GetComponent<TMP_Text>();
        Moodle7Image = MoodlesanityQuestboard.transform.Find("Moodle Image 7").gameObject.GetComponent<Image>();
        Moodle7Text = MoodlesanityQuestboard.transform.Find("Moodle Name 7").gameObject.GetComponent<TMP_Text>();
        Moodle8Image = MoodlesanityQuestboard.transform.Find("Moodle Image 8").gameObject.GetComponent<Image>();
        Moodle8Text = MoodlesanityQuestboard.transform.Find("Moodle Name 8").gameObject.GetComponent<TMP_Text>();
        MoodleTexts = [Moodle1Text, Moodle2Text, Moodle3Text, Moodle4Text, Moodle5Text, Moodle6Text, Moodle7Text, Moodle8Text];
        MoodleImages = [Moodle1Image, Moodle2Image, Moodle3Image, Moodle4Image, Moodle5Image, Moodle6Image, Moodle7Image, Moodle8Image];
        TextNotif1 = GameObject.Find("APCanvas(Clone)/APCanvas/PrintJSON Notification 1");
        Text1 = TextNotif1.transform.Find("Message").gameObject.GetComponent<TMP_Text>();
        TextNotif2 = GameObject.Find("APCanvas(Clone)/APCanvas/PrintJSON Notification 2");
        Text2 = TextNotif2.transform.Find("Message").gameObject.GetComponent<TMP_Text>();
        TextNotif3 = GameObject.Find("APCanvas(Clone)/APCanvas/PrintJSON Notification 3");
        Text3 = TextNotif3.transform.Find("Message").gameObject.GetComponent<TMP_Text>();
        Ipporttext = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/IPandPort").GetComponent<TMP_InputField>(); // address and port input
        Slot = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/Slot").GetComponent<TMP_InputField>(); // slot name input
        Password = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/Password").GetComponent<TMP_InputField>(); // password input
        ConnectButton = GameObject.Find("APCanvas(Clone)/APCanvas/Connection Background/Connect").GetComponent<Button>(); // connect to archipelago button
        Status = GameObject.Find("APCanvas(Clone)/APCanvas/Connected Background/Status").GetComponent<TMP_Text>(); // goal status tracker
        versionTag = GameObject.Find("APCanvas(Clone)/APCanvas/Version Tag").GetComponent<TMP_Text>();
        ConnectButton.onClick.AddListener(OnConnectPressed); // run connect function when button is pressed
        RerollButton.onClick.AddListener(() => RerollQuests(false));
        ItemNotif = GameObject.Find("APCanvas(Clone)/APCanvas/Item Notification");
        ItemText = GameObject.Find("APCanvas(Clone)/APCanvas/Item Notification/Notification Message").GetComponent<TMP_Text>();
        HintNotif = GameObject.Find("APCanvas(Clone)/APCanvas/Hint Notification");
        HintText = GameObject.Find("APCanvas(Clone)/APCanvas/Hint Notification/Notification Message").GetComponent<TMP_Text>();
        ErrorNotif = GameObject.Find("APCanvas(Clone)/APCanvas/Error Notification");
        ErrorText = GameObject.Find("APCanvas(Clone)/APCanvas/Error Notification/Notification Message").GetComponent<TMP_Text>();
        layerSelector = GameObject.Find("APCanvas(Clone)/APCanvas/Layer Selector");
        glButton = layerSelector.transform.Find("Gravel Lands Button").gameObject.GetComponent<Button>();
        dglButton = layerSelector.transform.Find("Deeper Gravel Lands Button").gameObject.GetComponent<Button>();
        ddButton = layerSelector.transform.Find("Dried Desert Button").gameObject.GetComponent<Button>();
        wlButton = layerSelector.transform.Find("Wasteland Button").gameObject.GetComponent<Button>();
        odButton = layerSelector.transform.Find("Overgrown Depths Button").gameObject.GetComponent<Button>();
        apItemsTracker = GameObject.Find("APCanvas(Clone)/APCanvas/AP Items Tracker");
        apItemsCounter = apItemsTracker.transform.Find("Counter").gameObject.GetComponent<TMP_Text>();
        UpdateSkillsanityValues(0, 60);
        UpdateSkillsanityValues(1, 60);
        UpdateSkillsanityValues(2, 60);
        versionTag.text = APLocale.Get("versionTag", APLocale.APLanguageType.UI) + Startup.CUAPVersion;
        StartCoroutine(CycleAPColors());
        ConnectionBackground.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("disconnected", APLocale.APLanguageType.UI);
        ConnectedBackground.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("connected", APLocale.APLanguageType.UI);
        ItemNotif.transform.Find("greentext").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("received", APLocale.APLanguageType.UI);
        HintNotif.transform.Find("yellowtext").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("hint", APLocale.APLanguageType.UI);
        ErrorNotif.transform.Find("redtext").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("error", APLocale.APLanguageType.UI);
        SkillsanityTracker.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("skilltracker", APLocale.APLanguageType.UI);
        MoodlesanityQuestboard.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("moodquestboard", APLocale.APLanguageType.UI);
        layerSelector.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("layerselector", APLocale.APLanguageType.UI);
        if (!File.Exists("ApConnection.txt")) return; // Read saved slot information from file
        var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
        Ipporttext.text = fileText[0];
        Password.text = fileText[1];
        Slot.text = fileText[2];
    }

    private void OnGUI()
    {
        if (Ipporttext is null) // some race condition nonsense can make Start fail to find input fields. just rerun Start if it happens.
        {
            Start();
            return;
        }
        if (skillsanityEnabled)
        {
            SkillsanityTracker.SetActive(ShowSkillTracker);
        }
        else
        {
            SkillsanityTracker.SetActive(false);
        }
        if (Moodlesanity.questboardMode)
        {
            MoodlesanityQuestboard.SetActive(InGame);
        }
        if (CraftingChecks.apItems)
        {
            apItemsTracker.SetActive(InGame);
        }
        if (!IsConnected())
        {
            ConnectionBackground.SetActive(true);
            ConnectedBackground.SetActive(false);
            APClientClass.Disconnect();
        }
        else
        {
            ConnectedBackground.SetActive(true);
            ConnectionBackground.SetActive(false);
        }
        if (!ShowMainGUI)
        {
            ConnectedBackground.SetActive(false);
            ConnectedBackground.SetActive(false);
            MoodlesanityQuestboard.SetActive(false);
            apItemsTracker.SetActive(false);
        }
    }

    public void OnConnectPressed()
    {
        if (!Ipporttext.text.Equals("localhost"))
        {
            if (!Ipporttext.text.Contains(":"))
            {
                EnqueueArchipelagoNotification(APLocale.Get("noPort", APLocale.APLanguageType.Errors), 3);
                Startup.Logger.LogError("Connection error: No server port was given.");
                return;
            }
            var ipPortSplit = Ipporttext.text.Split(':');
            if (!int.TryParse(ipPortSplit[1], out var port))
            {
                string errorMsg = APLocale.Get("invalidPort", APLocale.APLanguageType.Errors);
                errorMsg = errorMsg.Replace("<port>", ipPortSplit[1]);
                EnqueueArchipelagoNotification(errorMsg, 3);
                Startup.Logger.LogError($"Connection error: {ipPortSplit[1]} is not a valid port.");
                return;
            }
        }
        var error = TryConnect(Ipporttext.text, Slot.text, Password.text);
        if (error is not null)
        {
            EnqueueArchipelagoNotification(APLocale.Get("genericError", APLocale.APLanguageType.Errors) + string.Join("\n", error),3);
            Startup.Logger.LogError("Connection error: " + string.Join("\n", error));
            return;
        }
        File.WriteAllText("ApConnection.txt", $"{Ipporttext.text}\n{Password.text}\n{Slot.text}");
    }

    private void Update() => APClientClass.Update();

    private IEnumerator CycleAPColors()
    {
        int offset = 0;
        while (true)
        {
            var sb = new System.Text.StringBuilder();
            var baseText = APLocale.Get("archipelago", APLocale.APLanguageType.Messages);
            for (int i = 0; i < baseText.Length; i++) // "Archipelago" is 11 characters long
            {
                var color = APTextColors[(i + offset) % APTextColors.Length];
                string hex = ColorUtility.ToHtmlStringRGB(color);
                sb.Append($"<color=#{hex}>{baseText[i]}");
            }
            sb.Append("<color=#FFFFFF>");
            coloredAPText = sb.ToString();
            offset = (offset + 1) % APTextColors.Length;
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    public static void UpdateGUIDescriptions()
    {
        if (!InGame) return; // you can't see this on the main menu anyway
        if (selectedGoal == 1)
        {
            var maxDepth = (300 * DepthExtendersRecieved) + 300;
            Status.text =
                $"""
                {APLocale.Get("reachDepth1", APLocale.APLanguageType.UI)}
                {APLocale.Get("reachDepth2", APLocale.APLanguageType.UI)}
                {APLocale.Get("reachDepth3", APLocale.APLanguageType.UI)}
                """;
            Status.text = Status.text.Replace("<gd>", $"{DepthChecks.instance.GoalDepth}m"); // gd for Goal Depth
            Status.text = Status.text.Replace("<de>", DepthExtendersRecieved.ToString()); // de for Depth Extenders
            Status.text = Status.text.Replace("<md>", $"{maxDepth}m"); // md for Max Depth
        }
        else if (selectedGoal == 2)
        {
            Status.text =
                $"""
                {APLocale.Get("escapeOvergrown1", APLocale.APLanguageType.UI)}
                {APLocale.Get("escapeOvergrown2", APLocale.APLanguageType.UI)}
                {APLocale.Get("escapeOvergrown3", APLocale.APLanguageType.UI)}
                """;
            Status.text = Status.text.Replace("<lu>", DepthExtendersRecieved.ToString()); // lu for Layer Unlocks
            Status.text = Status.text.Replace("<dl>", LayerLocker.LayerIDToName[DepthExtendersRecieved]).Replace(" Unlock", ""); // dl for Deepest Layer
        }
        else if (selectedGoal == 3)
        {
            Status.text =
                $"""
                {APLocale.Get("defeatElder1", APLocale.APLanguageType.UI)}
                {APLocale.Get("defeatElder2", APLocale.APLanguageType.UI)}
                """;
            Status.text = Status.text.Replace("<ou>", LayerUnlockDictionary.Contains("Overgrown Depths Unlock") ? "Unlocked" : "Locked"); // ou for Overgrown Unlocked
        }
    }
    public static void UpdateSkillsanityValues(int skill, float newExp)
    {
        switch (skill)
        {
            case 0: // STR
                if (newExp == -1)
                {
                    SkillsanitySTR.text = APLocale.Get("strAllSent", APLocale.APLanguageType.UI);
                }
                else
                {
                    SkillsanitySTR.text = APLocale.Get("strRemaining", APLocale.APLanguageType.UI);
                    SkillsanitySTR.text = SkillsanitySTR.text.Replace("<exp>",$"{newExp}");
                }
                break;
            case 1: // RES
                if (newExp == -1)
                {
                    SkillsanityRES.text = APLocale.Get("resAllSent", APLocale.APLanguageType.UI);
                }
                else
                {
                    SkillsanityRES.text = APLocale.Get("resRemaining", APLocale.APLanguageType.UI);
                    SkillsanityRES.text = SkillsanityRES.text.Replace("<exp>", $"{newExp}");
                }
                break;
            case 2: // INT
                if (newExp == -1)
                {
                    SkillsanityINT.text = APLocale.Get("intAllSent", APLocale.APLanguageType.UI);
                }
                else
                {
                    SkillsanityINT.text = APLocale.Get("intRemaining", APLocale.APLanguageType.UI);
                    SkillsanityINT.text = SkillsanityINT.text.Replace("<exp>", $"{newExp}");
                }
                break;
            default: // none of the above?
                Startup.Logger.LogError("Skillsanity Error: UpdateSkillsanityValues was called with an invalid skill ({skill})");
                EnqueueArchipelagoNotification(APLocale.Get("skillUpdate", APLocale.APLanguageType.Errors) + $"({skill})", 3);
                break;
        }
    }
    public static void UpdateQuestboard(bool unlockingSlot = false)
    {
        if (unlockingSlot)
        {
            UnlockedSlots++;
        }
        if (!InGame) return;
        CheckIfOutOfSlots();
        int questCount = Moodlesanity.questsAvailable.Count;
        int maxSlots = Mathf.Min(UnlockedSlots, MoodleTexts.Length, questCount);
        QuestsRemaining.text = questCount + APLocale.Get("questsRemaining", APLocale.APLanguageType.UI);
        for (int i = 0; i < maxSlots; i++)
        {
            string questName = ShuffledQuests[i];
            MoodleTexts[i].text = questName.Replace("Moodlesanity - ", "");
            string internalId = Moodlesanity.CheckToInternalMoodID.GetValueOrDefault(questName);
            MoodleImages[i].sprite = Resources.Load<Sprite>($"moodles/{internalId}");
        }
    }
    private static void CheckIfOutOfSlots()
    {
        if (UnlockedSlots <= Moodlesanity.questsAvailable.Count) return;
        Sprite apLogo = Startup.apassets.LoadAsset<Sprite>("aplogo200");
        int maxSlots = Mathf.Min(UnlockedSlots, MoodleTexts.Length);
        for (int i = 0; i < maxSlots; i++)
        {
            MoodleTexts[i].text = APLocale.Get("noQuests", APLocale.APLanguageType.UI);
            MoodleImages[i].sprite = apLogo;
        }
    }
    public static void RerollQuests(bool force)
    {
        if (rerollCooldown > 0 && !force) return;
        ShuffledQuests = Moodlesanity.questsAvailable;
        for (int i = ShuffledQuests.Count - 1; i > 0; i--) // randomize the quest order
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (ShuffledQuests[i], ShuffledQuests[j]) = (ShuffledQuests[j], ShuffledQuests[i]);
        };
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(null); // stops space presses from being eaten
        UpdateQuestboard(false);
        if (!force)
        {
            instance.StartCoroutine(instance.RerollCooldown());
        }

    }
    private IEnumerator RerollCooldown()
    {
        if (rerollCooldownMax == 0)
        {
            yield return null; // there is no set cooldown. just return instantly
        }
        rerollCooldown = rerollCooldownMax;
        RerollText.text = $"({rerollCooldownMax})";
        while (rerollCooldown > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            rerollCooldown--;
            RerollText.text = $"({rerollCooldown})";
        }
        RerollText.text = APLocale.Get("reroll", APLocale.APLanguageType.UI);
    }
    public static void EnqueueArchipelagoNotification(string text, int severity)
    {
        if (ThreadingHelper.Instance == null) // V5.0.2 causes BepInEx's bootstrapper to fail creating this, so we'll do it ourselves.
        {
            Startup.Logger.LogWarning("BepInEx.ThreadingHelper is null. Recreating...");
            var threadingHelperType = typeof(ThreadingHelper);
            var initializeMethod = threadingHelperType.GetMethod("Initialize", BindingFlags.Static | BindingFlags.NonPublic);
            initializeMethod!.Invoke(null, null);
        }
        switch (severity)
        {
            case 0:
                TextQueue.Enqueue(text);
                ThreadingHelper.Instance.StartSyncInvoke(() => instance.StartCoroutine(ProcessTextNotif()));
                break;
            case 1:
                ItemQueue.Enqueue(text);
                if (!ItemProcessing)
                {
                    ItemProcessing = true;
                    ThreadingHelper.Instance.StartSyncInvoke(() => instance.StartCoroutine(ProcessItemQueue()));
                }
                break;
            case 2:
                HintQueue.Enqueue(text);
                if (!HintProcessing)
                {
                    HintProcessing = true;
                    ThreadingHelper.Instance.StartSyncInvoke(() => instance.StartCoroutine(ProcessHintQueue()));
                }
                break;
            case 3:
                ErrorQueue.Enqueue(text);
                if (!ErrorProcessing)
                {
                    ErrorProcessing = true;
                    ThreadingHelper.Instance.StartSyncInvoke(() => instance.StartCoroutine(ProcessErrorQueue()));
                }
                break;
        }
    }

    private static IEnumerator ProcessTextNotif()
    {
        while (TextQueue.Count > 0)
        {
            if (!TextNotif1.activeSelf)
            {
                TextNotif1.SetActive(true);
                Text1.text = TextQueue.Dequeue();
                yield return new WaitForSecondsRealtime(5);
                TextNotif1.SetActive(false);
                yield return 0; // one frame of downtime to make it clearer that the next notification is a new one
            }
            else if (!TextNotif2.activeSelf)
            {
                TextNotif2.SetActive(true);
                Text2.text = TextQueue.Dequeue();
                yield return new WaitForSecondsRealtime(5);
                TextNotif2.SetActive(false);
                yield return 0; // one frame of downtime to make it clearer that the next notification is a new one
            }
            else if (!TextNotif3.activeSelf)
            {
                TextNotif3.SetActive(true);
                Text3.text = TextQueue.Dequeue();
                yield return new WaitForSecondsRealtime(5);
                TextNotif3.SetActive(false);
                yield return 0; // one frame of downtime to make it clearer that the next notification is a new one
            }
            yield return null;
        }
    }
    private static IEnumerator ProcessItemQueue()
    {
        while (ItemQueue.Count > 0)
        {
            ItemText.text = ItemQueue.Dequeue();
            ItemNotif.SetActive(true);
            Sound.Play("warning", Vector2.zero, true, false, null, 1.2f, 1f, false, false);
            yield return new WaitForSecondsRealtime(3);
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
            yield return new WaitForSecondsRealtime(5);
            HintNotif.SetActive(false);
            yield return 0; // one frame of downtime to make it clearer that the next hint is a new one
        }
        HintProcessing = false;
    }
    private static IEnumerator ProcessErrorQueue()
    {
        while (ErrorQueue.Count > 0)
        {
            ErrorText.text = ErrorQueue.Dequeue();
            ErrorNotif.SetActive(true);
            yield return new WaitForSecondsRealtime(10);
            ErrorNotif.SetActive(false);
            yield return 0; // one frame of downtime to make it clearer that the next error is a new one
        }
        ErrorProcessing = false;
    }

    public static void ChangeLanguage()
    {
        if (instance == null)
        {
            return;
        }
        instance.ConnectionBackground.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("disconnected", APLocale.APLanguageType.UI);
        instance.ConnectedBackground.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("connected", APLocale.APLanguageType.UI);
        ItemNotif.transform.Find("greentext").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("received", APLocale.APLanguageType.UI);
        HintNotif.transform.Find("yellowtext").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("hint", APLocale.APLanguageType.UI);
        ErrorNotif.transform.Find("redtext").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("error", APLocale.APLanguageType.UI);
        instance.SkillsanityTracker.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("skilltracker", APLocale.APLanguageType.UI);
        instance.MoodlesanityQuestboard.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("moodquestboard", APLocale.APLanguageType.UI);
        layerSelector.transform.Find("Title").gameObject.GetComponent<TMP_Text>().text = APLocale.Get("layerselector", APLocale.APLanguageType.UI);
    }
}