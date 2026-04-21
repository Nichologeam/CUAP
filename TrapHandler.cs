using Archipelago.MultiClient.Net;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private Body Vitals;
    private WorldGeneration worldgen;
    public static PlayerCamera plrcam;
    private MoodleManager moodles;
    private readonly AccessTools.FieldRef<MoodleManager, float> moodleUpdateTime = AccessTools.FieldRefAccess<MoodleManager, float>("updateTime"); // this variable is private normally
    private float prevUpdateTime = 0.5f;
    private bool revControlActive;
    private float revControlEndTime;
    private bool unchippedActive;
    private float unchippedEndTime;
    private bool disfigActive;
    private float disfigEndTime;
    private int thornbackTrapLevel;
    public static bool mindwipeActive;
    private float mindwipeEndTime;
    string trapSender;
    private List<Item> heldItems = new List<Item>();

    private void OnEnable()
    {
        Client = APClientClass.session;
        Vitals = this.gameObject.GetComponent<Body>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        moodles = GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<MoodleManager>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    private void Start() // run this later so MoodleManager.Awake has indexed the sprites into MoodleManager.icons in time
    {
        var APLogo = Sprite.Create(
            texture: Startup.apassets.LoadAsset<Texture2D>("aplogo200"),
            pixelsPerUnit: 400,
            rect: new Rect(0, 0, 200, 200),
            pivot: new Vector2(0.5f, 0.5f));
        moodles.icons["death"] = APLogo; // replace unused Deceased moodle sprite
    }
    private void LateUpdate() // lateupdate so it's after normal moodle updates (no race condition, yippee!)
    {
        if (moodleUpdateTime(moodles) > prevUpdateTime && Vitals.alive) // timer reset, moodles were updated
        {
            if (revControlActive)
            {
                float remaining = Mathf.Max(0, revControlEndTime - Time.realtimeSinceStartup);
                TimeSpan ts = TimeSpan.FromSeconds(remaining);
                string formatted = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                string desc = APLocale.Get("revTrapDesc", APLocale.APLanguageType.UI);
                desc = desc.Replace("<time>", formatted);
                moodles.AddMoodle(5, "death", $"{APCanvas.coloredAPText}{APLocale.Get("revTrap", APLocale.APLanguageType.UI)}", desc, false, false);
            }
            if (unchippedActive)
            {
                float remaining = Mathf.Max(0, unchippedEndTime - Time.realtimeSinceStartup);
                TimeSpan ts = TimeSpan.FromSeconds(remaining);
                string formatted = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                string desc = APLocale.Get("unchipTrapDesc", APLocale.APLanguageType.UI);
                desc = desc.Replace("<time>", formatted);
                moodles.AddMoodle(5, "death", $"{APCanvas.coloredAPText}{APLocale.Get("unchipTrap", APLocale.APLanguageType.UI)}", desc, false, false);
            }
            if (disfigActive)
            {
                float remaining = Mathf.Max(0, disfigEndTime - Time.realtimeSinceStartup);
                TimeSpan ts = TimeSpan.FromSeconds(remaining);
                string formatted = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                string desc = APLocale.Get("disfigTrapDesc", APLocale.APLanguageType.UI);
                desc = desc.Replace("<time>", formatted);
                moodles.AddMoodle(5, "death", $"{APCanvas.coloredAPText}{APLocale.Get("disfigTrap", APLocale.APLanguageType.UI)}", desc, false, false);
            }
            if (mindwipeActive)
            {
                float remaining = Mathf.Max(0, mindwipeEndTime - Time.realtimeSinceStartup);
                TimeSpan ts = TimeSpan.FromSeconds(remaining);
                string formatted = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                string desc = APLocale.Get("wipeTrapDesc", APLocale.APLanguageType.UI);
                desc = desc.Replace("<time>", formatted);
                moodles.AddMoodle(5, "death", $"{APCanvas.coloredAPText}{APLocale.Get("wipeTrap", APLocale.APLanguageType.UI)}", desc, false, false);
            }
        }
        prevUpdateTime = moodleUpdateTime(moodles);
        if (thornbackTrapLevel == 1)
        {
            Vitals.horrifiedLevel = 75;
        }
        else if (thornbackTrapLevel == 2)
        {
            Vitals.horrifiedLevel = 200;
        }
    }
    public void ProcessTraps(string TrapName, string ItemSender)
    {
        trapSender = ItemSender;
        switch (TrapName)
        {
            case "Depression Trap":
                Vitals.happiness = -20;
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("depression", APLocale.APLanguageType.UI)}", false);
                break;
            case "Hearing Loss Trap":
                Vitals.hearingLoss = +50;
                string msg = APLocale.Get("hearing", APLocale.APLanguageType.UI);
                msg = msg.Replace("<sender>",ItemSender.ToUpper());
                plrcam.DoAlert(msg, false);
                break;
            case "Earthquake Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("quake", APLocale.APLanguageType.UI)}", false);
                worldgen.earthquakeDelay = 0; // start an earthquake
                worldgen.earthquakeIntensity = 2; // twice as intense as basegame earthquake
                worldgen.earthquakeTime = 15; // for 15 seconds
                break;
            case "Reverse Controls Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("rev", APLocale.APLanguageType.UI)}", false);
                StartCoroutine(ReverseControls());
                break;
            case "Sleep Trap":
                Vitals.sleeping = true;
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("sleep", APLocale.APLanguageType.UI)}", false);
                break;
            case "Unchipped Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("chipped", APLocale.APLanguageType.UI)}", false);
                StartCoroutine(UnchippedToggle());
                break;
            case "Elder Thornback Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("elder", APLocale.APLanguageType.UI)}", false);
                StartCoroutine(Thornback());
                break;
            case "Cave Ticks Trap":
                Instantiate(Resources.Load<GameObject>("caveticks"), gameObject.transform.position, Quaternion.identity);
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("ticks", APLocale.APLanguageType.UI)}", false);
                break;
            case "Bad Rep Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("rep", APLocale.APLanguageType.UI)}", false);
                foreach (var trader in FindObjectsOfType<TraderScript>())
                {
                    if (trader.hostile) continue; // don't bother making them hostile a second time
                    trader.hostility = 500;
                    Vitals.happiness += 3f; // counteract hostility happiness decrease
                }
                break;
            case "Disfigured Trap":
                if (Vitals.disfigured)
                {
                    break;
                }
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("disfig", APLocale.APLanguageType.UI)}", false);
                StartCoroutine(Disfigurement());
                break;
            case "Fellow Experiment":
                Instantiate(Resources.Load<GameObject>("corpse"), gameObject.transform.position, Quaternion.identity);
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("experiment", APLocale.APLanguageType.UI)}", false);
                break;
            case "Fragile Items Trap":
                heldItems.Clear();
                foreach (var slot in FindObjectsOfType<InventorySlot>())
                {
                    var item = slot.gameObject.GetComponentInChildren<Item>();
                    if (item != null)
                    {
                        heldItems.Add(item);
                        Debug.Log(slot.limb.fullName + " has " + item.fullName);
                    }
                }
                if (heldItems.Count == 0) break; // return early if there are no items
                Item chosenItem = heldItems[UnityEngine.Random.Range(0, heldItems.Count)];
                Debug.Log($"{chosenItem.fullName} was chosen to be damaged");
                if (chosenItem.TryGetComponent(out WaterContainerItem _))
                {
                    plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender} {APLocale.Get("fragile", APLocale.APLanguageType.UI)} {chosenItem.fullName}", false);
                }
                else
                {
                    string msg2 = APLocale.Get("fragileWater", APLocale.APLanguageType.UI);
                    msg2 = msg2.Replace("<item>", chosenItem.fullName);
                    plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{msg2}", false);
                }
                chosenItem.condition = 0;
                break;
            case "Mindwipe Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("mindwipe", APLocale.APLanguageType.UI)}", false);
                StartCoroutine(Mindwipe());
                break;
            case "Pushup Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("pushup", APLocale.APLanguageType.UI)}", false);
                plrcam.DoBodyWorkout(0);
                plrcam.ToggleWoundView(false); // DoBodyWorkout forces the woundview open/closed. this reverses that. nothing i can do about the sound effect though
                break;
            case "Temptation Trap":
                GameObject barrel = Instantiate(Resources.Load<GameObject>("minibarrel"), gameObject.transform.position, Quaternion.identity);
                WaterContainerItem barrelContents = barrel.GetComponent<WaterContainerItem>();
                barrelContents.AddLiquid("fentanyl",10000);
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("temptation", APLocale.APLanguageType.UI)}", false);
                break;
            case "Trip Trap":
                Vitals.Scream();
                Vitals.DropItem(0);
                Vitals.DropItem(1);
                Vitals.DropItem(2);
                Vitals.DropItem(3);
                Vitals.DropItem(4);
                Vitals.DropItem(5);
                Vitals.Ragdoll();
                Vitals.shock = 50;
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("trip", APLocale.APLanguageType.UI)}", false);
                break;
            default:
                Startup.Logger.LogError($"Trap item {TrapName} is unhandled!");
                APCanvas.EnqueueArchipelagoNotification($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{TrapName}{APLocale.Get("trapUnhandled", APLocale.APLanguageType.Errors)}",3);
                break;
        }
    }
    IEnumerator ReverseControls()
    {
        revControlActive = true;
        revControlEndTime = Time.realtimeSinceStartup + 10;
        Vitals.reversedControls = true;
        yield return new WaitForSecondsRealtime(10);
        Vitals.reversedControls = false;
        revControlActive = false;
    }
    IEnumerator UnchippedToggle()
    {
        if (worldgen.unchippedMode)
        {
            yield break; // the layer is solarstuck or the run is already unchipped.
        }
        unchippedActive = true;
        unchippedEndTime = Time.realtimeSinceStartup + 50;
        worldgen.unchippedMode = true;
        yield return new WaitForSecondsRealtime(50);
        worldgen.unchippedMode = false;
        GameObject.Find("LineOfSight").SetActive(false);
        unchippedActive = false;
    }
    IEnumerator Disfigurement()
    {
        disfigActive = true;
        disfigEndTime = Time.realtimeSinceStartup + 180;
        var prevHeadHealth = Vitals.limbs[0].muscleHealth; // disfiguring removes 25 muscle health...
        var prevHeadSkin = Vitals.limbs[0].skinHealth; // removes 50 skin health...
        var prevHeadBleed = Vitals.limbs[0].bleedAmount; // causes bleeding...
        var prevTrauma = Vitals.traumaAmount; // and adds 50 trauma
        Vitals.Disfigure();
        yield return new WaitForSecondsRealtime(180);
        Vitals.disfigured = false;
        Vitals.limbs[0].muscleHealth = prevHeadHealth; // so we restore them all afterwards
        Vitals.limbs[0].skinHealth = prevHeadSkin;
        Vitals.limbs[0].bleedAmount = prevHeadBleed;
        Vitals.traumaAmount = prevTrauma;
        disfigActive = false;
    }
    IEnumerator Mindwipe()
    {
        mindwipeActive = true;
        mindwipeEndTime = Time.realtimeSinceStartup + 70;
        var preWipehl = Vitals.hearingLoss; // Mindwipe causes a lot of hearing loss, so we restore it after
        var preWipebh = Vitals.brainHealth; // Again, Mindwipe changes brain health, so we restore it
        Skills skills = Vitals.skills;
        int INTSkillPreWipe = skills.INT; // Mindwipe resets INT to 0, so we'll save it to restore after
        float INTExpPreWipe = skills.expINT;
        int INTMaxPreWipe = skills.maxINT;
        int INTMinPreWipe = skills.minINT;
        MindwipeScript mw = Vitals.gameObject.AddComponent<MindwipeScript>();
        yield return new WaitForSecondsRealtime(70);
        Destroy(mw);
        Destroy(GameObject.Find("Main Camera/Canvas/MindwipeVignette(Clone)"));
        skills.INT = INTSkillPreWipe;
        skills.expINT = +INTExpPreWipe;
        skills.maxINT = INTMaxPreWipe;
        skills.minINT = INTMinPreWipe;
        Vitals.hearingLoss = preWipehl;
        if (Vitals.alive) Vitals.brainHealth = preWipebh; // only if Experiment didn't die during the mindwipe
        mindwipeActive = false;
    }
    IEnumerator Thornback()
    {
        plrcam.currentThreatTheme = 15; // play the Elder Thornback first phase theme
        plrcam.threatMusicTime = 90; // for 90 seconds
        thornbackTrapLevel = 1;
        yield return new WaitForSecondsRealtime(11); // timed to be on the beat drop for maximum effect (because i'm just cool like that)
        thornbackTrapLevel = 2;
        yield return new WaitForSecondsRealtime(79);
        thornbackTrapLevel = 0;
    }
}