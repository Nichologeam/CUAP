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
    private bool faultyActive;
    private float faultyEndTime;
    private bool disfigActive;
    private float disfigEndTime;
    private int thornbackTrapLevel;
    public static bool mindwipeActive;
    private float mindwipeEndTime;
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
        if (!APClientClass.hasBrainchip)
        {
            worldgen.unchippedMode = true;
            plrcam.SetUnchippedUI(true);
        }
        if (Vitals.alive && APClientClass.hasBrainchip && faultyActive) // randomize a bunch of things on the display
        {
            WoundView hp = WoundView.view;
            if (hp == null)
            {
                return;
            }
            
            // raw numbers
            hp.brainHealthText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 101)); // inclusive, exclusive
            hp.conscText.text = string.Format("{0}%", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.painText.text = string.Format("{0}%", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.bloodText.text = string.Format("{0:0.00}l", UnityEngine.Random.Range(0f, 5f)); // inclusive, inclusive
            hp.bleedText.text = string.Format("{0:0.00}l/m", UnityEngine.Random.Range(0f, 5f));
            hp.oxyText.text = string.Format("{0}%", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.immunityText.text = string.Format("{0}%", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.hungerText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 126));
            hp.thirstText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 201));
            hp.sickText.text = string.Format("{0}%", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.energyText.text = string.Format("{0}%", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.happyText.text = string.Format("{0:0.0}", UnityEngine.Random.Range(-100f, 100f));
            hp.radText.text = string.Format("{0:0.0}gy", UnityEngine.Random.Range(0f, 30f));
            hp.weightText.text = string.Format("{0:0.#}{1}", UnityEngine.Random.Range(10f, 80f), Units.WeightUnit(false));
            hp.tempText.text = UnityEngine.Random.Range(25f, 60f).ToString("0.#") + Units.TemperatureUnit(true); // doesn't work with string.Format (the vanilla game does this too)
            hp.skinText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.muscleText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.limbBleedText.text = string.Format("{0:0.00}\nl/m", UnityEngine.Random.Range(0f, 5f));
            hp.limbPainText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.infecText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.limbForceText.text = string.Format("{0}", UnityEngine.Random.RandomRangeInt(0, 101));
            hp.limbHealTimeText.text = string.Format("{0}m\n{1}s", UnityEngine.Random.RandomRangeInt(0, 20), UnityEngine.Random.RandomRangeInt(0, 60)); // max 19:59
            hp.bpmText.text = UnityEngine.Random.RandomRangeInt(0, 301) + " | " + UnityEngine.Random.RandomRangeInt(0, 301);
            hp.respiratoryText.text = string.Format("{0}/m", UnityEngine.Random.Range(0f, 25f));
            hp.opiumText.text = UnityEngine.Random.RandomRangeInt(-80, 80) + "op";
            hp.toxinText.text = string.Format("{0:0.0}ppm", UnityEngine.Random.Range(0f, 100f) * 0.1f);

            // displays and graphics
            hp.limbForceFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.limbBleedOuterImage.enabled = true;
            hp.limbBleedFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.limbFractureImage.enabled = true;
            hp.limbFractureImage.sprite = UnityEngine.Random.RandomRangeInt(0 ,2) == 1 ? hp.limbFracturedSprite : hp.limbDislocatedSprite; // random if fractured or dislocated
            hp.happinessIcon.sprite = hp.moodIcons[UnityEngine.Random.RandomRangeInt(0, 6)];
            hp.brainFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f); // the real game also does GetComponent on every frame
            hp.bloodFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.tempFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.foodFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.overfeedFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.overhydrateFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.thirstFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.muscleFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.skinFill.GetComponent<FilledImagePP>().fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.skinFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.brainFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.radFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.lungFill1.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.lungFill2.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.hemothoraxFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.opiumBack.enabled = true;
            hp.opiumFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            hp.toxinBack.enabled = true;
            hp.toxinFill.fillAmount = UnityEngine.Random.Range(0f, 1f);
            foreach (UnityEngine.UI.Image limb in hp.limbImages)
            {
                limb.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                // i was going to add different limb icons as well, but those are actually VERY complicated
            }
            hp.eyeGoneImage.enabled = UnityEngine.Random.RandomRangeInt(0, 2) == 1;
            hp.bothEyesGoneImage.enabled = UnityEngine.Random.RandomRangeInt(0, 2) == 1;
            hp.disfiguredImage.enabled = UnityEngine.Random.RandomRangeInt(0, 2) == 1;
            hp.cargoImage.enabled = UnityEngine.Random.RandomRangeInt(0, 2) == 1;
        }
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
            if (faultyActive)
            {
                float remaining = Mathf.Max(0, faultyEndTime - Time.realtimeSinceStartup);
                TimeSpan ts = TimeSpan.FromSeconds(remaining);
                string formatted = $"{ts.Minutes:D2}:{ts.Seconds:D2}";
                string desc = APLocale.Get("faultyTrapDesc", APLocale.APLanguageType.UI);
                desc = desc.Replace("<time>", formatted);
                moodles.AddMoodle(5, "death", $"{APCanvas.coloredAPText}{APLocale.Get("faultyTrap", APLocale.APLanguageType.UI)}", desc, false, false);
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
            Vitals.horrifiedLevel = 20;
        }
        else if (thornbackTrapLevel == 2)
        {
            Vitals.horrifiedLevel = 100;
        }
    }
    public void ProcessTraps(string TrapName, string ItemSender)
    {
        switch (TrapName)
        {
            case "Depression Trap":
                Vitals.happiness -= 20;
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("depression", APLocale.APLanguageType.UI)}", false);
                break;
            case "Hearing Loss Trap":
                Vitals.hearingLoss += 50;
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
            case "Fellow Sawian":
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
                Startup.Logger.LogInfo($"{chosenItem.fullName} was chosen to be damaged");
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
            case "Cheeseburger Trap":
                SettingDropdown unitsSetting = Settings.Get<SettingDropdown>("units");
                unitsSetting.SetValue(2); // cheeseburger units of measurement
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("burger", APLocale.APLanguageType.UI)}", false);
                break;
            case "Faulty Chip Trap":
                plrcam.DoAlert($"{APLocale.Get("trap", APLocale.APLanguageType.UI)}{ItemSender}{APLocale.Get("chip", APLocale.APLanguageType.UI)}", false);
                StartCoroutine(FaultyChip());
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
            yield break; // the layer is solarstruck or the run is already unchipped.
        }
        unchippedActive = true;
        unchippedEndTime = Time.realtimeSinceStartup + 50;
        worldgen.unchippedMode = true;
        plrcam.SetUnchippedUI(true);
        yield return new WaitForSecondsRealtime(50);
        worldgen.unchippedMode = false;
        plrcam.SetUnchippedUI(false);
        PlayerCamera.main.LOSHole.GetComponent<VisionMask>().ClearMask();
        unchippedActive = false;
    }
    IEnumerator FaultyChip()
    {
        if (worldgen.unchippedMode)
        {
            yield break; // the layer is solarstruck or the run is unchipped. the trap will have no effect
        }
        faultyActive = true;
        faultyEndTime = Time.realtimeSinceStartup + 50;
        yield return new WaitForSecondsRealtime(50);
        faultyActive = false;
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
        var happinessPreHorror = Vitals.happiness;
        thornbackTrapLevel = 1;
        yield return new WaitForSecondsRealtime(11); // timed to be on the beat drop for maximum effect (because i'm just cool like that)
        thornbackTrapLevel = 2;
        yield return new WaitForSecondsRealtime(79);
        thornbackTrapLevel = 0;
        Vitals.happiness = happinessPreHorror;
    }
}