using Archipelago.MultiClient.Net;
using HarmonyLib;
using KrokoshaCasualtiesMP;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

namespace CUAP;

public class TrapHandler : MonoBehaviour
{
    public static ArchipelagoSession Client;
    private Body Vitals;
    private WorldGeneration worldgen;
    private PlayerCamera plrcam;
    private MoodleManager moodles;
    private readonly AccessTools.FieldRef<MoodleManager, float> moodleUpdateTime = AccessTools.FieldRefAccess<MoodleManager, float>("updateTime"); // this variable is private normally
    private float prevUpdateTime = 0.5f;
    private bool revControlActive;
    private bool unchippedActive;
    private bool disfigActive;
    public static bool mindwipeActive;
    string trapSender;
    private List<Item> heldItems = new List<Item>();

    private void OnEnable()
    {
        Client = APClientClass.session;
        Vitals = this.gameObject.GetComponent<Body>();
        worldgen = GameObject.Find("World").GetComponent<WorldGeneration>();
        plrcam = GameObject.Find("Main Camera").GetComponent<PlayerCamera>();
        moodles = GameObject.Find("Main Camera/Canvas/Moodles").GetComponent<MoodleManager>();
        Startup.Logger.LogMessage("TrapHandler Ready!");
    }
    private void Start() // run this later so MoodleManager.Awake has indexed the sprites into MoodleManager.icons in time
    {
        var APLogo = Sprite.Create(
            texture: Startup.apassets.LoadAsset<Texture2D>("aplogo200"),
            pixelsPerUnit: 400,
            rect: new Rect(0, 0, 200, 200),
            pivot: new Vector2(0.5f, 0.5f));
        moodles.icons["death"] = APLogo; // replace unused Deceased moodle sprite
    }
    private void LateUpdate() // lateupdate so it's after normal moodle updates (no race condition, yippee!)
    {
        if (moodleUpdateTime(moodles) > prevUpdateTime && Vitals.alive) // timer reset, moodles were updated
        {
            if (revControlActive)
            {
                moodles.AddMoodle(5, "death", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Reversed Controls", trapSender + " reversed your controls! Lasts 10 seconds.", false, false);
            }
            if (unchippedActive)
            {
                moodles.AddMoodle(5, "death", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Unchipped", trapSender + " disabled your brainchip! Lasts 50 seconds.", false, false);
            }
            if (disfigActive)
            {
                moodles.AddMoodle(5, "death", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Disfigured", trapSender + " removed your jaw! Lasts 180 seconds.", false, false);
            }
            if (mindwipeActive)
            {
                moodles.AddMoodle(5, "death", "<color=#c97682>Ar<color=#75c275>ch<color=#ca94c2>ip<color=#d9a07d>el<color=#767ebd>ag<color=#eee391>o<color=#FFFFFF> Trap: Mindwipe", trapSender + " removed your memories! Lasts 70 seconds.", false, false);
            }
        }
        prevUpdateTime = moodleUpdateTime(moodles);
    }
    public void ProcessTraps(string TrapName, string ItemSender)
    {
        trapSender = ItemSender;
        if (TrapName == "Depression Trap")
        {
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                player.GetComponent<Body>().happiness -= 20;
            }
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} said something demoralizing. Mood decreased.", false);
        }
        if (TrapName == "Hearing Loss Trap")
        {
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                player.GetComponent<Body>().hearingLoss += 50;
            }
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: <b>WHAT?! WE CAN'T HEAR YOU {ItemSender.ToUpper()}!</b> Hearing loss increased.", false);
        }
        // if (TrapName == "Earthquake Trap") >>>>> not able to be synced in multiplayer. removed from this mod <<<<<
        if (TrapName == "Reverse Controls Trap")
        {
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} made you feel tipsy. Controls reversed.", false);
            StartCoroutine(ReverseControls());
        }
        if (TrapName == "Sleep Trap")
        {
            if (ScavWorldMap.rules.DisableSleep)
            {
                Chat.Server_ChatAnnouncement("Archipelago", "AP", "Received a Sleep Trap, but the server has sleeping disabled.");
                return; // sleeping is disabled in this server
            }
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                player.GetComponent<Body>().sleeping = true;
            }
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} thinks it's naptime. Good night!", false);
        }
        if (TrapName == "Unchipped Trap")
        {
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} is hacking into your brainchips!", false);
            StartCoroutine(UnchippedToggle());
        }
        // if (TrapName == "Elder Thornback Trap") >>>>> syncing nightmare. removed <<<<<
        if (TrapName == "Cave Ticks Trap")
        {
            var chosenPlayer = ServerMain.GetAllPlayerGameObjects()[UnityEngine.Random.Range(0, ServerMain.GetAllPlayerGameObjects().Count)];
            Instantiate(Resources.Load<GameObject>("caveticks"), chosenPlayer.transform.position, Quaternion.identity); // spawn at a random player
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} alerted the hoard. Good luck {chosenPlayer.GetComponent<NetPlayer>().playername}!", false);
        }
        if (TrapName == "Bad Rep Trap")
        {
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} spread gossip. All traders on this layer are now hostile.", false);
            foreach (var trader in FindObjectsOfType<TraderScript>())
            {
                if (trader.hostile) continue; // don't bother making them hostile a second time
                var packet = new KrokoshaTraderTrackerComponent.TraderStatePacket()
                {
                    reputation = 0,
                    hostility = 500,
                    freeDressing = false,
                    didHug = false,
                    freeAmount = 0,
                    haggleAmount = 0,
                    valueGiven = 0,
                    totalValueGiven = 0,
                };
                FastBufferWriter messageStream = new FastBufferWriter(Marshal.SizeOf(typeof(KrokoshaTraderTrackerComponent.TraderStatePacket)) + 8, (Unity.Collections.Allocator)2, -1);
                messageStream.WriteValueSafe<ulong>(trader.GetComponent<KrokoshaScavMultiGameObjectNetworkTracker>().syncinfo.syncid);
                KrokoshaTraderTrackerComponent.TraderStatePacket traderStatePacket = packet;
                messageStream.WriteValueSafe(traderStatePacket, default);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("TraderSync_ReputationState", ServerMain.AllClientIdsExceptHost, messageStream, NetworkDelivery.Reliable);
                messageStream.Dispose();
            }
            foreach (var player in ServerMain.GetAllPlayerGameObjects())
            {
                player.GetComponent<Body>().happiness += 3; // counteract hostility happiness decrease
            }
        }
        if (TrapName == "Disfigured Trap" && !Vitals.disfigured)
        {
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} thinks you all talk too much.", false);
            StartCoroutine(Disfigurement());
        }
        if (TrapName == "Fellow Experiment")
        {
            var chosenPlayer = ServerMain.GetAllPlayerGameObjects()[UnityEngine.Random.Range(0, ServerMain.GetAllPlayerGameObjects().Count)];
            Instantiate(Resources.Load<GameObject>("corpse"), chosenPlayer.transform.position, Quaternion.identity);
            ServerMain.Server_AnnounceAlert($"Archipelago Trap: {ItemSender} found {chosenPlayer.GetComponent<NetPlayer>().playername} a friend! ...wait", false);
        }
        if (TrapName == "Fragile Items Trap")
        {
            heldItems.Clear();
            foreach (var slot in FindObjectsOfType<InventorySlot>())
            {
                try
                {
                    heldItems.Add(slot.gameObject.GetComponentInChildren<Item>());
                    Debug.Log(slot.limb.fullName + " has " + slot.gameObject.GetComponentInChildren<Item>().fullName);
                }
                catch
                {
                    continue;
                }
            }
            Item chosenItem = heldItems.ElementAt(UnityEngine.Random.Range(0, heldItems.Count + 1));
            Debug.Log(chosenItem.fullName + " was chosen to be damaged");
            if (chosenItem.TryGetComponent<WaterContainerItem>(out WaterContainerItem _))
            {
                plrcam.DoAlert("Trap: " + ItemSender + " poked a hole in your " + chosenItem.fullName, false);
            }
            else
            {
                plrcam.DoAlert("Trap: " + ItemSender + " made your warranty expire. " + chosenItem.fullName + " was destroyed.", false);
            }
            chosenItem.condition = 0;
        }
        if (TrapName == "Mindwipe Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " thinks you know too much.", false);
            StartCoroutine(Mindwipe());
        }
        if (TrapName == "Pushup Trap")
        {
            plrcam.DoAlert("Trap: " + ItemSender + " demands you get on the ground and give them twenty!", false);
            plrcam.DoBodyWorkout(0);
            plrcam.ToggleWoundView(false); // DoBodyWorkout forces the woundview open/closed. this reverses that. nothing i can do about the sound effect though
        }
    }
    IEnumerator ReverseControls()
    {
        revControlActive = true;
        foreach (var player in ServerMain.GetAllPlayerGameObjects())
        {
            player.GetComponent<Body>().reversedControls = true;
        }
        yield return new WaitForSecondsRealtime(10);
        foreach (var player in ServerMain.GetAllPlayerGameObjects())
        {
            player.GetComponent<Body>().reversedControls = false;
        }
        revControlActive = false;
    }
    IEnumerator UnchippedToggle()
    {
        unchippedActive = true;
        foreach (var player in ServerMain.GetAllPlayerGameObjects())
        {
            player.GetComponent<NetPlayer>().unchipped = true;
        }
        yield return new WaitForSecondsRealtime(50);
        foreach (var player in ServerMain.GetAllPlayerGameObjects())
        {
            player.GetComponent<NetPlayer>().unchipped = false;
        }
        GameObject.Find("LineOfSight").SetActive(false);
        unchippedActive = false;
    }
    IEnumerator Disfigurement()
    {
        disfigActive = true;
        List<float> prevHeadHealth = [];
        foreach (var player in ServerMain.GetAllPlayerGameObjects())
        {
            prevHeadHealth.Add(player.GetComponent<Body>().limbs[0].muscleHealth); // disfiguring removes 25 muscle health
            player.GetComponent<Body>().Disfigure();
        }
        yield return new WaitForSecondsRealtime(180);
        int playernum = 0;
        foreach (var player in ServerMain.GetAllPlayerGameObjects())
        {
            player.GetComponent<Body>().disfigured = false;
            player.GetComponent<Body>().limbs[0].muscleHealth = prevHeadHealth[playernum]; // so we restore that afterwards
            playernum++;
        }
        disfigActive = false;
    }
    IEnumerator Mindwipe()
    {
        mindwipeActive = true;
        var preWipehl = Vitals.hearingLoss; // Mindwipe causes a lot of hearing loss, so we restore it after
        var preWipebh = Vitals.brainHealth; // Again, Mindwipe changes brain health, so we restore it
        Skills skills = Vitals.skills;
        int INTSkillPreWipe = skills.INT; // Mindwipe resets INT to 0, so we'll save it to restore after
        float INTExpPreWipe = skills.expINT;
        int INTMaxPreWipe = skills.maxINT;
        int INTMinPreWipe = skills.minINT;
        MindwipeScript mw = Vitals.gameObject.AddComponent<MindwipeScript>();
        yield return new WaitForSecondsRealtime(70);
        Destroy(mw);
        Destroy(GameObject.Find("Main Camera/Canvas/MindwipeVignette(Clone)"));
        skills.INT = INTSkillPreWipe;
        skills.expINT = +INTExpPreWipe;
        skills.maxINT = INTMaxPreWipe;
        skills.minINT = INTMinPreWipe;
        Vitals.hearingLoss = preWipehl;
        if (Vitals.alive) Vitals.brainHealth = preWipebh; // only if Experiment didn't die during the mindwipe
        mindwipeActive = false;
    }
    IEnumerator Thornback()
    {
        plrcam.currentThreatTheme = 15; // play the Elder Thornback first phase theme
        plrcam.threatMusicTime = 90; // for 90 seconds
        moodles.horrifiedLevel = 1;
        yield return new WaitForSecondsRealtime(11); // timed to be on the beat drop for maximum effect (because i'm just cool like that)
        moodles.horrifiedLevel = 3;
        yield return new WaitForSecondsRealtime(79);
        moodles.horrifiedLevel = 0;
    }
}