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
    private Talker PlayerTalker;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        PlayerTalker = GameObject.Find("Experiment/Body").GetComponent<Talker>();
        Startup.Logger.LogMessage("Dialog patches applied!");
    }
    public void ProcessDialog(Archipelago.MultiClient.Net.Models.ItemInfo item)
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
        Dictionary<int, string> UsefulItemDialog = new Dictionary<int, string>() // currently Progression and Useful have the same dialog. Change that?
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
}