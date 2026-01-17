using CreepyUtil.Archipelago.ApClient;
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
    private static Dictionary<string, string> MoodleNametoCheckName = new Dictionary<string, string>()
    {   // In the same order as in locations.py and the game's EN.json
        {"braindamage3","Moodlesanity - Comatose"},
        {"braindamage2","Moodlesanity - Severe neurophysiological deterioration"},
        {"braindamage1","Moodlesanity - Neurological damage"},
        {"braindamage0","Moodlesanity - Cognitive impairment"},
        {"cantbreathe3","Moodlesanity - Respiratory arrest"},
        {"thoraxdestroyed3","Moodlesanity - Lung failure"},
        {"oxygen0","Moodlesanity - Hypoxemic"},
        {"oxygen1","Moodlesanity - Very hypoxemic"},
        {"oxygen2","Moodlesanity - Asphyxiating"},
        {"cardiacarrest3","Moodlesanity - Cardiac arrest"},
        {"pain0","Moodlesanity - Discomfort"},
        {"pain1","Moodlesanity - Pain"},
        {"pain2","Moodlesanity - Severe pain"},
        {"pain3","Moodlesanity - Agony"},
        {"shock3","Moodlesanity - Shock"},
        {"overdose0","Moodlesanity - Opiated"},
        {"overdose1","Moodlesanity - Drugged"},
        {"overdose2","Moodlesanity - Highly drugged"},
        {"overdose3","Moodlesanity - Fatal opioid overdose"},
        {"withdrawl0","Moodlesanity - Opioid craving"},
        {"withdrawl1","Moodlesanity - Withdrawl"},
        {"withdrawl2","Moodlesanity - Severe withdrawl"},
        {"withdrawl3","Moodlesanity - Dying of withdrawl"},
        {"drugoverdose3","Moodlesanity - Drug overdose"},
        {"bleeding0","Moodlesanity - Minor bleeding"},
        {"bleeding1","Moodlesanity - Bleeding"},
        {"bleeding2","Moodlesanity - Heavy bleeding"},
        {"bleeding3","Moodlesanity - Catastrophic bleeding"},
        {"internalBleed1","Moodlesanity - Internal bleeding"}, // fun fact, this is the only moodle with a captial in its internal name
        {"lowbloodvolume0","Moodlesanity - Pale"},
        {"lowbloodvolume1","Moodlesanity - Hypovolemic"},
        {"lowbloodvolume2","Moodlesanity - Critically hypovolemic"},
        {"lowbloodvolume3","Moodlesanity - Exsanguinated"},
        {"highbloodvolume0","Moodlesanity - Bloated"},
        {"highbloodvolume1","Moodlesanity - Hypervolemic"},
        {"highbloodvolume2","Moodlesanity - Critically hypervolemic"},
        {"highbloodvolume3","Moodlesanity - Lethally hypervolemic"},
        {"exertion0","Moodlesanity - Slightly exerted"},
        {"exertion1","Moodlesanity - Exerted"},
        {"exertion2","Moodlesanity - Highly exerted"},
        {"exertion3","Moodlesanity - Totally exhausted"},
        {"brokenbone1","Moodlesanity - Fractured bone"},
        {"brokenbone2","Moodlesanity - Fractured bone"},
        {"brokenbone3","Moodlesanity - Fractured bone"},
        {"dislocation1","Moodlesanity - Dislocated joint"},
        {"dislocation2","Moodlesanity - Dislocated joint"},
        {"dislocation3","Moodlesanity - Dislocated joint"},
        {"brokenneck1","Moodlesanity - Fractured neck"},
        {"brokenribs1","Moodlesanity - Fractured ribs"},
        {"dislocatedjaw1","Moodlesanity - Dislocated jaw"},
        {"dislocatedspine1","Moodlesanity - Dislocated spine"},
        {"infected0","Moodlesanity - Infection"},
        {"infected1","Moodlesanity - Painful infection"},
        {"infected2","Moodlesanity - Severe infection"},
        {"infected3","Moodlesanity - Life-threatening infection"},
        {"sepsis1","Moodlesanity - Sepsis"},
        {"sepsis2","Moodlesanity - Severe sepsis"},
        {"sepsis3","Moodlesanity - Septic shock"},
        {"concussion3","Moodlesanity - Concussed"},
        {"unconscious3","Moodlesanity - Unconscious"},
        {"confused3","Moodlesanity - Incapacitated"},
        {"asleep4","Moodlesanity - Sleeping"},
        {"confused0","Moodlesanity - Confused"},
        {"confused1","Moodlesanity - Very confused"},
        {"confused2","Moodlesanity - Fainting"},
        {"tired0","Moodlesanity - Drowsy"},
        {"tired1","Moodlesanity - Tired"},
        {"tired2","Moodlesanity - Very tired"},
        {"tired3","Moodlesanity - Half-asleep"},
        {"hunger0","Moodlesanity - Peckish"},
        {"hunger1","Moodlesanity - Hungry"},
        {"hunger2","Moodlesanity - Very hungry"},
        {"hunger3","Moodlesanity - Starving"},
        {"hunger4","Moodlesanity - Satiated"},
        {"hunger5","Moodlesanity - Full"},
        {"thirst0","Moodlesanity - Thirsty"},
        {"thirst1","Moodlesanity - Dehydrated"},
        {"thirst2","Moodlesanity - Parched"},
        {"thirst3","Moodlesanity - Desiccated"},
        {"overhydrated0","Moodlesanity - Slaked"},
        {"overhydrated1","Moodlesanity - Overhydrated"},
        {"overhydrated2","Moodlesanity - Water-intoxicated"},
        {"sick0","Moodlesanity - Queasy"},
        {"sick1","Moodlesanity - Nauseous"},
        {"sick2","Moodlesanity - Sick"},
        {"sick3","Moodlesanity - Grossly sick"},
        {"hot0","Moodlesanity - Warm"},
        {"hot1","Moodlesanity - Hot"},
        {"hot2","Moodlesanity - Hyperthermia"},
        {"hot3","Moodlesanity - Heatstroke"},
        {"cold0","Moodlesanity - Chilly"},
        {"cold1","Moodlesanity - Cold"},
        {"cold2","Moodlesanity - Hypothermia"},
        {"cold3","Moodlesanity - Freezing to death"},
        {"wet0","Moodlesanity - Damp"},
        {"wet1","Moodlesanity - Wet"},
        {"wet2","Moodlesanity - Soaked"},
        {"wet3","Moodlesanity - Water-logged"},
        {"underweight0","Moodlesanity - Underweight"},
        {"underweight1","Moodlesanity - Skinny"},
        {"underweight2","Moodlesanity - Malnourished"},
        {"underweight3","Moodlesanity - Emaciated"},
        {"overweight0","Moodlesanity - Chubby"},
        {"overweight1","Moodlesanity - Overweight"},
        {"overweight2","Moodlesanity - Fat"},
        {"overweight3","Moodlesanity - Obese"},
        {"sad0","Moodlesanity - Feeling down"},
        {"gloomy1","Moodlesanity - Gloomy"},
        {"depressed2","Moodlesanity - Depressed"},
        {"miserable3","Moodlesanity - Miserable"},
        {"trauma1","Moodlesanity - Scared"},
        {"trauma2","Moodlesanity - Traumatized"},
        {"trauma3","Moodlesanity - Shell shocked"},
        {"happy4","Moodlesanity - Satisfied"},
        {"happy5","Moodlesanity - Excited"},
        {"happy6","Moodlesanity - Happy"},
        {"happy7","Moodlesanity - Gleeful"},
        {"impairedspeech0","Moodlesanity - Impaired speech"},
        {"hearingloss0","Moodlesanity - Impaired hearing"},
        {"hearingloss1","Moodlesanity - Hearing loss"},
        {"hearingloss2","Moodlesanity - Severe hearing loss"},
        {"autopump4","Moodlesanity - Life support"},
        {"autopump5","Moodlesanity - Life support"},
        {"autopump6","Moodlesanity - Life support"},
        {"autopump7","Moodlesanity - Life support"},
        {"autopump8","Moodlesanity - Life support"},
        {"encumbered0","Moodlesanity - Heavy load"},
        {"encumbered1","Moodlesanity - Encumbered"},
        {"encumbered2","Moodlesanity - Very encumbered"},
        {"encumbered3","Moodlesanity - Hampered"},
        {"irradiated0","Moodlesanity - Uncomfortable"},
        {"irradiated1","Moodlesanity - Radiation sickness"},
        {"irradiated2","Moodlesanity - Severe radiation sickness"},
        {"irradiated3","Moodlesanity - Chernobyl wannabe"},
        {"energized5","Moodlesanity - Energized"},
        {"hemothorax1","Moodlesanity - Hemothorax"},
        {"hollow0","Moodlesanity - Hollow"},
        {"badsleep0","Moodlesanity - Bad sleep"},
        {"lastleg8","Moodlesanity - Last stand"},
        {"dirty0","Moodlesanity - Dirty"},
        {"dirty1","Moodlesanity - Very dirty"},
        {"fightorflight0","Moodlesanity - Adrenaline"},
        {"fightorflight1","Moodlesanity - Adrenaline"},
        {"tachycardia0","Moodlesanity - Tachycardia"},
        {"tachycardia1","Moodlesanity - Tachycardia"},
        {"bradycardia0","Moodlesanity - Bradycardia"},
        {"bradycardia1","Moodlesanity - Bradycardia"},
        {"rnrs0","Moodlesanity - Rapid neuron regeneration sickness"},
        {"rippedjaw3","Moodlesanity - Disfigured"},
        {"rippedeye2","Moodlesanity - Half-blind"},
        {"rippedeye3","Moodlesanity - Blind"},
        {"lowimmunity1","Moodlesanity - Immunocompromised"},
        {"highimmunity5","Moodlesanity - Immunocompetent"},
        {"amputation3","Moodlesanity - Amputated"},
        {"clawdamage0","Moodlesanity - Dulled claws"},
        {"clawdamage1","Moodlesanity - Broken claws"},
        {"keratinbooster5","Moodlesanity - Boosted regrowth"},
        {"impendingdoom1","Moodlesanity - Sense of impending doom"},
        {"horrified3","Moodlesanity - HORRIFIED"},
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
            MoodleNametoCheckName.TryGetValue(mood.type, out string CheckName);
            APClientClass.ChecksToSendQueue.Enqueue(CheckName);
            AlreadySentChecks.Add(mood.type);
        }
    }
}