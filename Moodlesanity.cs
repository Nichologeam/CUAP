using Archipelago.MultiClient.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CUAP;

public class Moodlesanity : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private MoodleManager Moodles;
    public List<string> AlreadySentChecks = new List<string>();
    private WorldGeneration worldgen;
    public static bool questboardMode = false;
    private static List<string> questboardList = [];
    public static List<string> questsAvailable = [];
    public static int maxQuests = 14;
    private static readonly long startingMoodleId = 22318200;
    private static List<string> MoodleInternalNameList = new List<string>()
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
        "withdrawal0",
        "withdrawal1",
        "withdrawal2",
        "withdrawal3",
        "stimulants6",
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
        "brokenneck",
        "brokenribs",
        "dislocatedjaw",
        "dislocatedspine",
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
        "overhydrated3",
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
        "hollow",
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
    private static Dictionary<string, string> InternalMoodNameToCheck = new Dictionary<string, string>()
    {
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
        {"withdrawal0","Moodlesanity - Opioid craving"},
        {"withdrawal1","Moodlesanity - Withdrawal"},
        {"withdrawal2","Moodlesanity - Severe withdrawal"},
        {"withdrawal3","Moodlesanity - Dying of withdrawal"},
        {"stimulants6","Moodlesantiy - Stimulated"},
        {"drugoverdose3","Moodlesanity - Drug overdose"},
        {"bleeding0","Moodlesanity - Minor bleeding"},
        {"bleeding1","Moodlesanity - Bleeding"},
        {"bleeding2","Moodlesanity - Heavy bleeding"},
        {"bleeding3","Moodlesanity - Catastrophic bleeding"},
        {"internalBleed1","Moodlesanity - Internal bleeding"},
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
        {"brokenbone0","Moodlesanity - Fractured bone"},
        {"brokenbone1","Moodlesanity - Fractured bone"},
        {"brokenbone2","Moodlesanity - Fractured bone"},
        {"brokenbone3","Moodlesanity - Fractured bone"},
        {"dislocation0","Moodlesanity - Dislocated joint"},
        {"dislocation1","Moodlesanity - Dislocated joint"},
        {"dislocation2","Moodlesanity - Dislocated joint"},
        {"dislocation3","Moodlesanity - Dislocated joint"},
        {"brokenneck0","Moodlesanity - Fractured neck"},
        {"brokenneck1","Moodlesanity - Fractured neck"},
        {"brokenneck2","Moodlesanity - Fractured neck"},
        {"brokenneck3","Moodlesanity - Fractured neck"},
        {"brokenribs0","Moodlesanity - Fractured ribs"},
        {"brokenribs1","Moodlesanity - Fractured ribs"},
        {"brokenribs2","Moodlesanity - Fractured ribs"},
        {"brokenribs3","Moodlesanity - Fractured ribs"},
        {"dislocatedjaw0","Moodlesanity - Dislocated jaw"},
        {"dislocatedjaw1","Moodlesanity - Dislocated jaw"},
        {"dislocatedjaw2","Moodlesanity - Dislocated jaw"},
        {"dislocatedjaw3","Moodlesanity - Dislocated jaw"},
        {"dislocatedspine0","Moodlesanity - Dislocated spine"},
        {"dislocatedspine1","Moodlesanity - Dislocated spine"},
        {"dislocatedspine2","Moodlesanity - Dislocated spine"},
        {"dislocatedspine3","Moodlesanity - Dislocated spine"},
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
        {"overhydrated3","Moodlesanity - Water-intoxicated"},
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
        {"depression2","Moodlesanity - Depressed"},
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
        {"hollow5","Moodlesanity - Hollow"},
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
    public static Dictionary<string, string> CheckToInternalMoodID = InternalMoodNameToCheck.GroupBy(kv => kv.Value)
    .ToDictionary(
        g => g.Key,
        g => Regex.Replace(g.First().Key, @"\d+$", "") // remove numbers
    );

    private void OnEnable()
    {
        Client = APClientClass.session;
        Moodles = this.gameObject.GetComponent<MoodleManager>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        var options = APClientClass.slotdata;
        if (options.TryGetValue("Moodlesanity", out var moodlesanityoption)) // check if moodlesanity is enabled.
        {
            if ((long)moodlesanityoption == 1)
            {
                Startup.Logger.LogWarning("Moodlesanity is disabled, destroying script.");
                DestroyImmediate(this);
                return;
            }
            if ((long)moodlesanityoption == 2) // questboard
            {
                questboardMode = true;
                options.TryGetValue("QuestboardOrder", out object list);
                if (list is JArray jArray) // always will be true
                {
                    questboardList = jArray
                    .Select(jv => jv.ToString())
                    .ToList();
                }
                options.TryGetValue("QuestboardCooldown", out object timer);
                APCanvas.rerollCooldownMax = (int)(long)timer;
                RefreshMaxQuests(false);
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
            if (mood.type == "death5")
            {
                continue; // only used by Archipelago traps. obviously, those don't have checks
            }
            if (!questboardMode) // normal mode
            {
                var moodleIndex = MoodleInternalNameList.IndexOf(mood.type);
                if (moodleIndex == -1) // couldn't find it. try secondary method
                {
                    string baseName = System.Text.RegularExpressions.Regex.Replace(mood.type, @"\d+$", ""); // remove the number at the end
                    moodleIndex = MoodleInternalNameList.FindIndex(name => System.Text.RegularExpressions.Regex.Replace(name, @"\d+$", "") == baseName);
                    if (moodleIndex == -1) // still not found? throw error
                    {
                        Startup.Logger.LogError($"Moodle {mood.type} is not in the Moodlesanity index list!");
                        APCanvas.EnqueueArchipelagoNotification($"Moodlesanity Error! Moodle {mood.type} is not in the Moodlesanity index list!", 3);
                        AlreadySentChecks.Add(mood.type);
                        continue;
                    }
                }
                var CheckID = moodleIndex + startingMoodleId;
                APClientClass.ChecksToSend.Add(CheckID);
                AlreadySentChecks.Add(mood.type);
            }
            else // questboard mode
            {
                InternalMoodNameToCheck.TryGetValue(mood.type, out string checkName);
                if (checkName == null) // not found
                {
                    Startup.Logger.LogError($"Could not find assocaited check for moodle {mood.type}!");
                    APCanvas.EnqueueArchipelagoNotification($"Moodlesanity Error! Could not find assocaited check for moodle {mood.type}!", 3);
                    AlreadySentChecks.Add(mood.type);
                    continue;
                }
                if (APCanvas.ShuffledQuests.Take(APCanvas.UnlockedSlots).Contains(checkName)) // check only the slots we are displaying
                {
                    var CheckID = Client.Locations.GetLocationIdFromName(Client.Players.ActivePlayer.Game, checkName);
                    APClientClass.ChecksToSend.Add(CheckID);
                    questsAvailable.Remove(checkName); // this acts as our duplicate protection. no need to use AlreadySentChecks
                    APCanvas.UpdateQuestboard(false);
                }
            }
        }
    }
    public static void RefreshMaxQuests(bool increase)
    {
        if (increase)
        {
            maxQuests += 15;
        }
        questsAvailable.Clear();
        int max = Mathf.Min(maxQuests, questboardList.Count);
        for (int i = 0; i < max; i++)
        {
            string moodle = questboardList[i];
            long locationId = Client.Locations.GetLocationIdFromName(Client.Players.ActivePlayer.Game, moodle);
            if (!Client.Locations.AllLocationsChecked.Contains(locationId)) // do we have this already? only add it if we don't
            {
                questsAvailable.Add(moodle);
            }
        }
        APCanvas.RerollQuests(true);
    }
}