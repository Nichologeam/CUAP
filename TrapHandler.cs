using CreepyUtil.Archipelago;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using HarmonyLib;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ApClient Client;
    private Body Vitals;
    private WorldGeneration worldgen;
    private PlayerCamera plrcam;
    private MoodleManager moodles;
    private readonly AccessTools.FieldRef<MoodleManager, float> moodleUpdateTime = AccessTools.FieldRefAccess<MoodleManager, float>("updateTime"); // this variable is private normally (this is the only reason harmony is included in this project)
    private float prevUpdateTime = 0.5f;
    private bool revControlActive;
    private bool unchippedActive;
    private bool disfigActive;
    private bool mindwipeActive;
    private List<Item> heldItems = new List<Item>();

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Vitals = this.gameObject.GetComponent<Body>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        moodles = GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<MoodleManager>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    private void LateUpdate() // lateupdate so it's after normal moodle updates (no race condition, yippee!)
    {
        if (moodleUpdateTime(moodles) > prevUpdateTime) // timer reset, moodles were updated
        {
            if (revControlActive)
            {
                moodles.AddMoodle(5, "confused", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Reversed Controls", "Somebody reversed your controls! Lasts 10 seconds.", false, false);
            }
            if (unchippedActive)
            {
                moodles.AddMoodle(5, "death", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Unchipped", "Somebody disabled your brainchip! Lasts 50 seconds.", false, false);
            }
            if (disfigActive)
            {
                moodles.AddMoodle(5, "dislocatedjaw", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Disfigured", "Somebody removed your jaw! Lasts 180 seconds.", false, false);
            }
            if (mindwipeActive)
            {
                moodles.AddMoodle(5, "hollow", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Mindwipe", "Somebody removed your memories! Lasts 70 seconds.", false, false);
            }
        }
        prevUpdateTime = moodleUpdateTime(moodles);
    }
    public void ProcessTraps(string TrapName)
    {
        if (TrapName == "Depression Trap")
        {
            Vitals.happiness = -20;
        }
        if (TrapName == "Hearing Loss Trap")
        {
            Vitals.hearingLoss = +50;
        }
        if (TrapName == "Earthquake Trap")
        {
            worldgen.earthquakeDelay = 0; // start an earthquake
            worldgen.earthquakeIntensity = 2; // twice as intense as basegame earthquake
            worldgen.earthquakeTime = 15; // for 15 seconds
        }
        if (TrapName == "Reverse Controls Trap")
        {
            StartCoroutine(ReverseControls());
        }
        if (TrapName == "Sleep Trap")
        {
            Vitals.sleeping = true;
        }
        if (TrapName == "Unchipped Trap")
        {
            StartCoroutine(UnchippedToggle());
        }
        if (TrapName == "Elder Thornback Trap")
        {
            StartCoroutine(Thornback());
        }
        if (TrapName == "Cave Ticks Trap")
        {
            Instantiate(Resources.Load<GameObject>("caveticks"), gameObject.transform.position, Quaternion.identity);
        }
        if (TrapName == "Bad Rep Trap")
        {
            foreach (var trader in FindObjectsOfType<TraderScript>())
            {
                trader.hostility = 500;
            }
        }
        if (TrapName == "Disfigured Trap" && !Vitals.disfigured)
        {
            StartCoroutine(Disfigurement());
        }
        if (TrapName == "Fellow Experiment")
        {
            Instantiate(Resources.Load<GameObject>("corpse"), gameObject.transform.position, Quaternion.identity);
        }
        if (TrapName == "Fragile Items Trap")
        {
            heldItems.Clear();
            foreach (var slot in FindObjectsOfType<InventorySlot>())
            {
                try
                {
                    heldItems.Add(slot.gameObject.GetComponentInChildren<Item>());
                }
                catch
                {
                    continue;
                }
            }
            Item chosenItem = heldItems.ElementAt(UnityEngine.Random.Range(0, heldItems.Count));
            chosenItem.condition = 0.01f;
        }
        if (TrapName == "Mindwipe Trap")
        {
            StartCoroutine(Mindwipe());
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
        Vitals.disfigured = true;
        yield return new WaitForSecondsRealtime(180);
        Vitals.disfigured = false;
        disfigActive = false;
    }
    IEnumerator Mindwipe()
    {
        mindwipeActive = true;
        Skills skills = Vitals.gameObject.GetComponent<Skills>();
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
        mindwipeActive = false;
    }
    IEnumerator Thornback()
    {
        plrcam.currentThreatTheme = 15; // play the Elder Thornback first phase theme
        plrcam.threatMusicTime = 1000; // for 1000 frames
        moodles.horrifiedLevel = 1;
        yield return plrcam.threatMusicTime == 0;
        moodles.horrifiedLevel = 0;
    }
}