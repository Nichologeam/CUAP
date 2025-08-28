using CreepyUtil.Archipelago;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;

namespace CUAP;

public class DeathlinkManager : MonoBehaviour // To be placed on the player's Body gameObject.
{
    public static ApClient Client = APClientClass.Client;
    private TextMeshProUGUI DeathLinkText;
    private Body Vitals;
    private float DeathlinkCooldown;
    JObject options = Client.SlotData["options"] as JObject;

    private void OnEnable()
    {
        Vitals = this.gameObject.GetComponent<Body>();
        DeathLinkText = GameObject.Find("Main Camera/Canvas/TimeScaleShow/Text (TMP)").GetComponent<TextMeshProUGUI>();
        GameObject.Find("Main Camera/Canvas/TimeScaleShow/Image").SetActive(false);
        GameObject.Find("Main Camera/Canvas/TimeScaleShow").SetActive(true);
        DeathLinkText.text = ""; // remove the 1x in there, as this is actually an unused version of the speedup overlay.
        GameObject.Find("Main Camera/Canvas/TimeScaleShow").transform.SetAsLastSibling(); // overlay over top of everything else by moving to bottom of heirarchy
        DeathLinkText.transform.localPosition = Vector3.zero; // by default this is pushed to the left slightly for a sprite. i removed said sprite and the text is better centered.
        var options = Client.SlotData["options"] as JObject;
        if (options.TryGetValue("DeathLink", out var dloption)) // check if deathlink is enabled.
        {
            if (!Convert.ToBoolean(dloption))
            {
                Startup.Logger.LogWarning("Deathlink is disabled, destroying script.");
                Destroy(this); // we destroy the script this late so the text is set up for DepthChecks, which uses it regardless of deathlink being on
            }
        }
        Startup.Logger.LogMessage("DeathlinkManager is monitoring Vitals...");
    }
    private void Update()
    {
        Client = APClientClass.Client;
        if (!Vitals.alive && Vitals.brainHealth == 0) // Experiment is dead! Send Deathlink!
        {
            Startup.Logger.LogWarning("DeathlinkManager noticed that Experiment died! Sending Deathlink...");
            Client.SendDeathLink("Casualties: Unknown + " + Client.PlayerName);
            // todo: the game has ways to check the cause of death. read that value to determine cause for deathlink.
            Destroy(this); // This is in the update loop, so we should kill the script to not spam deathlinks. No damage will be done, because the player is forced back to main menu.
        }
        DeathlinkCooldown -= Time.deltaTime;
        if (DeathlinkCooldown <= 0 && DeathlinkCooldown >= -1)
        {
            DeathLinkText.text = "";
            DeathlinkCooldown = -2; // This makes it only empty the text once. There's probably a much better way to do this, but I don't really care.
        }
        Client.OnDeathLinkPacketReceived += ProcessDeathLink;
    }

    private void ProcessDeathLink(object sender, BouncedPacket e)
    {
        if (DeathlinkCooldown > 0)
        {
            return; // so, for some reason, OnDeathLinkPacketReceived is spammed about 350 ish times for each Deathlink sent.
            // This cooldown prevents 349 ish of those deathlinks from going though, as it entierly destroys Experiment if Large Limb Damage is on.
        }   
        DeathlinkCooldown = 15;
        if (options.TryGetValue("Deathlink", out var deathlinkoption)) // check deathlink option.
        {
            if (Convert.ToInt32(deathlinkoption) == 1)
            {
                DeathLinkText.text = "DeathLink recieved. Your run has ended.";
                DeathLinkText.autoSizeTextContainer = true; // fixes linewrapping off the screen
                Vitals.brainHealth = 0; // Instantly kill Experiment
                Destroy(this); // Destroy script so we don't send a deathlink next frame. No damage will be done, because the player is forced back to main menu.
            }
            else if (Convert.ToInt32(deathlinkoption) == 2) // Nearly exact replica of SelfHarmer.SelfHarm because we can't actually call it
            {
                Limb limb = Vitals.limbs[UnityEngine.Random.Range(1, Vitals.limbs.Length)]; // starting at 1 means the head can never be selected. prevents sudden comatose moodle.
                limb.muscleHealth -= 30f;
                limb.skinHealth -= 70f;
                limb.bleedAmount += 40f;
                limb.pain += 30f;
                Sound.Play("harmSting", Vector2.zero, true, false, null, 0.7f, 1f, false, false);
                DeathLinkText.text = "DeathLink recieved. Damage done to " + limb.fullName + ".";
                DeathLinkText.autoSizeTextContainer = true; // fixes linewrapping off the screen
            }
        }
    }
}