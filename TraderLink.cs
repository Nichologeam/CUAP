using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Archipelago.MultiClient.Net.Helpers.ArchipelagoSocketHelperDelagates;

namespace CUAP;

public class TraderLink : MonoBehaviour
{
    public static TraderLink instance;
    public static bool ringTrading;
    public static bool energyTrading;
    private PacketReceivedHandler packetHandler;
    private bool postConnect;
    private Button ringButton;
    private Button energyButton;
    private GameObject containerObject;
    private TMP_Text ringText;
    private TMP_Text energyText;
    private TMP_InputField amountInput;
    private Button depositButton;
    private TMP_Text depositText;
    private GameObject traderMenu;
    private bool usingRing;
    private bool usingEnergy;
    private bool transfering;
    private float failDisplayTimer;
    private int energyLinkBalance = 0;
    private int ringLinkBalance = 0;
    private long ringLinkID;

    private async void Start()
    {
        instance = this;
        postConnect = false;
        await Task.Delay(1000);
        containerObject = APCanvas.TraderLink;
        ringButton = APCanvas.TraderLinkRing;
        ringText = ringButton.GetComponentInChildren<TMP_Text>();
        energyButton = APCanvas.TraderLinkEnergy;
        energyText = energyButton.GetComponentInChildren<TMP_Text>();
        amountInput = APCanvas.TraderLinkAmount;
        depositButton = APCanvas.TraderLinkDeposit;
        depositText = depositButton.GetComponentInChildren<TMP_Text>();
        depositButton.onClick.AddListener(() => _ = Transfer());
        ringLinkID = UnityEngine.Random.Range(0, 999999999);
    }
    public void OnConnect()
    {
        if (!ringTrading && !energyTrading)
        {
            Startup.Logger.LogWarning("Both RingLink and EnergyLink are disabled. Destroying script.");
            Destroy(this);
            return;
        }
        if (packetHandler != null)
        {
            APClientClass.session.Socket.PacketReceived -= packetHandler; // handler is still attached from a previous session after disconnect
            // I know that this probably will never trigger, because I'm fairly sure session.Socket is destroyed on disconnect, but better safe than sorry.
        }
        ringText.text = APLocale.Get("tradeDisabled", APLocale.APLanguageType.UI);
        ringButton.interactable = false;
        ringButton.onClick.RemoveAllListeners();
        energyText.text = APLocale.Get("tradeDisabled", APLocale.APLanguageType.UI);
        energyButton.interactable = false;
        energyButton.onClick.RemoveAllListeners();
        if (ringTrading)
        {
            ringText.text = APLocale.Get("ringTrading", APLocale.APLanguageType.UI);
            ringButton.interactable = true;
            ringButton.onClick.AddListener(ChangeToRing);
        }
        if (energyTrading)
        {
            energyText.text = APLocale.Get("energyTrading", APLocale.APLanguageType.UI);
            energyButton.interactable = true;
            energyButton.onClick.AddListener(ChangeToEnergy);
            var packet1 = new SetNotifyPacket
            {
                Keys = [$"EnergyLink{APClientClass.session.Players.ActivePlayer.Team}"]
            };
            APClientClass.session.Socket.SendPacket(packet1); // send and forget
            var packet2 = new GetPacket
            {
                Keys = [$"EnergyLink{APClientClass.session.Players.ActivePlayer.Team}"]
            };
            APClientClass.session.Socket.SendPacket(packet2); // handle the reply below in OnReceivedPacket
        }
        packetHandler = (packet) => OnReceivedPacket(packet);
        APClientClass.session.Socket.PacketReceived += packetHandler;
        postConnect = true;
    }

    private void Update()
    {
        if (!postConnect) return; // only run this after connecting
        failDisplayTimer = failDisplayTimer - Time.deltaTime;
        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            containerObject.SetActive(false);
            return; // only apply in-game
        }
        if (traderMenu == null)
        {
            traderMenu = GameObject.Find("Main Camera/Canvas/MainView").transform.Find("TraderMenu").gameObject;
        }
        containerObject.SetActive(traderMenu.activeSelf); // only appear when the trader menu is open
        depositButton.interactable = !transfering;
        if (transfering) return;
        if (failDisplayTimer < 0)
        {
            depositText.text = APLocale.Get("tradeTransfer", APLocale.APLanguageType.UI);
        }
        if (!usingRing && !usingEnergy)
        {
            depositText.text = APLocale.Get("noTradeType", APLocale.APLanguageType.UI);
        }
        else if (usingRing)
        {
            ringButton.image.color = Color.green;
            energyButton.image.color = Color.white;
        }
        else if (usingEnergy)
        {
            ringButton.image.color = Color.white;
            energyButton.image.color = Color.green;
        }
    }

    private void ChangeToRing()
    {
        usingRing = true;
        usingEnergy = false;
    }

    private void ChangeToEnergy()
    {
        usingRing = false;
        usingEnergy = true;
    }

    private async Task Transfer()
    {
        try
        {
            if (int.TryParse(amountInput.text, out int tradeAmount))
            {
                if (0 < tradeAmount) // trying to deposit
                {
                    tradeAmount = Math.Min(tradeAmount, PlayerCamera.main.currentTrader.valueGiven); // clamp to the max the trader has (if it exceeds it)
                }
                if (usingRing)
                {
                    transfering = true;
                    depositText.text = APLocale.Get("tradeTransfering", APLocale.APLanguageType.UI);
                    if (0 > tradeAmount) // trying to withdrawl
                    {
                        if (Mathf.Abs(tradeAmount) > ringLinkBalance)
                        {
                            tradeAmount = -ringLinkBalance;
                        }
                    }
                    var bouncePacket = new BouncePacket
                    {
                        Tags = ["RingLink"],
                        Data = new Dictionary<string, JToken>
                        {
                            ["time"] = DateTime.UtcNow.ToUnixTimeStamp(),
                            ["source"] = ringLinkID,
                            ["amount"] = tradeAmount
                        }
                    };
                    await APClientClass.session.Socket.SendPacketAsync(bouncePacket);
                    PlayerCamera.main.currentTrader.valueGiven -= tradeAmount;
                    transfering = false;
                }
                else if (usingEnergy)
                {
                    transfering = true;
                    depositText.text = APLocale.Get("tradeTransfering", APLocale.APLanguageType.UI);
                    if (0 > tradeAmount) // trying to withdrawl
                    {
                        if (Mathf.Abs(tradeAmount) > energyLinkBalance)
                        {
                            tradeAmount = -energyLinkBalance;
                        }
                    }
                    var setPacket = new SetPacket
                    {
                        Key = $"EnergyLink{APClientClass.session.Players.ActivePlayer.Team}",
                        DefaultValue = 0,
                        WantReply = true,
                        Operations =
                        [
                            new()
                        {
                            OperationType = OperationType.Add,
                            Value = tradeAmount // if this is negative, the server subtracts from the DataStore value automatically.
                        },
                        new()
                        {
                            OperationType = OperationType.Max,
                            Value = 0
                        }
                        ]
                    };
                    await APClientClass.session.Socket.SendPacketAsync(setPacket);
                    PlayerCamera.main.currentTrader.valueGiven -= tradeAmount;
                    transfering = false;
                }
                else // neither RingLink nor EnergyLink are on. Do nothing.
                {
                    return;
                }
            }
            else // couldn't parse
            {
                Startup.Logger.LogWarning($"Couldn't parse \"{amountInput.text}\" as a number for a TraderLink transfer!");
                depositText.text = APLocale.Get("tradeTransferFail", APLocale.APLanguageType.UI);
                failDisplayTimer = 5;
            }
        }
        catch (Exception ex)
        {
            Startup.Logger.LogError($"TraderLink error! {ex}");
            APCanvas.EnqueueArchipelagoNotification(APLocale.Get("traderLink", APLocale.APLanguageType.Errors) + ex, 3);
            transfering = false;
        }
        finally
        {
            transfering = false;
            PlayerCamera.main.UpdateTradeTexts();
        }
    }

    private void OnReceivedPacket(ArchipelagoPacketBase packet)
    {
        switch (packet.PacketType)
        {
            case ArchipelagoPacketType.Retrieved when packet is RetrievedPacket Retpacket:
                if (Retpacket.Data.TryGetValue($"EnergyLink{APClientClass.session.Players.ActivePlayer.Team}", out JToken energyBalance))
                {
                    energyLinkBalance = energyBalance.ToObject<int>();
                }
                else
                {
                    Startup.Logger.LogWarning("Got a Retrieved packet that wasn't from an EnergyLink check. Keys are as follows...");
                    foreach (var key in Retpacket.Data.Keys)
                    {
                        Startup.Logger.LogWarning(key.ToString());
                    }
                }
                break;

            case ArchipelagoPacketType.Bounced when packet is BouncedPacket bounced:
                if (bounced.Tags?.Contains("RingLink") == true && usingRing)
                {
                    if (bounced.Data.TryGetValue("source", out JToken sender))
                    {
                        if (sender.ToObject<int>() == ringLinkID)
                        {
                            Startup.Logger.LogError("Got a RingLink packet from ourselves! Ignoring.");
                            APCanvas.EnqueueArchipelagoNotification(APLocale.Get("ringLinkSelf", APLocale.APLanguageType.Errors), 3);
                            return;
                        }
                    }
                    if (bounced.Data.TryGetValue("amount", out JToken ringBalance))
                    {
                        ringLinkBalance = ringBalance.ToObject<int>();
                    }
                    else
                    {
                        Startup.Logger.LogWarning("Got a RingLink packet that has unknown Keys. Keys are as follows...");
                        foreach (var key in bounced.Data.Keys)
                        {
                            Startup.Logger.LogWarning(key.ToString());
                        }
                    }
                }
                break;

            case ArchipelagoPacketType.SetReply when packet is SetReplyPacket reply:
                energyLinkBalance = reply.Value.ToObject<int>();
                break;
        }
    }
}
