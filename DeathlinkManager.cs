using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using BepInEx;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CUAP;

public class DeathlinkManager : MonoBehaviour // To be placed on the player's Body gameObject.
{
    public static ArchipelagoSession Client;
    private DeathLinkService dlService;
    private TextMeshProUGUI DeathLinkText;
    private Body Vitals;
    private float DeathlinkCooldown;
    public static bool DeathlinkSeverity = true;


    private void OnEnable()
    {
        Vitals = this.gameObject.GetComponent<Body>();
        DeathLinkText = GameObject.Find("Main Camera/Canvas/TimeScaleShow/Text (TMP)").GetComponent<TextMeshProUGUI>();
        GameObject.Find("Main Camera/Canvas/TimeScaleShow/Image").SetActive(false);
        GameObject.Find("Main Camera/Canvas/TimeScaleShow").SetActive(true);
        DeathLinkText.text = ""; // remove the 1x in there, as this is actually an unused version of the speedup overlay.
        GameObject.Find("Main Camera/Canvas/TimeScaleShow").transform.SetAsLastSibling(); // overlay over top of everything else by moving to bottom of heirarchy
        DeathLinkText.transform.localPosition = Vector3.zero; // by default this is pushed to the left slightly for a sprite. i removed said sprite and the text is better centered.
        if (!APCanvas.DeathlinkEnabled)
        {
            Startup.Logger.LogWarning("Deathlink is disabled, destroying script.");
            DestroyImmediate(this); // we destroy the script this late so the text is set up for DepthChecks, which uses it regardless of deathlink being on
            return;
        }
        Client = APClientClass.session;
        dlService = APClientClass.dlService;
        Startup.Logger.LogMessage("DeathlinkManager is monitoring Vitals...");
    }
    private void Update()
    {
        if (!Vitals.alive && Vitals.brainHealth == 0) // Experiment is dead! Send Deathlink!
        {
            SelectDeathLinkCause();
            DestroyImmediate(this); // This is in the update loop, so we should kill the script to not spam deathlinks. No damage will be done, because the player is forced back to main menu.
            return;
        }
        DeathlinkCooldown -= Time.deltaTime;
        if (DeathlinkCooldown <= 0 && DeathlinkCooldown >= -1)
        {
            DeathLinkText.text = "";
            DeathlinkCooldown = -2; // This makes it only empty the text once. There's probably a much better way to do this, but I don't really care.
        }
        dlService.OnDeathLinkReceived += ProcessDeathLink;
    }

    private void ProcessDeathLink(DeathLink dlPacket)
    {
        if (DeathlinkCooldown > 0)
        {
            return; // so, for some reason, OnDeathLinkPacketReceived is spammed about 350 ish times for each Deathlink sent.
            // This cooldown prevents 349 ish of those deathlinks from going though, as it entierly destroys Experiment if Large Limb Damage is on.
        }   
        if (dlPacket.Source.IsNullOrWhiteSpace()) // should never happen, realistically.
        {
            APCanvas.EnqueueArchipelagoNotification("Received a DeathLink packet with no sender. Ignoring.",3);
            Startup.Logger.LogWarning("Received a DeathLink packet with no sender. Ignoring.");
            return;
        }
        DeathlinkCooldown = 15;
        if (dlPacket.Cause.IsNullOrWhiteSpace())
        {
            DeathLinkText.text = dlPacket.Source + " died.";
        }
        else
        {
            DeathLinkText.text = dlPacket.Cause;
        }
        if (DeathlinkSeverity)
        {
            DeathLinkText.text = DeathLinkText.text + " Your run has ended.";
            DeathLinkText.autoSizeTextContainer = true; // fixes linewrapping off the screen
            Vitals.brainHealth = 0; // Instantly kill Experiment
            Destroy(this); // Destroy script so we don't send a deathlink next frame. No damage will be done, because the player is forced back to main menu.
        }
        else // Nearly exact replica of the V4 version of SelfHarmer.SelfHarm because we can't actually call it
        {
            DeathLinkText.autoSizeTextContainer = false;
            Limb limb = Vitals.limbs[UnityEngine.Random.Range(1, Vitals.limbs.Length)]; // starting at 1 means the head can never be selected.
            limb.muscleHealth -= 30f;
            limb.skinHealth -= 70f;
            limb.bleedAmount += 40f;
            limb.pain += 30f;
            Sound.Play("harmSting", Vector2.zero, true, false, null, 0.7f, 1f, false, false);
            DeathLinkText.text = DeathLinkText.text + " Damage done to " + limb.fullName + ".";
            DeathLinkText.autoSizeTextContainer = true; // fixes linewrapping off the screen
        }
    }

    void SelectDeathLinkCause()
    {
        Dictionary<int, string> DrownDeathMessages = new Dictionary<int, string>()
        {
            {0,Client.Players.ActivePlayer.Alias + " is part canine, not fish."},
            {1,Client.Players.ActivePlayer.Alias + " was too heavy to swim."},
            {2,Client.Players.ActivePlayer.Alias + " forgot their scuba gear."},
            {3,Client.Players.ActivePlayer.Alias + " forgot the importance of oxygen."}
        };
        Dictionary<int, string> BloodyDeathMessages = new Dictionary<int, string>()
        {
            {0,Client.Players.ActivePlayer.Alias + " fell off."},
            {1,Client.Players.ActivePlayer.Alias + " ran out of bandages."},
            {2,Client.Players.ActivePlayer.Alias + " couldn't stop the bleeding."}
        };
        Dictionary<int, string> GenericDeathMessages = new Dictionary<int, string>()
        {
            {0,Client.Players.ActivePlayer.Alias + " became a statistic."},
            {1,"Casualties: Unknown + " + Client.Players.ActivePlayer.Alias},
            {2,Client.Players.ActivePlayer.Alias + " met the same fate."}
        };
        if (Vitals.inWater)
        {
            DeathLink dlToSend = new DeathLink(Client.Players.ActivePlayer.Alias, DrownDeathMessages[UnityEngine.Random.Range(0, DrownDeathMessages.Count)]);
            dlService.SendDeathLink(dlToSend);
        }
        else if (Vitals.totalBleedSpeed > 0.02f) // I know this value seems low, but it is the same value the game uses for the bloody death screen
        {
            DeathLink dlToSend = new DeathLink(Client.Players.ActivePlayer.Alias, BloodyDeathMessages[UnityEngine.Random.Range(0, BloodyDeathMessages.Count)]);
            dlService.SendDeathLink(dlToSend);
        }
        else
        {
            DeathLink dlToSend = new DeathLink(Client.Players.ActivePlayer.Alias, GenericDeathMessages[UnityEngine.Random.Range(0, GenericDeathMessages.Count)]);
            dlService.SendDeathLink(dlToSend);
        }
    }
}