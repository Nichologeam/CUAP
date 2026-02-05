using Archipelago.MultiClient.Net;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CUAP;

public class Moodlesanity : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private MoodleManager Moodles;
    public List<string> AlreadySentChecks = new List<string>();
    private WorldGeneration worldgen;
    private static readonly long startingMoodleId = 22318200;
    private static List<string> MoodleNameList = new List<string>()
    {   // In the same order as in locations.py and the game's EN.json
        "braindamage3",
        "braindamage2",
        "braindamage1",
        "braindamage0",
        "cantbreathe3",
        "thoraxdestroyed3",
        "oxygen0",
        "oxygen1",
        "oxygen2",
        "cardiacarrest3",
        "pain0",
        "pain1",
        "pain2",
        "pain3",
        "shock3",
        "overdose0",
        "overdose1",
        "overdose2",
        "overdose3",
        "withdrawl0",
        "withdrawl1",
        "withdrawl2",
        "withdrawl3",
        "drugoverdose3",
        "bleeding0",
        "bleeding1",
        "bleeding2",
        "bleeding3",
        "internalBleed1", // fun fact, this is the only moodle with a captial in its internal name
        "lowbloodvolume0",
        "lowbloodvolume1",
        "lowbloodvolume2",
        "lowbloodvolume3",
        "highbloodvolume0",
        "highbloodvolume1",
        "highbloodvolume2",
        "highbloodvolume3",
        "exertion0",
        "exertion1",
        "exertion2",
        "exertion3",
        "brokenbone",
        "dislocation",
        "brokenneck1",
        "brokenribs1",
        "dislocatedjaw1",
        "dislocatedspine1",
        "infected0",
        "infected1",
        "infected2",
        "infected3",
        "sepsis1",
        "sepsis2",
        "sepsis3",
        "concussion3",
        "unconscious3",
        "confused3",
        "asleep4",
        "confused0",
        "confused1",
        "confused2",
        "tired0",
        "tired1",
        "tired2",
        "tired3",
        "hunger0",
        "hunger1",
        "hunger2",
        "hunger3",
        "hunger4",
        "hunger5",
        "thirst0",
        "thirst1",
        "thirst2",
        "thirst3",
        "overhydrated0",
        "overhydrated1",
        "overhydrated2",
        "sick0",
        "sick1",
        "sick2",
        "sick3",
        "hot0",
        "hot1",
        "hot2",
        "hot3",
        "cold0",
        "cold1",
        "cold2",
        "cold3",
        "wet0",
        "wet1",
        "wet2",
        "wet3",
        "underweight0",
        "underweight1",
        "underweight2",
        "underweight3",
        "overweight0",
        "overweight1",
        "overweight2",
        "overweight3",
        "sad0",
        "gloomy1",
        "depression2",
        "miserable3",
        "trauma1",
        "trauma2",
        "trauma3",
        "happy4",
        "happy5",
        "happy6",
        "happy7",
        "impairedspeech0",
        "hearingloss0",
        "hearingloss1",
        "hearingloss2",
        "autopump",
        "encumbered0",
        "encumbered1",
        "encumbered2",
        "encumbered3",
        "irradiated0",
        "irradiated1",
        "irradiated2",
        "irradiated3",
        "energized5",
        "hemothorax1",
        "hollow0",
        "badsleep0",
        "lastleg8",
        "dirty0",
        "dirty1",
        "fightorflight",
        "tachycardia",
        "bradycardia",
        "rnrs0",
        "rippedjaw3",
        "rippedeye2",
        "rippedeye3",
        "lowimmunity1",
        "highimmunity5",
        "amputation3",
        "clawdamage0",
        "clawdamage1",
        "keratinbooster5",
        "impendingdoom1",
        "horrified3",
    };

    private void OnEnable()
    {
        Client = APClientClass.session;
        Moodles = this.gameObject.GetComponent<MoodleManager>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        var options = APClientClass.slotdata;
        if (options.TryGetValue("Moodlesanity", out var moodlesanityoption)) // check if moodlesanity is enabled.
        {
            if (!Convert.ToBoolean(moodlesanityoption))
            {
                Startup.Logger.LogWarning("Moodlesanity is disabled, destroying script.");
                DestroyImmediate(this);
                return;
            }
        }
        Startup.Logger.LogMessage("Moodlesanity is monitoring moodles...");
    }
    private void Update()
    {
        Moodle[] moodleComponents = Moodles.GetComponentsInChildren<Moodle>();
        foreach (Moodle mood in moodleComponents) // For each moodle, send its check.
        {
            if (AlreadySentChecks.Contains(mood.type))
            {
                continue; // Avoid spamming the server by not even attempting to send a check we already have sent.
            }
            if (mood.type == "lowimmunity1" && worldgen.loadingObject.activeSelf)
            {
                continue; // There's a bug where Experiment is Immunocompromised for the first few frames during worldgen. This if statement makes the check not send in that case.
            }
            var moodleIndex = MoodleNameList.IndexOf(mood.type);
            if (moodleIndex == -1) // couldn't find it. try secondary method
            {
                string baseName = System.Text.RegularExpressions.Regex.Replace(mood.type, @"\d+$", ""); // remove the number at the end
                moodleIndex = MoodleNameList.FindIndex(name => System.Text.RegularExpressions.Regex.Replace(name, @"\d+$", "") == baseName);
                if (moodleIndex == -1) // still not found? throw error
                {
                    Startup.Logger.LogError($"Moodle {mood.type} is not in the Moodlesanity index list!");
                    APCanvas.EnqueueArchipelagoNotification($"Moodlesanity Error! Moodle {mood.type} is not in the Moodlesanity index list!",3);
                }
            }
            var CheckID = moodleIndex + startingMoodleId;
            APClientClass.ChecksToSend.Add(CheckID);
            AlreadySentChecks.Add(mood.type);
        }
    }
}