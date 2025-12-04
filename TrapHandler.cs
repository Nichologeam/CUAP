using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ApClient Client;
    private Body Vitals;
    private WorldGeneration worldgen;
    private PlayerCamera plrcam;
    private List<Item> heldItems = new List<Item>();

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Vitals = this.gameObject.GetComponent<Body>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    public void ProcessTraps(string TrapName)
    {
        if (TrapName == "Depression Trap")
        {
            Vitals.happiness =- 20;
        }
        if (TrapName == "Hearing Loss Trap")
        {
            Vitals.hearingLoss =+ 50;
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
            if (UnityEngine.Random.Range(0, 1) == 0)
            {
                plrcam.currentThreatTheme = 15; // play the Elder Thornback first phase theme
            }
            else
            {
                plrcam.currentThreatTheme = 10; // play the Elder Thornback second phase theme
            }
            plrcam.threatMusicTime = 3000; // for 3000 frames (50 seconds)
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
        Vitals.reversedControls = true;
        yield return new WaitForSecondsRealtime(10);
        Vitals.reversedControls = false;
    }
    IEnumerator UnchippedToggle()
    {
        worldgen.unchippedMode = true;
        yield return new WaitForSecondsRealtime(90);
        worldgen.unchippedMode = false;
    }
    IEnumerator Disfigurement()
    {
        Vitals.disfigured = true;
        yield return new WaitForSecondsRealtime(180);
        Vitals.disfigured = false;
    }
    IEnumerator Mindwipe()
    {
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
        skills.expINT =+ INTExpPreWipe;
        skills.maxINT = INTMaxPreWipe;
        skills.minINT = INTMinPreWipe;
    }
}