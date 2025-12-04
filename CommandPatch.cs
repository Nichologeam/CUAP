using CreepyUtil.Archipelago;
using UnityEngine;
using TMPro;

namespace CUAP;

public class CommandPatch : MonoBehaviour
{
    public static ApClient Client;
    private ConsoleScript Console;
    private TMP_InputField input;
    private string LastFrameText;
    private string ChatMessage;

    private void OnEnable()
    {
        Client = APClientClass.Client;
        Console = this.gameObject.GetComponent<ConsoleScript>();
        Startup.Logger.LogMessage("Console has been patched!");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && Console.isActiveAndEnabled) // Console exists and a message was just sent
        {
            input = Console.GetComponent<TMP_InputField>();
            try
            {
                if (LastFrameText.Substring(0, 4) != "talk")
                {
                    // Was the command 'talk'? If not, don't continue
                    return;
                }
                else
                {
                    // Split off the 'talk' command, then send the rest to the server
                    ChatMessage = LastFrameText.Substring(5);
                    Client.Say(ChatMessage);
                }
            }
            catch
            {
                return; // Message was less than 4 characters. Doing this to prevent errors when sending blank or short messages.
            }
        }
        try // Enter was not pressed this frame, does the console exist?
        {
            input = Console.GetComponent<TMP_InputField>();
            LastFrameText = input.text;
        }
        catch // No? The console isn't open. Return.
        {
            return; // This should realistically never happen, but you never know...
        }
    }
}