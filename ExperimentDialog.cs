using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.MessageLog.Parts;
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
            {0,$"I recieved a {item.ItemName} from {item.Player}."},
            {1,$"It's a {item.ItemName} from {item.Player}."},
            {2,$"Ah, it's just a {item.ItemName} that {item.Player} gave me."}
        };
        Dictionary<int, string> HopefulItemDialog = new Dictionary<int, string>()
        {
            {0,$"I'm feeling better! Thanks {item.Player}!"},
            {1,$"I'm feeling a little hopeful thanks to {item.Player}."},
            {2,$"Knowing {item.Player} is here makes me feel better."},
            {3,$"{item.Player} helps me keep going."},
        };
        Dictionary<int, string> DespairItemDialog = new Dictionary<int, string>()
        {
            {0,$"Everything will be okay, right {item.Player}?"},
            {1,$"Oh... thanks {item.Player}... I suppose..."},
            {2,$"{item.Player}...?"},
            {3,$"I... feel worse, {item.Player}."},
        };
        Dictionary<int, string> RecipeItemDialog = new Dictionary<int, string>()
        {
            {0,$"A {item.ItemName}? Maybe crafing it could help {item.Player} in return."},
            {1,$"{item.Player}, this {item.ItemName.Replace(" Recipe","")} is the best thing I've seen down here!"}, // trim recipe to make it make sense
            {2,$"{item.Player} just gave me an idea for a new recipe! How does a {item.ItemName.Replace(" Recipe","")} sound?"}
        };
        Dictionary<int, string> LayerItemDialog = new Dictionary<int, string>()
        {
            {0,$"{item.Player} unblocked the way to the {item.ItemName.Replace(" Unlock","")}!"}, // remove the unlock from the item name to make this make more sense
            {1,$"Looks like I can go somewhere new thanks to {item.Player}."},
            {2,$"Finally! I can go to the {item.ItemName.Replace(" Unlock","")}! Thanks, {item.Player}!"},
            {3,$"Hopefully the {item.ItemName.Replace(" Unlock","")} is better than here, {item.Player}..."}
        };
        Dictionary<int, string> ExtenderItemDialog = new Dictionary<int, string>()
        {
            {0,$"Looks like I can go deeper thanks to {item.Player}."},
            {1,$"I'm one step closer to the bottom, thanks to {item.Player}."},
            {2,$"You want me to go further {item.Player}? Aw..."},
            {3,$"The end is in sight, {item.Player}!"},
        };
        Dictionary<int, string> LimbItemDialog = new Dictionary<int, string>()
        {// displayed when receiving Progressive Left/Right Arm
            {0,$"Is that... my {item.ItemName.Replace("Progressive ","")}?! How did...? {item.Player}...?"}, // remove progressive to have "Left/Right Arm" left
            {1,$"Oh hey! My {item.ItemName.Replace("Progressive ","")}! I was wondering where I left it. Thanks, {item.Player}!"},
            {2,$"Don't ask how I lost this, {item.Player}."},
            {3,$"Uh... which side does this go on again, {item.Player}?"},
        };
        Dictionary<int, string> STRItemDialog = new Dictionary<int, string>()
        {// displayed when receiving Progressive STR
            {0,$"Did... did {item.Player} just give me steriods?"},
            {1,$"I am all pumped up thanks to {item.Player}!"},
            {2,$"*Extends claws dramatically towards {item.Player}* Haha!"},
            {3,$"Fuck yeah, {item.Player}! I'm {GameObject.Find("Experiment/Body").GetComponent<Body>().weightOffset * 0.34f + 50f} kilos of pure muscle!"}, // prints the player's body mass
        };
        Dictionary<int, string> RESItemDialog = new Dictionary<int, string>()
        {// displayed when receiving Progressive RES
            {0,$"{item.Player} makes me feel more determined."},
            {1,$"{item.Player}'s right! You're gonna have to do better than that to kill me!"}, // left for dead 2 reference anyone?
            {2,$"I've got this. Right, {item.Player}?"},
            {3,$"Even after all of this... {item.Player} keeps cheering for me to get back up again. It helps."},
        };
        Dictionary<int, string> INTItemDialog = new Dictionary<int, string>()
        {// displayed when receiving Progressive INT
            {0,$"Feels like an update got sideloaded into my brainchip. Was that you {item.Player}?"},
            {1,$"Who knew {item.Player} knew so much about the depths?"},
            {2,$"I'm like a walking library now, {item.Player}!"},
            {3,$"Alright. As long as you don't call me a nerd, {item.Player}."},
            {4,$"Erm, achkually, {item.Player}..."},
        };
        GameObject.Find("Experiment/Body").GetComponent<ExperimentDialog>().CompanionTextbox(item);
        if (item.ItemName == "Depth Extender") // depth extender
        {
            PlayerTalker.Talk(ExtenderItemDialog[UnityEngine.Random.Range(0, ExtenderItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName.Contains("Recipe")) // recipe
        {
            PlayerTalker.Talk(RecipeItemDialog[UnityEngine.Random.Range(0, RecipeItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName.EndsWith("Unlock")) // layer unlock
        {
            PlayerTalker.Talk(LayerItemDialog[UnityEngine.Random.Range(0, LayerItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Progressive Left Arm" || item.ItemName == "Progressive Right Arm") // limbs
        {
            PlayerTalker.Talk(LimbItemDialog[UnityEngine.Random.Range(0, LimbItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Progressive STR") // str
        {
            PlayerTalker.Talk(STRItemDialog[UnityEngine.Random.Range(0, STRItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Progressive RES") // res
        {
            PlayerTalker.Talk(RESItemDialog[UnityEngine.Random.Range(0, RESItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Progressive INT") // int
        {
            PlayerTalker.Talk(INTItemDialog[UnityEngine.Random.Range(0, INTItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Hope") // hope
        {
            PlayerTalker.Talk(HopefulItemDialog[UnityEngine.Random.Range(0, HopefulItemDialog.Count + 1)], null, true, false);
            return;
        }
        if (item.ItemName == "Despair") // despair
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
        APCanvas.EnqueueArchipelagoNotification($"Received {info.ItemName} from {info.Player}!",1);
    }
}