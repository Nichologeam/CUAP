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
    {   // In the same order as in locations.py and the game's EN.json
        {"Comatose",-966812996},
        {"Severe neurophysiological deterioration",-966812995},
        {"Neurological damage",-966812994},
        {"Cognitive impairment",-966812993},
        {"Respiratory arrest",-966812992},
        {"Lung failure",-966812991},
        {"Hypoxemic",-966812990},
        {"Very hypoxemic",-966812989},
        {"Asphyxiating",-966812988},
        {"Cardiac arrest",-966812987},
        {"Discomfort",-966812986},
        {"Pain",-966812985},
        {"Severe pain",-966812984},
        {"Agony",-966812983},
        {"Shock",-966812982},
        {"Opiated",-966812981},
        {"Drugged",-966812980},
        {"Highly drugged",-966812979},
        {"Fatal opioid overdose",-966812978},
        {"Opioid craving",-966812977},
        {"Withdrawl",-966812976},
        {"Severe withdrawl",-966812975},
        {"Dying of withdrawl",-966812974},
        {"Drug overdose", -96612973},
        {"Minor bleeding",-966812972},
        {"Bleeding",-966812971},
        {"Heavy bleeding",-966812970},
        {"Catastrophic bleeding",-966812969},
        {"Internal bleeding",-966812968},
        {"Pale",-966812967},
        {"Hypovolemic",-966812966},
        {"Critically hypovolemic",-966812965},
        {"Exsanguinated",-966812964},
        {"Bloated",-966812963},
        {"Hypervolemic",-966812962},
        {"Critically hypervolemic",-966812961},
        {"Lethally hypervolemic",-966812960},
        {"Slightly exerted",-966812959},
        {"Exerted",-966812958},
        {"Highly exerted",-966812957},
        {"Totally exhausted",-966812956},
        {"Fractured bone",-966812955},
        {"Dislocated joint",-966812954},
        {"Fractured neck",-966812953},
        {"Fractured ribs",-966812952},
        {"Dislocated jaw",-966812951},
        {"Dislocated spine",-966812950},
        {"Infection",-966812949},
        {"Painful infection",-966812948},
        {"Severe infection",-966812947},
        {"Life-threatening infection",-966812946},
        {"Sepsis",-966812945},
        {"Severe sepsis",-966812944},
        {"Septic shock",-966812943},
        {"Concussed",-966812942},
        {"Unconscious",-966812941},
        {"Incapacitated",-966812940},
        {"Asleep",-966812939},
        {"Confused",-966812938},
        {"Very confused",-966812937},
        {"Fainting",-966812936},
        {"Drowsy",-966812935},
        {"Tired",-966812934},
        {"Very tired",-966812933},
        {"Half-asleep",-966812932},
        {"Peckish",-966812931},
        {"Hungry",-966812930},
        {"Very hungry",-966812929},
        {"Starving",-966812928},
        {"Satiated",-966812927},
        {"Full",-966812926},
        {"Thirsty",-966812925},
        {"Dehydrated",-966812924},
        {"Parched",-966812923},
        {"Desiccated",-966812922},
        {"Slaked", -966182921},
        {"Overhydrated", -966182920},
        {"Water-intoxicated", -966182919},
        {"Queasy",-966812918},
        {"Nauseous",-966812917},
        {"Sick",-966812916},
        {"Grossly sick",-966812915},
        {"Warm",-966812914},
        {"Hot",-966812913},
        {"Hyperthermia",-966812912},
        {"Heatstroke",-966812911},
        {"Chilly",-966812910},
        {"Cold",-966812909},
        {"Hypothermia",-966812908},
        {"Freezing to death",-966812907},
        {"Damp",-966812906},
        {"Wet",-966812905},
        {"Soaked",-966812904},
        {"Water-logged",-966812903},
        {"Underweight",-966812902},
        {"Skinny",-966812901},
        {"Malnourished",-966812900},
        {"Emaciated",-966812899},
        {"Chubby",-966812898},
        {"Overweight",-966812897},
        {"Fat",-966812896},
        {"Obese",-966812895},
        {"Feeling down",-966812894},
        {"Gloomy",-966812893},
        {"Depressed",-966812892},
        {"Miserable",-966812891},
        {"Scared",-966812890},
        {"Traumatized",-966812879},
        {"Shell shocked",-966812878},
        {"Satisfied",-966812877},
        {"Excited",-966812876},
        {"Happy",-966812875},
        {"Gleeful",-966812874},
        {"Impaired speech",-966812873},
        {"Impaired hearing",-966812872},
        {"Hearing loss",-966812871},
        {"Severe hearing loss",-966812870},
        {"Life support",-966812869},
        {"Heavy load",-966812868},
        {"Encumbered",-966812867},
        {"Very encumbered",-966812866},
        {"Hampered",-966812865},
        {"Uncomfortable",-966812864},
        {"Radiation sickness",-966812863},
        {"Severe radiation sickness",-966812862},
        {"Chernobyl wannabe",-966812861},
        {"Energized",-966812860},
        {"Hemothorax",-966812859},
        {"Hollow",-966812858},
        {"Bad sleep",-966812857},
        {"Last stand",-966812856},
        {"Dirty",-966812855},
        {"Very dirty",-966812854},
        {"Adrenaline",-966812853},
        {"Tachycardia",-966812852},
        {"Bradycardia",-966812851},
        {"Rapid neuron regeneration sickness",-966812850},
        {"Disfigured",-966812849},
        {"Half-blind",-966812848},
        {"Blind",-966812847},
        {"Immunocompromised",-966812846},
        {"Immunocompetent",-966812845},
        {"Amputated",-966812844},
        {"Dulled claws",-966812843},
        {"Broken claws",-966812842},
        {"Boosted regrowth",-966812841},
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