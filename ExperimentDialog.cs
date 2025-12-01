using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Archipelago.MultiClient.Net.Models;

namespace CUAP;

public class ExperimentDialog : MonoBehaviour
{
    public static ApClient Client;
    private static Talker PlayerTalker;
    private static PlayerCamera playercam;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        PlayerTalker = GameObject.Find("Experiment/Body").GetComponent<Talker>();
        Startup.Logger.LogMessage("Dialog patches applied!");
    }
    public static void ProcessDialog(Archipelago.MultiClient.Net.Models.ItemInfo item)
    {
        // sticking all the dictionaries up here
        Dictionary<int, string> NormalItemDialog = new Dictionary<int, string>()
    {   
        {0,"I recieved a " + item.ItemName + " from " + item.Player + "."},
        {1,"It's a " + item.ItemName + " from " + item.Player + "."},
        {2,"A " + item.ItemName + ". It has a label. 'From: " + item.Player + "'"},
    };
        Dictionary<int, string> ProgressionItemDialog = new Dictionary<int, string>()
    {
        {0,"Huh? My " + item.ItemName + " from " + item.Player + "? How thoughtful!"},
        {1,item.Player + ", this " + item.ItemName + " is the best thing I've seen down here!"},
        {2,"Finally! My " + item.ItemName + "! Thanks, " + item.Player + "!"},
    };
        Dictionary<int, string> UsefulItemDialog = new Dictionary<int, string>() // todo: currently Progression and Useful have the same dialog. Change that?
    {
        {0,"Huh? My " + item.ItemName + " from " + item.Player + "? How thoughtful!"},
        {1,item.Player + ", this " + item.ItemName + " is the best thing I've seen down here!"},
        {2,"Finally! My " + item.ItemName + "! Thanks, " + item.Player + "!"},
    };
        Dictionary<int, string> TrapItemDialog = new Dictionary<int, string>()
    {
        {0,"Oh... my " + item.ItemName + "... Thanks, " + item.Player + "... I guess..."},
        {1,"A " + item.ItemName + ". Not sure what I'll use it for, but thanks anyways, " + item.Player + "."},
    };  // annoyingly ItemInfo does not contain a definition for ItemClassification, so I can't check it directly. This is a decent workaround.
        try
        {
            Body body = GameObject.Find("Experiment/Body").GetComponent<Body>();
            if (body.gameObject.GetComponent<MindwipeScript>() || // Are we Mindwiped?
                !body.conscious || // Unconsious, Sleeping, or dying?
                GameObject.Find("Main Camera/Canvas/Moodles/Moodleimpairedspeech") || // Speech Impaired?
                body.inWater) // Underwater?
            {
                BackupTextbox(item); // Then don't bother having Experiment speak
            }
            PlayerTalker = body.GetComponent<Talker>();
        }
        catch
        {
            BackupTextbox(item); // One of those calls failed? Assume Experiment can't talk
        }
        if (item.ItemName.EndsWith("Unlock")) // progression item
        {
            PlayerTalker.Talk(ProgressionItemDialog[UnityEngine.Random.Range(0, ProgressionItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName.EndsWith("Trap")) // trap item
        {
            PlayerTalker.Talk(TrapItemDialog[UnityEngine.Random.Range(0, TrapItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName.EndsWith("Recipe")) // useful item
        {
            PlayerTalker.Talk(UsefulItemDialog[UnityEngine.Random.Range(0, UsefulItemDialog.Count + 1)], null, true, false);
            return;
        }
        else // something else... probably filler
        {
            PlayerTalker.Talk(NormalItemDialog[UnityEngine.Random.Range(0, NormalItemDialog.Count + 1)], null, true, false);
        }
    }
    private static void BackupTextbox(Archipelago.MultiClient.Net.Models.ItemInfo info)
    {
        try
        {
            playercam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
            playercam.DoAlert("Received " + info.ItemName + " from " + info.Player, false);
        }
        catch
        { 
            // we're probably on the main menu, so don't bother.
        }
    }
}