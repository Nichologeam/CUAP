using BepInEx;
using BepInEx.Logging;
using System.IO;
using System.Linq;
using UnityEngine;
using static CUAP.APClientClass;

namespace CUAP;

// modified version of: https://github.com/SWCreeperKing/PowerwashSimAP/blob/master/src/APGui.cs
public class APGui : MonoBehaviour
{
    public static bool ShowGUI = true;
    public static string Ipporttext = "archipelago.gg:38281";
    public static string Password = "";
    public static string Slot = "Experiment";
    public static string State = "";
    public static Vector2 Offset;
    public static bool WasPressed;
    public static bool DeathlinkEnabled;

    public static GUIStyle TextStyle = new()
    {
        fontSize = 12,
        normal =
        {
            textColor = Color.white
        }
    };

    public static GUIStyle TextStyleGreen = new()
    {
        fontSize = 12,
        normal =
        {
            textColor = Color.green
        }
    };

    public static GUIStyle TextStyleRed = new()
    {
        fontSize = 12,
        normal =
        {
            textColor = Color.red
        }
    };

    private void Awake()
    {
        if (!File.Exists("ApConnection.txt")) return; // Read saved slot information from file
        var fileText = File.ReadAllText("ApConnection.txt").Replace("\r", "").Split('\n');
        Ipporttext = fileText[0];
        Password = fileText[1];
        Slot = fileText[2];
    }

    void OnGUI()
    {
        if (!ShowGUI) return;
        if (!IsConnected())
        {
            Offset = new(1700, 0);
            GUI.Box(new Rect(10 + Offset.x, 10 + Offset.y, 200, 300), "C:U Archipelago Client");

            GUI.Label(new Rect(20 + Offset.x, 40 + Offset.y, 300, 30), "Address:Port", TextStyle);
            Ipporttext = GUI.TextField(new Rect(20 + Offset.x, 60 + Offset.y, 180, 25), Ipporttext, 25);

            GUI.Label(new Rect(20 + Offset.x, 90 + Offset.y, 300, 30), "Password", TextStyle);
            Password = GUI.TextField(new Rect(20 + Offset.x, 110 + Offset.y, 180, 25), Password, 25);

            GUI.Label(new Rect(20 + Offset.x, 140 + Offset.y, 300, 30), "Slot", TextStyle);
            Slot = GUI.TextField(new Rect(20 + Offset.x, 160 + Offset.y, 180, 25), Slot, 25);
        }
        else
        {
            Offset = new(1700,-100);
            GUI.Box(new Rect(10 + Offset.x, 10 + Offset.y + 100, 200, 150), "Archipelago Client");
            GUI.Label(new Rect(20 + Offset.x, Offset.y + 155, 150, 35),
                    $"Depth Extenders: " + APClientClass.DepthExtendersRecieved + "          Max Depth: " + ((300 * APClientClass.DepthExtendersRecieved) + 300));
        }

        if (!IsConnected() && GUI.Button(new Rect(15 + Offset.x, 210 + Offset.y, 90, 30), "Connect"))
        {
            Startup.Logger.LogMessage("Connecting without Deathlink");
            DeathlinkEnabled = false;
            var ipPortSplit = Ipporttext.Split(':');
            if (!int.TryParse(ipPortSplit[1], out var port))
            {
                State = $"[{ipPortSplit[1]}] is not a valid port";
                return;
            }
            var error = TryConnect(port, Slot, ipPortSplit[0], Password, false);
            if (error is not null)
            {
                State = string.Join("\n", error);
                return;
            }
            
            State = "";
            File.WriteAllText("ApConnection.txt", $"{Ipporttext}\n{Password}\n{Slot}");
        }

        if (!IsConnected() && GUI.Button(new Rect(115 + Offset.x, 210 + Offset.y, 90, 30), "DeathLink"))
        {
            Startup.Logger.LogMessage("Connecting with Deathlink");
            DeathlinkEnabled = true;
            var ipPortSplit = Ipporttext.Split(':');
            if (!int.TryParse(ipPortSplit[1], out var port))
            {
                State = $"[{ipPortSplit[1]}] is not a valid port";
                return;
            }
            var error = TryConnect(port, Slot, ipPortSplit[0], Password, true);
            if (error is not null)
            {
                State = string.Join("\n", error);
                return;
            }
        }

        if (IsConnected() && GUI.Button(new Rect(20 + Offset.x, 210 + Offset.y, 180, 30), "Disconnect(Main menu only!)"))
        {
            Disconnect();
        }

        GUI.Label(new Rect(20 + Offset.x, 240 + Offset.y, 300, 30),
            State != "" ? State : IsConnected() ? "Connected" : "Not Connected",
            IsConnected() ? TextStyleGreen : TextStyleRed);
    }

    private void Update() => APClientClass.Update();
}