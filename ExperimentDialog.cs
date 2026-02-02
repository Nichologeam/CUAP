using Archipelago.MultiClient.Net;
using System.Collections.Generic;
using UnityEngine;

namespace CUAP;

public class ExperimentDialog : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private static Talker PlayerTalker;

    private void OnEnable()
    {
        Client = APClientClass.session;
        PlayerTalker = gameObject.GetComponent<Talker>();
        Startup.Logger.LogMessage("Dialogue patches applied!");
    }
    public static void ProcessDialog(Archipelago.MultiClient.Net.Models.ItemInfo item)
    {   
        Dictionary<int, string> NormalItemDialog = new Dictionary<int, string>()
        {   
            {0,"I recieved a " + item.ItemName + " from " + item.Player + "."},
            {1,"It's a " + item.ItemName + " from " + item.Player + "."},
            {2,"Ah, it's just a " + item.ItemName + " that " + item.Player + " gave me."}
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
            {2,item.Player + "...?"},
            {3,"I... feel worse, " + item.Player + "."},
        };
        Dictionary<int, string> RecipeItemDialog = new Dictionary<int, string>()
        {
            {0,"A " + item.ItemName + "? Maybe crafing it could help " + item.Player + " in return."},
            {1,item.Player + ", this " + item.ItemName.Replace(" Recipe","") + " is the best thing I've seen down here!"}, // trim recipe to make it make sense
            {2,item.Player + " just gave me an idea for a new recipe! How does a " + item.ItemName.Replace(" Recipe","") + " sound?"}
        };
        Dictionary<int, string> LayerItemDialog = new Dictionary<int, string>()
        {
            {0,item.Player + " unblocked the way to the " + item.ItemName.Replace(" Unlock","") + "!"}, // remove the unlock from the item name to make this make more sense
            {1,"Looks like I can go somewhere new thanks to " + item.Player + "."},
            {2,"Finally! I can go to the " + item.ItemName.Replace(" Unlock","") + "! Thanks, " + item.Player + "!"},
            {3,"Hopefully the " + item.ItemName.Replace(" Unlock","") + " is better than here, " + item.Player + "..."}
        };
        Dictionary<int, string> ExtenderItemDialog = new Dictionary<int, string>()
        {
            {0,"Looks like I can go deeper thanks to " + item.Player + "."},
            {1,"I'm one step closer to the bottom, thanks to " + item.Player + "."},
            {2,"You want me to go further " + item.Player + "? Aw..."},
            {3,"The end is in sight " + item.Player + "!"},
        };
        GameObject.Find("Experiment/Body").GetComponent<ExperimentDialog>().CompanionTextbox(item);
        if (item.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.Advancement) && !item.ItemName.EndsWith("Unlock")) // progression (not layer unlock)
        {
            PlayerTalker.Talk(ExtenderItemDialog[UnityEngine.Random.Range(0, ExtenderItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.Flags.HasFlag(Archipelago.MultiClient.Net.Enums.ItemFlags.NeverExclude)) // useful
        {
            PlayerTalker.Talk(RecipeItemDialog[UnityEngine.Random.Range(0, RecipeItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName.EndsWith("Unlock")) // progression (layer unlock)
        {
            PlayerTalker.Talk(LayerItemDialog[UnityEngine.Random.Range(0, LayerItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Hope") // hope filler item
        {
            PlayerTalker.Talk(HopefulItemDialog[UnityEngine.Random.Range(0, HopefulItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Despair") // despair filler item
        {
            PlayerTalker.Talk(DespairItemDialog[UnityEngine.Random.Range(0, DespairItemDialog.Count + 1)], null, true, false);
            return;
        }
        else // something else...
        {
            PlayerTalker.Talk(NormalItemDialog[UnityEngine.Random.Range(0, NormalItemDialog.Count + 1)], null, true, false);
        }
    }
    void CompanionTextbox(Archipelago.MultiClient.Net.Models.ItemInfo info)
    {
        APCanvas.EnqueueArchipelagoNotification("Received " + info.ItemName + " from " + info.Player + "!",1);
    }
}