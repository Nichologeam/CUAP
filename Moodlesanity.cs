using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

namespace CUAP;

public class Moodlesanity : MonoBehaviour
{
    public static ApClient Client;
    private MoodleManager Moodles;
    private List<string> AlreadySentChecks = new List<string>();
    private WorldGeneration worldgen;
    private static Dictionary<string, int> MoodleNametoCheckID = new Dictionary<string, int>()
    {   // In the same order as in locations.py
        {"Comatose",-966813079},
        {"Severe neurophysiological deterioration",-966813078},
        {"Neurological damage",-966813077},
        {"Cognitive impairment",-966813076},
        {"Respiratory arrest",-966813075},
        {"Lung failure",-966813074},
        {"Hypoxemic",-966813073},
        {"Very hypoxemic",-966813072},
        {"Asphyxiating",-966813071},
        {"Cardiac arrest",-966813070},
        {"Discomfort",-966813069},
        {"Pain",-966813068},
        {"Severe pain",-966813067},
        {"Agony",-966813066},
        {"Shock",-966813065},
        {"Opiated",-966813064},
        {"Drugged",-966813063},
        {"Highly drugged",-966813062},
        {"Fatal opioid overdose",-966813061},
        {"Opioid craving",-966813060},
        {"Withdrawl",-966813059},
        {"Severe withdrawl",-966813058},
        {"Dying of withdrawl",-966813057},
        {"Minor bleeding",-966813056},
        {"Bleeding",-966813055},
        {"Heavy bleeding",-966813054},
        {"Catastrophic bleeding",-966813053},
        {"Internal bleeding",-966813052},
        {"Pale",-966813051},
        {"Hypovolemic",-966813050},
        {"Critically hypovolemic",-966813049},
        {"Exsanguinated",-966813048},
        {"Bloated",-966813047},
        {"Hypervolemic",-966813046},
        {"Critically hypervolemic",-966813045},
        {"Lethally hypervolemic",-966813044},
        {"Slightly exerted",-966813043},
        {"Exerted",-966813042},
        {"Highly exerted",-966813041},
        {"Totally exhausted",-966813040},
        {"Fractured bone",-966813039},
        {"Dislocated joint",-966813038},
        {"Fractured neck",-966813037},
        {"Fractured ribs",-966813036},
        {"Dislocated jaw",-966813035},
        {"Dislocated spine",-966813034},
        {"Infection",-966813033},
        {"Painful infection",-966813032},
        {"Severe infection",-966813031},
        {"Life-threatening infection",-966813030},
        {"Sepsis",-966813029},
        {"Severe sepsis",-966813028},
        {"Septic shock",-966813027},
        {"Concussed",-966813026},
        {"Unconscious",-966813025},
        {"Incapacitated",-966813024},
        {"Asleep",-966813023},
        {"Confused",-966813022},
        {"Very confused",-966813021},
        {"Fainting",-966813020},
        {"Drowsy",-966813019},
        {"Tired",-966813018},
        {"Very tired",-966813017},
        {"Half-asleep",-966813016},
        {"Peckish",-966813015},
        {"Hungry",-966813014},
        {"Very hungry",-966813013},
        {"Starving",-966813012},
        {"Satiated",-966813011},
        {"Full",-966813010},
        {"Thirsty",-966813009},
        {"Dehydrated",-966813008},
        {"Parched",-966813007},
        {"Desiccated",-966813006},
        {"Queasy",-966813005},
        {"Nauseous",-966813004},
        {"Sick",-966813003},
        {"Grossly sick",-966813002},
        {"Warm",-966813001},
        {"Hot",-966813000},
        {"Hyperthermia",-966812999},
        {"Heatstroke",-966812998},
        {"Chilly",-966812997},
        {"Cold",-966812996},
        {"Hypothermia",-966812995},
        {"Freezing to death",-966812994},
        {"Damp",-966812993},
        {"Wet",-966812992},
        {"Soaked",-966812991},
        {"Water-logged",-966812990},
        {"Underweight",-966812989},
        {"Skinny",-966812988},
        {"Malnourished",-966812987},
        {"Emaciated",-966812986},
        {"Chubby",-966812985},
        {"Overweight",-966812984},
        {"Fat",-966812983},
        {"Obese",-966812982},
        {"Feeling down",-966812981},
        {"Gloomy",-966812980},
        {"Depressed",-966812979},
        {"Miserable",-966812978},
        {"Scared",-966812977},
        {"Traumatized",-966812976},
        {"Shell shocked",-966812975},
        {"Satisfied",-966812974},
        {"Excited",-966812973},
        {"Happy",-966812972},
        {"Gleeful",-966812971},
        {"Impaired speech",-966812970},
        {"Impaired hearing",-966812969},
        {"Hearing loss",-966812968},
        {"Severe hearing loss",-966812967},
        {"Life support",-966812966},
        {"Uncomfortable",-966812965},
        {"Radiation sickness",-966812964},
        {"Severe radiation sickness",-966812963},
        {"Chernobyl wannabe",-966812962},
        {"Energized",-966812961},
        {"Hemothorax",-966812960},
        {"Hollow",-966812959},
        {"Bad sleep",-966812958},
        {"Dirty",-966812957},
        {"Very dirty",-966812956},
        {"Adrenaline",-966812955},
        {"Rapid neuron regeneration sickness",-966812954},
        {"Disfigured",-966812953},
        {"Half-blind",-966812952},
        {"Blind",-966812951},
        {"Immunocompromised",-966812950},
        {"Immunocompetent",-966812949},
        {"Amputated",-966812948}
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
            if (AlreadySentChecks.Contains(mood.moodleName))
            {
                continue; // Avoid spamming the server by not even attempting to send a check we already have sent.
            }
            if (mood.moodleName == "Immunocompromised" && worldgen.loadingObject.activeSelf)
            {
                continue; // There's a bug where Experiment is Immunocompromised for the first few frames during worldgen. This if statement makes the check not send in that case.
            }
            MoodleNametoCheckID.TryGetValue(mood.moodleName, out int CheckID);
            APClientClass.ChecksToSendQueue.Enqueue(CheckID);
            AlreadySentChecks.Add(mood.moodleName);
        }
    }
}