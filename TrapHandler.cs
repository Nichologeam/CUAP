using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using HarmonyLib;
using Archipelago.MultiClient.Net;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private Body Vitals;
    private WorldGeneration worldgen;
    private PlayerCamera plrcam;
    private MoodleManager moodles;
    private readonly AccessTools.FieldRef<MoodleManager, float> moodleUpdateTime = AccessTools.FieldRefAccess<MoodleManager, float>("updateTime"); // this variable is private normally (this is the only reason harmony is included in this project)
    private float prevUpdateTime = 0.5f;
    private bool revControlActive;
    private bool unchippedActive;
    private bool disfigActive;
    public static bool mindwipeActive;
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
    private void LateUpdate() // lateupdate so it's after normal moodle updates (no race condition, yippee!)
    {
        if (moodleUpdateTime(moodles) > prevUpdateTime && Vitals.alive) // timer reset, moodles were updated
        {
            if (revControlActive)
            {
                moodles.AddMoodle(5, "confused", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Reversed Controls", trapSender + " reversed your controls! Lasts 10 seconds.", false, false);
            }
            if (unchippedActive)
            {
                moodles.AddMoodle(5, "death", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Unchipped", trapSender + " disabled your brainchip! Lasts 50 seconds.", false, false);
            }
            if (disfigActive)
            {
                moodles.AddMoodle(5, "dislocatedjaw", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Disfigured", trapSender + " removed your jaw! Lasts 180 seconds.", false, false);
            }
            if (mindwipeActive)
            {
                moodles.AddMoodle(5, "hollow", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Mindwipe", trapSender + " removed your memories! Lasts 70 seconds.", false, false);
            }
        }
        prevUpdateTime = moodleUpdateTime(moodles);
    }
    public void ProcessTraps(string TrapName, string ItemSender)
    {
        trapSender = ItemSender;
        if (TrapName == "Depression Trap")
        {
            Vitals.happiness = -20;
            plrcam.DoAlert("Trap: " + ItemSender + " said something demoralizing. Happiness decreased.", false);
        }
        if (TrapName == "Hearing Loss Trap")
        {
            Vitals.hearingLoss = +50;
            plrcam.DoAlert("Trap: <b>WHAT!? I CAN'T HEAR YOU " + ItemSender.ToUpper() + "!</b> Hearing loss increased.", false);
        }
        if (TrapName == "Earthquake Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " hit a fault line.", false);
            worldgen.earthquakeDelay = 0; // start an earthquake
            worldgen.earthquakeIntensity = 2; // twice as intense as basegame earthquake
            worldgen.earthquakeTime = 15; // for 15 seconds
        }
        if (TrapName == "Reverse Controls Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " made you feel tipsy. Controls reversed.", false);
            StartCoroutine(ReverseControls());
        }
        if (TrapName == "Sleep Trap")
        {
            Vitals.sleeping = true;
            plrcam.DoAlert("Trap: " + ItemSender + " thinks it's naptime. Good night!", false);
        }
        if (TrapName == "Unchipped Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " is hacking into your brainchip!", false);
            StartCoroutine(UnchippedToggle());
        }
        if (TrapName == "Elder Thornback Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " sent something big your way. Something <i>really</i> big.", false);
            StartCoroutine(Thornback());
        }
        if (TrapName == "Cave Ticks Trap")
        {
            Instantiate(Resources.Load<GameObject>("caveticks"), gameObject.transform.position, Quaternion.identity);
            plrcam.DoAlert("Trap: " + ItemSender + " alerted the hoard. Good luck!", false);
        }
        if (TrapName == "Bad Rep Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " spread gossip. All traders on this layer are now hostile.", false);
            foreach (var trader in FindObjectsOfType<TraderScript>())
            {
                if (trader.hostile) continue; // don't bother making them hostile a second time
                trader.hostility = 500;
                Vitals.happiness += 3f; // counteract hostility happiness decrease
            }
        }
        if (TrapName == "Disfigured Trap" && !Vitals.disfigured)
        {
            plrcam.DoAlert("Trap: " + ItemSender + " thinks you talk too much.", false);
            StartCoroutine(Disfigurement());
        }
        if (TrapName == "Fellow Experiment")
        {
            Instantiate(Resources.Load<GameObject>("corpse"), gameObject.transform.position, Quaternion.identity);
            plrcam.DoAlert("Trap: " + ItemSender + " found you a friend! ...wait", false);
        }
        if (TrapName == "Fragile Items Trap")
        {
            heldItems.Clear();
            foreach (var slot in FindObjectsOfType<InventorySlot>())
            {
                try
                {
                    heldItems.Add(slot.gameObject.GetComponentInChildren<Item>());
                    Debug.Log(slot.limb.fullName + " has " + slot.gameObject.GetComponentInChildren<Item>().fullName);
                }
                catch
                {
                    continue;
                }
            }
            Item chosenItem = heldItems.ElementAt(UnityEngine.Random.Range(0, heldItems.Count + 1));
            Debug.Log(chosenItem.fullName + " was chosen to be damaged");
            if (chosenItem.TryGetComponent<WaterContainerItem>(out WaterContainerItem _))
            {
                plrcam.DoAlert("Trap: " + ItemSender + " poked a hole in your " + chosenItem.fullName, false);
            }
            else
            {
                plrcam.DoAlert("Trap: " + ItemSender + " made your warranty expire. " + chosenItem.fullName + " was destroyed.", false);
            }
            chosenItem.condition = 0;
        }
        if (TrapName == "Mindwipe Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " thinks you know too much.", false);
            StartCoroutine(Mindwipe());
        }
        if (TrapName == "Pushup Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " demands you get on the ground and give them twenty!", false);
            plrcam.DoBodyWorkout(0);
            plrcam.ToggleWoundView(false); // DoBodyWorkout forces the woundview open/closed. this reverses that. nothing i can do about the sound effect though
        }
    }
    IEnumerator ReverseControls()
    {
        revControlActive = true;
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
        worldgen.unchippedMode = true;
        yield return new WaitForSecondsRealtime(50);
        worldgen.unchippedMode = false;
        GameObject.Find("LineOfSight").SetActive(false);
        unchippedActive = false;
    }
    IEnumerator Disfigurement()
    {
        disfigActive = true;
        var prevHeadHealth = Vitals.limbs[0].muscleHealth; // disfiguring removes 25 muscle health
        Vitals.disfigured = true;
        yield return new WaitForSecondsRealtime(180);
        Vitals.disfigured = false;
        Vitals.limbs[0].muscleHealth = prevHeadHealth; // so we restore that afterwards
        disfigActive = false;
    }
    IEnumerator Mindwipe()
    {
        mindwipeActive = true;
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
        Destroy(GameObject.Find("Main Camera/Canvas/MindwipeViginette(Clone)"));
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
        moodles.horrifiedLevel = 1;
        yield return new WaitForSecondsRealtime(11); // timed to be on the beat drop for maximum effect (because i'm just cool like that)
        moodles.horrifiedLevel = 3;
        yield return new WaitForSecondsRealtime(79);
        moodles.horrifiedLevel = 0;
    }
}