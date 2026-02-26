using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using BepInEx;
using KrokoshaCasualtiesMP;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CUAP;

public class DeathlinkManager : MonoBehaviour // To be placed on the player's Body gameObject.
{
    public static ArchipelagoSession Client;
    private DeathLinkService dlService;
    private Body Vitals;
    private float DeathlinkCooldown;
    public static bool DeathlinkSeverity = true;


    private void OnEnable()
    {
        var speedupText = GameObject.Find("Main Camera/Canvas/TimeScaleShow/Text (TMP)").GetComponent<TextMeshProUGUI>();
        GameObject.Find("Main Camera/Canvas/TimeScaleShow/Image").SetActive(false);
        GameObject.Find("Main Camera/Canvas/TimeScaleShow").SetActive(true);
        speedupText.text = ""; // remove the 1x in there, as this is actually an unused version of the speedup overlay.
        GameObject.Find("Main Camera/Canvas/TimeScaleShow").transform.SetAsLastSibling(); // overlay over top of everything else by moving to bottom of heirarchy
        speedupText.transform.localPosition = Vector3.zero; // by default this is pushed to the left slightly for a sprite. i removed said sprite and the text is better centered.
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
        if (ServerMain.GetAllDeadPlayers().Count > 0) // Someone died! Send Deathlink!
        {
            SelectDeathLinkCause();
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                player.GetComponent<Body>().brainHealth = 0; // kill all other players in the server
                player.GetComponent<NetPlayer>().dead = true; // make sure the server knows too
            }
            DestroyImmediate(this); // This is in the update loop, so we should kill the script to not spam deathlinks. No damage will be done, because the players are forced back to main menu.
            return;
        }
        DeathlinkCooldown -= Time.deltaTime;
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
            Chat.Server_ChatAnnouncement("DEATHLINK", "AP", $"{dlPacket.Source} died.");
        }
        else
        {
            Chat.Server_ChatAnnouncement("DEATHLINK", "AP", dlPacket.Cause);
        }
        if (DeathlinkSeverity)
        {
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                player.GetComponent<Body>().brainHealth = 0; // kill all other players in the server
                player.GetComponent<NetPlayer>().dead = true; // make sure the server knows too
            }
            Chat.Server_ChatAnnouncement("DEATHLINK", "AP", "Your run is over.");
            Destroy(this); // Destroy script so we don't send a deathlink next frame. No damage will be done, because the players are forced back to main menu.
        }
        else // Nearly exact replica of the V4 version of SelfHarmer.SelfHarm because we can't actually call it
        {
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                Vitals = player.GetComponent<Body>(); // damage a limb on each player
                Limb limb = Vitals.limbs[UnityEngine.Random.Range(1, Vitals.limbs.Length)]; // starting at 1 means the head can never be selected.
                limb.muscleHealth -= 30f;
                limb.skinHealth -= 70f;
                limb.bleedAmount += 40f;
                limb.pain += 30f;
            }
            Sound.Play("harmSting", Vector2.zero, true, false, null, 0.7f, 1f, false, false);
            Chat.Server_ChatAnnouncement("DEATHLINK", "AP", "Damage done to a random limb on all players.");
        }
    }

    void SelectDeathLinkCause()
    {
        var playerName = ServerMain.GetAllDeadPlayers()[1].playername;
        Dictionary<int, string> DrownDeathMessages = new Dictionary<int, string>()
        {
            {0,$"{playerName} is part canine, not fish."},
            {1,$"{playerName} was too heavy to swim."},
            {2,$"{playerName} forgot their scuba gear."},
            {3,$"{playerName} forgot the importance of oxygen."}
        };
        Dictionary<int, string> BloodyDeathMessages = new Dictionary<int, string>()
        {
            {0,$"{playerName} fell off."},
            {1,$"{playerName} ran out of bandages."},
            {2,$"{playerName} couldn't stop the bleeding."}
        };
        Dictionary<int, string> GenericDeathMessages = new Dictionary<int, string>()
        {
            {0,$"{playerName} became a statistic."},
            {1,$"Casualties: Unknown + {playerName}."},
            {2,$"{playerName} met the same fate."}
        };
        if (Vitals.inWater)
        {
            DeathLink dlToSend = new DeathLink(playerName, DrownDeathMessages[UnityEngine.Random.Range(0, DrownDeathMessages.Count)]);
            dlService.SendDeathLink(dlToSend);
        }
        else if (Vitals.totalBleedSpeed > 0.02f) // I know this value seems low, but it is the same value the game uses for the bloody death screen
        {
            DeathLink dlToSend = new DeathLink(playerName, BloodyDeathMessages[UnityEngine.Random.Range(0, BloodyDeathMessages.Count)]);
            dlService.SendDeathLink(dlToSend);
        }
        else
        {
            DeathLink dlToSend = new DeathLink(playerName, GenericDeathMessages[UnityEngine.Random.Range(0, GenericDeathMessages.Count)]);
            dlService.SendDeathLink(dlToSend);
        }
    }
}