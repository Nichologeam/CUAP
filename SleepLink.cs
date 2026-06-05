using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates;

namespace CUAP;

public class SleepLink : MonoBehaviour
{
    public static SleepLink instance;
    private PacketReceivedHandler packetHandler;
    private Body Vitals;
    private bool postConnect;
    private bool reactingToLink;
    private float linkCooldown = 0;

    private async void Start()
    {
        instance = this;
        postConnect = false;
    }
    public void OnConnect()
    {
        if (packetHandler != null)
        {
            APClientClass.session.Socket.PacketReceived -= packetHandler; // handler is still attached from a previous session after disconnect
            // I know that this probably will never trigger, because I'm fairly sure session.Socket is destroyed on disconnect, but better safe than sorry.
        }
        packetHandler = (packet) => OnReceivedPacket(packet);
        APClientClass.session.Socket.PacketReceived += packetHandler;
        postConnect = true;
    }

    private void Update()
    {
        if (!postConnect) return; // only run this after connecting
        if (SceneManager.GetActiveScene().name != "SampleScene" && Vitals == null)
        {
            Vitals = GameObject.Find("Experiment/Body").GetComponent<Body>();
        }
        linkCooldown -= Time.deltaTime;
        if (linkCooldown > 0) return;
        else
        {
            reactingToLink = false;
        }
        if (Vitals.sleeping && !reactingToLink)
        {
            var bouncePacket = new BouncePacket
            {
                // I'm implimenting this before Stardew Valley is, so I'm responsible for making the protocol, as there isn't one for me to base off of.
                // Given how little needs to happen, a Bounce based approach seems like the best one. It's as simple as a DeathLink, and that's all it needs to be.
                Tags = ["SleepLink"],
                Data = new Dictionary<string, JToken>
                {
                    ["time"] = DateTime.UtcNow.ToUnixTimeStamp(),
                    ["source"] = APClientClass.session.Players.ActivePlayer.Name
                }
            };
            APClientClass.session.Socket.SendPacketAsync(bouncePacket);
            linkCooldown = 20;
        }
    }

    private void OnReceivedPacket(ArchipelagoPacketBase packet)
    {
        if (packet is BouncedPacket bounced)
        {
            if (bounced.Tags?.Contains("SleepLink") == true)
            {
                if (bounced.Data.TryGetValue("source", out JToken sender))
                {
                    if (sender.ToObject<string>() == APClientClass.session.Players.ActivePlayer.Name)
                    {
                        Startup.Logger.LogError("Got a SleepLink packet from ourselves! Ignoring.");
                        APCanvas.EnqueueArchipelagoNotification(APLocale.Get("sleepLinkSelf", APLocale.APLanguageType.Errors), 3);
                        return;
                    }
                    if (Vitals != null)
                    {
                        Vitals.sleeping = true;
                        Vitals.energy = 0;
                        linkCooldown = 20; // the player can stay asleep for quite a while if conditions are right, so 20 seconds.
                        reactingToLink = true;
                    }
                    DepthChecks.instance.DisplayText.text = APLocale.Get("sleepReceived", APLocale.APLanguageType.UI);
                }
            }
        }
    }
}
