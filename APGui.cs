using System.IO;
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
    public static bool DeathlinkEnabled = false;
    public static string GUIDescription = "";
    public static bool InGame
    {
        get
        {
            var camera = GameObject.Find("Main Camera");
            return camera != null && camera.GetComponent<PlayerCamera>() != null;
        }
    }

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
            GUI.Box(new Rect(10 + Offset.x, 10 + Offset.y, 200, 300), "Archipelago Client");

            GUI.Label(new Rect(20 + Offset.x, 40 + Offset.y, 300, 30), "Address:Port", TextStyle);
            Ipporttext = GUI.TextField(new Rect(20 + Offset.x, 60 + Offset.y, 180, 25), Ipporttext, 25);

            GUI.Label(new Rect(20 + Offset.x, 90 + Offset.y, 300, 30), "Password", TextStyle);
            Password = GUI.TextField(new Rect(20 + Offset.x, 110 + Offset.y, 180, 25), Password, 25);

            GUI.Label(new Rect(20 + Offset.x, 140 + Offset.y, 300, 30), "Slot", TextStyle);
            Slot = GUI.TextField(new Rect(20 + Offset.x, 160 + Offset.y, 180, 25), Slot, 25);
        }
        else
        {
            try
            {
                if (InGame)
                {
                    Offset = new(0, 0);
                }
                else
                {
                    Offset = new(0, -100);
                }
            }
            catch
            {
                Offset = new(0, -100);
            }
            GUI.Box(new Rect(10 + Offset.x, 10 + Offset.y + 100, 200, 150), "Archipelago Client");
            GUI.Label(new Rect(15 + Offset.x, Offset.y + 155, 190, 150), GUIDescription);
        }

        if (!IsConnected() && GUI.Button(new Rect(20 + Offset.x, 210 + Offset.y, 180, 30), "Connect"))
        {
            var ipPortSplit = Ipporttext.Split(':');
            if (!int.TryParse(ipPortSplit[1], out var port))
            {
                State = $"[{ipPortSplit[1]}] is not a valid port";
                return;
            }
            var error = TryConnect(port, Slot, ipPortSplit[0], Password);
            if (error is not null)
            {
                State = string.Join("\n", error);
                return;
            }
            State = "";
            File.WriteAllText("ApConnection.txt", $"{Ipporttext}\n{Password}\n{Slot}");
        }
        if (!InGame)
        {
            if (IsConnected() && GUI.Button(new Rect(20 + Offset.x, 210 + Offset.y, 180, 30), "Disconnect"))
            {
                Disconnect();
            }
        }

        GUI.Label(new Rect(20 + Offset.x, 240 + Offset.y, 300, 30),
            State != "" ? State : IsConnected() ? "Connected" : "Not Connected",
            IsConnected() ? TextStyleGreen : TextStyleRed);
    }

    private void Update() => APClientClass.Update();

    public static void UpdateGUIDescriptions()
    {
        if (APClientClass.selectedGoal == 1)
        {
            var maxDepth = (300 * APClientClass.DepthExtendersRecieved) + 300;
            GUIDescription =
                """
                Goal: Reach Depth
                Depth Extenders: <de>
                Max Depth: <md>
                """;
            GUIDescription = GUIDescription.Replace("<de>", APClientClass.DepthExtendersRecieved.ToString());
            GUIDescription = GUIDescription.Replace("<md>", maxDepth.ToString());
        }
        else if (APClientClass.selectedGoal == 2)
        {
            GUIDescription =
                """
                Goal: Escape Overgrown
                Layer Unlocks: <lu>
                Deepest Layer: <dl>
                """;
            GUIDescription = GUIDescription.Replace("<lu>", APClientClass.DepthExtendersRecieved.ToString());
            GUIDescription = GUIDescription.Replace("<dl>", LayerLocker.LayerIDToName[APClientClass.DepthExtendersRecieved]).Replace(" Unlock", "");
        }
        else if (APClientClass.selectedGoal == 3)
        {
            GUIDescription =
                """
                Goal: Defeat Elder
                Overgrown: <ou>
                """;
            GUIDescription = GUIDescription.Replace("<ou>", APClientClass.LayerUnlockDictionary.Contains("Overgrown Depths Unlock") ? "Unlocked" : "Locked");
        }
        else if (APClientClass.selectedGoal == 4)
        {
            GUIDescription =
                """
                Goal: Craftsanity
                Unlocked: <ru>
                Crafted: <rc>
                """;
            GUIDescription = GUIDescription.Replace("<ru>", Recipes.recipes.Count + "/112");
            GUIDescription = GUIDescription.Replace("<rc>", CraftingChecks.CraftedRecipes.ToString() + "/112");
        }
    }
}