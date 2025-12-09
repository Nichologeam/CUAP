using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace CUAP;

public class Moodlesanity : MonoBehaviour
{
    public static ApClient Client;
    private MoodleManager Moodles;
    private List<string> AlreadySentChecks = new List<string>();
    private WorldGeneration worldgen;
    private static Dictionary<string, int> MoodleNametoCheckID = new Dictionary<string, int>()
    {   // In the same order as in locations.py and the game's EN.json
        {"braindamage3",-966812996},
        {"braindamage2",-966812995},
        {"braindamage1",-966812994},
        {"braindamage0",-966812993},
        {"cantbreathe3",-966812992},
        {"thoraxdestroyed3",-966812991},
        {"oxygen0",-966812990},
        {"oxygen1",-966812989},
        {"oxygen2",-966812988},
        {"cardiacarrest3",-966812987},
        {"pain0",-966812986},
        {"pain1",-966812985},
        {"pain2",-966812984},
        {"pain3",-966812983},
        {"shock3",-966812982},
        {"overdose0",-966812981},
        {"overdose1",-966812980},
        {"overdose2",-966812979},
        {"overdose3",-966812978},
        {"withdrawl0",-966812977},
        {"withdrawl1",-966812976},
        {"withdrawl2",-966812975},
        {"withdrawl3",-966812974},
        {"drugoverdose3", -96612973},
        {"bleeding0",-966812972},
        {"bleeding1",-966812971},
        {"bleeding2",-966812970},
        {"bleeding3",-966812969},
        {"internalBleed1",-966812968}, // fun fact, this is the only moodle with a captial in its internal name
        {"lowbloodvolume0",-966812967},
        {"lowbloodvolume1",-966812966},
        {"lowbloodvolume2",-966812965},
        {"lowbloodvolume3",-966812964},
        {"highbloodvolume0",-966812963},
        {"highbloodvolume1",-966812962},
        {"highbloodvolume2",-966812961},
        {"highbloodvolume3",-966812960},
        {"exertion0",-966812959},
        {"exertion1",-966812958},
        {"exertion2",-966812957},
        {"exertion3",-966812956},
        {"brokenbone1",-966812955},
        {"brokenbone2",-966812955},
        {"brokenbone3",-966812955},
        {"dislocation1",-966812954},
        {"dislocation2",-966812954},
        {"dislocation3",-966812954},
        {"brokenneck1",-966812953},
        {"brokenribs1",-966812952},
        {"dislocatedjaw1",-966812951},
        {"dislocatedspine1",-966812950},
        {"infected0",-966812949},
        {"infected1",-966812948},
        {"infected2",-966812947},
        {"infected3",-966812946},
        {"sepsis1",-966812945},
        {"sepsis2",-966812944},
        {"sepsis3",-966812943},
        {"concussion3",-966812942},
        {"unconscious3",-966812941},
        {"confused3",-966812940},
        {"asleep4",-966812939},
        {"confused0",-966812938},
        {"confused1",-966812937},
        {"confused2",-966812936},
        {"tired0",-966812935},
        {"tired1",-966812934},
        {"tired2",-966812933},
        {"tired3",-966812932},
        {"hunger0",-966812931},
        {"hunger1",-966812930},
        {"hunger2",-966812929},
        {"hunger3",-966812928},
        {"hunger4",-966812927},
        {"hunger5",-966812926},
        {"thirst0",-966812925},
        {"thirst1",-966812924},
        {"thirst2",-966812923},
        {"thirst3",-966812922},
        {"overhydrated0", -966182921},
        {"overhydrated1", -966182920},
        {"overhydrated2", -966182919},
        {"sick0",-966812918},
        {"sick1",-966812917},
        {"sick2",-966812916},
        {"sick3",-966812915},
        {"hot0",-966812914},
        {"hot1",-966812913},
        {"hot2",-966812912},
        {"hot3",-966812911},
        {"cold0",-966812910},
        {"cold1",-966812909},
        {"cold2",-966812908},
        {"cold3",-966812907},
        {"wet0",-966812906},
        {"wet1",-966812905},
        {"wet2",-966812904},
        {"wet3",-966812903},
        {"underweight0",-966812902},
        {"underweight1",-966812901},
        {"underweight2",-966812900},
        {"underweight3",-966812899},
        {"overweight0",-966812898},
        {"overweight1",-966812897},
        {"overweight2",-966812896},
        {"overweight3",-966812895},
        {"sad0",-966812894},
        {"gloomy1",-966812893},
        {"depressed2",-966812892},
        {"miserable3",-966812891},
        {"trauma1",-966812890},
        {"trauma2",-966812889},
        {"trauma3",-966812888},
        {"happy4",-966812887},
        {"happy5",-966812886},
        {"happy6",-966812885},
        {"happy7",-966812884},
        {"impairedspeech0",-966812883},
        {"hearingloss0",-966812882},
        {"hearingloss1",-966812881},
        {"hearingloss2",-966812880},
        {"autopump4",-966812879},
        {"autopump5",-966812879},
        {"autopump6",-966812879},
        {"autopump7",-966812879},
        {"autopump8",-966812879},
        {"encumbered0",-966812878},
        {"encumbered1",-966812877},
        {"encumbered2",-966812876},
        {"encumbered3",-966812875},
        {"irradiated0",-966812874},
        {"irradiated1",-966812873},
        {"irradiated2",-966812872},
        {"irradiated3",-966812871},
        {"energized5",-966812870},
        {"hemothorax1",-966812869},
        {"hollow0",-966812868},
        {"badsleep0",-966812867},
        {"lastleg8",-966812866},
        {"dirty0",-966812865},
        {"dirty1",-966812864},
        {"fightorflight0",-966812863},
        {"fightorflight1",-966812863},
        {"tachycardia0",-966812862},
        {"tachycardia1",-966812862},
        {"bradycardia0",-966812861},
        {"bradycardia1",-966812861},
        {"rnrs0",-966812860},
        {"rippedjaw3",-966812859},
        {"rippedeye2",-966812858},
        {"rippedeye3",-966812857},
        {"lowimmunity1",-966812856},
        {"highimmunity5",-966812855},
        {"amputation3",-966812854},
        {"clawdamage0",-966812853},
        {"clawdamage1",-966812852},
        {"keratinbooster5",-966812851},
        {"impendingdoom1",-966812850},
        {"horrified3",-966812849},
    };

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Moodles = this.gameObject.GetComponent<MoodleManager>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("Moodlesanity", out var moodlesanityoption)) // check if moodlesanity is enabled.
        {
            if (!Convert.ToBoolean(moodlesanityoption))
            {
                Startup.Logger.LogWarning("Moodlesanity is disabled, destroying script.");
                Destroy(this);
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
            if (mood.type == "Immunocompromised" && worldgen.loadingObject.activeSelf)
            {
                continue; // There's a bug where Experiment is Immunocompromised for the first few frames during worldgen. This if statement makes the check not send in that case.
            }
            MoodleNametoCheckID.TryGetValue(mood.type, out int CheckID);
            APClientClass.ChecksToSendQueue.Enqueue(CheckID);
            AlreadySentChecks.Add(mood.type);
        }
    }
}