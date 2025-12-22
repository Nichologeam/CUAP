using CreepyUtil.Archipelago;
using System.Collections.Generic;
using UnityEngine;

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
        Startup.Logger.LogMessage("Dialogue patches applied!");
    }
    public static void ProcessDialog(Archipelago.MultiClient.Net.Models.ItemInfo item)
    {   // annoyingly ItemInfo does not contain a definition for ItemClassification, so I can't check it directly. This is a decent workaround.
        Dictionary<int, string> NormalItemDialog = new Dictionary<int, string>()
        {   
            {0,"I recieved a " + item.ItemName + " from " + item.Player + "."},
            {1,"It's a " + item.ItemName + " from " + item.Player + "."}
        };
        Dictionary<int, string> HopefulItemDialog = new Dictionary<int, string>()
        {
            {0,"I'm feeling better! Thanks " + item.Player + "!"},
            {1,"I'm feeling a little hopeful thanks to " + item.Player + "."},
            {2,"Knowing " + item.Player + " is here makes me feel better."},
            {3,item.Player + " helps me keep going."},
        };
        Dictionary<int, string> DespairItemDialog = new Dictionary<int, string>()
        {
            {0,"Everything will be okay, right " + item.Player + "?"},
            {1,"Oh... thanks " + item.Player + "... I suppose..."},
            {2,item.Player + "?"},
            {3,"I... feel worse, " + item.Player + "."},
        };
        Dictionary<int, string> ProgressionItemDialog = new Dictionary<int, string>()
        {
            {0,"Huh? My " + item.ItemName + " from " + item.Player + "? How thoughtful!"},
            {1,item.Player + ", this " + item.ItemName + " is the best thing I've seen down here!"},
            {2,"Finally! My " + item.ItemName + "! Thanks, " + item.Player + "!"},
        };
        Dictionary<int, string> TrapItemDialog = new Dictionary<int, string>()
        {
            {0,"Oh... my " + item.ItemName + "... Thanks, " + item.Player + "... I guess..."},
            {1,"A " + item.ItemName + ". Not sure what I'll use it for, but thanks anyways, " + item.Player + "."},
        };
        try
        {
            Body body = GameObject.Find("Experiment/Body").GetComponent<Body>();
            PlayerTalker = body.GetComponent<Talker>();
            if (body.gameObject.GetComponent<MindwipeScript>() || // Are we Mindwiped?
                !body.conscious || // Unconsious, Sleeping, or dying?
                PlayerTalker.impairedSpeech || // Speech Impaired?
                PlayerTalker.brainDamaged || // Brain Damaged?
                body.inWater) // Underwater?
            {
                BackupTextbox(item); // Then don't bother having Experiment speak
                return;
            }
        }
        catch
        {
            BackupTextbox(item); // One of those calls failed? Assume Experiment can't talk
            return;
        }
        if (item.ItemName.EndsWith("Unlock") || item.ItemName.EndsWith("Recipe")) // progression or useful item
        {
            PlayerTalker.Talk(ProgressionItemDialog[UnityEngine.Random.Range(0, ProgressionItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName.EndsWith("Trap")) // trap item
        {
            PlayerTalker.Talk(TrapItemDialog[UnityEngine.Random.Range(0, TrapItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Hope")
        {
            PlayerTalker.Talk(HopefulItemDialog[UnityEngine.Random.Range(0, HopefulItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Despair")
        {
            PlayerTalker.Talk(DespairItemDialog[UnityEngine.Random.Range(0, DespairItemDialog.Count + 1)], null, true, false);
            return;
        }
        else // something else...
        {
            PlayerTalker.Talk(NormalItemDialog[UnityEngine.Random.Range(0, NormalItemDialog.Count + 1)], null, true, false);
        }
    }
    private static void BackupTextbox(Archipelago.MultiClient.Net.Models.ItemInfo info)
    {
        try
        {
            playercam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
            playercam.DoAlert("Received " + info.ItemName + " from " + info.Player + " (" + info.LocationDisplayName + ")", false);
        }
        catch
        { 
            // we're probably on the main menu, so don't bother.
        }
    }
}