using UnityEngine;
using UnityEngine.UI;

namespace CUAP;

public class SawianUnlocks : MonoBehaviour
{
    private static PreRunScript prScript;
    private static bool hasExperiment;
    private static bool hasMilky;
    private static bool hasDune;

    public static void SetupSawianUnlocks()
    {
        PlayerPrefs.SetInt("wonchar0", 1); // unlocks Milky
        PlayerPrefs.SetInt("wonchar1", 1); // unlocks Dune
        prScript = PreRunScript.instance;
        hasExperiment = false;
        hasMilky = false;
        hasDune = false;
        SyncSawianButtons();
        bool[] unlocked = {hasExperiment, hasMilky, hasDune};
        if (!unlocked[PreRunScript.characterSelected]) // the selected sawian is locked. change it
        {
            for (int i = 0; i < unlocked.Length; i++)
            {
                if (unlocked[i])
                {
                    PreRunScript.characterSelected = i;
                    break;
                }
            }
        }
    }

    public static void SyncSawianButtons()
    {
        bool[] unlocked = { hasExperiment, hasMilky, hasDune };
        for (int i = 0; i < unlocked.Length; i++)
        {
            GameObject character = prScript.charImages[i].gameObject;
            Button button = character.GetComponent<Button>();
            UITooltip tooltip = character.GetComponent<UITooltip>();
            if (unlocked[i]) // this sawian is unlocked, remove the tooltip and make it interactable
            {
                if (tooltip != null)
                {
                    Destroy(tooltip);
                }
                button.interactable = true;
            }
            else // this sawian is locked, add a tooltip and make it not interactable
            {
                tooltip = character.AddComponent<UITooltip>();
                tooltip.skipLocale = true;
                tooltip.tipName = $"{APLocale.Get("charLocked", APLocale.APLanguageType.UI)}{APCanvas.coloredAPText}!";
                tooltip.tipDesc = APLocale.Get("charLockedDesc", APLocale.APLanguageType.UI);
                button.interactable = false;
            }
        }
    }

    public static void UnlockSawian(int Sawian)
    {
        switch (Sawian)
        {
            case 0:
                hasExperiment = true;
                break;
            case 1:
                hasMilky = true;
                break;
            case 2:
                hasDune = true;
                break;
            default:
                Startup.Logger.LogWarning($"Tried to unlock a Sawian that does not exist ({Sawian})");
                break;
        }
        if (!APCanvas.InGame) // on main menu
        {
            SyncSawianButtons();
        }
    }
}